namespace Sandswept.Items.Whites
{
    internal class RedSpringWater : ItemBase<RedSpringWater>
    {
        public override string ItemName => "Red Spring Water";

        public override string ItemLangTokenName => "RED_SPRING_WATER";

        public override string ItemPickupDesc => "Slightly increase regeneration. This effect is tripled while in danger.";

        public override string ItemFullDescription => StringExtensions.AutoFormat("Increase $shbase health regeneration$se by $sh0.5hp/s$se $ss(+0.5hp/s per stack)$se while out of danger. This effect is $shtripled$se while in danger.");

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
                var passiveRegen = 0.5f + 0.5f * (stacks - 1);
                var combatRegen = 1.5f + 1.5f * (stacks - 1);

                if (sender.outOfDanger)
                {
                    args.baseRegenAdd += passiveRegen + 0.2f * passiveRegen * (sender.level - 1);
                }
                if (!sender.outOfDanger)
                {
                    args.baseRegenAdd += combatRegen + 0.2f * combatRegen * (sender.level - 1);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}