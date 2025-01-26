using System.Collections;
using System.Linq;

namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Hallowed Ichor")]
    public class HallowedIchor : ItemBase<HallowedIchor>
    {
        public override string ItemName => "Hallowed Ichor";

        public override string ItemLangTokenName => "HALLOWED_ICHOR";

        public override string ItemPickupDesc => "Chests can be re-opened additional times, but scale the difficulty even faster.";

        public override string ItemFullDescription => $"Chests can be $sure-opened {baseExtraChestInteractions}$se $ss(+{stackExtraChestInteractions} per stack)$se additional times, but scale the difficulty $su{chestReopenDifficultyCoefficientMultiplierAdd * 100f}%$se faster.".AutoFormat();

        public override string ItemLore => "A large wine glass with a blue liquid inside it, its handle resembling a cross shape. Supposed to be a terrariar reference somewhat. You can do whatever with it";

        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => Main.Assets.LoadAsset<GameObject>("PickupTheirProminence.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texTheirProminence.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.InteractableRelated, ItemTag.AIBlacklist };

        [ConfigField("Base Extra Chest Interactions", "", 1)]
        public static int baseExtraChestInteractions;

        [ConfigField("Stack Extra Chest Interactions", "", 1)]
        public static int stackExtraChestInteractions;

        [ConfigField("Chest Re-open Difficulty Coefficient Flat Add", "Adds to the current difficulty scaling value each time a chest is re-opened. This is calculated first.", 0.15f)]
        public static float chestReopenDifficultyCoefficientFlatAdd;

        [ConfigField("Chest Re-open Difficulty Coefficient Multiplier Add", "Multiplies the current difficulty value by 1 + this value each time a chest is re-opened. This is calculated last.", 0.08f)]
        public static float chestReopenDifficultyCoefficientMultiplierAdd;

        public static GameObject permanentHallowedIchorTracker;

        public static float cachedDifficultyDefScalingValue = -696912345f;

        public static int itemCount = 0;

        public override void Init(ConfigFile config)
        {
            permanentHallowedIchorTracker = new GameObject("Hallowed Ichor Tracker", typeof(SetDontDestroyOnLoad), typeof(HallowedIchorController));

            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            GlobalEventManager.OnInteractionsGlobal += GlobalEventManager_OnInteractionsGlobal;
            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
        }

        private void Run_onRunDestroyGlobal(Run run)
        {
            var runDifficultyDef = DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty);
            runDifficultyDef.scalingValue = cachedDifficultyDefScalingValue;
            // just in case idk
        }

        private void Run_onRunStartGlobal(Run run)
        {
            var runDifficultyDef = DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty);
            cachedDifficultyDefScalingValue = runDifficultyDef.scalingValue;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            itemCount = Util.GetItemCountGlobal(instance.ItemDef.itemIndex, true);
        }

        private void GlobalEventManager_OnInteractionsGlobal(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            if (itemCount <= 0)
            {
                return;
            }

            if (interactableObject.TryGetComponent<PurchaseInteraction>(out var purchaseInteraction))
            {
                if (!IsChest(purchaseInteraction))
                {
                    return;
                }

                var maxPurchases = 1 + baseExtraChestInteractions + stackExtraChestInteractions * (itemCount - 1);

                var hallowedIchorChestController = interactableObject.GetComponent<HallowedIchorChestController>() ? interactableObject.GetComponent<HallowedIchorChestController>() : interactableObject.AddComponent<HallowedIchorChestController>();
                // Main.ModLogger.LogError("hallowed ichor chest controller is " + hallowedIchorChestController);
                // Main.ModLogger.LogError("opened count BEFORE is " + hallowedIchorChestController.openedCount);
                hallowedIchorChestController.openedCount++;
                // Main.ModLogger.LogError("opened count AFTREERERERR is " + hallowedIchorChestController.openedCount);

                if (hallowedIchorChestController.openedCount > 1)
                {
                    // Main.ModLogger.LogError("opened count is more than 1");
                    permanentHallowedIchorTracker.GetComponent<HallowedIchorController>().Recalculate();
                }

                var chestBehavior = interactableObject.GetComponent<ChestBehavior>();
                if (!chestBehavior)
                {
                    // Main.ModLogger.LogError("couldnt get chest behavior");
                    return;
                }

                if (hallowedIchorChestController.canDropItem)
                {
                    // Main.ModLogger.LogError("trying to drop extra item");
                    chestBehavior.RollItem();
                }

                if (hallowedIchorChestController.openedCount >= maxPurchases)
                {
                    // Main.ModLogger.LogError("opened count is more than equal to max purchases, returning, cant drop item");
                    hallowedIchorChestController.canDropItem = false;
                    return;
                }

                purchaseInteraction.StartCoroutine(SetRepurchaseAsAvailable(interactableObject, hallowedIchorChestController));
            }
        }

        private bool IsChest(PurchaseInteraction purchaseInteraction)
        {
            return purchaseInteraction.displayNameToken.StartsWith("CHEST") || purchaseInteraction.displayNameToken.StartsWith("CATEGORYCHEST");
        }

        public IEnumerator SetRepurchaseAsAvailable(GameObject interactableObject, HallowedIchorChestController hallowedIchorChestController)
        {
            // Main.ModLogger.LogError("setrepurchaseasavailable called");
            yield return new WaitForSeconds(EntityStates.Barrel.Opening.duration);

            var chestBehavior = interactableObject.GetComponent<ChestBehavior>();
            if (!chestBehavior)
            {
                // Main.ModLogger.LogError("no chestbehavior found");
                yield break;
            }

            var entityStateMachine = interactableObject.GetComponent<EntityStateMachine>();
            if (!entityStateMachine)
            {
                // Main.ModLogger.LogError("no esm found");
                yield break;
            }

            entityStateMachine.SetNextState(new EntityStates.Barrel.Closing());

            yield return new WaitForSeconds(EntityStates.Barrel.Closing.duration / 2f);
            var effectData = new EffectData()
            {
                rotation = Quaternion.identity,
                origin = interactableObject.transform.position,
                start = interactableObject.transform.position,
                scale = 3f
            };
            EffectManager.SpawnEffect(Paths.GameObject.LightningFlash, effectData, true);
            Util.PlaySound("Play_UI_item_land_command", interactableObject);
            Util.PlaySound("Play_UI_item_land_command", interactableObject);
            yield return new WaitForSeconds(EntityStates.Barrel.Closing.duration / 2f);
            EffectManager.SpawnEffect(Paths.GameObject.LightningFlash, effectData, true);

            var purchaseInteraction = interactableObject.GetComponent<PurchaseInteraction>();
            if (!purchaseInteraction)
            {
                // Main.ModLogger.LogError("no purchaseinteraction found");
                yield break;
            }

            purchaseInteraction.SetAvailableTrue();
            // idk if this is necessary but just in case >_<
            chestBehavior.NetworkisChestOpened = false;
            // idk if this is necessary but just in case >_<
            hallowedIchorChestController.canDropItem = true;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }

    public class HallowedIchorChestController : MonoBehaviour
    {
        public int openedCount = 0;
        public bool canDropItem = false;
    }

    public class HallowedIchorController : MonoBehaviour
    {
        public void Recalculate()
        {
            if (!Run.instance)
            {
                return;
            }

            var flatIncrease = HallowedIchor.chestReopenDifficultyCoefficientFlatAdd;
            var multiplier = 1f + HallowedIchor.chestReopenDifficultyCoefficientMultiplierAdd;

            Run.ambientLevelCap = int.MaxValue;

            var runDifficultyDef = DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty);
            runDifficultyDef.scalingValue += flatIncrease;
            runDifficultyDef.scalingValue *= multiplier;
        }
    }
}