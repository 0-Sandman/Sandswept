using static Sandswept.Utils.Components.MaterialControllerComponents;

namespace Sandswept.Items.Greens
{
    public class SunFragment : ItemBase<SunFragment>
    {
        public static DamageColorIndex SolarFlareColour = DamageColourHelper.RegisterDamageColor(new Color32(255, 150, 25, 255));

        public static DamageAPI.ModdedDamageType SunFragmentDamageType;

        public override string ItemName => "Sun Fragment";

        public override string ItemLangTokenName => "SUN_FRAGMENT";

        public override string ItemPickupDesc => "Create a blinding flash on hit that stuns and ignites enemies";

        public override string ItemFullDescription => "$su7%$se chance on hit to create a $sublinding flash$se in a $su12m$se area, $sustunning$se for $su2s$se and $signiting$se enemies for $sd150%$se $ss(+150% per stack)$se TOTAL damage.".AutoFormat();

        public override string ItemLore => "Maybe less hell to code";

        public override string AchievementName => "A cycle, broken.";

        public override string AchievementDesc => "Destroy a child of the stars";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("SunFragmentPrefab.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texSunFragment.png");

        public GameObject FragmentVFX;

        public static GameObject FragmentVFXSphere;
        public static ProcType sunFragmentDoTProcType = (ProcType)1298571298;
        public static ProcType sunFragmentAreaProcType = (ProcType)1298571264;

        public override void Init(ConfigFile config)
        {
            SunFragmentDamageType = DamageAPI.ReserveDamageType();

            FragmentVFX = Main.MainAssets.LoadAsset<GameObject>("FragmentFXRing.prefab");
            var component = FragmentVFX.AddComponent<EffectComponent>();
            component.applyScale = true;
            Main.EffectPrefabs.Add(FragmentVFX);

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
            if (!damageInfo.procChainMask.HasProc(sunFragmentDoTProcType) && attacker && DamageAPI.HasModdedDamageType(damageInfo, SunFragmentDamageType))
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
                            var totalDamage = 1f * stack;
                            var dot = new InflictDotInfo()
                            {
                                attackerObject = damageInfo.attacker,
                                victimObject = self.gameObject,
                                totalDamage = damageInfo.damage * totalDamage,
                                damageMultiplier = 3f,
                                dotIndex = DotController.DotIndex.Burn,
                                maxStacksFromAttacker = null
                            };

                            StrengthenBurnUtils.CheckDotForUpgrade(inventory, ref dot);
                            DotController.InflictDot(ref dot);

                            damageInfo.procChainMask.AddProc(sunFragmentDoTProcType);
                        }
                    }
                }
            }
            orig(self, damageInfo);
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;

            if (damageInfo.procChainMask.HasProc(sunFragmentAreaProcType))
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
                if (Util.CheckRoll(7f * damageInfo.procCoefficient, attackerBody.master))
                {
                    EffectData effectData = new()
                    {
                        origin = victimBody.corePosition,
                        rotation = Util.QuaternionSafeLookRotation(damageInfo.force != Vector3.zero ? damageInfo.force : Random.onUnitSphere),
                        scale = 12f
                    };
                    EffectData effectData2 = new()
                    {
                        origin = victimBody.corePosition,
                        scale = 12f
                    };
                    EffectManager.SpawnEffect(FragmentVFX, effectData, true);
                    EffectManager.SpawnEffect(FragmentVFXSphere, effectData2, true);

                    var setStateOnHurt = victimBody.GetComponent<SetStateOnHurt>();
                    setStateOnHurt?.SetStun(2f);

                    var damage = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, 1.5f * stack);

                    BlastAttack blastAttack = new()
                    {
                        radius = 12f,
                        baseDamage = damage,
                        procCoefficient = 0.33f,
                        crit = damageInfo.crit,
                        damageColorIndex = SolarFlareColour,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        falloffModel = BlastAttack.FalloffModel.None,
                        attacker = attackerBody.gameObject,
                        teamIndex = attackerBody.teamComponent.teamIndex,
                        position = damageInfo.position
                    };

                    blastAttack.AddModdedDamageType(SunFragmentDamageType);
                    blastAttack.Fire();

                    damageInfo.procChainMask.AddProc(sunFragmentAreaProcType);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}