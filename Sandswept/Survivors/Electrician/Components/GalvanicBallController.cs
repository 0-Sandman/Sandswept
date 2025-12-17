using System;
using System.Linq;
using IL.RoR2.Items;
using R2API.Networking.Interfaces;
using RoR2.Orbs;

namespace Sandswept.Survivors.Electrician
{
    public class GalvanicBallController : MonoBehaviour
    {
        public float radius = 13f;
        public float damage = 1f;
        public bool hasBouncedEnemy = false;
        public ProjectileDamage pDamage;
        public ProjectileController controller;
        public bool hasStuck = false;
        public CharacterBody owner;
        public Rigidbody body;
        public float stopwatch = 0f;
        public float damageCoeff;
        public float interval;
        public float range;
        public float maxTargets;
        public Transform modelTransform;
        public GameObject impactVFX;

        public void Start()
        {
            pDamage = GetComponent<ProjectileDamage>();
            controller = GetComponent<ProjectileController>();
            owner = controller.owner.GetComponent<CharacterBody>();
            body = GetComponent<Rigidbody>();

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[owner.skinIndex].nameToken;

                impactVFX = skinNameToken switch
                {
                    "VOLT_SKIN_COVENANT_NAME" => VFX.GalvanicBolt.impactCovenant,
                    _ => VFX.GalvanicBolt.impactDefault
                };
            }

            GetComponent<ProjectileProximityBeamController>().attackInterval /= owner.attackSpeed;

            var projectileProximityBeamController = GetComponent<ProjectileProximityBeamController>();
            damageCoeff = projectileProximityBeamController.damageCoefficient;
            interval = projectileProximityBeamController.attackInterval;
            range = projectileProximityBeamController.attackRange;
            maxTargets = projectileProximityBeamController.attackFireCount;
            projectileProximityBeamController.enabled = false;
            // what is the point ? ? ?

            controller.ghost.GetComponentInChildren<ParticleSystem>().Stop();
        }

        public void OnDestroy()
        {
            if (controller.ghost)
            {
                GameObject.Destroy(controller.ghost.gameObject);
            }
        }

        public Transform GetModelTransform()
        {
            if (!owner || !owner.modelLocator)
            {
                return null;
            }
            return owner.modelLocator.modelTransform;
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
                if (!box)
                {
                    continue;
                }

                var boxHealthComponent = box.healthComponent;
                if (!boxHealthComponent)
                {
                    continue;
                }

                var enemyBody = boxHealthComponent.body;
                if (!enemyBody)
                {
                    continue;
                }

                if (box && !alreadyStruck.Contains(boxHealthComponent) && enemyBody.teamComponent.teamIndex != team)
                {
                    VoltLightningOrb orb = new()
                    {
                        procCoefficient = 1,
                        damageValue = pDamage.damage * damageCoeff,
                        attacker = controller.owner,
                        origin = base.transform.position,
                        target = box,
                        isCrit = pDamage.crit,
                        teamIndex = team,
                        attackerBody = owner,
                        victimBody = enemyBody
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
                        Util.PlaySound("Play_mage_R_lightningBlast", gameObject);
                        EffectManager.SpawnEffect(impactVFX, new EffectData
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
                        hasStuck = true;

                        if (controller.ghost)
                        {
                            controller.ghost.GetComponentInChildren<ParticleSystem>().Play();
                            controller.ghost.gameObject.FindComponent<MeshRenderer>("Radius").enabled = true;
                        }
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