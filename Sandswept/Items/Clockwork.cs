using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace Sandswept.Items
{
    public class Clockwork : ItemBase<Clockwork>
    {
        public override string ItemName => "Fractured Timepiece";

        public override string ItemLangTokenName => "CLOCKWORK";

        public override string ItemPickupDesc => "Gain attack speed and cooldown reduction";

        public override string ItemFullDescription => "Gain 10% (+7.5% per stack) attack speed and 5% (+2.5% per stack) cooldown reduction.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => null;


        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += GiveStats;
        }

        private void GiveStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int stacks = GetCount(sender);

            if (stacks > 0)
            {
                args.cooldownMultAdd -= 0.075f + (0.075f * --stacks);
                args.attackSpeedMultAdd += 0.08f + (0.08f * --stacks);
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
