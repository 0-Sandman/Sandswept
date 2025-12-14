
using LookingGlass.ItemStatsNameSpace;
using MonoMod.RuntimeDetour;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2.UI;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Sandswept.Items.Lunars
{
    [ConfigSection("Items :: Hallowed Ichor")]
    public class HallowedIchor : ItemBase<HallowedIchor>
    {
        public override string ItemName => "Hallowed Ichor";

        public override string ItemLangTokenName => "HALLOWED_ICHOR";

        public override string ItemPickupDesc => "Chests may be re-opened an additional time... $lcBUT re-opening them increases difficulty permanently.$ec".AutoFormat();

        public override string ItemFullDescription => $"Chests may be $sure-opened {baseExtraChestInteractions}$se $ss(+{stackExtraChestInteractions} per stack)$se additional times, but $sure-opening$se them increases $sudifficulty$se by $sr{chestReopenDifficultyCoefficientMultiplierAdd * 300f}%$se permanently. $srDifficulty increase scales with chest rarity$se.".AutoFormat(); // this will be inaccurate no matter what but this is somewhat accurate for the first use on monsoon on singleplayer lmao

        public override string ItemLore =>
        """
        This will be the most potent creation I give to you. I have weaved many of these artifacts for vermin like you, but this one was far more costly than the others. It will be given only to a scarce few of my most dedicated servants. If used properly, it will be worth the price spent constructing it.

        It is my blood, held in a vessel of my design. Superior blood. It grants me -- along with my treacherous brother -- the power to shape the compounds. A transfusion is sufficient to enable its effects. It cannot provide the extent of my abilities in such limited quantity, but it will allow you to create more of the trinkets you cling to so tightly.

        Its use will not go unnoticed, however. He will sense its presence, and send his vermin to hunt you down. If you use its power to create suitable weapons, and avoid encountering him in the flesh, I do not suspect you will have any issue dispatching your pursuers.
        """;
        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => Main.sandsweptHIFU.LoadAsset<GameObject>("HallowedIchorHolder.prefab");

        public override Sprite ItemIcon => Main.sandsweptHIFU.LoadAsset<Sprite>("texHallowedIchor.png");

        public override ItemTag[] ItemTags => [ItemTag.Utility, ItemTag.InteractableRelated, ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.BrotherBlacklist];

        [ConfigField("Base Extra Chest Interactions", "", 1)]
        public static int baseExtraChestInteractions;

        [ConfigField("Stack Extra Chest Interactions", "", 1)]
        public static int stackExtraChestInteractions;

        [ConfigField("Chest Reopen Difficulty Coefficient Flat Add", "Just check the Formula Example.", 0.5f)]
        public static float chestReopenDifficultyCoefficientFlatAdd;

        [ConfigField("Chest Reopen Difficulty Coefficient Multiplier Add", "Just check the Formula Example..", 0.1f)]
        public static float chestReopenDifficultyCoefficientMultiplierAdd;

        [ConfigField("Per Player Divisor Add", "Just check the Formula Example...", 0.1f)]
        public static float perPlayerDivisorAdd;

        [ConfigField("Chest Reopen Difficulty Coefficient Flat Add Scalar", "Just check the Formula Example....", 0.06f)]
        public static float chestDifficultyCoefficientFlatAddScalar;

        [ConfigField("Chest Reopen Difficulty Coefficient Multiplier Scalar", "Just check the Formula Example.....", 0.08f)]
        public static float chestDifficultyCoefficientMultiplierScalar;

        [ConfigField("Formula Example", "For Moonsoon, where its Difficulty Def Scaling Value is 3, One small chest reopen's starting from 0 re-opens formula is as follows:\n (3 + Chest Reopen Difficulty Coefficient Multiplier Add) * Chest Reopen Difficulty Coefficient Multiplier Add. Which basically means that with unaltered config options, Monsoon suddenly goes from +50% difficulty scaling to +92.5%. 2 (base, Rainstorm) + 50% = 3, 2 + 92.5% = 3.85. For a reopen of a large chest starting from 0 reopens with 2 players total on Monsoon:\n (Difficulty Def Scaling Value + ((Chest Reopen Difficulty Coefficient Flat Add + (Chest Reopen Difficulty Coefficient Flat Add * Chest Reopen Difficulty Coefficient Flat Add Scalar * Chest Tier)) / ((1 - Per Player Divisor Add) + (Per Player Divisor Add * Player Count)))) * (1 + (((Chest Reopen Difficulty Coefficient Multiplier Add + ((1 + Chest Reopen Difficulty Coefficient Multiplier Add) * Chest Reopen Difficulty Coefficient Multiplier Scalar * Chest Tier)))) / ((1 - Per Player Divisor Add) + (Per Player Divisor Add * Player Count))). For any subsequent reopens, substitute Difficulty Def Scaling Value for the number you just got from this formula. Chest Tier (and therefore scalars') formulas are unused for small chests.", true)]
        public static bool formulaExample;

        public override string AchievementName => "Break Away";

        public override string AchievementDesc => "Complete the Primordial Teleporter without picking up any items on the stage.";

        public static GameObject permanentHallowedIchorTracker;

        public static float cachedDifficultyDefScalingValue = -1f;
        public static float rainstormScalingValue = 2;

        public static int itemCount = 0;

        public static float currentDifficultyDefScalingValue = -1f;

        public bool isScoreboardOpen = false;

        private static Hook onScoreboardOpenedHook;
        private static Hook onScoreboardClosedHook;

        public static Color32 hallowedIchorBlue = new(124, 198, 255, 255);
        public static Color32 cachedTimerColor = Color.white;

        public static bool anyoneHadHallowedIchorThisStage = false;
        public static bool anyoneHadHallowedIchorThisRun = false;

        public static int globalReopenCount = 0;

        public static GameObject vfx;

        public static List<string> stageBlacklist = ["bazaar", "computationalexchange"];

        public override void Init()
        {
            base.Init();
            permanentHallowedIchorTracker = new GameObject("Hallowed Ichor Tracker", typeof(SetDontDestroyOnLoad), typeof(HallowedIchorController));
            NetworkingAPI.RegisterMessageType<CallRecalculate>();
            NetworkingAPI.RegisterMessageType<CallUpdateValue>();
            SetUpVFX();
        }

        public void SetUpVFX()
        {
            vfx = PrefabAPI.InstantiateClone(Paths.GameObject.LightningStrikeImpactEffect, "Hallowed Ichor VFX", false);
            // VFXUtils.MultiplyScale(vfx, 0.5f);
            VFXUtils.MultiplyDuration(vfx, 3f);
            VFXUtils.AddLight(vfx, hallowedIchorBlue, 15f, 30f, 2f);
            VFXUtils.RecolorMaterialsAndLights(vfx, new Color32(0, 114, 255, 255), hallowedIchorBlue, true);
            vfx.transform.Find("Backdrop").localScale = Vector3.one * 0.15f;
            vfx.transform.Find("Point light").gameObject.SetActive(false);
            ContentAddition.AddEffect(vfx);
        }

        public override void Hooks()
        {
            CharacterBody.onBodyInventoryChangedGlobal += TrackStackCount;
            GlobalEventManager.OnInteractionsGlobal += AddChestStackAndRecalculate;
            Run.onRunStartGlobal += CacheValues;
            Run.onRunDestroyGlobal += UnsetValues;

            On.RoR2.UI.RunTimerUIController.Update += PerformJitterUI;
            On.RoR2.UI.RunTimerUIController.Start += AddJitterUI;
            RoR2.Stage.onServerStageBegin += OnStageStartUpdateClientValues;
            On.RoR2.PlayerCharacterMasterController.OnBodyDeath += OnBodyDeathUpdateClientValues;
            // spectator still has desync I believe?

            var targetMethod = typeof(ScoreboardController).GetMethod(nameof(ScoreboardController.OnEnable), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var destMethod = typeof(HallowedIchor).GetMethod(nameof(OnScoreboardOpened), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            onScoreboardOpenedHook = new Hook(targetMethod, destMethod, this);

            targetMethod = typeof(ScoreboardController).GetMethod(nameof(ScoreboardController.OnDisable), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            destMethod = typeof(HallowedIchor).GetMethod(nameof(OnScoreboardClosed), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            onScoreboardClosedHook = new Hook(targetMethod, destMethod, this);
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Overall Chest Reopen Count: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Utility);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
            itemStatsDef.descriptions.Add("Maximum Chest Reopen Count Per Chest: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Utility);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
            itemStatsDef.descriptions.Add("Difficulty Increase vs Rainstorm: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Death);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.descriptions.Add("Difficulty Increase vs Selected Difficulty: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Death);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);

            itemStatsDef.calculateValues = (master, stack) =>
            {
                List<float> values = new()
                {
                    globalReopenCount,
                    baseExtraChestInteractions + stackExtraChestInteractions * (itemCount - 1),
                    (currentDifficultyDefScalingValue / rainstormScalingValue) - 1,
                    (currentDifficultyDefScalingValue / cachedDifficultyDefScalingValue) - 1
                };

                return values;
            };

            return itemStatsDef;
        }

        private void OnBodyDeathUpdateClientValues(On.RoR2.PlayerCharacterMasterController.orig_OnBodyDeath orig, PlayerCharacterMasterController self)
        {
            orig(self);
            new CallUpdateValue().Send(NetworkDestination.Clients);
        }

        private void OnStageStartUpdateClientValues(RoR2.Stage stage)
        {
            new CallUpdateValue().Send(NetworkDestination.Clients);
        }

        private void AddJitterUI(On.RoR2.UI.RunTimerUIController.orig_Start orig, RunTimerUIController self)
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
        private void PerformJitterUI(On.RoR2.UI.RunTimerUIController.orig_Update orig, RoR2.UI.RunTimerUIController self)
        {
            if (anyoneHadHallowedIchorThisRun)
            {
                double time = 0f;

                if (Run.instance)
                {
                    var stopwatch = Run.instance.GetRunStopwatch();
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

        private void UnsetValues(Run run)
        {
            var runDifficultyDef = DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty);
            runDifficultyDef.scalingValue = cachedDifficultyDefScalingValue;
            anyoneHadHallowedIchorThisStage = false;
            anyoneHadHallowedIchorThisRun = false;
            globalReopenCount = 0;
        }

        private void CacheValues(Run run)
        {
            var runDifficultyDef = DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty);
            rainstormScalingValue = DifficultyCatalog.GetDifficultyDef(DifficultyIndex.Normal).scalingValue;
            cachedDifficultyDefScalingValue = runDifficultyDef.scalingValue;
            currentDifficultyDefScalingValue = cachedDifficultyDefScalingValue;
        }

        private void TrackStackCount(CharacterBody body)
        {
            if (GetPlayerItemCountGlobal(instance.ItemDef.itemIndex, false) > 0)
            {
                anyoneHadHallowedIchorThisRun = true;
            }

            itemCount = GetPlayerItemCountGlobal(instance.ItemDef.itemIndex, true);
            // Main.ModLogger.LogError("TrackStatCount: itemCount set to " + itemCount);
            if (itemCount > 0)
            {
                anyoneHadHallowedIchorThisStage = true;
            }
            else
            {
                anyoneHadHallowedIchorThisStage = false;
            }
        }

        private void AddChestStackAndRecalculate(Interactor interactor, IInteractable interactable, GameObject interactableObject)
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

                var sceneName = SceneManager.GetActiveScene().name;
                if (stageBlacklist.Contains(sceneName))
                {
                    return;
                }

                if (hallowedIchorChestController.openedCount > 1)
                {
                    // Main.ModLogger.LogError("opened count is more than 1");
                    var chestTier = GetChestTier(purchaseInteraction);
                    permanentHallowedIchorTracker.GetComponent<HallowedIchorController>().Recalculate(chestTier);
                    new CallRecalculate(interactor.GetComponent<NetworkIdentity>().netId, chestTier).Send(NetworkDestination.Clients);
                    globalReopenCount++;
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
            return purchaseInteraction.displayNameToken.StartsWith("CHEST") || purchaseInteraction.displayNameToken.StartsWith("CATEGORYCHEST") || purchaseInteraction.displayNameToken.StartsWith("GOLDCHEST");
        }

        private int GetChestTier(PurchaseInteraction purchaseInteraction)
        {
            int chestTier = 1;
            if (purchaseInteraction.displayNameToken.Contains("CHEST2"))
            {
                chestTier = 2;
            }
            if (purchaseInteraction.displayNameToken.Contains("GOLDCHEST"))
            {
                chestTier = 3;
            }
            return chestTier;
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

            EffectManager.SpawnEffect(vfx, effectData, true);
            if (interactableObject.TryGetComponent<Highlight>(out var highlight))
            {
                highlight.highlightColor = Highlight.HighlightColor.custom;
                highlight.CustomColor = hallowedIchorBlue;
                highlight.strength = 1f;
                highlight.isOn = true;
            }

            yield return new WaitForSeconds(EntityStates.Barrel.Closing.duration / 2f);

            Util.PlaySound("Play_ichor_proc", interactableObject);

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

            var itemDisplay = SetUpFollowerIDRS(0.6f, 33f);

            return new ItemDisplayRuleDict(new ItemDisplayRule()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(-1f, -0.8f, -0.4f),
                localScale = new Vector3(0.25f, 0.25f, 0.25f),

                followerPrefab = itemDisplay,
                limbMask = LimbFlags.None,
                followerPrefabAddress = new AssetReferenceGameObject("")
            });
        }
    }

    public class HallowedIchorChestController : MonoBehaviour
    {
        public int openedCount = 0;
        public bool canDropItem = false;
    }

    public class HallowedIchorController : MonoBehaviour
    {
        public void Recalculate(int chestTier)
        {
            if (!Run.instance)
            {
                return;
            }

            var flatIncreaseFromChestTier = 0f;
            var multiplierIncreaseFromChestTier = 0f;

            if (chestTier > 1)
            {
                flatIncreaseFromChestTier = HallowedIchor.chestReopenDifficultyCoefficientFlatAdd * HallowedIchor.chestDifficultyCoefficientFlatAddScalar * chestTier;
                multiplierIncreaseFromChestTier = (1f + HallowedIchor.chestReopenDifficultyCoefficientMultiplierAdd) * HallowedIchor.chestDifficultyCoefficientMultiplierScalar * chestTier;
            }

            var playerScalar = (1f - HallowedIchor.perPlayerDivisorAdd) + (HallowedIchor.perPlayerDivisorAdd * Run.instance.participatingPlayerCount);

            var flatIncrease = (HallowedIchor.chestReopenDifficultyCoefficientFlatAdd + flatIncreaseFromChestTier) / playerScalar;
            var multiplier = 1f + ((HallowedIchor.chestReopenDifficultyCoefficientMultiplierAdd + multiplierIncreaseFromChestTier) / playerScalar);

            Run.ambientLevelCap = int.MaxValue;

            var runDifficultyDef = DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty);
            runDifficultyDef.scalingValue += flatIncrease;
            runDifficultyDef.scalingValue *= multiplier;
            HallowedIchor.currentDifficultyDefScalingValue = runDifficultyDef.scalingValue;
        }

        public void UpdateValue()
        {
            if (!Run.instance)
            {
                return;
            }

            var runDifficultyDef = DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty);
            HallowedIchor.currentDifficultyDefScalingValue = runDifficultyDef.scalingValue;
        }

    }

    public class FreakyText : MonoBehaviour
    {
        public HGTextMeshProUGUI HGTextMeshProUGUI;
        public float timer;
        public float updateInterval = 0.4f;
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

    public class CallRecalculate : INetMessage
    {
        public NetworkInstanceId objID;
        public int chestTier;

        public CallRecalculate()
        {
        }

        public CallRecalculate(NetworkInstanceId objID, int chestTier)
        {
            this.objID = objID;
            this.chestTier = chestTier;
        }

        public void Deserialize(NetworkReader reader)
        {
            objID = reader.ReadNetworkId();
            chestTier = reader.ReadInt32();
        }

        public void OnReceived()
        {
            if (NetworkServer.active)
            {
                // Main.ModLogger.LogError("tried running onreceived for host");
                return;
            }

            // Main.ModLogger.LogError("OnReceived() called for client");

            HallowedIchor.permanentHallowedIchorTracker.GetComponent<HallowedIchorController>().Recalculate(chestTier);
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(objID);
            writer.Write(chestTier);
        }
    }

    public class CallUpdateValue : INetMessage
    {
        public NetworkInstanceId objID;

        public CallUpdateValue()
        {
        }

        public CallUpdateValue(NetworkInstanceId objID)
        {
            this.objID = objID;
        }

        public void Deserialize(NetworkReader reader)
        {
            objID = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            if (NetworkServer.active)
            {
                // Main.ModLogger.LogError("tried running onreceived for host");
                return;
            }

            // Main.ModLogger.LogError("OnReceived() called for client");

            HallowedIchor.permanentHallowedIchorTracker.GetComponent<HallowedIchorController>().UpdateValue();
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(objID);
        }
    }
}
