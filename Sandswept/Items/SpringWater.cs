using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace Sandswept.Items
{
    internal class SpringWater : ItemBase<SpringWater>
    {
        public override string ItemName => "Red Spring Water";

        public override string ItemLangTokenName => "RED_SPRING_WATER";

        public override string ItemPickupDesc => "Gain regen that increases in combat";

        public override string ItemFullDescription => "Gain 0.5 (+0.5 per stack) base regen, this effect doubles in combat";

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
            RecalculateStatsAPI.GetStatCoefficients += GiveStats;
        }

        private void GiveStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int stacks = GetCount(sender);
            if (stacks > 0)
            {
                if (sender.outOfDanger)
                {
                    args.baseRegenAdd = 0.5f + 0.5f * (stacks - 1);
                }
                if (!sender.outOfDanger)
                {
                    args.baseRegenAdd = 1f + 1f * (stacks - 1);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
