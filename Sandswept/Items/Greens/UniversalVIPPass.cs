namespace Sandswept.Items.Greens
{
    public class UniversalVIPPass : ItemBase<UniversalVIPPass>
    {
        public class PassBehavior : MonoBehaviour
        {
            public float storedTotal = 0f;
        }

        public override string ItemName => "Universal VIP Pass";

        public override string ItemLangTokenName => "VIP_PASS";

        public override string ItemPickupDesc => "Store a portion of spent gold as a bonus on the next stage.";

        public override string ItemFullDescription => "Whenever you make a $sugold purchase$se, store $su15%$se $ss(+10% per stack)$se of the spent gold as $sucredit$se. $suReceive money$se equal to $sucredit$se on the next stage.".AutoFormat();

        public override string ItemLore => "Funny pt.2";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("UniVIPPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("UniVIPIcon.png");

        public override bool AIBlacklisted => true;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            GlobalEventManager.OnInteractionsGlobal += GlobalEventManager_OnInteractionsGlobal;
            Stage.onStageStartGlobal += Stage_onStageStartGlobal;
        }

        private void GlobalEventManager_OnInteractionsGlobal(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            var purchaseInteraction = interactableObject.GetComponent<PurchaseInteraction>();
            if (!purchaseInteraction)
            {
                return;
            }

            if (purchaseInteraction.costType != CostTypeIndex.Money)
            {
                return;
            }

            if (!interactor)
            {
                return;
            }

            var passBehavior = interactor.GetComponent<PassBehavior>();
            if (!passBehavior)
            {
                return;
            }

            var interactorBody = interactor.GetComponent<CharacterBody>();
            if (!interactorBody)
            {
                return;
            }

            var stack = GetCount(interactorBody);
            if (stack > 0)
            {
                passBehavior.storedTotal += purchaseInteraction.cost * Util.ConvertAmplificationPercentageIntoReductionPercentage(0.15f + 0.10f * (stack - 1));
            }
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            var stack = GetCount(body);
            if (stack > 0 && !body.GetComponent<PassBehavior>())
            {
                body.AddComponent<PassBehavior>();
            }
            else if (stack <= 0 && body.GetComponent<PassBehavior>())
            {
                body.RemoveComponent<PassBehavior>();
            }
        }

        private void Stage_onStageStartGlobal(Stage obj)
        {
            foreach (CharacterBody body in CharacterBody.instancesList)
            {
                var passBehavior = body.GetComponent<PassBehavior>();
                if (!passBehavior)
                {
                    return;
                }

                var stack = GetCount(body);
                if (stack <= 0)
                {
                    body.RemoveComponent<PassBehavior>();
                }

                var master = body.master;
                if (!master)
                {
                    return;
                }

                master.GiveMoney((uint)passBehavior.storedTotal);
                passBehavior.storedTotal = 0;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}