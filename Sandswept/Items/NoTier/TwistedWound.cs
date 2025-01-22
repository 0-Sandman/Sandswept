namespace Sandswept.Items.NoTier
{
    internal class TwistedWound : ItemBase<TwistedWound>
    {
        public override string ItemName => "Twisted Wound";

        public override string ItemLangTokenName => "TWISTED_WOUND";

        public override string ItemPickupDesc => "A crystalline mark of your greed.";

        public override string ItemFullDescription => "A crystalline mark of your greed.".AutoFormat();

        public override string ItemLore => "OwO UwU :333 >w< >_< <3 uwuuu owo <3 <3";

        public override ItemTier Tier => ItemTier.NoTier;

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => null;

        public override ItemTag[] ItemTags => new ItemTag[] { };

        public override void Init(ConfigFile config)
        {
            base.Init(config);
        }

        public override void Hooks()
        {
            GetStatCoefficients += GiveStats;
        }

        private void GiveStats(CharacterBody sender, StatHookEventArgs args)
        {
            if (!sender.inventory) return;
            int c = sender.inventory.GetItemCount(ItemDef);

            if (c > 0)
            {
                args.baseCurseAdd += 0.01f * c;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}