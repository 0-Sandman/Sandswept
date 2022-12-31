using BepInEx.Configuration;
using R2API;
using RoR2;
using Sandswept.Utils;
using UnityEngine;
using static Sandswept.Main;
using static Sandswept.Utils.Components.MaterialControllerComponents;

namespace Sandswept.Items
{
    public class UniVIP : ItemBase<UniVIP>
    {
        public class PassBehaviour : CharacterBody.ItemBehavior 
        {
            public CharacterBody body;
            public float storedTotal;

            public void Start()
            {
                On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin1;
                On.RoR2.Inventory.RemoveItem_ItemIndex_int += Inventory_RemoveItem_ItemIndex_int;
                On.RoR2.Stage.Start += Stage_Start;
            }

            public void OnDestroy()
            {
                Debug.Log("Item removed");
                On.RoR2.PurchaseInteraction.OnInteractionBegin -= PurchaseInteraction_OnInteractionBegin1;
                On.RoR2.Inventory.RemoveItem_ItemIndex_int -= Inventory_RemoveItem_ItemIndex_int;
                On.RoR2.Stage.Start -= Stage_Start;
            }

            private void PurchaseInteraction_OnInteractionBegin1(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
            {
                orig(self, activator);
                if (!activator || self.costType != CostTypeIndex.Money)
                {
                    return;
                }

                body = activator.GetComponent<CharacterBody>();
                int stacks = body.inventory.GetItemCount(instance.ItemDef);
                if (stacks > 0)
                {
                    storedTotal += Util.ConvertAmplificationPercentageIntoReductionPercentage((((float)stacks - 1) * stackScaler) + (float)moneyScaler) * self.cost;
                    Debug.Log(storedTotal);
                    return;
                }
                return;
            }

            private void Stage_Start(On.RoR2.Stage.orig_Start orig, Stage self)
            {
                orig(self);

                CharacterMaster bodyMaster = base.GetComponent<CharacterMaster>();

                bodyMaster.GiveMoney((uint)storedTotal);
                storedTotal= 0;

                return;
            }

            private void Inventory_RemoveItem_ItemIndex_int(On.RoR2.Inventory.orig_RemoveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
            {
                orig(self, itemIndex, count);
                int stack = self.GetItemCount(instance.ItemDef);
                if (stack == 0)
                {
                    PassBehaviour behaviourCheck = self.GetComponent<PassBehaviour>();
                    if (behaviourCheck)
                    {
                        Destroy(this);
                    }
                }
                return;
            }
        }

        public static ConfigOption<float> moneyScaler;

        public static ConfigOption<float> stackScaler;

        public override string ItemName => "Universal VIP Pass";

        public override string ItemLangTokenName => "VIP_PASS";

        public override string ItemPickupDesc => "Store interactable costs as a bonus next stage.";

        public override string ItemFullDescription => "Store <style=cIsUtility>15%</style> <style=cStack>(+5% per stack)</style> of all used interactable costs, recieve the total stored amount on the next stage.";

        public override string ItemLore => "Cum!!";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("UniVIPPrefab.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("UniVIPIcon.png");


        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            moneyScaler = config.ActiveBind("Item: " + ItemName, "Cost value stored", 0.15f, "Set preferred money value stored");
            stackScaler = config.ActiveBind("Item: " + ItemName, "Value stored per stack", 0.05f, "Set preferred item scaling");
        }
        public override void Hooks()
        {
            On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
        }

        private void Inventory_GiveItem_ItemIndex_int(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            orig(self, itemIndex, count);
            int stack = self.GetItemCount(ItemDef);
            if (stack >= 1)
            {
            PassBehaviour behaviourCheck = self.GetComponent<PassBehaviour>();
            if (!behaviourCheck)
            {
                self.gameObject.AddComponent<PassBehaviour>();
            }
            }
            return;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            MeshRenderer component = ItemModel.transform.Find("UniVIP").GetComponent<MeshRenderer>();
            var materialAssign = ItemModel.gameObject.AddComponent<HGControllerFinder>();
            materialAssign.Renderer = component;
            return new ItemDisplayRuleDict();
        }
    }
}
