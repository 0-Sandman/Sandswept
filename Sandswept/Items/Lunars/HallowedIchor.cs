/*
using MonoMod.RuntimeDetour;
using RoR2.UI;
using System.Collections;
using System.Linq;

namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Hallowed Ichor")]
    public class HallowedIchor : ItemBase<HallowedIchor>
    {
        public override string ItemName => "Hallowed Ichor";

        public override string ItemLangTokenName => "HALLOWED_ICHOR";

        public override string ItemPickupDesc => "Chests may be reopened an additional time... <color=#FF7F7F>BUT reopening them increases time scaling permanently.</color>";

        public override string ItemFullDescription => $"Chests may be $sureopened {baseExtraChestInteractions}$se $ss(+{stackExtraChestInteractions} per stack)$se additional times, but $sureopening$se them increases $sutime scaling$se by $su{1f + (chestReopenDifficultyCoefficientMultiplierAdd * 3f)}x$se permanently.".AutoFormat(); // this will be inaccurate no matter what but this is somewhat accurate for the first use on monsoon lmao

        public override string ItemLore => "This will be the most potent creation I give to you, my servant. I have weaved many of these artifacts for vermin like you, but this one was far more costly than the others. It will be given only to a scarce few of my most dedicated servants. If used properly, it will be worth the price spent constructing it.\r\n\r\nIt is my blood, held in a vessel of my design. Superior blood. It grants me — along with my treacherous brother — the power to shape the compounds. A transfusion is sufficient to enable its effects. It cannot provide the extent of my abilities in such limited quantity, but it will allow you to create more of the trinkets you cling to so tightly.\r\n\r\nIts use will not go unnoticed, however. He will sense its presence, and send his vermin to hunt you down. Use its power to create suitable weapons and avoid encountering him in the flesh, and I do not suspect you will have any issue dispatching your pursuers.";

        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => Main.Assets.LoadAsset<GameObject>("PickupTheirProminence.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texTheirProminence.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.InteractableRelated, ItemTag.AIBlacklist };

        [ConfigField("Base Extra Chest Interactions", "", 1)]
        public static int baseExtraChestInteractions;

        [ConfigField("Stack Extra Chest Interactions", "", 1)]
        public static int stackExtraChestInteractions;

        [ConfigField("Chest Re-open Difficulty Coefficient Flat Add", "Adds to the current difficulty scaling value each time a chest is re-opened. This is calculated first.", 0.5f)]
        public static float chestReopenDifficultyCoefficientFlatAdd;

        [ConfigField("Chest Re-open Difficulty Coefficient Multiplier Add", "Multiplies the current difficulty value by 1 + this value each time a chest is re-opened. This is calculated last.", 0.12f)]
        public static float chestReopenDifficultyCoefficientMultiplierAdd;

        public static GameObject permanentHallowedIchorTracker;

        public static float cachedDifficultyDefScalingValue = -1f;

        public static int itemCount = 0;

        public static float currentDifficultyDefScalingValue = -1f;

        public bool isScoreboardOpen = false;

        private static Hook overrideHook;
        private static Hook overrideHook2;

        public static Color32 hallowedIchorBlue = new(124, 198, 255, 255);
        public static Color32 cachedTimerColor = Color.white;

        public static bool anyoneHadHallowedIchor = false;

        public static GameObject vfx;

        public override void Init(ConfigFile config)
        {
            permanentHallowedIchorTracker = new GameObject("Hallowed Ichor Tracker", typeof(SetDontDestroyOnLoad), typeof(HallowedIchorController));

            vfx = PrefabAPI.InstantiateClone(Paths.GameObject.LightningStrikeImpactEffect, "Hallowed Ichor VFX", false);
            // VFXUtils.MultiplyScale(vfx, 0.5f);
            VFXUtils.MultiplyDuration(vfx, 3f);
            VFXUtils.AddLight(vfx, hallowedIchorBlue, 15f, 30f, 2f);
            VFXUtils.RecolorMaterialsAndLights(vfx, new Color32(0, 114, 255, 255), hallowedIchorBlue, true);
            vfx.transform.Find("Backdrop").localScale = Vector3.one * 0.15f;
            vfx.transform.Find("Point light").gameObject.SetActive(false);
            ContentAddition.AddEffect(vfx);

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
            On.RoR2.UI.RunTimerUIController.Update += RunTimerUIController_Update;
            On.RoR2.UI.RunTimerUIController.Start += RunTimerUIController_Start;
            var targetMethod = typeof(ScoreboardController).GetMethod(nameof(ScoreboardController.OnEnable), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var destMethod = typeof(HallowedIchor).GetMethod(nameof(OnScoreboardOpened), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            overrideHook = new Hook(targetMethod, destMethod, this);
            targetMethod = typeof(ScoreboardController).GetMethod(nameof(ScoreboardController.OnDisable), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            destMethod = typeof(HallowedIchor).GetMethod(nameof(OnScoreboardClosed), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            overrideHook2 = new Hook(targetMethod, destMethod, this);
        }

        private void RunTimerUIController_Start(On.RoR2.UI.RunTimerUIController.orig_Start orig, RunTimerUIController self)
        {
            orig(self);
            if (self.runStopwatchTimerTextController.TryGetComponent<HGTextMeshProUGUI>(out var hgTextMeshProUGUI))
            {
                cachedTimerColor = hgTextMeshProUGUI.color;
                var wobblyText = self.runStopwatchTimerTextController.AddComponent<FreakyText>();
                wobblyText.enabled = false;
            }
        }

        private void OnScoreboardOpened(Action<ScoreboardController> orig, ScoreboardController self)
        {
            isScoreboardOpen = true;
            orig(self);
        }

        private void OnScoreboardClosed(Action<ScoreboardController> orig, ScoreboardController self)
        {
            isScoreboardOpen = false;
            orig(self);
        }

        // the code may be unoptimized buuut it doesn't really take up performance
        private void RunTimerUIController_Update(On.RoR2.UI.RunTimerUIController.orig_Update orig, RoR2.UI.RunTimerUIController self)
        {
            if (anyoneHadHallowedIchor)
            {
                double time = 0f;
                var stopwatch = Run.instance.GetRunStopwatch();

                if (Run.instance)
                {
                    var wobblyText = self.runStopwatchTimerTextController.GetComponent<FreakyText>();

                    if (isScoreboardOpen)
                    {
                        wobblyText.enabled = false;
                        time = stopwatch;
                    }
                    else
                    {
                        wobblyText.enabled = true;
                        time = stopwatch * (currentDifficultyDefScalingValue / cachedDifficultyDefScalingValue);
                        if (Util.CheckRoll(1.5f))
                        {
                            time *= -1f;
                        }
                    }
                }

                if (self.runStopwatchTimerTextController)
                {
                    self.runStopwatchTimerTextController.seconds = time;
                    if (self.runStopwatchTimerTextController.TryGetComponent<HGTextMeshProUGUI>(out var hgTextMeshProUGUI))
                    {
                        if (isScoreboardOpen)
                        {
                            hgTextMeshProUGUI.color = cachedTimerColor;
                        }
                        else
                        {
                            hgTextMeshProUGUI.color = hallowedIchorBlue;
                        }
                    }
                    return;
                }

                if (self.spriteAsNumberManager)
                {
                    self.spriteAsNumberManager.SetTimerValue((int)time);
                }
            }
            else
            {
                orig(self);
            }
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
            currentDifficultyDefScalingValue = cachedDifficultyDefScalingValue;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            itemCount = Util.GetItemCountGlobal(instance.ItemDef.itemIndex, true);
            if (itemCount > 0)
            {
                anyoneHadHallowedIchor = true;
            }
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
                    if (interactableObject.TryGetComponent<Highlight>(out var highlight))
                    {
                        highlight.strength = 0f;
                        highlight.isOn = false;
                    }
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

            var effectData = new EffectData()
            {
                rotation = Quaternion.identity,
                origin = interactableObject.transform.position + new Vector3(0f, 3.5f, 0f),
                scale = 1f
            };

            yield return new WaitForSeconds(EntityStates.Barrel.Opening.duration);
            EffectManager.SpawnEffect(vfx, effectData, true);
            if (interactableObject.TryGetComponent<Highlight>(out var highlight))
            {
                highlight.highlightColor = Highlight.HighlightColor.custom;
                highlight.CustomColor = hallowedIchorBlue;
                highlight.strength = 1f;
                highlight.isOn = true;
            }

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

            Util.PlaySound("Play_UI_item_land_command", interactableObject);
            Util.PlaySound("Play_UI_item_land_command", interactableObject);
            yield return new WaitForSeconds(EntityStates.Barrel.Closing.duration / 2f);

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
            HallowedIchor.currentDifficultyDefScalingValue = runDifficultyDef.scalingValue;
        }
    }

    public class FreakyText : MonoBehaviour
    {
        public HGTextMeshProUGUI HGTextMeshProUGUI;
        public float timer;
        public float updateInterval = 0.5f;
        public float yMovementMultiplier = 3f;
        public float xMovementMultiplier = 0.5f;

        public void Start()
        {
            HGTextMeshProUGUI = GetComponent<HGTextMeshProUGUI>();
        }

        public void Update()
        {
            timer += Time.deltaTime;
            if (timer >= updateInterval)
            {
                HGTextMeshProUGUI.ForceMeshUpdate();

                var textInfo = HGTextMeshProUGUI.textInfo;

                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    var charInfo = textInfo.characterInfo[i];

                    if (!charInfo.isVisible)
                    {
                        continue;
                    }

                    var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                    for (int j = 0; j < 4; ++j)
                    {
                        var orig = verts[charInfo.vertexIndex + j];

                        verts[charInfo.vertexIndex + j] = orig + new Vector3(Mathf.Sin(Time.time * 2f + orig.x * 0.01f) * xMovementMultiplier, Mathf.Sin(Time.time * 2f + orig.x * 0.01f) * yMovementMultiplier, 0);
                    }
                }

                for (int i = 0; i < textInfo.meshInfo.Length; ++i)
                {
                    var meshInfo = textInfo.meshInfo[i];

                    meshInfo.mesh.vertices = meshInfo.vertices;

                    HGTextMeshProUGUI.UpdateGeometry(meshInfo.mesh, i);
                }

                timer = 0f;
            }
        }
    }
}
*/