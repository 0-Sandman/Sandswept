using LookingGlass.ItemStatsNameSpace;
using Rewired.ComponentControls.Effects;
using RoR2.EntityLogic;
using System.Runtime.CompilerServices;
using UnityEngine.Events;
using UnityEngine.Networking.NetworkSystem;
using static Sandswept.Utils.Components.MaterialControllerComponents;
using static UnityEngine.UI.GridLayoutGroup;

// ss2 ahh code
namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Amber Knife")]
    public class AmberKnife : ItemBase<AmberKnife>
    {
        public override string ItemName => "Amber Knife";

        public override string ItemLangTokenName => "AMBER_KNIFE";

        public override string ItemPickupDesc => "Chance to fire a piercing knife that grants barrier on hit.";

        public override string ItemFullDescription => $"Gain a $sd{chance}%$se chance on hit to fire a $sdpiercing knife$se for $sd{baseDamage * 100f}%$se $ss(+{stackDamage * 100f}% per stack)$se base damage that gives $sh{percentBarrierGain * 100f}%$se plus an additional $sh{flatBarrierGain} barrier$se.".AutoFormat();

        public override string ItemLore =>
        """
        Order: Amber Knife
        Tracking Number: 534*****
        Estimated Delivery: 06/15/2056
        Shipping Method: High Priority
        Shipping Address: Outer Ring Lab, Venus
        Shipping Details:

        This is an ancient ritual artifact, once used by Neptunian priests in sacrifices, said to protect them from attack and assassination. This was not without credence, it seems, as the knife operates similarly to the ultra-phasic shield technology used in the assassination of Mars's ambassador two months back.

        Of course, we can't use such an old and fragile weapon in our own operations, but its effect seems more potent than what the assassin used. Along with serving your planet, you'll receive generous funding to discover how it works and incorporate it into something more usable.
        """;
        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("AmberKnifeHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texAmberKnife.png");

        public override ItemTag[] ItemTags => [ItemTag.Damage, ItemTag.Utility, ItemTag.Healing, ItemTag.CanBeTemporary];

        public static ModdedProcType amberKnife = ProcTypeAPI.ReserveProcType();

        public static GameObject amberKnifeProjectile;
        public static GameObject amberKnifeGhost;

        public static ProjectileOverlapAttack projectileOverlapAttack;

        public static GameObject impactVFX;

        public override float modelPanelParametersMinDistance => 4f;
        public override float modelPanelParametersMaxDistance => 12f;

        [ConfigField("Chance", "", 12f)]
        public static float chance;

        [ConfigField("Base Damage", "Decimal.", 1.8f)]
        public static float baseDamage;

        [ConfigField("Stack Damage", "Decimal.", 1.8f)]
        public static float stackDamage;

        [ConfigField("Proc Coefficient", "", 1f)]
        public static float procCoefficient;

        [ConfigField("Flat Barrier Gain", "Decimal.", 3f)]
        public static float flatBarrierGain;

        [ConfigField("Percent Barrier Gain", "Decimal.", 0.035f)]
        public static float percentBarrierGain;

        public static ProjectileController projectileController;

        public static UnityEvent UnityGames = new();

        // why tf does it bounce so oddly
        // also the unity event doesnt work bruhhhh
        public override void Init()
        {
            base.Init();
            SetUpProjectile();
        }

        public void SetUpProjectile()
        {
            impactVFX = PrefabAPI.InstantiateClone(Paths.GameObject.MercSwordFinisherSlash, "Amber Knife Impact VFX", false);

            var effectComponent = impactVFX.AddComponent<EffectComponent>();
            effectComponent.soundName = "Play_bandit2_m2_impact";

            var scaleParticleSystemDuration = impactVFX.GetComponent<ScaleParticleSystemDuration>();
            scaleParticleSystemDuration.newDuration = 0.2f;
            scaleParticleSystemDuration.initialDuration = 0.2f;

            impactVFX.GetComponent<DestroyOnTimer>().duration = 0.1f;

            impactVFX.transform.Find("Sparks").gameObject.SetActive(false);

            var swingTrail = impactVFX.transform.Find("SwingTrail").GetComponent<ParticleSystemRenderer>();
            swingTrail.transform.localScale = Vector3.one * 0.5f;

            var newSwingTrailMaterial = new Material(Paths.Material.matMercSwipe2);
            newSwingTrailMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampRailgun);
            newSwingTrailMaterial.SetColor("_TintColor", new Color32(211, 124, 0, 255));
            newSwingTrailMaterial.SetFloat("_Boost", 7f);

            swingTrail.material = newSwingTrailMaterial;

            impactVFX.transform.Find("SwingDistortion?").localScale = Vector3.one * 0.5f;

            ContentAddition.AddEffect(impactVFX);

            amberKnifeGhost = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ShivGhostAlt.prefab").WaitForCompletion(), "Amber Knife Ghost", false);

            amberKnifeGhost.transform.localScale = new Vector3(2f, 2f, 2f);

            var mesh = amberKnifeGhost.transform.GetChild(0);
            mesh.localEulerAngles = Vector3.zero;
            mesh.localScale *= 0.33f;

            var rot = mesh.AddComponent<RotateAroundAxis>();
            rot.enabled = true;
            rot.speed = RotateAroundAxis.Speed.Fast;
            rot.fastRotationSpeed = 800f;
            rot.rotateAroundAxis = RotateAroundAxis.RotationAxis.Y;

            var rot2 = mesh.AddComponent<RotateAroundAxis>();
            rot2.enabled = true;
            rot2.speed = RotateAroundAxis.Speed.Fast;
            rot2.fastRotationSpeed = 1200f;
            rot2.rotateAroundAxis = RotateAroundAxis.RotationAxis.X;

            var rot3 = mesh.AddComponent<RotateAroundAxis>();
            rot3.enabled = true;
            rot3.speed = RotateAroundAxis.Speed.Fast;
            rot3.fastRotationSpeed = 1200f;
            rot3.rotateAroundAxis = RotateAroundAxis.RotationAxis.Z;

            var randomRot2 = mesh.AddComponent<SetRandomRotation>();
            randomRot2.setRandomXRotation = true;
            randomRot2.setRandomYRotation = true;
            randomRot2.setRandomZRotation = true;

            var mf = mesh.GetComponent<MeshFilter>(); // couldnt resist naming it mf
            mf.mesh = Main.hifuSandswept.LoadAsset<Mesh>("AmberKnife.fbx");

            var meshRenderer = mesh.GetComponent<MeshRenderer>();
            meshRenderer.material = Main.hifuSandswept.LoadAsset<Material>("matAmberKnife.mat");

            amberKnifeProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ShivProjectile.prefab").WaitForCompletion(), "Amber Knife Projectile", true);

            var rigidBody = amberKnifeProjectile.GetComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.freezeRotation = true;

            var amb = amberKnifeProjectile.AddComponent<AmberKnifeProjectile>();

            var sphereCollider = amberKnifeProjectile.GetComponent<SphereCollider>();
            sphereCollider.material = Addressables.LoadAssetAsync<PhysicMaterial>("RoR2/Base/Common/physmatDefault.physicMaterial").WaitForCompletion();

            var projectileDamage = amberKnifeProjectile.GetComponent<ProjectileDamage>();
            projectileDamage.damageType = DamageType.Generic;

            amberKnifeProjectile.RemoveComponent<ProjectileSingleTargetImpact>();
            amberKnifeProjectile.RemoveComponent<ProjectileStickOnImpact>();

            amberKnifeProjectile.RemoveComponent<DelayedEvent>();
            amberKnifeProjectile.RemoveComponent<EventFunctions>();

            projectileController = amberKnifeProjectile.GetComponent<ProjectileController>();
            projectileController.procCoefficient = procCoefficient;
            projectileController.ghostPrefab = amberKnifeGhost;

            var projectileSimple = amberKnifeProjectile.GetComponent<ProjectileSimple>();
            projectileSimple.enableVelocityOverLifetime = true;
            projectileSimple.desiredForwardSpeed = 130f;
            projectileSimple.velocityOverLifetime = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.005f, 1f), new Keyframe(1f, 1.5f));

            var hitBox = amberKnifeProjectile.AddComponent<HitBox>();

            var hitBoxGroup = amberKnifeProjectile.AddComponent<HitBoxGroup>();
            hitBoxGroup.hitBoxes = new HitBox[] { hitBox };

            projectileOverlapAttack = amberKnifeProjectile.AddComponent<ProjectileOverlapAttack>();
            projectileOverlapAttack.damageCoefficient = 1f;
            projectileOverlapAttack.impactEffect = Paths.GameObject.OmniImpactVFXSlash;
            projectileOverlapAttack.forceVector = Vector3.zero;
            projectileOverlapAttack.overlapProcCoefficient = procCoefficient;
            projectileOverlapAttack.resetInterval = -1f;
            //projectileOverlapAttack.onServerHit = UnityGames;
            //projectileOverlapAttack.onServerHit.AddListener(amb.AddBarrier);

            amberKnifeProjectile.layer = LayerIndex.ragdoll.intVal;

            // amberKnifeProjectile.transform.localScale = new Vector3(2f, 2f, 2f);

            var swingTrailProjectile = amberKnifeProjectile.transform.GetChild(0).GetChild(1).GetComponent<ParticleSystemRenderer>();

            var newMat = new Material(Paths.Material.matBandit2SlashBlade);
            newMat.SetColor("_TintColor", new Color32(255, 180, 40, 255));
            newMat.SetFloat("_Boost", 6.25f);
            newMat.SetFloat("_AlphaBoost", 3.766f);

            swingTrailProjectile.material = newMat;

            // amberKnifeProjectile.AddComponent<AmberKnifeProjectile>();

            PrefabAPI.RegisterNetworkPrefab(amberKnifeProjectile);
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Fire Chance: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.descriptions.Add("Base Damage: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.hasChance = true;
            itemStatsDef.chanceScaling = ItemStatsDef.ChanceScaling.DoesNotScale;
            itemStatsDef.calculateValuesNew = (luck, stack, procChance) =>
            {
                List<float> values = new()
                {
                    LookingGlass.Utils.CalculateChanceWithLuck(chance * procChance * 0.01f, luck),
                    baseDamage + stackDamage * (stack - 1)
                };

                return values;
            };

            return itemStatsDef;
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            if (report.damageInfo.procChainMask.HasModdedProc(amberKnife))
            {
                return;
            }

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var master = attackerBody.master;
            if (!master)
            {
                return;
            }

            var stack = GetCount(attackerBody);
            var knifeDamage = baseDamage + stackDamage * (stack - 1);
            if (stack > 0)
            {
                if (Util.CheckRoll(chance * Main.GetProcRateForBaseDamageProc(report.damageInfo) * report.damageInfo.procCoefficient, master))
                {
                    var fpi = new FireProjectileInfo()
                    {
                        damage = attackerBody.damage * knifeDamage,
                        crit = attackerBody.RollCrit(),
                        position = attackerBody.inputBank.GetAimRay().origin,
                        rotation = Util.QuaternionSafeLookRotation(attackerBody.inputBank.GetAimRay().direction),
                        force = 0f,
                        owner = attackerBody.gameObject,
                        procChainMask = default,
                        projectilePrefab = amberKnifeProjectile,
                    };

                    Util.PlayAttackSpeedSound("Play_bandit2_m2_slash", attackerBody.gameObject, 1.5f);

                    fpi.procChainMask.AddModdedProc(amberKnife);

                    // fpi.projectilePrefab.GetComponent<AmberKnifeProjectile>().owner = attackerBody;
                    ProjectileManager.instance.FireProjectile(fpi);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {

            var itemDisplay = SetUpIDRS();

            ItemDisplayRuleDict i = new();

            #region Sandswept Survivors
            /*
            i.Add("RangerBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00387F, 0.11857F, 0.01629F),
                    localAngles = new Vector3(84.61184F, 220.3867F, 47.41245F),
                    localScale = new Vector3(0.14531F, 0.14659F, 0.14531F),

                    followerPrefab = itemDisplay,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            i.Add("ElectricianBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.01041F, 0.08162F, -0.00924F),
                    localAngles = new Vector3(85.0407F, 197.8464F, 22.78797F),
                    localScale = new Vector3(0.12683F, 0.11843F, 0.11843F),

                    followerPrefab = itemDisplay,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );
            */

            #endregion

            return i;

        }

        public class AmberKnifeProjectile : NetworkBehaviour, IProjectileImpactBehavior
        {
            public static GameObject impactSpark;

            public CharacterBody owner;

            public float stopwatch = 0;

            public void Start()
            {
                GetComponent<ProjectileOverlapAttack>().onServerHit = new();
                GetComponent<ProjectileOverlapAttack>().onServerHit.AddListener(AddBarrier);
            }

            public void FixedUpdate()
            {
                if (!NetworkServer.active) return;
                GetComponent<Rigidbody>().velocity = transform.forward.normalized * 80f;
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch > 10) Destroy(gameObject);

                owner ??= GetComponent<ProjectileController>().owner.GetComponent<CharacterBody>();
            }

            public void AddBarrier()
            {
                var randomRotation = new Vector3(Run.instance.spawnRng.RangeFloat(0f, 360f), Run.instance.spawnRng.RangeFloat(0f, 360f), Run.instance.spawnRng.RangeFloat(0f, 360f));
                EffectManager.SimpleEffect(impactVFX, transform.position, Util.QuaternionSafeLookRotation(randomRotation), true);
                if (owner != null)
                {
                    owner.healthComponent.AddBarrier(flatBarrierGain + (owner.healthComponent.fullHealth * percentBarrierGain));
                }
            }

            public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
            {
                Destroy(base.gameObject);
            }
        }
    }
}