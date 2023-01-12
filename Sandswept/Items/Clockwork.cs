using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Sandswept.Items
{
    public class Clockwork : ItemBase<Clockwork>
    {
        public override string ItemName => "Clockwork";

        public override string ItemLangTokenName => "CLOCKWORK";

        public override string ItemPickupDesc => "Gain attack speed and cooldown reduction";

        public override string ItemFullDescription => "Gain 10% (+5% per stack) and 5% (+2.5% per stack) cooldown reduction.";

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
                args.cooldownMultAdd += 5f + 2.5f * (stacks - 1);
                args.attackSpeedMultAdd += 10f + 5f * (stacks - 1);
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
