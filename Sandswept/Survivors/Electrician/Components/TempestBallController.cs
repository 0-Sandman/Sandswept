using System;
using R2API.Networking.Interfaces;
using RoR2.Orbs;

namespace Sandswept.Survivors.Electrician
{
    public class TempestBallController : MonoBehaviour
    {
        public float ticksPerSecond = 4;
        public SphereCollider sphere;
        public LineRenderer lr;
        private float stopwatch = 0f;
        private float delay;
        private ProjectileController controller;
        private ProjectileDamage damage;
        private BlastAttack attack;
        public static Dictionary<CharacterBody, List<TempestBallController>> orbs = new();
        private ProjectileSimple simple;
        private bool locked = false;
        private CharacterBody body;
        private float damagePerTick;
        private Transform lineOrigin;
        private float sigma;

        public void Start()
        {
            controller = GetComponent<ProjectileController>();
            damage = GetComponent<ProjectileDamage>();

            delay = 1f / ticksPerSecond;

            simple = GetComponent<ProjectileSimple>();

            if (controller.owner)
            {
                body = controller.owner.GetComponent<CharacterBody>();

                if (body)
                {
                    if (!orbs.ContainsKey(body)) orbs.Add(body, new());
                    orbs[body].Add(this);

                    lineOrigin = body.GetComponent<ModelLocator>().modelTransform.GetComponent<ChildLocator>().FindChild("MuzzleOrb");
                }
            }

            simple.updateAfterFiring = true;

            damagePerTick = damage.damage / ticksPerSecond;
        }

        public void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                stopwatch += Time.fixedDeltaTime;

                if (stopwatch >= delay)
                {
                    stopwatch = 0f;

                    HandleBlastAuthority(base.transform.position);
                }
            }

            simple.desiredForwardSpeed += simple.desiredForwardSpeed * 0.5f * Time.fixedDeltaTime;

            if (!body) return;

            lr.SetPosition(0, base.transform.position);
            lr.SetPosition(1, lineOrigin.position);

        }
        public void HandleBlastAuthority(Vector3 pos)
        {
            SphereSearch search = new()
            {
                radius = sphere.radius,
                mask = LayerIndex.entityPrecise.mask,
                origin = base.transform.position
            };
            search.RefreshCandidates();
            search.FilterCandidatesByDistinctHurtBoxEntities();
            search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(TeamIndex.Player));

            foreach (HurtBox box in search.GetHurtBoxes())
            {
                VoltLightningOrb orb = new()
                {
                    attacker = body.gameObject,
                    damageValue = damagePerTick,
                    isCrit = damage.crit,
                    origin = base.transform.position,
                    procCoefficient = Mathf.Clamp(1f - Util.Remap(Vector3.Distance(box.transform.position, base.transform.position), 3f, 15f, 0, 0.45f), 0.55f, 1f),
                    target = box,
                    teamIndex = TeamIndex.Player,
                    damageType = DamageType.SlowOnHit
                };

                OrbManager.instance.AddOrb(orb);
            }
        }

        public static void LockAllOrbs(CharacterBody body)
        {
            if (orbs.ContainsKey(body))
            {
                foreach (TempestBallController orb in orbs[body])
                {
                    orb.Lock();
                }
            }
        }

        public void Lock()
        {
            LockNetworked();
            new CallNetworkedMethod(base.gameObject, "LockNetworked").Send(R2API.Networking.NetworkDestination.Clients);
        }

        public void LockNetworked()
        {
            simple.desiredForwardSpeed = 0;
            simple.updateAfterFiring = true;
            locked = true;
            lr.enabled = false;
            Util.PlaySound("Play_gravekeeper_attack2_shoot_singleChain", base.gameObject);
        }

        public void OnDestroy()
        {
            if (controller.owner)
            {
                CharacterBody body = controller.owner.GetComponent<CharacterBody>();

                if (body && orbs.ContainsKey(body))
                {
                    orbs[body].Remove(this);
                }
            }
        }
    }
}