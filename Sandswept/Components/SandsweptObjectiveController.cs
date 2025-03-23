/*
using RoR2.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sandswept.Components
{
    public static class ObjectiveSystem
    {
        public static Xoroshiro128Plus objectiveRng;

        public static void Init()
        {
            LanguageAPI.Add("SANDSWEPT_OBJECTIVE_CHARGE_TP_EARLY", "Find and activate the <style=cDeath>Teleporter <sprite name=\"TP\" tint=1></style> within <style=cDeath>{0}s!</style>");
            LanguageAPI.Add("SANDSWEPT_OBJECTIVE_GET_UP_TO_X_ITEMS", "Get up to <style=cIsUtility>{0} items</style>.");
            On.RoR2.Stage.Start += Stage_Start;
            On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
        }

        private static void Inventory_GiveItem_ItemIndex_int(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            orig(self, itemIndex, count);
            var itemDef = ItemCatalog.GetItemDef(itemIndex);
            var master = self.GetComponent<CharacterMaster>();
            if (itemDef && !itemDef.hidden && itemDef.canRemove && master && master.TryGetComponent<SandsweptObjectiveController>(out var sandsweptObjectiveController))
            {
                sandsweptObjectiveController.currentItemCount++;
            }
        }

        // use unity events to listen when currentPickupCount >= maxPickupCount and currentTeleporterTime >= maxTeleporterTime (or vice versa)
        // and then remove the objective, like MoonBatteryMissionController do AND also grant a random reward type
        // there also needs to be reward ui and current/max counter for the item objective but like guhhhhhh :(( me when ui
        // there is probably no reason for GetRandomObjective to be a tuple anymore, or the whole ObjectiveType enum, but who knows lol, I was experimenting around and couldn't figure out another way, sorry

        public static bool IsActualInteractable(GameObject interactable)
        {
            if (interactable.TryGetComponent<InteractionProcFilter>(out var interactionProcFilter))
            {
                return interactionProcFilter.shouldAllowOnInteractionBeginProc;
            }
            if (interactable.GetComponent<VehicleSeat>())
            {
                return false;
            }
            if (interactable.GetComponent<NetworkUIPromptController>())
            {
                return true;
            }
            return false;
        }

        private static IEnumerator Stage_Start(On.RoR2.Stage.orig_Start orig, RoR2.Stage self)
        {
            yield return orig(self);
            foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController.instances)
            {
                if (pcmc.TryGetComponent<SandsweptObjectiveController>(out var sandsweep))
                {
                    objectiveRng = new(RoR2Application.rng);
                    sandsweep.GetRandomObjective(objectiveRng, self, pcmc.body);
                }
                else
                {
                    objectiveRng = new(RoR2Application.rng);
                    var sandswept = pcmc.AddComponent<SandsweptObjectiveController>();
                    sandswept.GetRandomObjective(objectiveRng, self, pcmc.body);
                }
            }
        }
    }

    public class SandsweptObjectiveController : MonoBehaviour
    {
        public int maxItemCount;
        public int currentItemCount;

        public float maxTeleporterTimer;
        public float currentTeleporterTimer;

        public int rerollCount = 0;
        public int maxRerollCount = 10;

        public CharacterBody body;
        public PlayerCharacterMasterController pcmc;
        public Inventory inventory;

        public bool addedChargeTPEarly = false;
        public bool addedGetUpToXItems = false;

        public string chargeTPEarlyToken = "SANDSWEPT_OBJECTIVE_CHARGE_TP_EARLY";
        public string getUpToXItemsToken = "SANDSWEPT_OBJECTIVE_GET_UP_TO_X_ITEMS";

        public enum ObjectiveType
        {
            ChargeTPEarly,
            GetUpToXItems
        }

        public enum RewardType
        {
            WhiteItem,
            GreenItem,
            RedItem,
            YellowItem,
            LunarItem,
            VoidItem,
            Equipment,
            LunarEquipment,
            // HealRevive -- fucking SOTS man
        }

        public enum EquipmentTier
        {
            Default,
            Lunar
        }

        public void Start()
        {
            inventory = GetComponent<Inventory>();
            pcmc = GetComponent<PlayerCharacterMasterController>();
            body = pcmc.body;
        }

        public void FixedUpdate()
        {
            if (currentTeleporterTimer >= 0f)
            {
                currentTeleporterTimer -= Time.fixedDeltaTime;
                // currentTeleporterTimer = Mathf.Max(0f, currentTeleporterTimer);
            }
        }

        public Tuple<float, ObjectiveType> GetRandomObjective(Xoroshiro128Plus rng, RoR2.Stage stage, CharacterBody body)
        {
            if (rerollCount >= maxRerollCount)
            {
                return null;
            }
            var randomTeleporterActivationTime = GetRandomTeleporterActivationTime(rng, stage, body);
            if (randomTeleporterActivationTime <= -1f && !addedChargeTPEarly)
            {
                GetRandomObjective(rng, stage, body);
                rerollCount++;

                Main.ModLogger.LogError("could not find teleporter, rerolling objective");

                // reroll, failed to find teleporter instance
            }
            else
            {
                maxTeleporterTimer = randomTeleporterActivationTime;
                currentTeleporterTimer = randomTeleporterActivationTime;
                addedChargeTPEarly = true;
                ObjectivePanelController.collectObjectiveSources += AddChargeTpEarlyObjective;
                return new Tuple<float, ObjectiveType>(randomTeleporterActivationTime, ObjectiveType.ChargeTPEarly);
            }

            var randomItemCount = GetRandomPickupCount();
            if (randomItemCount != -1 && !addedGetUpToXItems)
            {
                GetRandomObjective(rng, stage, body);
                rerollCount++;

                Main.ModLogger.LogError("could not find purchaseinteractions, rerolling objective");

                // reroll, failed to find objects with a purchase interaction component
            }
            else
            {
                addedGetUpToXItems = true;
                maxItemCount = randomItemCount;
                currentItemCount = 0;
                ObjectivePanelController.collectObjectiveSources += AddGetUpToXPickupsObjective;
                return new Tuple<float, ObjectiveType>(randomItemCount, ObjectiveType.ChargeTPEarly);
            }

            return null;
        }

        private void AddChargeTpEarlyObjective(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
        {
            if (currentTeleporterTimer > 0f)
            {
                objectiveSourcesList.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                {
                    master = master,
                    objectiveType = typeof(ChargeTPEarlyObjectiveTracker),
                    source = this
                });
            }
        }

        private void AddGetUpToXPickupsObjective(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
        {
            objectiveSourcesList.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
            {
                master = master,
                objectiveType = typeof(GetUpToXPickupsObjectiveTracker),
                source = this
            });
        }

        public float GetRandomTeleporterActivationTime(Xoroshiro128Plus rng, RoR2.Stage stage, CharacterBody body)
        {
            var hasTp = TeleporterInteraction.instance;
            if (!hasTp)
            {
                return -99f;
            }
            var randomTime = rng.RangeFloat(180f, 240f);
            return randomTime;
        }

        public int GetRandomPickupCount()
        {
            var purchaseInteractions = InstanceTracker.GetInstancesList<PurchaseInteraction>().Count;
            return purchaseInteractions > 2 ? Mathf.RoundToInt(purchaseInteractions * 0.8f) : -1;
            // 2 * 0.8 = 1.6 = 2 rounded so ehh
            // 3 makes you sacrifice at least 1 item
            // a multiplier of purchase interaction count makes it better on lower interactable credit stages but muuch worse on higher ones
            // like 15 purchase interactions become... > get up to 12 items
            // technically could still be worth it for like a band or atg or some red but probably very unviable on smth like abyssal depths or sky meadow
        }

        public void GrantRewards(int count, CharacterBody body, RewardType rewardType)
        {
            switch (rewardType)
            {
                case RewardType.WhiteItem:
                    GiveRandomTieredItems(count, ItemTier.Tier1);
                    break;

                case RewardType.GreenItem:
                    GiveRandomTieredItems(count, ItemTier.Tier2);
                    break;

                case RewardType.RedItem:
                    GiveRandomTieredItems(count, ItemTier.Tier3);
                    break;

                case RewardType.LunarItem:
                    GiveRandomTieredItems(count, ItemTier.Lunar);
                    break;

                case RewardType.YellowItem:
                    GiveRandomTieredItems(count, ItemTier.Boss);
                    break;

                case RewardType.VoidItem:
                    GiveRandomTieredItems(count, ItemTier.VoidTier1);
                    break;

                case RewardType.Equipment:
                    GiveRandomEquipment(EquipmentTier.Default);
                    break;

                case RewardType.LunarEquipment:
                    GiveRandomEquipment(EquipmentTier.Lunar);
                    break;
            }
        }

        public void GiveRandomTieredItems(int count, ItemTier tier)
        {
            WeightedSelection<List<PickupIndex>> weightedSelection = new(8);
            List<PickupIndex> voidItems = new();
            voidItems.Concat(Run.instance.availableVoidTier1DropList);
            voidItems.Concat(Run.instance.availableVoidTier2DropList);
            voidItems.Concat(Run.instance.availableVoidTier3DropList);
            voidItems.Concat(Run.instance.availableVoidBossDropList);

            List<PickupIndex> pickups = tier switch
            {
                ItemTier.Tier1 => Run.instance.availableTier1DropList,
                ItemTier.Tier2 => Run.instance.availableTier2DropList,
                ItemTier.Tier3 => Run.instance.availableTier3DropList,
                ItemTier.Boss => Run.instance.availableTier3DropList,
                ItemTier.Lunar => Run.instance.availableLunarItemDropList,
                ItemTier.VoidTier1 => voidItems,
                _ => Run.instance.availableTier1DropList
            };

            weightedSelection.AddChoice(pickups, 100f);

            for (int i = 0; i < count; i++)
            {
                List<PickupIndex> list = weightedSelection.Evaluate(Random.value);
                var pickupDef = PickupCatalog.GetPickupDef(list[Random.Range(0, list.Count)]);
                inventory.GiveItem((pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None, 1);
            }
        }

        public void GiveRandomEquipment(EquipmentTier tier)
        {
            if (tier != EquipmentTier.Default)
            {
                GiveRandomLunarEquipment();
                return;
            }
            inventory.GiveRandomEquipment();
        }

        public void GiveRandomLunarEquipment()
        {
            if (NetworkServer.active)
            {
                var pickupIndex = Run.instance.availableLunarEquipmentDropList[Random.Range(0, Run.instance.availableLunarEquipmentDropList.Count)];
                var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                inventory.SetEquipmentIndex((pickupDef != null) ? pickupDef.equipmentIndex : EquipmentIndex.None);
            }
        }

        public void OnDisable()
        {
            ObjectivePanelController.collectObjectiveSources -= AddChargeTpEarlyObjective;
            ObjectivePanelController.collectObjectiveSources -= AddGetUpToXPickupsObjective;
        }
    }

    public class ChargeTPEarlyObjectiveTracker : ObjectivePanelController.ObjectiveTracker
    {
        public float currentTeleporterTimer = -1;

        public override string GenerateString()
        {
            var SandsweptObjectiveController = (SandsweptObjectiveController)sourceDescriptor.source;
            currentTeleporterTimer = SandsweptObjectiveController.currentTeleporterTimer;
            return string.Format(Language.GetString(SandsweptObjectiveController.chargeTPEarlyToken), currentTeleporterTimer);
        }

        public override bool IsDirty()
        {
            return ((SandsweptObjectiveController)this.sourceDescriptor.source).currentTeleporterTimer != currentTeleporterTimer;
        }
    }

    public class GetUpToXPickupsObjectiveTracker : ObjectivePanelController.ObjectiveTracker
    {
        public int currentItemCount = -1;

        public override string GenerateString()
        {
            var SandsweptObjectiveController = (SandsweptObjectiveController)sourceDescriptor.source;
            currentItemCount = SandsweptObjectiveController.currentItemCount;
            return string.Format(Language.GetString(SandsweptObjectiveController.getUpToXItemsToken), currentItemCount);
        }

        public override bool IsDirty()
        {
            return ((SandsweptObjectiveController)this.sourceDescriptor.source).currentItemCount != currentItemCount;
        }
    }
}
*/