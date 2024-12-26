using EntityStates.NullifierMonster;
using MonoMod.Cil;
using R2API.Utils;
using Rewired.Demos;
using Rewired.Utils.Classes.Data;
using RoR2;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static Rewired.UI.ControlMapper.ControlMapper;

namespace Sandswept.Interactables.Regular
{
    // destinations don't get changed properly (stage 2 would land you on stage 1 simulacrum if it worked)
    // destinations are hardcoded to work with specific stage numbers (messing with stage count would mess with the stage order completely once you use a shrine of ruin)
    // enemy pools don't get swapped for some reason
    // item cost doesn't take item stacks into account
    // also for some reason it shows the tab tooltip of shrine of sacrifice??
    // also make a new cost type def cause I don't want shitty ass scrap working on this :beenormal:
    // fuck scrap making this a free interactable :beenormal:
    [ConfigSection("Interactables :: Shrine of Ruin")]
    internal class ShrineOfRuin : InteractableBase<ShrineOfRuin>
    {
        public override string Name => "Shrine of Ruin";

        public override InteractableCategory Category => InteractableCategory.Shrines;

        public override int MaxSpawnsPerStage => 1;

        public override int CreditCost => 45;

        public override HullClassification Size => HullClassification.Golem;

        public override int MinimumStageToAppearOn => 2;

        public override int SpawnWeight => 1;

        public GameObject prefab;

        public override bool OrientToFloor => true;
        public override bool SkipOnSacrifice => true;

        public override bool SpawnInVoid => false;

        public override bool SpawnInSimulacrum => false;

        public override bool SlightlyRandomizeOrientation => false;

        [ConfigField("White Item Cost", "", 10)]
        public static int whiteItemCost;

        [ConfigField("Combat Director Credit Multiplier", "", 2f)]
        public static float combatDirectorCreditMultiplier;

        [ConfigField("Scene Director Interactable Credit Multiplier", "", 0.45f)]
        public static float sceneDirectorInteractableCreditMultiplier;

        public static GameObject shrineVFX;

        public static bool shouldCorruptNextStage = false;
        public static bool shouldReplaceDrops = false;

        public static DccsPool voidEnemiesDccsPool;

        public static CostTypeIndex costTypeIndex;
        public CostTypeDef def;

        public override void Init()
        {
            base.Init();

            def = new()
            {
                buildCostString = delegate (CostTypeDef def, CostTypeDef.BuildCostStringContext c)
                {
                    c.stringBuilder.Append(whiteItemCost + " Common Items");
                },

                isAffordable = delegate (CostTypeDef def, CostTypeDef.IsAffordableContext c)
                {
                    var interactor = c.activator;
                    if (interactor)
                    {
                        var interactorBody = interactor.GetComponent<CharacterBody>();
                        if (interactorBody)
                        {
                            return ShrineOfRuinController.HasMetRequirement(interactorBody);
                        }
                    }

                    return false;
                },

                payCost = delegate (CostTypeDef def, CostTypeDef.PayCostContext c)
                {
                },
                colorIndex = ColorCatalog.ColorIndex.Tier1Item
            };

            On.RoR2.CostTypeCatalog.Init += (orig) =>
            {
                orig();

                int index = CostTypeCatalog.costTypeDefs.Length;
                Array.Resize(ref CostTypeCatalog.costTypeDefs, index + 1);
                costTypeIndex = (CostTypeIndex)index;

                CostTypeCatalog.Register(costTypeIndex, def);
            };

            prefab = PrefabAPI.InstantiateClone(Paths.GameObject.ShrineBlood, "Shrine of Ruin", true);
            var modelBase = prefab.transform.Find("Base");
            modelBase.transform.localPosition = Vector3.zero;
            var mdl = Main.prodAssets.LoadAsset<GameObject>("assets/sandswept/shrineruin.fbx");
            prefab.GetComponent<ModelLocator>().modelTransform = mdl.transform;
            mdl.name = "mdlShrineOfRuin";
            mdl.transform.localScale = Vector3.one * 70;
            mdl.AddComponent<EntityLocator>().entity = prefab;
            mdl.AddComponent<ChildLocator>().transformPairs = new ChildLocator.NameTransformPair[] { new() { name = "FireworkOrigin", transform = prefab.transform.Find("Symbol") } };
            var areYouFuckingKiddingMe = mdl.GetComponent<MeshRenderer>();
            areYouFuckingKiddingMe.material.shader = Paths.Shader.HGStandard;

            var to = mdl.AddComponent<BoxCollider>();
            to.center = Vector3.zero; to.size = Vector3.one * 0.04f;
            prefab.GetComponent<DitherModel>().bounds = to;
            prefab.GetComponent<DitherModel>().renderers[0] = mdl.GetComponent<MeshRenderer>();
            prefab.GetComponent<Highlight>().targetRenderer = mdl.GetComponent<MeshRenderer>();

            var symbol = prefab.transform.Find("Symbol");
            symbol.localPosition = new(-4, 8, -2);
            var meshRenderer = symbol.GetComponent<MeshRenderer>();
            meshRenderer.material.mainTexture = Main.prodAssets.LoadAsset<Texture2D>("assets/sandswept/shrineruinicon.png");
            meshRenderer.material.SetColor("_TintColor", new Color32(255, 255, 255, 255));

            shrineVFX = PrefabAPI.InstantiateClone(Utils.Assets.GameObject.ShrineUseEffect, "Shrine of Ruin VFX", false);
            shrineVFX.GetComponent<EffectComponent>().soundName = string.Empty;
            ContentAddition.AddEffect(shrineVFX);

            var purchaseInteraction = prefab.GetComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = "SANDSWEPT_SHRINE_RUIN_NAME";
            purchaseInteraction.contextToken = "SANDSWEPT_SHRINE_RUIN_CONTEXT";
            purchaseInteraction.Networkavailable = true;
            purchaseInteraction.costType = costTypeIndex;
            purchaseInteraction.cost = whiteItemCost;

            var genericDisplayNameProvider = prefab.GetComponent<GenericDisplayNameProvider>();
            genericDisplayNameProvider.displayToken = "SANDSWEPT_SHRINE_RUIN_NAME";

            UnityEngine.Object.DestroyImmediate(prefab.GetComponent<ShrineBloodBehavior>()); // kill yourself

            prefab.AddComponent<ShrineOfRuinController>();

            prefab.AddComponent<UnityIsAFuckingPieceOfShit2>();

            modelBase.Find("mdlShrineHealing").gameObject.SetActive(false);
            mdl.transform.parent = modelBase;

            var expansionRequirementComponent = prefab.AddComponent<ExpansionRequirementComponent>();
            expansionRequirementComponent.requiredExpansion = Main.SandsweptExpansionDef;

            var expansionRequirementComponent2 = prefab.AddComponent<ExpansionRequirementComponent>();
            expansionRequirementComponent2.requiredExpansion = Utils.Assets.ExpansionDef.DLC1;

            PrefabAPI.RegisterNetworkPrefab(prefab);

            LanguageAPI.Add("SANDSWEPT_SHRINE_RUIN_NAME", "Shrine of Ruin");
            LanguageAPI.Add("SANDSWEPT_SHRINE_RUIN_CONTEXT", "Offer to Shrine of Ruin");

            var inspectDef = ScriptableObject.CreateInstance<InspectDef>();
            var inspectInfo = inspectDef.Info = new()
            {
                TitleToken = genericDisplayNameProvider.displayToken,
                DescriptionToken = "SANDSWEPT_SHRINE_RUIN_DESCRIPTION",
                FlavorToken = "Gay Sex #Sandswept",
                isConsumedItem = false,
                Visual = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texShrineIconOutlined.png").WaitForCompletion(),
                TitleColor = Color.white
            };
            // add this to base later tbh?
            LanguageAPI.Add("SANDSWEPT_SHRINE_RUIN_DESCRIPTION", "When activated by a survivor, the Shrine of Ruin consumes " + whiteItemCost + " white items from the survivor's inventory and corrupts the next stage.");

            LanguageAPI.Add("SANDSWEPT_SHRINE_RUIN_USE_MESSAGE_2P", "<style=cShrine>The corruption spreads.</color>");
            LanguageAPI.Add("SANDSWEPT_SHRINE_RUIN_USE_MESSAGE", "<style=cShrine>The corruption spreads.</color>");

            prefab.GetComponent<GenericInspectInfoProvider>().InspectInfo = Object.Instantiate(prefab.GetComponent<GenericInspectInfoProvider>().InspectInfo);
            prefab.GetComponent<GenericInspectInfoProvider>().InspectInfo.Info = inspectInfo;

            interactableSpawnCard.prefab = prefab;

            var allEnemiesPoolEntries = new DccsPool.PoolEntry[1];
            allEnemiesPoolEntries[0] = new()
            {
                weight = 1f,
                dccs = Utils.Assets.DirectorCardCategorySelection.dccsITVoidMonsters,
            };

            var allCategories = new DccsPool.Category[1];
            allCategories[0] = new()
            {
                name = "Standard",
                categoryWeight = 1f,
                alwaysIncluded = allEnemiesPoolEntries,
                includedIfNoConditionsMet = Array.Empty<DccsPool.PoolEntry>(),
                includedIfConditionsMet = Array.Empty<DccsPool.ConditionalPoolEntry>()
            };

            voidEnemiesDccsPool = ScriptableObject.CreateInstance<DccsPool>();
            voidEnemiesDccsPool.poolCategories = allCategories;

            // On.RoR2.ClassicStageInfo.Start += ClassicStageInfo_Start;
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            On.RoR2.SceneExitController.Begin += OnSceneExit;
            On.RoR2.Run.PickNextStageSceneFromCurrentSceneDestinations += HandleScenes;
            On.RoR2.SceneDirector.Start += Gyatttttt;
            On.RoR2.BasicPickupDropTable.GenerateDropPreReplacement += OnGenerateDrop;

            PostInit();
        }

        private PickupIndex OnGenerateDrop(On.RoR2.BasicPickupDropTable.orig_GenerateDropPreReplacement orig, BasicPickupDropTable self, Xoroshiro128Plus rng)
        {
            if (shouldReplaceDrops && self.bossWeight == 0f && self.equipmentWeight < 1f)
            {
                VoidedPickupTable table = new(self, rng);
                return table.GenerateDrop();
            }

            return orig(self, rng);
        }

        [ConCommand(commandName = "sandswept_add_ruin_stack", helpText = "Forcefully triggers Shrine of Ruin effect.", flags = ConVarFlags.SenderMustBeServer)]
        public static void CC_AddRuinStack(ConCommandArgs args)
        {
            if (!Run.instance)
            {
                Debug.Log("sandswept_add_ruin_stack requires an active run!");
                return;
            }

            shouldCorruptNextStage = true;
        }

        private void Gyatttttt(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            if (shouldCorruptNextStage && SceneManager.GetActiveScene().name.StartsWith("it"))
            {
                shouldCorruptNextStage = false;
                self.teleporterSpawnCard = Paths.InteractableSpawnCard.iscTeleporter;
                ClassicStageInfo.instance.sceneDirectorInteractibleCredits = (int)(ClassicStageInfo.instance.sceneDirectorInteractibleCredits * sceneDirectorInteractableCreditMultiplier);
            }

            orig(self);
        }

        private void HandleScenes(On.RoR2.Run.orig_PickNextStageSceneFromCurrentSceneDestinations orig, Run self)
        {
            if (shouldCorruptNextStage)
            {
                WeightedSelection<SceneDef> ws = new WeightedSelection<SceneDef>();
                SceneCatalog.mostRecentSceneDef.AddDestinationsToWeightedSelection(ws, (x) =>
                {
                    SceneDef simulacrumScene = SceneCatalog.FindSceneDef("it" + x.cachedName);

                    return simulacrumScene && self.CanPickStage(x);
                });

                if (ws.choices.Length > 0)
                {
                    self.PickNextStageScene(ws);
                    return;
                }
            }

            if (shouldReplaceDrops)
            {
                SceneDef scene = SceneCatalog.FindSceneDef(SceneCatalog.mostRecentSceneDef.cachedName.Substring(2));
                WeightedSelection<SceneDef> ws = new WeightedSelection<SceneDef>();
                scene.AddDestinationsToWeightedSelection(ws, (x) =>
                {
                    return self.CanPickStage(x);
                });

                self.PickNextStageScene(ws);
                return;
            }

            orig(self);
        }

        private void OnSceneExit(On.RoR2.SceneExitController.orig_Begin orig, SceneExitController self)
        {
            if (shouldCorruptNextStage)
            {
                Run.instance.PickNextStageSceneFromCurrentSceneDestinations();

                SceneDef originalScene = self.useRunNextStageScene ? Run.instance.nextStageScene : self.destinationScene;

                SceneDef simulacrumScene = SceneCatalog.FindSceneDef("it" + originalScene.cachedName);

                if (simulacrumScene)
                {
                    self.destinationScene = simulacrumScene;
                    Run.instance.nextStageScene = simulacrumScene;
                }
            }

            orig(self);
        }

        private void SceneManager_activeSceneChanged(Scene oldScene, Scene newScene)
        {
            if (shouldCorruptNextStage)
            {
                new GameObject("hopoo why").AddComponent<DirectorCore>();
                var sceneInfo = GameObject.Find("SceneInfo");
                var obj = GameObject.Instantiate(Paths.GameObject.Director);
                if (obj.GetComponent<SceneDirector>())
                {
                    obj.GetComponent<SceneDirector>().enabled = false;
                }
                foreach (var dir in obj.GetComponents<CombatDirector>())
                {
                    dir.creditMultiplier = combatDirectorCreditMultiplier;
                }
                NetworkServer.Spawn(obj);

                shouldReplaceDrops = true;

                if (!sceneInfo)
                {
                    // Main.ModLogger.LogError("no scene info found");
                    return;
                }

                if (sceneInfo.TryGetComponent<ClassicStageInfo>(out var classicStageInfo))
                {
                    // classicStageInfo.monsterDccsPool = voidEnemiesDccsPool;
                    // classicStageInfo.monsterCategories = Utils.Assets.DirectorCardCategorySelection.dccsITVoidMonsters;
                }
            }
            else
            {
                shouldReplaceDrops = false;
            }
        }

        private void ClassicStageInfo_Start(On.RoR2.ClassicStageInfo.orig_Start orig, ClassicStageInfo self)
        {
            if (shouldCorruptNextStage)
            {
                self.monsterDccsPool = voidEnemiesDccsPool;
                self.RebuildCards();
            }

            orig(self);
        }

        public class VoidedPickupTable
        {
            public WeightedSelection<PickupIndex> TierSelection = new();
            public Xoroshiro128Plus rng;

            public void PopulateFromDropTable(BasicPickupDropTable table)
            {
                TierSelection.Clear();
                AddToSelection(Run.instance.availableVoidTier1DropList, TierSelection, table, table.tier1Weight);
                AddToSelection(Run.instance.availableVoidTier2DropList, TierSelection, table, table.tier2Weight);
                AddToSelection(Run.instance.availableVoidTier3DropList, TierSelection, table, table.tier3Weight);
                AddToSelection(Run.instance.availableVoidTier1DropList, TierSelection, table, table.voidTier1Weight);
                AddToSelection(Run.instance.availableVoidTier2DropList, TierSelection, table, table.voidTier2Weight);
                AddToSelection(Run.instance.availableVoidTier3DropList, TierSelection, table, table.voidTier3Weight);
                AddToSelection(Run.instance.availableEquipmentDropList, TierSelection, table, table.equipmentWeight);
                AddToSelection(Run.instance.availableLunarCombinedDropList, TierSelection, table, table.lunarCombinedWeight);
                AddToSelection(Run.instance.availableVoidBossDropList, TierSelection, table, table.bossWeight);
            }

            public VoidedPickupTable(BasicPickupDropTable table, Xoroshiro128Plus rng)
            {
                PopulateFromDropTable(table);
                this.rng = rng;
            }

            public PickupIndex GenerateDrop()
            {
                return TierSelection.Evaluate(rng.nextNormalizedFloat);
            }

            public ItemTierDef GetTierForSelection(List<PickupIndex> selection)
            {
                return ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(selection[0]).itemIndex)._itemTierDef;
            }

            public void AddToSelection(List<PickupIndex> indices, WeightedSelection<PickupIndex> selection, BasicPickupDropTable table, float weight)
            {
                foreach (PickupIndex index in indices)
                {
                    if (!IsFilterRequired() || PassesFilter(index))
                    {
                        selection.AddChoice(index, weight);
                    }
                }

                bool IsFilterRequired()
                {
                    if (table.requiredItemTags.Length == 0)
                    {
                        return table.bannedItemTags.Length == 0;
                    }

                    return true;
                }

                bool PassesFilter(PickupIndex index)
                {
                    PickupDef def = PickupCatalog.GetPickupDef(index);
                    if (def.itemIndex != ItemIndex.None)
                    {
                        ItemDef item = ItemCatalog.GetItemDef(def.itemIndex);

                        foreach (ItemTag value in table.bannedItemTags)
                        {
                            if (Array.IndexOf(item.tags, value) != -1)
                            {
                                return false;
                            }
                        }

                        foreach (ItemTag value in table.requiredItemTags)
                        {
                            if (Array.IndexOf(item.tags, value) == -1)
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                    return false;
                }
            }
        }
    }

    public class UnityIsAFuckingPieceOfShit2 : MonoBehaviour
    {
        public PurchaseInteraction purchaseInteraction;
        public ShrineOfRuinController shrineRuinBehavior;

        public void Start()
        {
            shrineRuinBehavior = GetComponent<ShrineOfRuinController>();
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            purchaseInteraction.costType = ShrineOfRuin.costTypeIndex;
            purchaseInteraction.onPurchase.AddListener(shrineRuinBehavior.AddShrineStack);
        }
    }

    public class ShrineOfRuinController : ShrineBehavior
    {
        public int maxPurchaseCount = 1;

        public float costMultiplierPerPurchase;

        public Transform symbolTransform;

        private PurchaseInteraction purchaseInteraction;

        private int purchaseCount;

        private float refreshTimer;

        private const float refreshDuration = 2f;

        private bool waitingForRefresh;

        public int itemCount = ShrineOfSacrifice.itemCount;

        public override int GetNetworkChannel()
        {
            return RoR2.Networking.QosChannelIndex.defaultReliable.intVal;
        }

        private void Start()
        {
            // Main.ModLogger.LogError("shrine sacrifice behavior start");
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            symbolTransform = transform.Find("Symbol");
        }

        public void FixedUpdate()
        {
            if (waitingForRefresh)
            {
                refreshTimer -= Time.fixedDeltaTime;
                if (refreshTimer <= 0f && purchaseCount < maxPurchaseCount)
                {
                    purchaseInteraction.SetAvailable(true);
                    purchaseInteraction.Networkcost = ShrineOfRuin.whiteItemCost;
                    waitingForRefresh = false;
                }
            }
        }

        public void AddShrineStack(Interactor interactor)
        {
            // Main.ModLogger.LogError("trying to run add shrine stack");
            if (!NetworkServer.active)
            {
                // Main.ModLogger.LogError("NETWORK SERVER NOT ACTRIVE EEEE ");
                // Debug.LogWarning("[Server] function 'System.Void RoR2.ShrineBloodBehavior::AddShrineStack(RoR2.Interactor)' called on client");
                return;
            }
            waitingForRefresh = true;

            var interactorBody = interactor.GetComponent<CharacterBody>();

            var inventory = interactorBody.inventory;
            if (!inventory || inventory.itemAcquisitionOrder == null)
            {
                return;
            }

            WeightedSelection<ItemIndex> itemsToRemove = new();

            int numItems = 0;

            foreach (var item in inventory.itemAcquisitionOrder)
            {
                var def = ItemCatalog.GetItemDef(item);
                if (def.tier != ItemTier.Tier1 || def.ContainsTag(ItemTag.Scrap)) continue;
                var count = inventory.GetItemCount(def);
                itemsToRemove.AddChoice(item, count); numItems += count;
            }

            if (numItems < ShrineOfRuin.whiteItemCost)
            {
                return;
            }

            for (int i = 0; i < ShrineOfRuin.whiteItemCost; i++)
            {
                var idx = itemsToRemove.EvaluateToChoiceIndex(Run.instance.treasureRng.nextNormalizedFloat);
                var choice = itemsToRemove.GetChoice(idx);
                inventory.RemoveItem(ItemCatalog.GetItemDef(choice.value));
                if (choice.weight <= 1)
                {
                    itemsToRemove.RemoveChoice(idx);
                }
                else
                {
                    itemsToRemove.ModifyChoiceWeight(idx, choice.weight - 1);
                }
            }

            if (Run.instance)
            {
                ShrineOfRuin.shouldCorruptNextStage = true;
            }

            if (interactorBody)
            {
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = interactorBody,
                    baseToken = "SANDSWEPT_SHRINE_RUIN_USE_MESSAGE",
                });
            }

            EffectManager.SpawnEffect(ShrineOfRuin.shrineVFX, new EffectData
            {
                origin = base.transform.position,
                rotation = Quaternion.identity,
                scale = 1.5f,
                color = new Color32(96, 20, 87, 255)
            }, true);

            Util.PlaySound("Play_deathProjectile_pulse", gameObject);
            Util.PlaySound("Play_deathProjectile_pulse", gameObject);
            StartCoroutine(TheVoices());

            purchaseCount++;
            refreshTimer = 2f;
            if (purchaseCount >= maxPurchaseCount)
            {
                symbolTransform.gameObject.SetActive(false);
                CallRpcSetPingable(false);
                gameObject.SetActive(false);
            }
        }

        public IEnumerator TheVoices()
        {
            Util.PlaySound("Play_voidRaid_fog_explode", gameObject);

            yield return null;

            // yield return new WaitForSeconds(1f);

            // Util.PlaySound("Stop_voidRaid_fog_affectPlayer", gameObject);
        }

        public static bool HasMetRequirement(CharacterBody interactorBody)
        {
            var inventory = interactorBody.inventory;
            if (!inventory || inventory.itemAcquisitionOrder == null)
            {
                return false;
            }

            WeightedSelection<ItemIndex> itemsToRemove = new();

            int numItems = 0;

            foreach (var item in inventory.itemAcquisitionOrder)
            {
                var def = ItemCatalog.GetItemDef(item);
                if (def.tier != ItemTier.Tier1 || def.ContainsTag(ItemTag.Scrap)) continue;
                var count = inventory.GetItemCount(def);
                itemsToRemove.AddChoice(item, count); numItems += count;
            }

            if (numItems < ShrineOfRuin.whiteItemCost)
            {
                return false;
            }

            return true;
        }

        private void UNetVersion()
        { }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            return base.OnSerialize(writer, forceAll);
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            base.OnDeserialize(reader, initialState);
        }

        public override void PreStartClient()
        {
            base.PreStartClient();
        }
    }
}