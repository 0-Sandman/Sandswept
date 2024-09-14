using static Sandswept.Utils.Components.MaterialControllerComponents;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Sun Fragment")]
    public class SunFragment : ItemBase<SunFragment>
    {
        public static DamageColorIndex SolarFlareColour = DamageColourHelper.RegisterDamageColor(new Color32(255, 150, 25, 255));

        public static DamageAPI.ModdedDamageType SunFragmentDamageType;

        public override string ItemName => "Sun Fragment";

        public override string ItemLangTokenName => "SUN_FRAGMENT";

        public override string ItemPickupDesc => "Create a blinding flash on hit that stuns and ignites enemies.";

        public override string ItemFullDescription => ("$su" + chance + "%$se chance on hit to create a $sublinding flash$se in a $su" + explosionRadius + "m$se radius, $sustunning$se for $su" + stunDuration + "s$se and $sdigniting$se enemies for $sd" + d(baseTotalDamage) + "$se $ss(+" + d(stackTotalDamage) + " per stack)$se TOTAL damage.").AutoFormat();

        public override string ItemLore => "\"What the hell is that?\"\r\n\r\n\"Technically? It's an eggshell. I nabbed it from the debris of one of those ghastly beasts' eggs.\"\r\n\r\n\"What--? Why would you do something like that?\"\r\n\r\n\"Oh, relax. None of them were around, that \"nest\" is long abandoned. Besides, it's useful.\"\r\n\r\n\"How?\"\r\n\r\n\"Well, turns out, it's got a ton of energy in it. It's still warm -- feel it, see? -- even though it's been sitting there for weeks. I guess those things use the heat from the eggs for automatic incubation, something like that. Thankfully I didn't try to take one from a fresh egg, might've burnt my hand off for all I know.\"\r\n\r\n\"And about it being useful...\"\r\n\r\n\"Yes, yes -- you see, that energy isn't just dormant: if you hit it, say, with a bullet, it'll explode. The light is blinding and will probably set you on fire, so don't drop it.\"\r\n\r\n\"And if we shoot it from a distance, placed near those horrors...\"\r\n\r\n\"Exactly. In this hellscape, it's as close to a flashbang as we'll get.\"\r\n\r\n\"Maybe we'll be able to get off this blasted planet after all, then.\"\r\n\r\n\"Oh, about that. If anyone asks, make up some cool name for this. They'll probably believe anything. I want nothing to do with the UES if we survive this, and that means selling all this stuff for as high of a price as we can muster -- \"alien eggshell\" isn't the most attractive name.\"";

        public override string AchievementName => "A cycle, broken.";

        public override string AchievementDesc => "Mutilate a child of the stars...";

        [ConfigField("Chance", "", 9f)]
        public static float chance;

        [ConfigField("Explosion Radius", "", 12f)]
        public static float explosionRadius;

        [ConfigField("Explosion Proc Coefficient", "", 0.33f)]
        public static float explosionProcCoefficient;

        [ConfigField("Stun Duration", "", 1.5f)]
        public static float stunDuration;

        [ConfigField("Base TOTAL Damage", "Decimal.", 1.5f)]
        public static float baseTotalDamage;

        [ConfigField("Stack TOTAL Damage", "Decimal.", 1.5f)]
        public static float stackTotalDamage;

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("SunFragmentPrefab.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texSunFragment.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };

        public override bool nonstandardScaleModel => true;

        public GameObject FragmentVFX;

        public static GameObject FragmentVFXSphere;
        public static ProcType sunFragmentDoTProcType = (ProcType)1298571298;
        public static ProcType sunFragmentAreaProcType = (ProcType)1298571264;

        public override void Init(ConfigFile config)
        {
            var sunFragment = Main.MainAssets.LoadAsset<GameObject>("SunFragmentPrefab.prefab");
            var sunFragmentMat = sunFragment.transform.GetChild(0).GetComponent<MeshRenderer>().material;
            sunFragmentMat.SetFloat("_NormalStrength", 0.8263923f);

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
            if (!damageInfo.procChainMask.HasProc(sunFragmentDoTProcType) && attacker && damageInfo.HasModdedDamageType(SunFragmentDamageType))
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
                            var totalDamage = baseTotalDamage + stackTotalDamage * (stack - 1);
                            var dot = new InflictDotInfo()
                            {
                                attackerObject = damageInfo.attacker,
                                victimObject = self.gameObject,
                                totalDamage = damageInfo.damage * totalDamage,
                                damageMultiplier = 3f,
                                dotIndex = DotController.DotIndex.Burn,
                                maxStacksFromAttacker = null,
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
                if (Util.CheckRoll(chance * damageInfo.procCoefficient, attackerBody.master))
                {
                    EffectData effectData = new()
                    {
                        origin = victimBody.corePosition,
                        rotation = Util.QuaternionSafeLookRotation(damageInfo.force != Vector3.zero ? damageInfo.force : Random.onUnitSphere),
                        scale = explosionRadius
                    };
                    EffectData effectData2 = new()
                    {
                        origin = victimBody.corePosition,
                        scale = explosionRadius
                    };
                    EffectManager.SpawnEffect(FragmentVFX, effectData, true);
                    EffectManager.SpawnEffect(FragmentVFXSphere, effectData2, true);

                    var setStateOnHurt = victimBody.GetComponent<SetStateOnHurt>();
                    setStateOnHurt?.SetStun(stunDuration);

                    // var damage = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, baseTotalDamage + stackTotalDamage * (stack - 1));

                    BlastAttack blastAttack = new()
                    {
                        radius = explosionRadius,
                        baseDamage = Mathf.Epsilon, // dont ask
                        procCoefficient = explosionProcCoefficient,
                        crit = damageInfo.crit,
                        damageColorIndex = SolarFlareColour,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        falloffModel = BlastAttack.FalloffModel.None,
                        attacker = attackerBody.gameObject,
                        teamIndex = attackerBody.teamComponent.teamIndex,
                        position = damageInfo.position,
                        damageType = DamageType.Silent | DamageType.BypassArmor | DamageType.BypassBlock // I said dont ask
                    };

                    blastAttack.AddModdedDamageType(SunFragmentDamageType);
                    blastAttack.Fire();

                    damageInfo.procChainMask.AddProc(sunFragmentAreaProcType);

                    AkSoundEngine.PostEvent(Events.Play_fireballsOnHit_shoot, victimBody.gameObject);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}