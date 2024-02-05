using System;
using static Sandswept.Utils.Components.MaterialControllerComponents;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Millenium")]
    public class Millenium : ItemBase<Millenium>
    {
        public override string ItemName => "Millenium";

        public override string ItemLangTokenName => "MILLENIUM";

        public override string ItemPickupDesc => "Create a tidal cataclysm on hit that grounds and collapses enemies. Corrupts all Sun Fragments.";

        public override string ItemFullDescription => ("$su" + chance + "%$se chance on hit to create a $sdtidal cataclysm$se in a $su" + baseExplosionRadius + "m$se (+" + stackExplosionRadius + "m per stack) radius, $sdcollapsing$se and $sugrounding$se enemies for $sd400%$se base damage.").AutoFormat();

        public override string ItemLore => "This voice.\r\nI hear ringing..\r\nIt asks.\r\nIt invades my mind.\r\nMy hearing, reversed..\r\nI'm falling\r\n\r\n[...]\r\n\r\nThis maze, reversed..\r\nCircled by a close fog\r\n\r\n[...]\r\n\r\nI've lost.";

        [ConfigField("Chance", "", 5f)]
        public static float chance;

        [ConfigField("Base Explosion Radius", "", 12f)]
        public static float baseExplosionRadius;

        [ConfigField("Stack Explosion Radius", "", 4f)]
        public static float stackExplosionRadius;

        [ConfigField("Explosion Proc Coefficient", "", 0.2f)]
        public static float explosionProcCoefficient;

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("SunFragmentPrefab.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texSunFragment.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };

        public GameObject milleniumVFX;

        public static GameObject FragmentVFXSphere;
        public static ProcType milleniumDoT = (ProcType)3298571298;
        public static ProcType milleniumAoE = (ProcType)3298571264;

        public static DamageColorIndex milleniumColor = DamageColourHelper.RegisterDamageColor(new Color32(75, 27, 174, 255));

        public static DamageAPI.ModdedDamageType milleniumDamageType;

        public override void Init(ConfigFile config)
        {
            milleniumDamageType = DamageAPI.ReserveDamageType();

            milleniumVFX = Main.MainAssets.LoadAsset<GameObject>("FragmentFXRing.prefab");
            var component = milleniumVFX.AddComponent<EffectComponent>();
            component.applyScale = true;
            Main.EffectPrefabs.Add(milleniumVFX);

            FragmentVFXSphere = Main.MainAssets.LoadAsset<GameObject>("FragmentFXSphere.prefab");
            var Renderer = FragmentVFXSphere.GetComponent<ParticleSystemRenderer>();
            var val = FragmentVFXSphere.AddComponent<HGIntersectionController>();
            val.Renderer = Renderer;
            var val3 = val.Renderer.material;
            Material val4 = Object.Instantiate(val3);
            val4.SetColor("_TintColor", new Color32(255, 120, 0, 255));
            val4.SetTexture("_Cloud1Tex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Grandparent/texGrandparentDetailGDiffuse.png").WaitForCompletion());
            val4.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampParentTeleport.png").WaitForCompletion());
            val4.SetFloat("_IntersectionStrength", 0.95f);
            Renderer.material = val4;
            var component2 = FragmentVFXSphere.AddComponent<EffectComponent>();
            component2.applyScale = true;
            Main.EffectPrefabs.Add(FragmentVFXSphere);

            CreateLang();
            CreateUnlockLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var attacker = damageInfo.attacker;
            if (!damageInfo.procChainMask.HasProc(milleniumDoT) && attacker && damageInfo.HasModdedDamageType(milleniumDamageType))
            {
                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    var inventory = attackerBody.inventory;
                    if (inventory)
                    {
                        var stack = inventory.GetItemCount(instance.ItemDef);
                        if (stack > 0)
                        {
                            DotController.DotDef dotDef = DotController.GetDotDef(DotController.DotIndex.Fracture);
                            DotController.InflictDot(self.gameObject, damageInfo.attacker, DotController.DotIndex.Fracture, dotDef.interval, 1f);

                            damageInfo.procChainMask.AddProc(milleniumDoT);
                        }
                    }
                }
            }
            orig(self, damageInfo);
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;

            if (damageInfo.procChainMask.HasProc(milleniumAoE))
            {
                return;
            }

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var victimBody = report.victimBody;
            if (!victimBody)
            {
                return;
            }

            var stack = GetCount(attackerBody);
            if (stack > 0)
            {
                if (Util.CheckRoll(chance * damageInfo.procCoefficient, attackerBody.master))
                {
                    EffectData effectData = new()
                    {
                        origin = victimBody.corePosition,
                        rotation = Util.QuaternionSafeLookRotation(damageInfo.force != Vector3.zero ? damageInfo.force : Random.onUnitSphere),
                        scale = baseExplosionRadius
                    };
                    EffectData effectData2 = new()
                    {
                        origin = victimBody.corePosition,
                        scale = baseExplosionRadius
                    };
                    EffectManager.SpawnEffect(milleniumVFX, effectData, true);
                    EffectManager.SpawnEffect(FragmentVFXSphere, effectData2, true);

                    // var damage = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, baseTotalDamage + stackTotalDamage * (stack - 1));

                    float mass;

                    if (victimBody.characterMotor) mass = victimBody.characterMotor.mass;
                    else if (victimBody.rigidbody) mass = victimBody.rigidbody.mass;
                    else mass = 1f;

                    BlastAttack blastAttack = new()
                    {
                        radius = baseExplosionRadius,
                        baseDamage = Mathf.Epsilon, // dont ask
                        procCoefficient = explosionProcCoefficient,
                        crit = damageInfo.crit,
                        damageColorIndex = milleniumColor,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        falloffModel = BlastAttack.FalloffModel.None,
                        attacker = attackerBody.gameObject,
                        teamIndex = attackerBody.teamComponent.teamIndex,
                        position = damageInfo.position,
                        damageType = DamageType.Silent | DamageType.BypassArmor | DamageType.BypassBlock, // I said dont ask
                        bonusForce = new Vector3(0f, -10f * mass, 0f)
                    };

                    blastAttack.AddModdedDamageType(milleniumDamageType);
                    blastAttack.Fire();

                    damageInfo.procChainMask.AddProc(milleniumAoE);

                    AkSoundEngine.PostEvent(Events.Play_voidRaid_snipe_impact, victimBody.gameObject);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}