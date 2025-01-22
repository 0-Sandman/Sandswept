using UnityEngine.SceneManagement;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Universal VIP Pass")]
    public class UniversalVIPPass : ItemBase<UniversalVIPPass>
    {
        public override string ItemName => "Universal VIP Pass";

        public override string ItemLangTokenName => "VIP_PASS";

        public override string ItemPickupDesc => "Category chests have a chance to drop multiple items.";

        public override string ItemFullDescription => $"$suCategory chests$se have a $su{chance}%$se chance of dropping $su{baseExtraItems}$se $ss(+{stackExtraItems} per stack)$se $suextra items$se.".AutoFormat();

        public override string ItemLore => "\"I'm sorry, sir, this is a restricted area. We can't allow you in.\"\r\n\r\n\"Special orders from the UES. It would be in your best interests to make an exception.\"\r\n\r\n\"I'm afraid I can't do that. We've received specific instruction from two of our guests not to let you in. It would be against our policy to betray their trust in our services.\"\r\n\r\n\"Those two 'guests' have stolen from the UESC. You are harboring criminals. If you do not let us in, you will be obstructing justice in violation of interplanetary law.\"\r\n\r\n\"The UESC does not have legal jurisdiction over Pluto, sir. We are under no obligation to allow you in. If you do not vacate the premises, I will be forced to call security, and make no mistake, our security is the best of the best.\"\r\n\r\n...\r\n\r\n\"...is that...?\"\r\n\r\n\"Yes. Universal. This is serious business. I'll ask one more time: let us through.\"\r\n\r\n\"...right away, sir.\"";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("UniVIPPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("UniVIPIcon.png");

        public override bool nonstandardScaleModel => true;

        [ConfigField("Chance", "", 60f)]
        public static float chance;

        [ConfigField("Base Extra Items", "", 1)]
        public static int baseExtraItems;

        [ConfigField("Stack Extra Items", "", 1)]
        public static int stackExtraItems;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.InteractableRelated, ItemTag.AIBlacklist, ItemTag.CannotDuplicate };

        public override void Init(ConfigFile config)
        {
            var uniVip = Main.MainAssets.LoadAsset<GameObject>("UniVIPPrefab.prefab");
            var uniVipMat = uniVip.transform.GetChild(0).GetComponent<MeshRenderer>().material;
            uniVipMat.SetColor("_Color", new Color32(205, 205, 205, 249));

            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            // SceneDirector.onPostPopulateSceneServer += SceneDirector_onPostPopulateSceneServer;
            On.RoR2.GlobalEventManager.OnInteractionBegin += GlobalEventManager_OnInteractionBegin;
            // Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
        }

        private void Run_onRunDestroyGlobal(Run run)
        {
            totalCategoryChests = 0;
        }

        public static int totalCategoryChests = 0;

        private void SceneDirector_onPostPopulateSceneServer(SceneDirector sd)
        {
            int currentStageCategoryChests = 0;
            var purchaseInteractions = GameObject.FindObjectsOfType<PurchaseInteraction>();
            for (int i = 0; i < purchaseInteractions.Length; i++)
            {
                var purchaseInteraction = purchaseInteractions[i];
                if (purchaseInteraction.displayNameToken.ToLower().Contains("category") || purchaseInteraction.contextToken.ToLower().Contains("category"))
                {
                    totalCategoryChests++;
                    currentStageCategoryChests++;
                }
            }

            Main.ModLogger.LogError("Stage: " + SceneManager.GetActiveScene().name);
            Main.ModLogger.LogError("Category chests on stage: " + currentStageCategoryChests);
            Main.ModLogger.LogError("Category chests this run: " + totalCategoryChests);
            Main.ModLogger.LogError("You would get this many items on average with a chance of " + chance + "%: " + (totalCategoryChests * (chance / 100f)));
        }

        private void GlobalEventManager_OnInteractionBegin(On.RoR2.GlobalEventManager.orig_OnInteractionBegin orig, GlobalEventManager self, Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            bool isCategoryChest = false;
            var chestBehavior = interactableObject.GetComponent<ChestBehavior>();
            if (chestBehavior)
            {
                var purchaseInteraction = interactableObject.GetComponent<PurchaseInteraction>();
                if (purchaseInteraction)
                {
                    isCategoryChest = isCategoryChest || purchaseInteraction.displayNameToken.ToLower().Contains("category") || purchaseInteraction.contextToken.ToLower().Contains("category");
                }
                var genericDisplayNameProvider = interactableObject.GetComponent<GenericDisplayNameProvider>();
                if (genericDisplayNameProvider)
                {
                    isCategoryChest = isCategoryChest || genericDisplayNameProvider.displayToken.ToLower().Contains("category");
                }

                if (isCategoryChest)
                {
                    if (interactor)
                    {
                        var interactorBody = interactor.GetComponent<CharacterBody>();
                        if (interactorBody)
                        {
                            var stack = GetCount(interactorBody);
                            var scale = 0.5f + Run.instance.participatingPlayerCount * 0.5f;
                            if (stack > 0 && Util.CheckRoll(chance / scale))
                            {
                                var isCategoryChestFinal = interactableObject.name.ToLower().Contains("category") || isCategoryChest;
                                if (isCategoryChestFinal)
                                {
                                    var extraItemCount = baseExtraItems + stackExtraItems * (stack - 1);

                                    chestBehavior.dropCount = 1 + extraItemCount;

                                    Util.PlaySound("Play_UI_commandHUD_select", chestBehavior.gameObject);
                                    Util.PlaySound("Play_UI_commandHUD_select", chestBehavior.gameObject);

                                    if (Random.Range(0f, 100f) >= 96f)
                                    {
                                        Chat.AddMessage("<style=cIsDamage>Developer 1</style>: Universal VIP Paws :3 x3 OwO UwU :3 <3");
                                        Chat.AddMessage("<style=cIsUtility>Developer 2</style>: What???");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            orig(self, interactor, interactable, interactableObject);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}