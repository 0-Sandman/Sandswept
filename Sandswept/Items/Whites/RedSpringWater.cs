namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Red Spring Water")]
    internal class RedSpringWater : ItemBase<RedSpringWater>
    {
        public override string ItemName => "Red Spring Water";

        public override string ItemLangTokenName => "RED_SPRING_WATER";

        public override string ItemPickupDesc => "Increase health regeneration for every buff you have.";

        public override string ItemFullDescription => ("Increase $shhealth regeneration$se by $sh" + baseRegen + " hp/s$se, plus an additional $sh" + baseRegenPerBuff + " hp/s$se $ss(+" + stackRegenPerBuff + " hp/s per stack)$se for $suevery buff you have$se.").AutoFormat();

        public override string ItemLore => "//--AUTO-TRANSCRIPTION FROM UES [Redacted] --//\r\n\"I don't know if you should be drinking that.\"\r\n\r\n\"Why not? I feel amazing!\"\r\n\r\n\"We don't know what's in that stuff. No xenologist would ever recommend you drink a strange red liquid from an unknown planet.\"\r\n\r\n\"Well, it's helping, and we'll need everything we can get down there.\"\r\n\r\n\"Better to be without it than to be poisoned. When we gave it to Kyle to help him recover, it just made things worse.\"\r\n\r\n\"Well, unlike Kyle, I'm not sick. And, unlike Kyle, it's making me stronger.\"\r\n\r\n\"It's your funeral.\"";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("RedSpringWaterHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texRedSpringWater.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Healing, ItemTag.AIBlacklist };

        [ConfigField("Base Regen", "Only for the first stack", 0.4f)]
        public static float baseRegen;

        [ConfigField("Base Regen Per Buff", "", 0.4f)]
        public static float baseRegenPerBuff;

        [ConfigField("Stack Regen Per Buff", "", 0.4f)]
        public static float stackRegenPerBuff;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
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
                var regen = baseRegen + (baseRegenPerBuff + stackRegenPerBuff * (stack - 1));
                var fromBase = baseRegen + 0.2f * baseRegen * (sender.level - 1);

                float totalRegen = 0;

                for (BuffIndex index = (BuffIndex)0; (int)index < BuffCatalog.buffCount; index++)
                {
                    BuffDef buff = BuffCatalog.GetBuffDef(index);
                    if (buff && !buff.isDebuff && sender.HasBuff(buff))
                    {
                        totalRegen += regen + 0.2f * regen * (sender.level - 1);
                    }
                }

                totalRegen += fromBase;

                args.baseRegenAdd += totalRegen;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}