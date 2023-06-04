namespace Sandswept.Items.Whites
{
    public class Clockwork : ItemBase<Clockwork>
    {
        public override string ItemName => "Fractured Timepiece";

        public override string ItemLangTokenName => "CLOCKWORK";

        public override string ItemPickupDesc => "Gain attack speed and cooldown reduction";

        public override string ItemFullDescription => StringExtensions.AutoFormat("Gain $sd10%$se $ss(+7.5% per stack)$se attack speed and $su5%$se $ss(+2.5% per stack)$se cooldown reduction.");

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => null;

        public override void Init(ConfigFile config)
        {
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
                args.cooldownMultAdd *= 1f - (0.05f + 0.025f * (stacks - 1));
                args.attackSpeedMultAdd += 0.1f + 0.075f * (stacks - 1);
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}