using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace Sandswept.Items
{
    public class UniVIP : ItemBase<UniVIP>
    {
        public class PassBehaviour : MonoBehaviour
        {
            public CharacterBody body;
            public float storedTotal;

            public void Start()
            {
                On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin1;
                Stage.onStageStartGlobal += Stage_onStageStartGlobal;
            }

            private void Stage_onStageStartGlobal(Stage obj)
            {
                CharacterMaster bodyMaster = body.master;
                var check = body.inventory.GetItemCount(instance.ItemDef);
                if (check == 0)
                {
                    Destroy(this);
                }

                if (bodyMaster)
                {
                    bodyMaster.GiveMoney((uint)storedTotal);
                    storedTotal = 0;
                }
            }

            private void PurchaseInteraction_OnInteractionBegin1(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
            {
                if (activator && self.costType == CostTypeIndex.Money)
                {
                    var funnyBody = activator.GetComponent<CharacterBody>();
                    if (funnyBody = body)
                    {
                        int stacks = body.inventory.GetItemCount(instance.ItemDef);
                        if (stacks > 0)
                        {
                            storedTotal += self.cost * Util.ConvertAmplificationPercentageIntoReductionPercentage(0.15f + 0.10f * (stacks - 1));
                        }
                    }
                }
                orig(self, activator);
            }
        }
        public override string ItemName => "Universal VIP Pass";

        public override string ItemLangTokenName => "VIP_PASS";

        public override string ItemPickupDesc => "Store interactable costs as a bonus next stage.";

        public override string ItemFullDescription => "Store <style=cIsUtility>15%</style> <style=cStack>(+10% per stack)</style> of all used interactable costs, recieve the total stored amount on the next stage.";

        public override string ItemLore => "Funny pt.2";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("UniVIPPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("UniVIPIcon.png");


        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += InventoryChanged;
        }

        private void InventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            int stack = GetCount(self);
            if (stack >= 1)
            {
                PassBehaviour behaviourCheck = self.GetComponent<PassBehaviour>();
                if (!behaviourCheck)
                {
                    var token = self.gameObject.AddComponent<PassBehaviour>();
                    token.body = self;
                }
            }
            return;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
