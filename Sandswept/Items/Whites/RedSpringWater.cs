namespace Sandswept.Items.Whites
{
    internal class RedSpringWater : ItemBase<RedSpringWater>
    {
        public override string ItemName => "Red Spring Water";

        public override string ItemLangTokenName => "RED_SPRING_WATER";

        public override string ItemPickupDesc => "Increase health regeneration for every buff you have.";

        public override string ItemFullDescription => "Increase $shhealth regeneration$se by $sd0.3 hp/s$se Sss(+0.3 per stack, +0.06 per level)$se for $suevery buff you have$se.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("SpringWaterPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("SpringWaterIcon.png");

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
            int stacks = GetCount(sender);
            if (stacks > 0)
            {
                float regen = (0.3f * stacks) + ((0.06f * stacks) * sender.level);

                float totalRegen = 0;

                for (BuffIndex index = (BuffIndex)0; (int)index < BuffCatalog.buffCount; index++) {
                    BuffDef buff = BuffCatalog.GetBuffDef(index);
                    if (buff && !buff.isDebuff && sender.HasBuff(buff)) {
                        totalRegen += regen;
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