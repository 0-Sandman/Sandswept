using System;
using System.Collections;
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

        public static GameObject staticSnareImpactVFX;

        public override void LoadAssets()
        {
            base.LoadAssets();

            Body = Main.Assets.LoadAsset<GameObject>("ElectricianBody.prefab");

            Body.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpStandard;
            var networkIdentity = Body.GetComponent<NetworkIdentity>();
            networkIdentity.localPlayerAuthority = true;
            networkIdentity.enabled = true;
            networkIdentity.serverOnly = false;

            var cb = Body.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Paths.GameObject.StandardCrosshair;
            cb.preferredPodPrefab = Paths.GameObject.RoboCratePod;

            SurvivorDef = Main.Assets.LoadAsset<SurvivorDef>("sdElectrician.asset");
            SurvivorDef.cachedName = "Electrician"; // for eclipse fix
            var kcm = Body.GetComponent<KinematicCharacterController.KinematicCharacterMotor>();
            kcm.playerCharacter = true;

            PrefabAPI.RegisterNetworkPrefab(Body);
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

            // this gets instantiatecloned to break its prefab status so i can parent stuff to it over in CreateVFX
            TempestSphere = PrefabAPI.InstantiateClone(Main.Assets.LoadAsset<GameObject>("TempestSphereProjectile.prefab"), "TempestSphereProjectile");
            ContentAddition.AddProjectile(TempestSphere);

            StaticSnare = Main.Assets.LoadAsset<GameObject>("TripwireMineProjectile.prefab");
            ContentAddition.AddProjectile(StaticSnare);

            Main.Instance.StartCoroutine(CreateVFX());

            On.RoR2.HealthComponent.TakeDamage += HandleGroundingShock;
        }

        public IEnumerator CreateVFX()
        {
            GameObject sphereVFX = new("joe sigma");
            sphereVFX.transform.position = Vector3.zero;
            sphereVFX.transform.localPosition = Vector3.zero;

            GameObject tempestSphereIndicator = PrefabAPI.InstantiateClone(Paths.GameObject.VagrantNovaAreaIndicator, "TempestSphereIndicator", false);
            tempestSphereIndicator.GetComponentInChildren<ParticleSystemRenderer>().gameObject.SetActive(false);
            tempestSphereIndicator.GetComponent<MeshRenderer>().sharedMaterials = new Material[] {
                Paths.Material.matWarbannerSphereIndicator,
                Paths.Material.matLightningSphere
            };
            tempestSphereIndicator.RemoveComponent<ObjectScaleCurve>();
            yield return new WaitForSeconds(0.1f);
            tempestSphereIndicator.transform.localScale = new(14f, 14f, 14f);
            tempestSphereIndicator.RemoveComponent<AnimateShaderAlpha>();

            GameObject tempestOrb = PrefabAPI.InstantiateClone(Paths.GameObject.VoidSurvivorChargeMegaBlaster, "TempestOrb", false);
            tempestOrb.transform.Find("Base").gameObject.SetActive(false);
            tempestOrb.transform.Find("Base (1)").gameObject.SetActive(false);
            tempestOrb.transform.Find("Sparks, In").GetComponent<ParticleSystemRenderer>().sharedMaterial = Paths.Material.matLoaderCharging;
            tempestOrb.transform.Find("Sparks, Misc").GetComponent<ParticleSystemRenderer>().sharedMaterial = Paths.Material.matIceOrbCore;
            tempestOrb.transform.Find("OrbCore").GetComponent<MeshRenderer>().sharedMaterials = new Material[] { Paths.Material.matLoaderLightningTile, Paths.Material.matJellyfishLightningSphere };
            tempestOrb.transform.RemoveComponent<ObjectScaleCurve>();
            yield return new WaitForSeconds(0.1f);
            tempestOrb.transform.localScale = new(3f, 3f, 3f);

            tempestSphereIndicator.transform.parent = sphereVFX.transform;
            tempestOrb.transform.parent = sphereVFX.transform;
            sphereVFX.transform.SetParent(TempestSphere.transform);
            tempestSphereIndicator.transform.position = Vector3.zero;
            tempestSphereIndicator.transform.localPosition = Vector3.zero;
            tempestOrb.transform.position = Vector3.zero;
            tempestOrb.transform.localPosition = Vector3.zero;
            TempestSphere.GetComponentInChildren<LineRenderer>().sharedMaterial = Paths.Material.matLightningLongYellow;

            staticSnareImpactVFX = PrefabAPI.InstantiateClone(Paths.GameObject.LoaderGroundSlam, "Sigma Gyatt Rizz Ohio Fa-", false);
            foreach (ShakeEmitter ughShakesButt in staticSnareImpactVFX.GetComponents<ShakeEmitter>())
            {
                ughShakesButt.enabled = false;
            }
            var effectComponent = staticSnareImpactVFX.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_loader_m1_impact";
            ContentAddition.AddEffect(staticSnareImpactVFX);
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

            effect = Electrician.staticSnareImpactVFX;
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
            if (!hasBouncedEnemy && NetworkServer.active)
            {
                if (collision.collider)
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
                    EffectManager.SpawnEffect(Electrician.staticSnareImpactVFX, new EffectData
                    {
                        origin = attack.position,
                        scale = attack.radius * 2f
                    }, true);

                    var rb = GetComponent<Rigidbody>();
                    rb.useGravity = true;
                    rb.velocity = Vector3.zero;
                    // rb.velocity += Physics.gravity;
                }
            }
        }
    }

    public class TempestBallController : MonoBehaviour
    {
        public float ticksPerSecond = 6;
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