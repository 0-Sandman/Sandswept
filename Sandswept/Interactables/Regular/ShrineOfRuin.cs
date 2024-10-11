using R2API.Utils;
using Rewired.Demos;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;
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

        public override int MinimumStageToAppearOn => 1;

        public override int SpawnWeight => 1;

        public GameObject prefab;

        public override bool OrientToFloor => true;
        public override bool SkipOnSacrifice => true;

        public override bool SpawnInVoid => false;

        public override bool SpawnInSimulacrum => false;

        public override bool SlightlyRandomizeOrientation => false;

        [ConfigField("White Item Cost", "", 8)]
        public static int whiteItemCost;

        [ConfigField("Item Count", "", 2)]
        public static int itemCount;

        public static GameObject shrineVFX;

        public static bool shouldCorruptNextStage = false;

        public static DccsPool voidEnemiesDccsPool;

        public override void Init()
        {
            base.Init();

            prefab = PrefabAPI.InstantiateClone(Paths.GameObject.ShrineBlood, "Shrine of Ruin", true);
            var mdl = prefab.transform.Find("Base/mdlShrineHealing").gameObject;
            mdl.name = "mdlShrineRuin";
            mdl.GetComponent<MeshFilter>().sharedMesh = Main.prodAssets.LoadAsset<Mesh>("assets/sandswept/shrinesacrifice.fbx");
            mdl.GetComponent<MeshRenderer>().sharedMaterial = Main.prodAssets.LoadAsset<Material>("assets/sandswept/shrinesacrifice.fbx");
            var symbol = prefab.transform.Find("Symbol");
            symbol.localPosition = new(0, 4, 0);
            var meshRenderer = symbol.GetComponent<MeshRenderer>();
            meshRenderer.material.mainTexture = Main.prodAssets.LoadAsset<Texture2D>("assets/sandswept/shrinesacrificeicon.png");
            meshRenderer.material.SetColor("_TintColor", new Color32(255, 255, 255, 255));

            shrineVFX = PrefabAPI.InstantiateClone(Utils.Assets.GameObject.ShrineUseEffect, "Shrine of Ruin VFX", false);
            shrineVFX.GetComponent<EffectComponent>().soundName = "Play_affix_void_bug_spawn";
            ContentAddition.AddEffect(shrineVFX);

            var purchaseInteraction = prefab.GetComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = "SANDSWEPT_SHRINE_RUIN_NAME";
            purchaseInteraction.contextToken = "SANDSWEPT_SHRINE_RUIN_CONTEXT";
            purchaseInteraction.Networkavailable = true;
            purchaseInteraction.costType = CostTypeIndex.None;
            purchaseInteraction.cost = 0;

            var genericDisplayNameProvider = prefab.GetComponent<GenericDisplayNameProvider>();
            genericDisplayNameProvider.displayToken = "SANDSWEPT_SHRINE_RUIN_NAME";

            UnityEngine.Object.DestroyImmediate(prefab.GetComponent<ShrineBloodBehavior>()); // kill yourself

            prefab.AddComponent<ShrineRuinBehavior>();

            prefab.AddComponent<UnityIsAFuckingPieceOfShit2>();

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
            LanguageAPI.Add("SANDSWEPT_SHRINE_RUIN_DESCRIPTION", "When activated by a survivor the Shrine of Ruin consumes " + whiteItemCost + " white items from the survivor's inventory and corrupts the next stage.");

            LanguageAPI.Add("SANDSWEPT_SHRINE_RUIN_USE_MESSAGE_2P", "<style=cShrine>The corruption spreads.</color>");
            LanguageAPI.Add("SANDSWEPT_SHRINE_RUIN_USE_MESSAGE", "<style=cShrine>The corruption spreads.</color>");

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
                categoryWeight = 1,
                alwaysIncluded = allEnemiesPoolEntries,
            };

            voidEnemiesDccsPool = new()
            {
                poolCategories = allCategories
            };

            On.RoR2.ClassicStageInfo.Start += ClassicStageInfo_Start;
            On.RoR2.PickupDropTable.GenerateDropFromWeightedSelection += PickupDropTable_GenerateDropFromWeightedSelection;
            On.RoR2.PickupDropTable.GenerateUniqueDropsFromWeightedSelection += PickupDropTable_GenerateUniqueDropsFromWeightedSelection;

            PostInit();
        }

        private PickupIndex PickupDropTable_GenerateDropFromWeightedSelection(On.RoR2.PickupDropTable.orig_GenerateDropFromWeightedSelection orig, Xoroshiro128Plus rng, WeightedSelection<PickupIndex> weightedSelection)
        {
            OverwriteDrop(weightedSelection);
            return orig(rng, weightedSelection);
        }

        private PickupIndex[] PickupDropTable_GenerateUniqueDropsFromWeightedSelection(On.RoR2.PickupDropTable.orig_GenerateUniqueDropsFromWeightedSelection orig, int maxDrops, Xoroshiro128Plus rng, WeightedSelection<PickupIndex> weightedSelection)
        {
            OverwriteDrop(weightedSelection);
            return orig(maxDrops, rng, weightedSelection);
        }

        private void OverwriteDrop(WeightedSelection<PickupIndex> weightedSelection)
        {
            if (shouldCorruptNextStage)
            {
                var dropPickup = PickupIndex.none;

                WeightedSelection<List<PickupIndex>> selector = new();
                selector.AddChoice(Run.instance.availableVoidTier1DropList, 50f);
                selector.AddChoice(Run.instance.availableVoidTier2DropList, 34f);
                selector.AddChoice(Run.instance.availableVoidTier3DropList, 8f);
                selector.AddChoice(Run.instance.availableVoidBossDropList, 8f);

                List<PickupIndex> dropList = selector.Evaluate(Run.instance.treasureRng.nextNormalizedFloat);
                if (dropList != null && dropList.Count > 0)
                {
                    dropPickup = Run.instance.treasureRng.NextElementUniform(dropList);
                }

                weightedSelection.Clear();
                weightedSelection.AddChoice(dropPickup, 1f);
            }
        }

        private void ClassicStageInfo_Start(On.RoR2.ClassicStageInfo.orig_Start orig, ClassicStageInfo self)
        {
            orig(self);
            if (shouldCorruptNextStage)
            {
                self.monsterDccsPool = voidEnemiesDccsPool;
                self.RebuildCards();
            }
        }
    }

    public class UnityIsAFuckingPieceOfShit2 : MonoBehaviour
    {
        public PurchaseInteraction purchaseInteraction;
        public ShrineRuinBehavior shrineRuinBehavior;

        public void Start()
        {
            shrineRuinBehavior = GetComponent<ShrineRuinBehavior>();
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            purchaseInteraction.onPurchase.AddListener(SoTrue);
        }

        public void SoTrue(Interactor interactor)
        {
            shrineRuinBehavior.AddShrineStack(interactor);
        }
    }

    public class ShrineRuinBehavior : ShrineBehavior
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
                    purchaseInteraction.Networkcost = ShrineOfSacrifice.curseCost;
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
            if (!inventory)
            {
                return;
            }

            List<ItemDef> itemsToRemove = new();

            var items = inventory.itemAcquisitionOrder;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var itemDef = ItemCatalog.GetItemDef(item);

                if (itemDef.tier == ItemTier.Tier1)
                {
                    Main.ModLogger.LogError("found white item " + itemDef.name);
                    itemsToRemove.Add(itemDef);
                }

                if (itemsToRemove.Count >= ShrineOfRuin.whiteItemCost)
                {
                    Main.ModLogger.LogError("found 6 items to remove, exiting loop");
                    break;
                }
            }

            if (itemsToRemove.Count < ShrineOfRuin.whiteItemCost)
            {
                Main.ModLogger.LogError("couldnt get 6 white items");
                return;
            }

            foreach (ItemDef itemDef in itemsToRemove)
            {
                Main.ModLogger.LogError("trying to remove items");
                inventory.RemoveItem(itemDef);
            }

            if (Run.instance)
            {
                ShrineOfRuin.shouldCorruptNextStage = true;
                // there's definitely a better way of doing this but I know jack shit about stages and it's a really messy system
                var currentStageCount = Run.instance.stageClearCount % Run.stagesPerLoop;

                var currentStageDestinationsGroup = SceneCatalog.currentSceneDef.destinationsGroup;

                var titanicPlainsSimulacrum = new SceneCollection.SceneEntry()
                {
                    sceneDef = SceneCatalog.GetSceneDefFromSceneName("itgolemplains"),
                    weightMinusOne = 0
                };

                var abandonedAqueductSimulacrum = new SceneCollection.SceneEntry()
                {
                    sceneDef = SceneCatalog.GetSceneDefFromSceneName("itgoolake"),
                    weightMinusOne = 0
                };

                var aphelianSanctuarySimulacrum = new SceneCollection.SceneEntry()
                {
                    sceneDef = SceneCatalog.GetSceneDefFromSceneName("itancientloft"),
                    weightMinusOne = 0
                };

                var rallypointDeltaSimulacrum = new SceneCollection.SceneEntry()
                {
                    sceneDef = SceneCatalog.GetSceneDefFromSceneName("itfrozenwall"),
                    weightMinusOne = 0
                };

                var abyssalDepthsSimulacrum = new SceneCollection.SceneEntry()
                {
                    sceneDef = SceneCatalog.GetSceneDefFromSceneName("itdampcave"),
                    weightMinusOne = 0
                };

                var skyMeadowSimulacrum = new SceneCollection.SceneEntry()
                {
                    sceneDef = SceneCatalog.GetSceneDefFromSceneName("itskymeadow"),
                    weightMinusOne = 0
                };

                switch (currentStageCount)
                {
                    case 0:
                        Main.ModLogger.LogError("setting destination to plains simulacrum");
                        currentStageDestinationsGroup._sceneEntries = new SceneCollection.SceneEntry[] { titanicPlainsSimulacrum };
                        break;

                    case 1:
                        Main.ModLogger.LogError("setting destinations to aqueduct, sanctuary simulacrum");
                        currentStageDestinationsGroup._sceneEntries = new SceneCollection.SceneEntry[] { abandonedAqueductSimulacrum, aphelianSanctuarySimulacrum };
                        break;

                    case 2:
                        Main.ModLogger.LogError("setting destination to rpd simulacrum");
                        currentStageDestinationsGroup._sceneEntries = new SceneCollection.SceneEntry[] { rallypointDeltaSimulacrum };
                        break;

                    case 3:
                        Main.ModLogger.LogError("setting destination to depths simulacrum");
                        currentStageDestinationsGroup._sceneEntries = new SceneCollection.SceneEntry[] { abyssalDepthsSimulacrum };
                        break;

                    case 4:
                        Main.ModLogger.LogError("setting destination to meadow simulacrum");
                        currentStageDestinationsGroup._sceneEntries = new SceneCollection.SceneEntry[] { skyMeadowSimulacrum };
                        break;

                    default:
                        Main.ModLogger.LogError("setting should corrupt next stage to false");
                        ShrineOfRuin.shouldCorruptNextStage = false;
                        break;
                }
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

            purchaseCount++;
            refreshTimer = 2f;
            if (purchaseCount >= maxPurchaseCount)
            {
                symbolTransform.gameObject.SetActive(false);
                CallRpcSetPingable(false);
                gameObject.SetActive(false);
            }
        }

        private void UNetVersion()
        {
        }

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