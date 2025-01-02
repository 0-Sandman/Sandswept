using RoR2.EntityLogic;
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

        public override string ItemFullDescription => ("Gain a $sd" + chance + "%$se chance on hit to fire a $sdknife$se for $sd" + d(baseDamage) + "$se $ss(+" + d(stackDamage) + " per stack)$se base damage that $sdpierces$s. Gain $sh" + d(percentBarrierGain) + " barrier$se for every pierce with the knife.").AutoFormat();

        public override string ItemLore => "<style=cMono>Order: Amber Knife\r\nTracking Number: 534*****\r\nEstimated Delivery: 06/15/2056\r\nShipping Method: High Priority\r\nShipping Address: Outer Ring Lab, Venus\r\nShipping Details:\r\n\r\n</style>This is an ancient ritual artifact, once used by Neptunian priests in sacrifices, said to protect them from attack and assassination. This was not without credence, it seems, as the knife operates similarly to the ultra-phasic shield technology used in the assassination of Mars's ambassador two months back.\r\n\r\nOf course, we can't use such an old and fragile weapon in our own operations, but its effect seems more potent than what the assassin used. Along with serving your planet, you'll receive generous funding to discover how it works and incorporate it into something more usable.\r\n";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("AmberKnifeHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texAmberKnife.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Utility, ItemTag.Healing };

        public override bool nonstandardScaleModel => false;
        // wharever im pupy
        // I tried most of the combinations of blender export scale, unity scale
        // convert units and prefab child transform scale
        // and it all looked like farlands with nonstandardscalemodel as false,
        // and really far away with the standard scale

        public static ModdedProcType amberKnife = ProcTypeAPI.ReserveProcType();

        public static GameObject amberKnifeProjectile;
        public static GameObject amberKnifeGhost;

        public static ProjectileOverlapAttack projectileOverlapAttack;

        [ConfigField("Chance", "", 10f)]
        public static float chance;

        [ConfigField("Base Damage", "Decimal.", 1.5f)]
        public static float baseDamage;

        [ConfigField("Stack Damage", "Decimal.", 1.5f)]
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
        public override void Init(ConfigFile config)
        {
            amberKnifeGhost = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ShivGhostAlt.prefab").WaitForCompletion(), "Amber Knife Ghost", false);

            amberKnifeGhost.transform.localScale = new Vector3(2f, 2f, 2f);

            var mesh = amberKnifeGhost.transform.GetChild(0);

            var mf = mesh.GetComponent<MeshFilter>(); // couldnt resist naming it mf
            mf.mesh = Main.hifuSandswept.LoadAsset<Mesh>("AmberKnifeMesh.fbx");

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

            var swingTrail = amberKnifeProjectile.transform.GetChild(0).GetChild(1).GetComponent<ParticleSystemRenderer>();

            var newMat = Object.Instantiate(Paths.Material.matBandit2SlashBlade);
            newMat.SetColor("_TintColor", new Color32(255, 180, 40, 255));
            newMat.SetFloat("_Boost", 6.25f);
            newMat.SetFloat("_AlphaBoost", 3.766f);

            swingTrail.material = newMat;

            // amberKnifeProjectile.AddComponent<AmberKnifeProjectile>();

            PrefabAPI.RegisterNetworkPrefab(amberKnifeProjectile);

            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
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
                if (Util.CheckRoll(chance * report.damageInfo.procCoefficient, master))
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

                    Util.PlaySound("Play_bandit2_m2_slash", attackerBody.gameObject);

                    fpi.procChainMask.AddModdedProc(amberKnife);

                    // fpi.projectilePrefab.GetComponent<AmberKnifeProjectile>().owner = attackerBody;
                    ProjectileManager.instance.FireProjectile(fpi);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
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
                EffectManager.SimpleImpactEffect(Paths.GameObject.MagmaWormImpactExplosion, base.transform.position, -transform.forward, transmit: true);
                if (owner != null)
                {
                    owner.healthComponent.AddBarrier(owner.healthComponent.fullHealth * percentBarrierGain);
                }
            }

            public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
            {
                Destroy(base.gameObject);
            }
        }
    }
}