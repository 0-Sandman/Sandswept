namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Red Spring Water")]
    internal class RedSpringWater : ItemBase<RedSpringWater>
    {
        public override string ItemName => "Red Spring Water";

        public override string ItemLangTokenName => "RED_SPRING_WATER";

        public override string ItemPickupDesc => "Increase health regeneration for every buff you have.";

        public override string ItemFullDescription => ("Increase $shhealth regeneration$se by $sh" + baseRegenPerBuff + " hp/s$se $ss(+" + stackRegenPerBuff + " hp/s per stack)$se for $suevery buff you have$se.").AutoFormat();

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("Assets/Sandswept/RedSpringWaterHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texRedSpringWater.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Healing, ItemTag.AIBlacklist };

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
                var regen = baseRegenPerBuff + stackRegenPerBuff * (stack - 1);

                float totalRegen = 0;

                for (BuffIndex index = (BuffIndex)0; (int)index < BuffCatalog.buffCount; index++)
                {
                    BuffDef buff = BuffCatalog.GetBuffDef(index);
                    if (buff && !buff.isDebuff && sender.HasBuff(buff))
                    {
                        totalRegen += regen + 0.2f * regen * (sender.level - 1);
                    }
                }

                args.baseRegenAdd += totalRegen;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}