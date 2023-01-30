using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace Sandswept.Items
{
    public class MakeshiftPlate : ItemBase<MakeshiftPlate>
    {
        public class UpdateToken : MonoBehaviour
        {
            public CharacterBody body;
            public float armour = 0;
            public float Timer;
            public void FixedUpdate()
            {
                Timer += Time.fixedDeltaTime;
                if (!(Timer >= 0.05))
                {
                    return;
                }
                var stacks = body.inventory.GetItemCount(instance.ItemDef);
                if (stacks > 0 && armour != body.armor)
                {
                    armour = body.armor;
                    body.statsDirty = true;
                }
            }
        }
        public override string ItemName => "Makeshift Plate";

        public override string ItemLangTokenName => "MAKESHIFT_PLATE";

        public override string ItemPickupDesc => "Grants bonus armour based on current armour";

        public override string ItemFullDescription => "Gain 5 (+5 per stack) armour. Grants an additional 5 (+1 per stack) armour for every 25 armour you have.";

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
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            RecalculateStatsAPI.GetStatCoefficients += AddArmour;
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            var check = self.gameObject.GetComponent<UpdateToken>();
            if (!check)
            {
                var stacks = GetCount(self);
                if (stacks > 0)
                {
                    var token = self.gameObject.AddComponent<UpdateToken>();
                    token.body = self;
                }
            }
            orig(self);
        }

        private void AddArmour(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var stacks = GetCount(sender);
            if (stacks > 0)
            {
                var stackScaling = 5 * stacks;
                args.armorAdd += stackScaling + (sender.armor - sender.armor % 25) / 25 * (5 + (1 * stacks - 1));
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
