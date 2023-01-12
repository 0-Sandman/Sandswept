using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace Sandswept.Items
{
    public class MakeshiftPlate : ItemBase<MakeshiftPlate>
    {
        public override string ItemName => "Makeshift Plate";

        public override string ItemLangTokenName => "MAKESHIFT_PLATE";

        public override string ItemPickupDesc => "Grants bonus armour based on current armour";

        public override string ItemFullDescription => "Gain 5 (+5 per stack) armour. Grants 5 (+1 per stack) additonal armour for every 25 armour you have.";

        public override string ItemLore => "I hope ceremonial jar is coded soon :Yeah3D:";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("MakeshiftPlatePrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("MakeshiftPlateIcon.png");


        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += AddArmour;
        }

        private void AddArmour(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var stacks = GetCount(sender);
            if (stacks > 0)
            {
                args.armorAdd += (5 * stacks) + (sender.armor + (5 * stacks) - sender.armor + (5 * stacks) % 25) / 25 * (5 + 1 * (stacks - 1));
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
