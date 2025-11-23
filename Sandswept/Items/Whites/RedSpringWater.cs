using LookingGlass.ItemStatsNameSpace;

namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Red Spring Water")]
    internal class RedSpringWater : ItemBase<RedSpringWater>
    {
        public override string ItemName => "Red Spring Water";

        public override string ItemLangTokenName => "RED_SPRING_WATER";

        public override string ItemPickupDesc => "Increase health regeneration for every unique buff you have.";

        public override string ItemFullDescription => $"Increase $shhealth regeneration$se by $sh{baseRegen} hp/s$se, plus an additional $sh{baseRegenPerBuff} hp/s$se $ss(+{stackRegenPerBuff} hp/s per stack)$se for $suevery unique buff you have$se.".AutoFormat();

        public override string ItemLore =>
        """
        <style=cMono>
        //--AUTO-TRANSCRIPTION FROM UES [Redacted] --//
        </style>
        "I don't know if you should be drinking that."

        "Why not? I feel amazing!"

        "We don't know what's in that stuff. No xenologist would ever recommend you drink an unknown red liquid from a more unknown planet."

        "Well, it's only ever been of help to me. We need everything we can get down there."

        "Better to be without it than to be poisoned. When we gave it to Kyle to help him recover, it just made things worse."

        "Well, unlike Kyle, I'm not sick. Probably because of this stuff, too, with how much faster my wounds are healing."

        "It's your funeral."
        """;
        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("RedSpringWaterHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texRedSpringWater.png");

        public override ItemTag[] ItemTags => [ItemTag.Healing, ItemTag.CanBeTemporary, ItemTag.FoodRelated];

        public override float modelPanelParametersMinDistance => 4f;
        public override float modelPanelParametersMaxDistance => 12f;

        [ConfigField("Base Regen", "Only for the first stack", 0.6f)]
        public static float baseRegen;

        [ConfigField("Base Regen Per Buff", "", 0.6f)]
        public static float baseRegenPerBuff;

        [ConfigField("Stack Regen Per Buff", "", 0.6f)]
        public static float stackRegenPerBuff;

        public override void Init()
        {
            base.Init();
            SetUpMaterials();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Base Regen: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Healing);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.FlatHealing);
            itemStatsDef.descriptions.Add("Level Scaled: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Healing);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.FlatHealing);
            itemStatsDef.calculateValues = (master, stack) =>
            {
                float totalUnscaledRegenGain = 0f;
                float totalScaledRegenGain = 0f;
                var body = master.GetBody();
                if (body)
                {
                    float gainPerBuff = 0f;
                    float counter = 0.5f;
                    totalUnscaledRegenGain = baseRegen;

                    for (BuffIndex index = (BuffIndex)0; (int)index < BuffCatalog.buffCount; index++)
                    {
                        BuffDef buff = BuffCatalog.GetBuffDef(index);
                        if (buff && !buff.isDebuff && body.HasBuff(buff))
                        {
                            counter += 0.5f;
                            gainPerBuff += (baseRegenPerBuff + stackRegenPerBuff * (stack - 1)) / counter;
                        }
                    }

                    totalUnscaledRegenGain += gainPerBuff;
                    totalScaledRegenGain = totalUnscaledRegenGain + (totalUnscaledRegenGain * 0.2f * (body.level - 1));
                }

                // dont scale value with level cause lookingglass doesnt

                List<float> values = new()
                {
                    totalUnscaledRegenGain,
                    totalScaledRegenGain
                };

                return values;
            };

            return itemStatsDef;
        }

        public void SetUpMaterials()
        {
            var powerElixirGlassMat = new Material(Utils.Assets.Material.matHealingPotionGlass);
            powerElixirGlassMat.SetFloat("_Boost", 0.25f);
            powerElixirGlassMat.SetFloat("_RimPower", 1.706559f);
            powerElixirGlassMat.SetFloat("_RimStrength", 3.538423f);
            powerElixirGlassMat.SetFloat("_AlphaBoost", 1.147384f);
            powerElixirGlassMat.SetFloat("IntersectionStrength", 1f);

            var redSpringWaterHolder = Main.hifuSandswept.LoadAsset<GameObject>("RedSpringWaterHolder.prefab");
            var model = redSpringWaterHolder.transform.GetChild(0);
            var jarMr = model.GetChild(1).GetComponent<MeshRenderer>();
            jarMr.material = powerElixirGlassMat;
        }

        public override void Hooks()
        {
            GetStatCoefficients += GiveStats;
        }

        private void GiveStats(CharacterBody sender, StatHookEventArgs args)
        {
            var stack = GetCount(sender);
            if (stack > 0)
            {
                float gainPerBuff = 0f;
                float counter = 0.5f;
                var gainFromBase = baseRegen + 0.2f * baseRegen * (sender.level - 1);
                float totalRegenGain = gainFromBase;

                for (BuffIndex index = (BuffIndex)0; (int)index < BuffCatalog.buffCount; index++)
                {
                    BuffDef buff = BuffCatalog.GetBuffDef(index);
                    if (buff && !buff.isDebuff && sender.HasBuff(buff))
                    {
                        counter += 0.5f;
                        gainPerBuff += (baseRegenPerBuff + stackRegenPerBuff * (stack - 1)) / counter;
                    }
                }

                totalRegenGain += gainPerBuff + 0.2f * gainPerBuff * (sender.level - 1);

                args.baseRegenAdd += totalRegenGain;
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
            */

            i.Add("ElectricianBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "ClavicleR",
                    localPos = new Vector3(0.03164F, 0.29731F, -0.24579F),
                    localAngles = new Vector3(338.0341F, 190.7353F, 189.2521F),
                    localScale = new Vector3(0.06921F, 0.06463F, 0.06463F),

                    followerPrefab = itemDisplay,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            #endregion

            return i;

        }
    }
}