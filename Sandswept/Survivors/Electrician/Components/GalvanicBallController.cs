using System;
using System.Linq;
using IL.RoR2.Items;
using RoR2.Orbs;

namespace Sandswept.Survivors.Electrician
{
    public class GalvanicBallController : MonoBehaviour
    {
        public float radius = 13f;
        public float damage = 1f;
        private bool hasBouncedEnemy = false;
        private ProjectileDamage pDamage;
        private ProjectileController controller;
        private bool hasStuck = false;
        private CharacterBody owner;
        private Rigidbody body;
        private float stopwatch = 0f;
        private float damageCoeff;
        private float interval;
        private float range;
        private float maxTargets;

        public void Start()
        {
            pDamage = GetComponent<ProjectileDamage>();
            controller = GetComponent<ProjectileController>();
            owner = controller.owner.GetComponent<CharacterBody>();
            body = GetComponent<Rigidbody>();

            GetComponent<ProjectileProximityBeamController>().attackInterval /= owner.attackSpeed;

            var p = GetComponent<ProjectileProximityBeamController>();
            damageCoeff = p.damageCoefficient;
            interval = p.attackInterval;
            range = p.attackRange;
            maxTargets = p.attackFireCount;
            p.enabled = false;
        }

        public void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;

            if (stopwatch >= interval && NetworkServer.active)
            {
                stopwatch = 0f;
                ShockNearby();
            }
        }

        public void ShockNearby()
        {
            Collider[] collidersTmp = Physics.OverlapSphere(base.transform.position, radius, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore);
            List<HealthComponent> alreadyStruck = new();
            IEnumerable<Collider> colliders = collidersTmp.Shuffle(Run.instance.spawnRng);

            TeamIndex team = GetComponent<TeamFilter>().teamIndex;

            for (int i = 0; i < colliders.Count(); i++)
            {
                HurtBox box = colliders.ElementAt(i).GetComponent<HurtBox>();

                if (box && !alreadyStruck.Contains(box.healthComponent) && box.healthComponent.body.teamComponent.teamIndex != team)
                {
                    LightningOrb orb = new()
                    {
                        procCoefficient = 1,
                        damageValue = pDamage.damage * damageCoeff,
                        bouncesRemaining = 0,
                        lightningType = LightningOrb.LightningType.Loader,
                        attacker = controller.owner,
                        origin = base.transform.position,
                        target = box,
                        isCrit = pDamage.crit,
                        teamIndex = team
                    };

                    OrbManager.instance.AddOrb(orb);

                    alreadyStruck.Add(box.healthComponent);

                    if (alreadyStruck.Count() >= maxTargets)
                    {
                        return;
                    }
                }
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (!hasStuck && NetworkServer.active)
            {
                if (collision.collider)
                {
                    BlastAttack attack = new()
                    {
                        radius = radius,
                        attacker = owner.gameObject,
                        position = collision.contacts[0].point,
                        crit = pDamage.crit,
                        losType = BlastAttack.LoSType.None,
                        falloffModel = BlastAttack.FalloffModel.None,
                        damageType = pDamage.damageType,
                        teamIndex = owner.teamComponent.teamIndex,
                        procCoefficient = 1f,
                        baseDamage = pDamage.damage * damage
                    };

                    if (!hasBouncedEnemy)
                    {
                        attack.Fire();
                        Util.PlaySound("Play_loader_R_shock", base.gameObject);
                        EffectManager.SpawnEffect(Electrician.staticSnareImpactVFX, new EffectData
                        {
                            origin = attack.position,
                            scale = attack.radius * 2f
                        }, true);
                        hasBouncedEnemy = true;
                    }

                    var rb = GetComponent<Rigidbody>();

                    if (collision.collider.gameObject.layer == LayerIndex.world.intVal)
                    {
                        rb.isKinematic = true;
                        rb.velocity = Vector3.zero;
                        base.transform.up = collision.contacts[0].normal;
                        base.GetComponentInChildren<ParticleSystem>().Play();
                        hasStuck = true;

                        base.gameObject.FindComponent<MeshRenderer>("Radius").enabled = true;
                    }
                    else
                    {
                        rb.useGravity = true;
                        rb.velocity = Vector3.zero;
                        // rb.velocity += Physics.gravity;
                    }
                }
            }
        }
    }
}