using System.Collections;
using System.Linq;

namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Hallowed Ichor")]
    public class HallowedIchor : ItemBase<HallowedIchor>
    {
        public override string ItemName => "Hallowed Ichor";

        public override string ItemLangTokenName => "HALLOWED_ICHOR";

        public override string ItemPickupDesc => "Enemies are more numerous and more often elite. Chests can be re-opened additional times, but attract even more enemies and elites permanently.";

        public override string ItemFullDescription => $"Enemies are $su{baseCombatDirectorCreditMultiplierGain * 100f}%$se more numerous and more often elite. Chests can be $sure-opened {baseExtraChestInteractions}$se $ss(+{stackExtraChestInteractions} per stack)$se additional times, but attract $su{baseOnChestReopenCombatDirectorCreditMultiplierAndEliteBiasGain * 100f}%$se $ss(+{stackOnChestReopenCombatDirectorCreditMultiplierAndEliteBiasGain * 100f}% per stack)$se more enemies and elites $supermanently$se.".AutoFormat();

        public override string ItemLore => "A large wine glass with a blue liquid inside it, its handle resembling a cross shape. Supposed to be a terrariar reference somewhat. You can do whatever with it";

        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => Main.Assets.LoadAsset<GameObject>("PickupTheirProminence.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texTheirProminence.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.InteractableRelated, ItemTag.AIBlacklist };

        [ConfigField("Base Combat Director Credit Multiplier And Elite Bias Gain", "Decimal.", 0.5f)]
        public static float baseCombatDirectorCreditMultiplierGain;

        [ConfigField("Base Extra Chest Interactions", "", 1)]
        public static int baseExtraChestInteractions;

        [ConfigField("Stack Extra Chest Interactions", "", 1)]
        public static int stackExtraChestInteractions;

        [ConfigField("Base On Chest Re-open Combat Director Credit Multiplier And Elite Bias Gain", "Decimal.", 0.2f)]
        public static float baseOnChestReopenCombatDirectorCreditMultiplierAndEliteBiasGain;

        [ConfigField("Stack On Chest Re-open Combat Director Credit Multiplier And Elite Bias Gain", "Decimal.", 0.2f)]
        public static float stackOnChestReopenCombatDirectorCreditMultiplierAndEliteBiasGain;

        public float cachedCombatDirectorCreditMultiplier = -999f;

        public float cachedCombatDirectorEliteBias = -999f;

        public static GameObject permanentHallowedIchorTracker;

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
            On.RoR2.CombatDirector.Awake += CombatDirector_Awake;
        }

        private void CombatDirector_Awake(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            if (cachedCombatDirectorCreditMultiplier < -998f)
            {
                cachedCombatDirectorCreditMultiplier = self.creditMultiplier;
            }

            if (cachedCombatDirectorEliteBias < -998f)
            {
                cachedCombatDirectorEliteBias = self.eliteBias;
            }

            orig(self);
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            itemCount = Util.GetItemCountGlobal(instance.ItemDef.itemIndex, false, true);
            if (itemCount <= 0)
            {
                return;
            }

            permanentHallowedIchorTracker.GetComponent<HallowedIchorController>().Recalculate(itemCount);

            if (permanentHallowedIchorTracker.TryGetComponent<HallowedIchorController>(out var hallowedIchorController))
            {
                hallowedIchorController.Recalculate(itemCount);

                foreach (CombatDirector combatDirector in CombatDirector.instancesList)
                {
                    combatDirector.creditMultiplier = cachedCombatDirectorCreditMultiplier + (hallowedIchorController.totalIncreases - 1f);
                    combatDirector.eliteBias = cachedCombatDirectorEliteBias + (hallowedIchorController.totalIncreases - 1f);
                }
            }
        }

        private void GlobalEventManager_OnInteractionsGlobal(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            if (interactor.TryGetComponent<CharacterBody>(out var interactorBody))
            {
                var stack = GetCount(interactorBody);
                if (stack <= 0)
                {
                    return;
                }

                if (interactableObject.TryGetComponent<PurchaseInteraction>(out var purchaseInteraction))
                {
                    if (!purchaseInteraction)
                    {
                        return;
                    }

                    if (!IsChest(purchaseInteraction))
                    {
                        return;
                    }

                    var maxPurchases = 1 + baseExtraChestInteractions + stackExtraChestInteractions * (stack - 1);

                    var chestPurchaseCounter = interactableObject.GetComponent<ChestPurchaseCounter>() ? interactableObject.GetComponent<ChestPurchaseCounter>() : interactableObject.AddComponent<ChestPurchaseCounter>();
                    chestPurchaseCounter.openedCount++;

                    if (chestPurchaseCounter.openedCount >= maxPurchases)
                    {
                        return;
                    }

                    purchaseInteraction.StartCoroutine(SetRepurchaseAsAvailable(interactableObject));
                }
            }
        }

        public IEnumerator SetRepurchaseAsAvailable(GameObject interactableObject)
        {
            yield return new WaitForSeconds(0.25f);

            var chestBehavior = interactableObject.GetComponent<ChestBehavior>();
            if (!chestBehavior)
            {
                Main.ModLogger.LogError("no chestbehavior found");
                yield break;
            }

            var entityStateMachine = interactableObject.GetComponent<EntityStateMachine>();
            if (!entityStateMachine)
            {
                Main.ModLogger.LogError("no esm found");
                yield break;
            }

            entityStateMachine.SetNextState(new EntityStates.Barrel.Closing());

            // play vfx here ig
            yield return new WaitForSeconds(0.25f);
            // or here

            var purchaseInteraction = interactableObject.GetComponent<PurchaseInteraction>();
            if (!purchaseInteraction)
            {
                Main.ModLogger.LogError("no purchaseinteraction found");
                yield break;
            }

            purchaseInteraction.SetAvailableTrue();
            chestBehavior.NetworkisChestOpened = false;
            // idk if this is necessary but just in case >_<
        }

        private bool IsChest(PurchaseInteraction purchaseInteraction)
        {
            if (!purchaseInteraction)
            {
                return false;
            }
            return purchaseInteraction.displayNameToken.StartsWith("CHEST") || purchaseInteraction.displayNameToken.StartsWith("CATEGORYCHEST");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }

    public class ChestPurchaseCounter : MonoBehaviour
    {
        public int openedCount = 0;
    }

    public class HallowedIchorController : MonoBehaviour
    {
        public float totalIncreases = 0f;

        public void Recalculate(int itemCount)
        {
            Main.ModLogger.LogError("hallowedichorcontroller recalculate called");
            totalIncreases = 1f + HallowedIchor.baseCombatDirectorCreditMultiplierGain;
            var finalIncrease = HallowedIchor.baseOnChestReopenCombatDirectorCreditMultiplierAndEliteBiasGain + HallowedIchor.stackOnChestReopenCombatDirectorCreditMultiplierAndEliteBiasGain * (itemCount - 1);
            for (int i = 0; i < itemCount; i++)
            {
                totalIncreases *= finalIncrease;
            }
        }
    }
}