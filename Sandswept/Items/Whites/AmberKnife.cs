using RoR2.EntityLogic;
using UnityEngine.Events;
using static Sandswept.Utils.Components.MaterialControllerComponents;

namespace Sandswept.Items.Whites
{
    public class AmberKnife : ItemBase<AmberKnife>
    {
        public override string ItemName => "Amber Knife";

        public override string ItemLangTokenName => "AMBER_KNIFE";

        public override string ItemPickupDesc => "'Critical Strikes' give temporary barrier.";

        public override string ItemFullDescription => StringExtensions.AutoFormat("Gain a $sd10%$se chance on hit to fire a $sdknife$se for $sd120%$se $ss(+120% per stack)$se base damage that $sdpierces$se, gain $sh10$se plus an additional $sh2% barrier$se for every pierce with the knife.");

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("AmberKnifePrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("AmberKnifeIcon.png");

        public static ProcType amberKnife = (ProcType)12785281;

        public static GameObject amberKnifeProjectile;
        public static GameObject amberKnifeGhost;

        public UnityEvent UnityGames = new();
        public static ProjectileOverlapAttack projectileOverlapAttack;

        // why tf does it bounce so oddly
        // also the unity event doesnt work bruhhhh
        public override void Init(ConfigFile config)
        {
            amberKnifeGhost = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ShivGhostAlt.prefab").WaitForCompletion(), "Amber Knife Ghost", false);

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

            var sphereCollider = amberKnifeProjectile.GetComponent<SphereCollider>();
            sphereCollider.material = Addressables.LoadAssetAsync<PhysicMaterial>("RoR2/Base/Common/physmatDefault.physicMaterial").WaitForCompletion();

            var projectileDamage = amberKnifeProjectile.GetComponent<ProjectileDamage>();
            projectileDamage.damageType = DamageType.Generic;

            amberKnifeProjectile.RemoveComponent<ProjectileSingleTargetImpact>();
            // amberKnifeProjectile.RemoveComponent<ProjectileStickOnImpact>();

            var projectileStickOnImpact = amberKnifeProjectile.GetComponent<ProjectileStickOnImpact>();
            projectileStickOnImpact.ignoreCharacters = true;

            // amberKnifeProjectile.RemoveComponent<DelayedEvent>();
            // amberKnifeProjectile.RemoveComponent<EventFunctions>();

            var hitBox = amberKnifeProjectile.AddComponent<HitBox>();

            var hitBoxGroup = amberKnifeProjectile.AddComponent<HitBoxGroup>();
            hitBoxGroup.hitBoxes = new HitBox[] { hitBox };

            projectileOverlapAttack = amberKnifeProjectile.AddComponent<ProjectileOverlapAttack>();
            projectileOverlapAttack.damageCoefficient = 1f;
            projectileOverlapAttack.impactEffect = null; // change this probably
            projectileOverlapAttack.forceVector = Vector3.zero;
            UnityGames.AddListener(OnServerHit);

            // amberKnifeProjectile.transform.localScale = new Vector3(2f, 2f, 2f);

            var projectileController = amberKnifeProjectile.GetComponent<ProjectileController>();

            projectileController.ghostPrefab = amberKnifeGhost;

            PrefabAPI.RegisterNetworkPrefab(amberKnifeProjectile);

            var component = ItemModel.transform.Find("AmberKnife").Find("Knife").gameObject;
            var renderer = component.GetComponent<MeshRenderer>();
            var controller = component.AddComponent<HGStandardController>();
            controller.Renderer = renderer;
            controller.Material = renderer.materials[0];
            var material = controller.Material;
            material.SetTexture("_EmTex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Grandparent/texGrandparentDetailGDiffuse.png").WaitForCompletion());
            material.SetFloat("_EmPower", 10f);
            material.SetColor("_EmColor", new Color32(50, 0, 0, 255));
            material.SetTexture("_FresnelRamp", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampTwotone.jpg").WaitForCompletion());
            material.SetFloat("_FresnelPower", 13f);
            material.SetFloat("_FresnelBoost", 2.5f);
            material.EnableKeyword("FRESNEL_EMISSION");
            renderer.material = material;
            CreateLang();
            CreateItem();
            Hooks();
        }

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

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
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
            var knifeDamage = 1.2f * stack;
            if (stack > 0)
            {
                if (!report.damageInfo.procChainMask.HasProc(amberKnife) && Util.CheckRoll(10f, master))
                {
                    var fpi = new FireProjectileInfo()
                    {
                        damage = attackerBody.damage * knifeDamage,
                        crit = attackerBody.RollCrit(),
                        position = attackerBody.inputBank.GetAimRay().origin,
                        rotation = Util.QuaternionSafeLookRotation(attackerBody.inputBank.GetAimRay().direction),
                        force = 500f,
                        owner = attackerBody.gameObject,
                        procChainMask = default,
                        projectilePrefab = amberKnifeProjectile
                    };
                    report.damageInfo.procChainMask.AddProc(amberKnife);
                    ProjectileManager.instance.FireProjectile(fpi);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}