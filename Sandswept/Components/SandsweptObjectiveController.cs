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
            LanguageAPI.Add("SANDSWEPT_OBJECTIVE_CHARGE_TP_EARLY", "Find and activate the <style=cDeath>Teleporter <sprite name=\"TP\" tint=1></style> within <style=cDeath>{0}s!</style>");
            LanguageAPI.Add("SANDSWEPT_OBJECTIVE_GET_UP_TO_X_PICKUPS", "Find up to <style=cIsUtility>{0} pickups</style>");
            On.RoR2.Stage.Start += Stage_Start;
            GlobalEventManager.OnInteractionsGlobal += GlobalEventManager_OnInteractionsGlobal;
        }

        // use unity events to listen when currentPickupCount >= maxPickupCount and currentTeleporterTime >= maxTeleporterTime (or vice versa)
        // and then remove the objective, like MoonBatteryMissionController do AND also grant a random reward type
        // reward count should prolly scale with stages completed

        private static void GlobalEventManager_OnInteractionsGlobal(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            if (!interactor)
            {
                return;
            }
            var body = interactor.GetComponent<CharacterBody>();
            if (!body)
            {
                return;
            }

            var master = body.master;
            if (!master)
            {
                return;
            }

            if (IsActualInteractable(interactableObject))
            {
                return;
            }

            if (master.TryGetComponent<SandsweptObjectiveController>(out var sandsweptObjectiveController))
            {
                sandsweptObjectiveController.currentPickupCount++;
            }
        }

        public static bool IsActualInteractable(GameObject interactable)
        {
            if (interactable.TryGetComponent<InteractionProcFilter>(out var interactionProcFilter))
            {
                return interactionProcFilter.shouldAllowOnInteractionBeginProc;
            }
            if (interactable.GetComponent<VehicleSeat>())
            {
                return true;
            }
            if (interactable.GetComponent<NetworkUIPromptController>())
            {
                return true;
            }
            return false;
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

        public CharacterBody body;
        public PlayerCharacterMasterController pcmc;
        public Inventory inventory;

        public bool addedChargeTPEarly = false;
        public bool addedGetUpToXPickups = false;

        public string chargeTPEarlyToken = "SANDSWEPT_OBJECTIVE_CHARGE_TP_EARLY";
        public string getUpToXPickupsToken = "SANDSWEPT_OBJECTIVE_GET_UP_TO_X_PICKUPS";

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

            var randomPickupCount = GetRandomPickupCount();
            if (randomPickupCount == -1 && !addedGetUpToXPickups)
            {
                GetRandomObjective(rng, stage, body);
                rerollCount++;
                // reroll, failed to find objects with a purchase interaction component
            }
            else
            {
                addedGetUpToXPickups = true;
                maxPickupCount = randomPickupCount;
                currentPickupCount = 0;
                ObjectivePanelController.collectObjectiveSources += AddGetUpToXPickupsObjective;
                return new Tuple<float, ObjectiveType>(randomPickupCount, ObjectiveType.ChargeTPEarly);
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
        public int currentPickupCount = -1;

        public override string GenerateString()
        {
            var SandsweptObjectiveController = (SandsweptObjectiveController)sourceDescriptor.source;
            currentPickupCount = SandsweptObjectiveController.currentPickupCount;
            return string.Format(Language.GetString(SandsweptObjectiveController.getUpToXPickupsToken), currentPickupCount);
        }

        public override bool IsDirty()
        {
            return ((SandsweptObjectiveController)this.sourceDescriptor.source).currentPickupCount != currentPickupCount;
        }
    }
}