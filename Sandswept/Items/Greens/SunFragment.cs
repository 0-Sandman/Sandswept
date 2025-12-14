using LookingGlass.ItemStatsNameSpace;
using static Sandswept.Utils.Components.MaterialControllerComponents;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Sun Fragment")]
    public class SunFragment : ItemBase<SunFragment>
    {
        public static DamageColorIndex SolarFlareColour = DamageColourHelper.RegisterDamageColor(new Color32(255, 150, 25, 255));

        public override string ItemName => "Sun Fragment";

        public override string ItemLangTokenName => "SUN_FRAGMENT";

        public override string ItemPickupDesc => "Create a blinding flash on hit that stuns and ignites enemies.";

        public override string ItemFullDescription => $"$su{chance}%$se chance on hit to create a $sublinding flash$se in a $su{explosionRadius}m$se radius, $sustunning$se for $su{stunDuration}s$se and $sdigniting$se enemies for $sd{baseTotalDamage * 100f}%$se $ss(+{stackTotalDamage * 100f}% per stack)$se TOTAL damage.".AutoFormat();

        public override string ItemLore =>
        """
        "What the hell is that?"

        "Technically? It's an eggshell. I nabbed it from the debris of one of those ghastly beasts' eggs."

        "What--? Why would you go over there? Have you seen the size of those things?"

        "Oh, relax. None of them were around, that "nest" is long abandoned. Besides, this thing's useful."

        "How?"

        "Well, turns out, it's got a ton of energy in it. It's still warm -- feel it, see? -- even though it's been -- careful, careful, don't drop it -- even though it's been sitting there for weeks. I guess the heat from the eggs is some sort of automatic incubation, something like that. I'm glad I didn't try to take one from a fresh egg, or I might've burnt my hand off."

        "So, what, it's an organic hand warmer? I thought you said it was useful."

        "Yes, yes, I was getting to that. You see, that energy isn't just dormant: if you hit it, say, with a bullet, it'll release. The light is blinding and will probably set you or anything else nearby on fire."

        "And if we shoot it from a distance, while all those horrors are around it..."

        "Exactly. In this hellscape, it's as close to a flashbang as we'll get."

        "Maybe we'll be able to get off this damned planet after all, then."

        "Oh, about that. If anyone asks, make up some cool name for this. They'll probably believe anything. I want nothing to do with the UES if we survive this, and that means selling anything we find for as high of a price as we can muster -- "alien eggshell" isn't the most attractive name."

        """;
        public override string AchievementName => "A cycle, broken.";

        public override string AchievementDesc => "Scrap a Planula...";

        [ConfigField("Chance", "", 8f)]
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
        //

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.mainAssets.LoadAsset<GameObject>("SunFragmentPrefab.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texSunFragment.png");

        public override ItemTag[] ItemTags => [ItemTag.Damage, ItemTag.Utility, ItemTag.CanBeTemporary];

        public GameObject FragmentVFX;

        public static GameObject FragmentVFXSphere;

        public static ModdedProcType sunFragment = ProcTypeAPI.ReserveProcType();

        public override void Init()
        {
            base.Init();
            SetUpVFX();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Stun and Burn Chance: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Utility);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.descriptions.Add("TOTAL Damage: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.hasChance = true;
            itemStatsDef.chanceScaling = ItemStatsDef.ChanceScaling.DoesNotScale;
            itemStatsDef.calculateValuesNew = (luck, stack, procChance) =>
            {
                List<float> values = new()
                {
                    LookingGlass.Utils.CalculateChanceWithLuck(chance * procChance * 0.01f, luck),
                    baseTotalDamage + stackTotalDamage * (stack - 1)
                };

                return values;
            };

            return itemStatsDef;
        }

        public void SetUpVFX()
        {
            var sunFragment = Main.mainAssets.LoadAsset<GameObject>("SunFragmentPrefab.prefab");
            var sunFragmentMat = sunFragment.transform.GetChild(0).GetComponent<MeshRenderer>().material;
            sunFragmentMat.SetFloat("_NormalStrength", 0.8263923f);

            FragmentVFX = Main.mainAssets.LoadAsset<GameObject>("FragmentFXRing.prefab");
            var component = FragmentVFX.AddComponent<EffectComponent>();
            component.applyScale = true;
            Main.EffectPrefabs.Add(FragmentVFX);

            FragmentVFXSphere = Main.mainAssets.LoadAsset<GameObject>("FragmentFXSphere.prefab");
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
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;

            if (damageInfo.procChainMask.HasModdedProc(sunFragment))
            {
                return;
            }

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var inventory = attackerBody.inventory;
            if (!inventory)
            {
                return;
            }

            var victimBody = report.victimBody;
            if (!victimBody)
            {
                return;
            }

            var stack = GetCount(attackerBody);
            if (stack <= 0)
            {
                return;
            }

            if (!Util.CheckRoll(chance * damageInfo.procCoefficient, attackerBody.master))
            {
                return;
            }

            damageInfo.procChainMask.AddModdedProc(sunFragment);

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

            // var damage = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, baseTotalDamage + stackTotalDamage * (stack - 1));

            BlastAttack blastAttack = new()
            {
                radius = explosionRadius,
                baseDamage = 0, // dont ask
                procCoefficient = explosionProcCoefficient,
                crit = damageInfo.crit,
                damageColorIndex = SolarFlareColour,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                falloffModel = BlastAttack.FalloffModel.None,
                attacker = attackerBody.gameObject,
                teamIndex = attackerBody.teamComponent.teamIndex,
                position = damageInfo.position,
                damageType = DamageType.Silent,
                procChainMask = damageInfo.procChainMask
            };

            var result = blastAttack.Fire();

            var totalDamage = baseTotalDamage + stackTotalDamage * (stack - 1);

            for (int i = 0; i < result.hitPoints.Length; i++)
            {
                var hitPoint = result.hitPoints[i];
                var hurtBox = hitPoint.hurtBox;
                if (!hurtBox)
                {
                    continue;
                }
                var hc = hurtBox.healthComponent;
                if (!hc)
                {
                    continue;
                }

                var body = hc.body;

                var dot = new InflictDotInfo()
                {
                    attackerObject = damageInfo.attacker,
                    victimObject = body.gameObject,
                    totalDamage = damageInfo.damage * totalDamage,
                    damageMultiplier = 2f * stack,
                    dotIndex = DotController.DotIndex.Burn,
                    maxStacksFromAttacker = null,
                };

                var setStateOnHurt = body.GetComponent<SetStateOnHurt>();
                setStateOnHurt?.SetStun(stunDuration);

                StrengthenBurnUtils.CheckDotForUpgrade(inventory, ref dot);
                DotController.InflictDot(ref dot);
            }

            Util.PlaySound("Play_fireballsOnHit_shoot", victimBody.gameObject);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var itemDisplay = SetUpFollowerIDRS(0.4f, 60f, true, 10f, false, 0f, false, 0f);

            return new ItemDisplayRuleDict(new ItemDisplayRule()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(-1f, -0.7f, -0.9f),
                localScale = new Vector3(0.25f, 0.25f, 0.25f),

                followerPrefab = itemDisplay,
                limbMask = LimbFlags.None,
                followerPrefabAddress = new AssetReferenceGameObject("")
            });
        }
    }
}