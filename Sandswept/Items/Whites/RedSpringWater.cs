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

        public override string ItemLore => "<style=cMono>//--AUTO-TRANSCRIPTION FROM UES [Redacted] --//</style>\r\n\r\n\"I don't know if you should be drinking that.\"\r\n\r\n\"Why not? I feel amazing!\"\r\n\r\n\"We don't know what's in that stuff. No xenologist would ever recommend you drink an unknown red liquid from a more unknown planet.\"\r\n\r\n\"Well, it's only ever been of help to me. We need everything we can get down there.\"\r\n\r\n\"Better to be without it than to be poisoned. When we gave it to Kyle to help him recover, it just made things worse.\"\r\n\r\n\"Well, unlike Kyle, I'm not sick. Probably because of this stuff, too, with how much faster my wounds are healing.\"\r\n\r\n\"It's your funeral.\"\r\n";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("RedSpringWaterHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texRedSpringWater.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Healing };

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
            itemStatsDef.descriptions.Add("Healing: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Healing);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.FlatHealing);
            itemStatsDef.calculateValues = (master, stack) =>
            {
                float totalRegenGain = 0f;
                var body = master.GetBody();
                if (body)
                {
                    float gainPerBuff = 0f;
                    float counter = 0.5f;
                    totalRegenGain = baseRegen;

                    for (BuffIndex index = (BuffIndex)0; (int)index < BuffCatalog.buffCount; index++)
                    {
                        BuffDef buff = BuffCatalog.GetBuffDef(index);
                        if (buff && !buff.isDebuff && body.HasBuff(buff))
                        {
                            counter += 0.5f;
                            gainPerBuff += (baseRegenPerBuff + stackRegenPerBuff * (stack - 1)) / counter;
                        }
                    }

                    totalRegenGain += gainPerBuff;
                }

                // dont scale value with level cause lookingglass doesnt

                List<float> values = new()
                {
                    totalRegenGain,
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
            return new ItemDisplayRuleDict();
        }
    }
}