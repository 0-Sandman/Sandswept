using RoR2.EntityLogic;
using UnityEngine.Events;
using static Sandswept.Utils.Components.MaterialControllerComponents;

// ss2 ahh code
namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Amber Knife")]
    public class AmberKnife : ItemBase<AmberKnife>
    {
        public override string ItemName => "Amber Knife";

        public override string ItemLangTokenName => "AMBER_KNIFE";

        public override string ItemPickupDesc => "Chance to fire a piercing knife that grants barrier on hit.";

        public override string ItemFullDescription => ("Gain a $sd" + chance + "%$se chance on hit to fire a $sdknife$se for $sd" + d(baseDamage) + "$se $ss(+" + d(stackDamage) + " per stack)$se base damage that $sdpierces$se, gain $sh" + d(percentBarrierGain) + " barrier$se for every pierce with the knife.").AutoFormat();

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("Assets/Sandswept/AmberKnifeHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texAmberKnife.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Utility, ItemTag.Healing };

        public static ProcType amberKnife = (ProcType)12785281;

        public static GameObject amberKnifeProjectile;
        public static GameObject amberKnifeGhost;

        public UnityEvent UnityGames = new();
        public static ProjectileOverlapAttack projectileOverlapAttack;

        [ConfigField("Chance", "", 10f)]
        public static float chance;

        [ConfigField("Base Damage", "Decimal.", 1.2f)]
        public static float baseDamage;

        [ConfigField("Stack Damage", "Decimal.", 1.2f)]
        public static float stackDamage;

        [ConfigField("Proc Coefficient", "", 1f)]
        public static float procCoefficient;

        [ConfigField("Percent Barrier Gain", "Decimal.", 0.04f)]
        public static float percentBarrierGain;

        // why tf does it bounce so oddly
        // also the unity event doesnt work bruhhhh
        public override void Init(ConfigFile config)
        {
            amberKnifeGhost = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ShivGhostAlt.prefab").WaitForCompletion(), "Amber Knife Ghost", false);
            AmberKnifeProjectile.impactSpark = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniImpactVFXLarge.prefab").WaitForCompletion();

            amberKnifeGhost.transform.localScale = new Vector3(2f, 2f, 2f);
            /*
            var mesh = amberKnifeProjectile.transform.GetChild(0);

            var mf = mesh.GetComponent<MeshFilter>(); // couldnt resist naming it mf
            mf.mesh = Addressables.LoadAssetAsync<Mesh>("").WaitForCompletion();

            var meshRenderer = mesh.GetComponent<MeshRenderer>();
            meshRenderer.material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Bandit2/matBandit2Knife.mat").WaitForCompletion();
            */
            amberKnifeProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ShivProjectile.prefab").WaitForCompletion(), "Amber Knife Projectile", true);

            var rigidBody = amberKnifeProjectile.GetComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.freezeRotation = true;

            var sphereCollider = amberKnifeProjectile.GetComponent<SphereCollider>();
            sphereCollider.material = Addressables.LoadAssetAsync<PhysicMaterial>("RoR2/Base/Common/physmatDefault.physicMaterial").WaitForCompletion();

            var projectileDamage = amberKnifeProjectile.GetComponent<ProjectileDamage>();
            projectileDamage.damageType = DamageType.Generic;

            amberKnifeProjectile.RemoveComponent<ProjectileSingleTargetImpact>();
            amberKnifeProjectile.RemoveComponent<ProjectileStickOnImpact>();

            amberKnifeProjectile.RemoveComponent<DelayedEvent>();
            amberKnifeProjectile.RemoveComponent<EventFunctions>();

            var hitBox = amberKnifeProjectile.AddComponent<HitBox>();

            var hitBoxGroup = amberKnifeProjectile.AddComponent<HitBoxGroup>();
            hitBoxGroup.hitBoxes = new HitBox[] { hitBox };

            projectileOverlapAttack = amberKnifeProjectile.AddComponent<ProjectileOverlapAttack>();
            projectileOverlapAttack.damageCoefficient = 1f;
            projectileOverlapAttack.impactEffect = null; // change this probably
            projectileOverlapAttack.forceVector = Vector3.zero;
            projectileOverlapAttack.overlapProcCoefficient = procCoefficient;
            projectileOverlapAttack.resetInterval = -1f;

            // amberKnifeProjectile.transform.localScale = new Vector3(2f, 2f, 2f);

            var projectileController = amberKnifeProjectile.GetComponent<ProjectileController>();
            projectileController.procCoefficient = procCoefficient;
            projectileController.ghostPrefab = amberKnifeGhost;

            amberKnifeProjectile.AddComponent<AmberKnifeProjectile>();

            PrefabAPI.RegisterNetworkPrefab(amberKnifeProjectile);

            CreateLang();
            CreateItem();
            Hooks();
        }

        /*
        public void OnServerHit()
        {
            // what the fuck it doesn't work I love unity
            Main.ModLogger.LogError("projectile overlap attack is " + projectileOverlapAttack);
            Main.ModLogger.LogError("unity games is " + UnityGames);
            if (projectileOverlapAttack && UnityGames != null)
            {
                var owner = projectileOverlapAttack.projectileController.owner;
                if (owner)
                {
                    Main.ModLogger.LogError("owner exists, " + owner);
                    var ownerHc = owner.GetComponent<HealthComponent>();
                    if (ownerHc)
                    {
                        Main.ModLogger.LogError("owner hc exists");
                        ownerHc.AddBarrier(10f);
                        ownerHc.AddBarrier(ownerHc.fullCombinedHealth * 0.02f);
                    }
                }
            }
        }
        */

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            if (report.damageInfo.procChainMask.HasProc(amberKnife))
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
                        procChainMask = new(),
                        projectilePrefab = amberKnifeProjectile,
                    };

                    Util.PlaySound("Play_bandit2_m2_slash", attackerBody.gameObject);

                    fpi.procChainMask.AddProc(amberKnife);

                    fpi.projectilePrefab.GetComponent<AmberKnifeProjectile>().owner = attackerBody;
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

            public void FixedUpdate()
            {
                if (!NetworkServer.active) return;
                GetComponent<Rigidbody>().velocity = transform.forward.normalized * 60f;
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch > 10) Destroy(gameObject);
            }

            public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
            {
                if (!impactInfo.collider.GetComponent<HurtBox>())
                {
                    Destroy(gameObject);
                    return;
                }
                Physics.IgnoreCollision(GetComponent<Collider>(), impactInfo.collider);
                EffectManager.SimpleImpactEffect(impactSpark, impactInfo.estimatedPointOfImpact, -transform.forward, transmit: true);
                if (owner != null)
                {
                    owner.healthComponent.AddBarrier(owner.healthComponent.fullHealth * percentBarrierGain);
                }
            }
        }
    }
}