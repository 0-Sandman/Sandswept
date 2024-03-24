using RoR2.UI;
using System;
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
            On.RoR2.Stage.Start += Stage_Start;
        }

        private static void Stage_Start(On.RoR2.Stage.orig_Start orig, RoR2.Stage self)
        {
            orig(self);
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
        public int maxPickupCount;
        public int currentPickupCount;

        public float maxTeleporterTimer;
        public float currentTeleporterTimer;

        public int rerollCount = 0;
        public int maxRerollCount = 10;

        public Inventory inventory;

        public enum ObjectiveType
        {
            ChargeTPEarly,
            GetUpToXPickups
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
            HealRevive
        }

        public enum EquipmentTier
        {
            Default,
            Lunar
        }

        public void Start()
        {
            inventory = GetComponent<Inventory>();
        }

        private void OnCollectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
        {
            objectiveSourcesList.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
            {
                master = master,
                objectiveType = typeof(),
                source = this
            });
        }

        public Tuple<float, ObjectiveType> GetRandomObjective(Xoroshiro128Plus rng, RoR2.Stage stage, CharacterBody body)
        {
            if (rerollCount >= maxRerollCount)
            {
                return null;
            }
            var randomTeleporterActivationTime = GetRandomTeleporterActivationTime(rng, stage, body);
            if (randomTeleporterActivationTime <= -1f)
            {
                GetRandomObjective(rng, stage, body);
                rerollCount++;
                // reroll, failed to find teleporter instance
            }
            else
            {
                return new Tuple<float, ObjectiveType>(randomTeleporterActivationTime, ObjectiveType.ChargeTPEarly);
            }

            var randomPickupCount = GetRandomPickupCount();
            if (randomPickupCount == -1)
            {
                GetRandomObjective(rng, stage, body);
                rerollCount++;
                // reroll, failed to find objects with a purchase interaction component
            }
            else
            {
                return new Tuple<float, ObjectiveType>(randomPickupCount, ObjectiveType.ChargeTPEarly);
            }

            return null;
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
            var purchaseInteractions = FindObjectsOfType<PurchaseInteraction>().Length;
            return purchaseInteractions > 0 ? purchaseInteractions : -1;
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
            ObjectivePanelController.collectObjectiveSources -= OnCollectObjectiveSources;
        }
    }
}