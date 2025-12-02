/*
using static Sandswept.Main;

namespace Sandswept.Equipment
{
    [ConfigSection("Equipment :: Luck as a Constant")]
    public class LuckAsAConstant : EquipmentBase
    {
        public override string EquipmentName => "Luck as a Constant";

        public override string EquipmentLangTokenName => "LUCK_AS_A_CONSTANT";

        public override string EquipmentPickupDesc => "Upgrade 2 random items. Consumed on use.";

        public override string EquipmentFullDescription => ("$suUpgrade 2$se random items. Equipment is $suconsumed on use$se.").AutoFormat();

        public override string EquipmentLore => "hiii/hiiii/hiiiii/hiiiiii/hiiiiiii/hiiiiiiii/hiiiiiiiii/hiiiiiiiiii";

        public override GameObject EquipmentModel => prodAssets.LoadAsset<GameObject>("assets/sandswept/sandsweeper.fbx");

        public override Sprite EquipmentIcon => hifuSandswept.LoadAsset<Sprite>("texSandSweeper.png");

        public override float Cooldown => 1f;

        [ConfigField("Item Upgrade Count", "", 2)]
        public static int maxUpgradeCount;

        public override void Init()
        {
            base.Init();
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            var body = slot.characterBody;
            if (!body)
            {
                return false;
            }

            var inventory = body.inventory;
            if (!inventory)
            {
                return false;
            }

            var master = body.master;
            if (!master)
            {
                return false;
            }

            if (TryUpgrade(master, inventory))
            {
                // CharacterMasterNotificationQueue.SendTransformNotification(master, inventory.currentEquipmentIndex, EquipmentIndex.None, CharacterMasterNotificationQueue.TransformationType.Default);
                // throws
                inventory.SetEquipmentIndex(EquipmentIndex.None);
                return true;
            }

            return false;
        }

        private bool TryUpgrade(CharacterMaster master, Inventory inventory)
        {
            List<PickupIndex> greenItemList = new(Run.instance.availableTier2DropList);
            List<PickupIndex> redItemList = new(Run.instance.availableTier3DropList);
            List<ItemIndex> itemList = new(inventory.itemAcquisitionOrder);

            Util.ShuffleList(itemList, Run.instance.runRNG);

            int upgradeCount = 0;
            int item = 0;

            while (upgradeCount < maxUpgradeCount && item < itemList.Count)
            {
                var previousItem = ItemCatalog.GetItemDef(itemList[item]);
                ItemDef upgradedItem = null;
                List<PickupIndex> upgradeFromToList = null;

                switch (previousItem.tier)
                {
                    case ItemTier.Tier1:
                        upgradeFromToList = greenItemList;
                        break;

                    case ItemTier.Tier2:
                        upgradeFromToList = redItemList;
                        break;
                }

                if (upgradeFromToList != null && upgradeFromToList.Count > 0)
                {
                    Util.ShuffleList(upgradeFromToList, Run.instance.runRNG);
                    upgradeFromToList.Sort(CompareTags);
                    upgradedItem = ItemCatalog.GetItemDef(upgradeFromToList[0].itemIndex);
                }

                if (upgradedItem != null)
                {
                    if (inventory.GetItemCount(upgradedItem.itemIndex) == 0)
                    {
                        itemList.Add(upgradedItem.itemIndex);
                    }

                    upgradeCount++;

                    inventory.RemoveItem(previousItem.itemIndex, 1);
                    inventory.GiveItem(upgradedItem.itemIndex, 1);
                    CharacterMasterNotificationQueue.SendTransformNotification(master, previousItem.itemIndex, upgradedItem.itemIndex, CharacterMasterNotificationQueue.TransformationType.CloverVoid);
                }

                item++;

                int CompareTags(PickupIndex lhs, PickupIndex rhs)
                {
                    int num4 = 0;
                    int num5 = 0;
                    ItemDef itemDef2 = ItemCatalog.GetItemDef(lhs.itemIndex);
                    ItemDef itemDef3 = ItemCatalog.GetItemDef(rhs.itemIndex);
                    if (previousItem.ContainsTag(ItemTag.Damage))
                    {
                        if (itemDef2.ContainsTag(ItemTag.Damage))
                        {
                            num4 = 1;
                        }
                        if (itemDef3.ContainsTag(ItemTag.Damage))
                        {
                            num5 = 1;
                        }
                    }
                    if (previousItem.ContainsTag(ItemTag.Healing))
                    {
                        if (itemDef2.ContainsTag(ItemTag.Healing))
                        {
                            num4 = 1;
                        }
                        if (itemDef3.ContainsTag(ItemTag.Healing))
                        {
                            num5 = 1;
                        }
                    }
                    if (previousItem.ContainsTag(ItemTag.Utility))
                    {
                        if (itemDef2.ContainsTag(ItemTag.Utility))
                        {
                            num4 = 1;
                        }
                        if (itemDef3.ContainsTag(ItemTag.Utility))
                        {
                            num5 = 1;
                        }
                    }
                    return num5 - num4;
                }
            }
            if (upgradeCount > 0)
            {
                var bodyObject = master.bodyInstanceObject;
                if (bodyObject)
                {
                    Util.PlaySound("Play_item_proc_extraLife", bodyObject);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
*/