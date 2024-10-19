using EntityStates.AffixVoid;
using EntityStates.NullifierMonster;
using MonoMod.Cil;
using R2API.Utils;
using Rewired.Demos;
using RoR2;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using System;
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

        public static GameObject shrineVFX;

        public static bool shouldCorruptNextStage = false;
        public static bool shouldReplaceDrops = false;

        public static DccsPool voidEnemiesDccsPool;

        public static CostTypeIndex costTypeIndex;
        public CostTypeDef def;
        public BasicPickupDropTable Table;
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
                }
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
            purchaseInteraction.costType = costTypeIndex;
            purchaseInteraction.cost = whiteItemCost;

            var genericDisplayNameProvider = prefab.GetComponent<GenericDisplayNameProvider>();
            genericDisplayNameProvider.displayToken = "SANDSWEPT_SHRINE_RUIN_NAME";
            UnityEngine.Object.DestroyImmediate(prefab.GetComponent<ShrineBloodBehavior>()); // kill yourself

            prefab.AddComponent<ShrineOfRuinController>();
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
            LanguageAPI.Add("SANDSWEPT_SHRINE_RUIN_DESCRIPTION", "When activated by a survivor, the Shrine of Ruin consumes " + whiteItemCost + " white items from the survivor's inventory and corrupts the next stage.");

            LanguageAPI.Add("SANDSWEPT_SHRINE_RUIN_USE_MESSAGE_2P", "<style=cShrine>The corruption spreads.</color>");
            LanguageAPI.Add("SANDSWEPT_SHRINE_RUIN_USE_MESSAGE", "<style=cShrine>The corruption spreads.</color>");
            prefab.GetComponent<GenericInspectInfoProvider>().InspectInfo.Info = inspectInfo;

            interactableSpawnCard.prefab = prefab;
            Table = new BasicPickupDropTable()
            {
                selector = new(),
                tier1Weight = 0,
                tier2Weight = 0,
                tier3Weight = 0,
                voidTier1Weight = 50f,
                voidTier2Weight = 34f,
                voidTier3Weight = 8f,
                voidBossWeight = 8f
            };
            On.RoR2.ClassicStageInfo.RebuildCards += (orig, self, a, b) =>
            {
                if (shouldCorruptNextStage && !SceneCatalog.currentSceneDef.cachedName.StartsWith("moon"))
                {
                    self.monsterDccsPool.poolCategories = [new() {
                        name = "Ruin",
                        categoryWeight = 99999f, // realerstagetweaker compat
                        alwaysIncluded = [new() {
                            dccs = Utils.Assets.DirectorCardCategorySelection.dccsITVoidMonsters,
                            weight = 1f
                        }],
                        includedIfConditionsMet = [],
                        includedIfNoConditionsMet = []
                    }];
                    shouldCorruptNextStage = false;
                    shouldReplaceDrops = true;
                    Table.selector = new();
                    Table.GenerateWeightedSelection(Run.instance);
                }
                else shouldReplaceDrops = false;
                orig(self, a, b);
            };
            On.RoR2.ChestBehavior.Roll += (orig, self) =>
            {
                if (NetworkServer.active && shouldReplaceDrops && (!self.dropTable || (self.dropTable is BasicPickupDropTable && ((BasicPickupDropTable)self.dropTable).equipmentWeight == 0)))
                    self.dropTable = Table;
                orig(self);
            };
            On.RoR2.ShopTerminalBehavior.GenerateNewPickupServer_bool += (orig, self, nh) =>
            {
                if (NetworkServer.active && !self.hasBeenPurchased && shouldReplaceDrops && (!self.dropTable || (self.dropTable is BasicPickupDropTable && (((BasicPickupDropTable)self.dropTable).equipmentWeight == 0))))
                    self.dropTable = Table;
                orig(self, nh);
            };
            On.RoR2.ShrineChanceBehavior.AddShrineStack += (orig, self, activator) =>
            {
                if (NetworkServer.active && activator.GetComponent<CharacterBody>() && activator.GetComponent<CharacterBody>().inventory && shouldReplaceDrops && (!self.dropTable || (((BasicPickupDropTable)self.dropTable).equipmentWeight == 0)))
                    self.dropTable = Table;
                orig(self, activator);
            };
            PostInit();
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

        public static Dictionary<int, string[]> SimulVariants = new()
        {
            {0, ["itgoolake", "itancientloft"]},
            {1, ["itfrozenwall"]},
            {2, ["itdampcave"]},
            {3, ["itskymeadow"]},
            {4, ["itgolemplains", "itmoon"]},
        };
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
            if (!NetworkServer.active) return;
            waitingForRefresh = true;
            var interactorBody = interactor.GetComponent<CharacterBody>();
            var inventory = interactorBody.inventory;
            if (!inventory || inventory.itemAcquisitionOrder == null) return;

            WeightedSelection<ItemIndex> itemsToRemove = new();
            int numItems = 0;
            foreach (var item in inventory.itemAcquisitionOrder)
            {
                var def = ItemCatalog.GetItemDef(item);
                if (def.tier != ItemTier.Tier1 || def.ContainsTag(ItemTag.Scrap)) continue;
                var count = inventory.GetItemCount(def);
                itemsToRemove.AddChoice(item, count); numItems += count;
            }
            if (numItems < ShrineOfRuin.whiteItemCost) return;
            for (int i = 0; i < ShrineOfRuin.whiteItemCost; i++)
            {
                var idx = itemsToRemove.EvaluateToChoiceIndex(Run.instance.treasureRng.nextNormalizedFloat);
                var choice = itemsToRemove.GetChoice(idx);
                inventory.RemoveItem(ItemCatalog.GetItemDef(choice.value));
                if (choice.weight <= 1) itemsToRemove.RemoveChoice(idx);
                else itemsToRemove.ModifyChoiceWeight(idx, choice.weight - 1);
            }

            if (Run.instance)
            {
                ShrineOfRuin.shouldCorruptNextStage = true;
                // there's definitely a better way of doing this but I know jack shit about stages and it's a really messy system
                var currentStageCount = Run.instance.stageClearCount % Run.stagesPerLoop;
                var currentStageDestinationsGroup = SceneCatalog.currentSceneDef.destinationsGroup;
                string[] list = ["itmoon"];
                if (SimulVariants.ContainsKey(currentStageCount)) list = SimulVariants[currentStageCount];
                currentStageDestinationsGroup._sceneEntries = list.Select(x => new SceneCollection.SceneEntry() { sceneDef = SceneCatalog.GetSceneDefFromSceneName(x), weightMinusOne = 0 }).ToArray();
            }

            if (interactorBody) Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
            {
                subjectAsCharacterBody = interactorBody,
                baseToken = "SANDSWEPT_SHRINE_RUIN_USE_MESSAGE",
            });

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

        public static bool HasMetRequirement(CharacterBody interactorBody)
        {
            var inventory = interactorBody.inventory;
            if (!inventory || inventory.itemAcquisitionOrder == null) return false;

            WeightedSelection<ItemIndex> itemsToRemove = new();
            int numItems = 0;
            foreach (var item in inventory.itemAcquisitionOrder)
            {
                var def = ItemCatalog.GetItemDef(item);
                if (def.tier != ItemTier.Tier1 || def.ContainsTag(ItemTag.Scrap)) continue;
                var count = inventory.GetItemCount(def);
                itemsToRemove.AddChoice(item, count); numItems += count;
            }
            if (numItems < ShrineOfRuin.whiteItemCost) return false;
            return true;
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