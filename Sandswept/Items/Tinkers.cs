using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace Sandswept.Items
{
    public class Tinkers : ItemBase<Tinkers>
    {
        public override string ItemName => "Tinkers";

        public override string ItemLangTokenName => "TINKERS_";

        public override string ItemPickupDesc => "Your drones gain damage and attack speed.";

        public override string ItemFullDescription => "Your drones gain 15% (+10% per stack) damage and 15% (+15% per stack) attack speed.";

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
            if (sender.bodyFlags == CharacterBody.BodyFlags.Mechanical)
            {
                var master = sender.master.minionOwnership.ownerMaster;

                var masterBody = master.GetBody();

                int stacks = GetCount(masterBody);

                if (stacks > 0)
                {
                    Debug.Log("diedie diedie ideideideiediiedieieiddeiedi");
                    args.damageMultAdd = 15f + 10f * (stacks - 1);
                    args.attackSpeedMultAdd = 15f * stacks;
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
