using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace Sandswept.Items.Whites
{
    public class MakeshiftPlate : ItemBase<MakeshiftPlate>
    {
        public class UpdateToken : MonoBehaviour
        {
            public CharacterBody body;
            public float armor = 0;
            public float timer;

            public void FixedUpdate()
            {
                timer += Time.fixedDeltaTime;
                if (timer < 0.05f)
                {
                    return;
                }
                var stacks = body.inventory.GetItemCount(instance.ItemDef);
                if (stacks > 0 && armor != body.armor)
                {
                    armor = body.armor;
                    body.statsDirty = true;
                }
            }
        }

        public override string ItemName => "Makeshift Plate";

        public override string ItemLangTokenName => "MAKESHIFT_PLATE";

        public override string ItemPickupDesc => "Grants bonus armor based on current armor";

        public override string ItemFullDescription => "Gain $sh5$se $ss(+5 per stack)$se armor. Grants an additional $sh5$se $ss(+1 per stack)$se $sharmor$se for every $sh25 armor$se you have.";

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
            GetStatCoefficients += AddArmour;
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

        private void AddArmour(CharacterBody sender, StatHookEventArgs args)
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