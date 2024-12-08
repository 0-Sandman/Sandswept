using System;
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
                }
            }

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

            lr.SetPosition(0, base.transform.position);
            lr.SetPosition(1, body.corePosition);
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
                LightningOrb orb = new()
                {
                    attacker = body.gameObject,
                    damageValue = damagePerTick,
                    bouncesRemaining = 0,
                    isCrit = damage.crit,
                    lightningType = LightningOrb.LightningType.Loader,
                    origin = base.transform.position,
                    procCoefficient = 1f,
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
                    orb.simple.desiredForwardSpeed = 0;
                    orb.simple.updateAfterFiring = true;
                    orb.locked = true;
                    orb.lr.enabled = false;
                    Util.PlaySound("Play_gravekeeper_attack2_shoot_singleChain", orb.gameObject);
                }
            }
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