using System;
using System.Linq;

namespace Sandswept.Survivors.Electrician
{
    public class Electrician : SurvivorBase<Electrician>
    {
        public override string Name => "Electrician";

        public override string Description => "TBD";

        public override string Subtitle => "TBD";

        public override string Outro => "TBD";

        public override string Failure => "TBD";
        public static GameObject GalvanicBolt;
        public static GameObject TempestSphere;
        public static GameObject StaticSnare;
        public static DamageAPI.ModdedDamageType Grounding = DamageAPI.ReserveDamageType();

        public override void LoadAssets()
        {
            base.LoadAssets();

            Body = Main.Assets.LoadAsset<GameObject>("ElectricianBody.prefab");

            Body.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpStandard;

            var cb = Body.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Paths.GameObject.StandardCrosshair;
            cb.preferredPodPrefab = Paths.GameObject.RoboCratePod;

            SurvivorDef = Main.Assets.LoadAsset<SurvivorDef>("sdElectrician.asset");
            SurvivorDef.cachedName = "Electrician"; // for eclipse fix

            Master = PrefabAPI.InstantiateClone(Paths.GameObject.EngiMonsterMaster, "ElectricianMaster");

            SkillLocator locator = Body.GetComponent<SkillLocator>();
            ReplaceSkills(locator.primary, new SkillDef[] { Skills.GalvanicBolt.instance });
            ReplaceSkills(locator.secondary, new SkillDef[] { Skills.TempestSphere.instance });
            ReplaceSkills(locator.utility, new SkillDef[] { Skills.StaticSnare.instance });
            ReplaceSkills(locator.special, new SkillDef[] { Skills.SignalOverload.instance });

            "SANDSWEPT_ELECTR_PASSIVE_NAME".Add("Volatile Shields");
            "SANDSWEPT_ELECTR_PASSIVE_DESC".Add("Discharge yo <style=cIsUtility>balls</style> when struck by enemy <style=cDeath>jaws</style>.");

            GalvanicBolt = Main.Assets.LoadAsset<GameObject>("GalvanicBallProjectile.prefab");
            ContentAddition.AddProjectile(GalvanicBolt);

            TempestSphere = Main.Assets.LoadAsset<GameObject>("TempestSphereProjectile.prefab");
            TempestSphere.transform.Find("AreaIndicator").GetComponent<MeshRenderer>().sharedMaterial = Paths.Material.matGrandparentGravArea;
            ContentAddition.AddProjectile(TempestSphere);

            StaticSnare = Main.Assets.LoadAsset<GameObject>("TripwireMineProjectile.prefab");
            ContentAddition.AddProjectile(StaticSnare);

            On.RoR2.HealthComponent.TakeDamage += HandleGroundingShock;
        }

        private void HandleGroundingShock(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.HasModdedDamageType(Grounding) && NetworkServer.active)
            {
                CharacterMotor motor = self.GetComponent<CharacterMotor>();
                RigidbodyMotor motor2 = self.GetComponent<RigidbodyMotor>();

                if ((motor && !motor.isGrounded) || (motor2))
                {
                    damageInfo.damage *= 1.5f;
                    damageInfo.damageType |= DamageType.Shock5s;
                    damageInfo.damageColorIndex = DamageColorIndex.Luminous;

                    EffectManager.SpawnEffect(Paths.GameObject.SojournExplosionVFX, new EffectData
                    {
                        origin = self.body.corePosition,
                        scale = self.body.bestFitRadius
                    }, true);

                    PhysForceInfo info = default;
                    info.massIsOne = true;
                    info.force = Vector3.down * 40f;

                    if (motor) motor.ApplyForceImpulse(in info);
                    if (motor2) motor2.ApplyForceImpulse(in info);
                }
            }

            orig(self, damageInfo);
        }
    }

    public class TripwireController : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public Transform explo;
        public float interval;
        public int hitRate;
        public static List<TripwireController> AllTripwireControllers = new();
        private float stopwatch;
        private BulletAttack attack;
        public TripwireController paired;
        private ProjectileDamage pDamage;
        private ProjectileController controller;
        private float stopwatchBeam;
        private float delay;
        private float stopwatch2 = 0f;
        private BlastAttack blast;
        private GameObject effect;

        public void Start()
        {
            controller = GetComponent<ProjectileController>();
            pDamage = GetComponent<ProjectileDamage>();

            delay = 1f / hitRate;

            attack = new()
            {
                damage = pDamage.damage * 3f * delay,
                radius = lineRenderer.startWidth,
                damageType = DamageType.Stun1s,
                isCrit = pDamage.crit,
                owner = controller.owner,
                procCoefficient = 0.1f,
                stopperMask = LayerIndex.noCollision.mask,
                falloffModel = BulletAttack.FalloffModel.None
            };

            blast = new()
            {
                radius = 3f,
                attacker = attack.owner,
                crit = pDamage.crit,
                losType = BlastAttack.LoSType.None,
                falloffModel = BlastAttack.FalloffModel.None,
                damageType = pDamage.damageType,
                teamIndex = controller.teamFilter.teamIndex,
                procCoefficient = 1f,
                baseDamage = pDamage.damage * 3f
            };
            blast.damageType = DamageType.Shock5s;

            effect = Paths.GameObject.LoaderGroundSlam;
        }

        public void FixedUpdate()
        {
            stopwatch2 += Time.fixedDeltaTime;

            if (stopwatch2 >= 0.2f && !paired && AllTripwireControllers.Count >= 2)
            {
                paired = AllTripwireControllers.OrderBy(x => Vector3.Distance(base.transform.position, x.transform.position)).FirstOrDefault(x => x.paired != this && x != this);
                stopwatch2 = 0f;
            }

            lineRenderer.enabled = paired;

            if (paired)
            {
                lineRenderer.SetPosition(0, explo.position);
                lineRenderer.SetPosition(1, paired.explo.position);
            }

            if (!NetworkServer.active)
            {
                return;
            }

            if (paired)
            {
                stopwatchBeam += Time.fixedDeltaTime;

                if (stopwatchBeam >= delay)
                {
                    stopwatchBeam = 0f;

                    attack.origin = explo.position;
                    attack.aimVector = (paired.explo.position - explo.position).normalized;
                    attack.maxDistance = Vector3.Distance(explo.position, paired.explo.position);

                    attack.Fire();
                }
            }

            stopwatch += Time.fixedDeltaTime;

            if (stopwatch >= interval)
            {
                stopwatch = 0f;

                blast.position = explo.position;
                blast.Fire();

                EffectManager.SpawnEffect(effect, new EffectData
                {
                    origin = blast.position,
                    scale = blast.radius * 2
                }, true);
            }
        }

        public void OnEnable()
        {
            AllTripwireControllers.Add(this);
        }

        public void OnDisable()
        {
            AllTripwireControllers.Remove(this);
        }
    }

    public class GalvanicBallController : MonoBehaviour
    {
        public float radius = 7f;
        public float damage = 1f;
        private bool hasBouncedEnemy = false;
        private ProjectileDamage pDamage;
        private ProjectileController controller;
        private CharacterBody owner;
        private Rigidbody body;

        public void Start()
        {
            pDamage = GetComponent<ProjectileDamage>();
            controller = GetComponent<ProjectileController>();
            owner = controller.owner.GetComponent<CharacterBody>();
            body = GetComponent<Rigidbody>();
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (!hasBouncedEnemy && NetworkServer.active && body.velocity.magnitude > 10f)
            {
                if (collision.collider && collision.collider.GetComponent<HurtBox>())
                {
                    hasBouncedEnemy = true;

                    BlastAttack attack = new()
                    {
                        radius = radius,
                        attacker = owner.gameObject,
                        position = base.transform.position,
                        crit = pDamage.crit,
                        losType = BlastAttack.LoSType.None,
                        falloffModel = BlastAttack.FalloffModel.None,
                        damageType = pDamage.damageType,
                        teamIndex = owner.teamComponent.teamIndex,
                        procCoefficient = 1f,
                        baseDamage = pDamage.damage * damage
                    };

                    attack.Fire();

                    Util.PlaySound("Play_loader_R_shock", base.gameObject);
                    EffectManager.SpawnEffect(Paths.GameObject.LoaderGroundSlam, new EffectData
                    {
                        origin = attack.position,
                        scale = attack.radius * 2f
                    }, true);
                }
            }
        }
    }

    public class TempestBallController : MonoBehaviour
    {
        public float ticksPerSecond = 10;
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

            attack = new()
            {
                radius = sphere.radius,
                attacker = controller.owner,
                baseDamage = damage.damage / ticksPerSecond,
                crit = damage.crit,
                damageType = damage.damageType,
                procCoefficient = 1f,
                teamIndex = controller.teamFilter.teamIndex,
                losType = BlastAttack.LoSType.None,
                falloffModel = BlastAttack.FalloffModel.None
            };
        }

        public void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                stopwatch += Time.fixedDeltaTime;

                if (stopwatch >= delay)
                {
                    stopwatch = 0f;

                    attack.position = base.transform.position;
                    attack.Fire();
                }
            }

            lr.SetPosition(0, base.transform.position);
            lr.SetPosition(1, body.corePosition);
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