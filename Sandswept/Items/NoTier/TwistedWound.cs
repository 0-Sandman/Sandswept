using LookingGlass.ItemStatsNameSpace;

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

        public override GameObject ItemModel => Paths.GameObject.DisplayEliteBeadSpike;

        public override Sprite ItemIcon => Main.sandsweptHIFU.LoadAsset<Sprite>("texTwistedWound.png");

        public override ItemTag[] ItemTags => [];

        public override bool CanRemove => false;

        public override void Init()
        {
            base.Init();
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

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Curse Amount: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Death);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);

            itemStatsDef.calculateValues = (master, stack) =>
            {
                List<float> values = new()
                {
                    (100 * stack) / (100 + stack) * 0.01f
                };

                return values;
            };

            return itemStatsDef;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}