using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API.Utils;
using RoR2.ExpansionManagement;
using RoR2.Orbs;
using RoR2.UI;
using Sandswept.Interactables.Regular;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using ThreeEyedGames;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using static RoR2.CostTypeDef;

namespace Sandswept.Interactables.Regular
{
    [ConfigSection("Interactables :: Shrine of Ruin")]
    internal class ShrineOfRuin : InteractableBase<ShrineOfRuin>
    {
        public override string Name => "Shrine of Ruin";

        public override InteractableCategory Category => InteractableCategory.Shrines;

        public override int MaxSpawnsPerStage => 1;

        public override int CreditCost => directorCreditCost;

        public override HullClassification Size => HullClassification.Golem;

        public override int MinimumStageToAppearOn => 2;

        public override int SpawnWeight => 1;

        public GameObject prefab;
        public override List<Stage> Stages { get; } = new() { Stage.DistantRoost, Stage.TitanicPlains, Stage.SiphonedForest, Stage.VerdantFalls, Stage.ViscousFalls, Stage.ShatteredAbodes, Stage.DisturbedImpact, Stage.AbandonedAqueduct, Stage.WetlandAspect, Stage.AphelianSanctuary, Stage.ReformedAltar, Stage.RallypointDelta, Stage.ScorchedAcres, Stage.SulfurPools, Stage.TreebornColony, Stage.GoldenDieback, Stage.AbyssalDepths, Stage.SirensCall, Stage.SunderedGrove };

        public override bool OrientToFloor => true;
        public override bool SkipOnSacrifice => true;

        public override bool SpawnInVoid => false;

        public override bool SpawnInSimulacrum => false;

        public override bool SlightlyRandomizeOrientation => false;

        public override string inspectInfoDescription => $"When activated by a survivor, the Shrine of Ruin consumes {whiteItemCost} random white items from the survivor's inventory and corrupts the next stage.";

        [ConfigField("Director Credit Cost", "", 10)]
        public static int directorCreditCost;

        [ConfigField("White Item Cost", "", 10)]
        public static int whiteItemCost;

        [ConfigField("Combat Director Credit Multiplier", "", 2.5f)]
        public static float combatDirectorCreditMultiplier;

        [ConfigField("Scene Director Interactable Credits", "", 220)]
        public static int sceneDirectorInteractableCredits;

        public static GameObject shrineVFX;

        public static bool shouldCorruptNextStage = false;

        public static bool shouldReplaceDrops = false;

        public static DccsPool voidEnemiesDccsPool;

        public static CostTypeIndex costTypeIndex;
        public CostTypeDef def;

        public static List<LanguageAPI.LanguageOverlay> teleporterLanguageOverlays = new();

        public static Material corruptedTeleporterFresnelMaterial;
        public static Material corruptedTeleporterMaterial;
        public static Material corruptedTeleporterWaterMaterial;
        public static Material corruptedTeleporterProngMaterial;
        public static Material corruptedTeleporterFireMaterial;
        public static Material corruptedTeleporterBeamMaterial;
        public static Material corruptedTeleporterRingMaterial;
        public static Material corruptedTeleporterLightningMaterial;

        public static GameObject voidLink;

        public static GameObject decal;

        public static GameObject globalKurwaTracker;

        public override void Init()
        {
            base.Init();

            globalKurwaTracker = new GameObject("Shrine of Ruin VFX Tracker", typeof(SetDontDestroyOnLoad), typeof(KurwaRunnerKurwa));

            NetworkingAPI.RegisterMessageType<CallVFXCoroutine>();

            corruptedTeleporterFresnelMaterial = new Material(Paths.Material.matTeleporterFresnelOverlay);
            corruptedTeleporterFresnelMaterial.SetColor("_TintColor", new Color32(255, 0, 164, 255));
            corruptedTeleporterFresnelMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampArtifactShellSoft);

            corruptedTeleporterMaterial = new Material(Paths.Material.matTeleporterClean);
            corruptedTeleporterMaterial.SetColor("_Color", new Color32(219, 139, 209, 255));
            corruptedTeleporterMaterial.SetFloat("_SpecularStrength", 1f);
            corruptedTeleporterMaterial.SetFloat("_SpecularExponent", 1.5f);

            corruptedTeleporterWaterMaterial = new Material(Paths.Material.matLunarTeleporterWater);
            corruptedTeleporterWaterMaterial.SetColor("_Color", new Color32(255, 0, 242, 255));
            corruptedTeleporterWaterMaterial.SetColor("_SpecColor", new Color32(197, 0, 183, 255));
            corruptedTeleporterWaterMaterial.SetColor("_FoamColor", new Color32(255, 0, 249, 255));

            corruptedTeleporterProngMaterial = new Material(Paths.Material.matTeleporterClean);
            corruptedTeleporterProngMaterial.SetColor("_Color", new Color32(255, 0, 223, 255));
            corruptedTeleporterProngMaterial.SetFloat("_SpecularStrength", 0.2f);
            corruptedTeleporterProngMaterial.SetFloat("_SpecularExponent", 1.5f);

            corruptedTeleporterFireMaterial = new Material(Paths.Material.matTPLunarFire);
            corruptedTeleporterFireMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampBell);

            corruptedTeleporterBeamMaterial = new Material(Paths.Material.matTPLunarLaser);
            corruptedTeleporterBeamMaterial.SetColor("_TintColor", new Color32(85, 0, 255, 255));
            corruptedTeleporterBeamMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);

            corruptedTeleporterRingMaterial = new Material(Paths.Material.matTPShockwave);
            corruptedTeleporterRingMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampAncientWisp);
            corruptedTeleporterRingMaterial.SetFloat("_Boost", 7f);

            corruptedTeleporterLightningMaterial = new Material(Paths.Material.matTPLightning);
            corruptedTeleporterLightningMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampAncientWisp);
            corruptedTeleporterLightningMaterial.SetTexture("_MainTex", Paths.Texture2D.texVoidGrass);
            corruptedTeleporterLightningMaterial.SetFloat("_Boost", 20f);
            corruptedTeleporterLightningMaterial.SetFloat("_AlphaBoost", 1f);

            voidLink = Paths.GameObject.VoidSurvivorBeamTracer.InstantiateClone("Shrine of Ruin Corrupted Teleporter Link", false);
            voidLink.transform.GetChild(0).gameObject.SetActive(false);
            voidLink.transform.GetChild(1).gameObject.SetActive(false);

            voidLink.AddComponent<TracerComponentSucks>();

            var tracer = voidLink.GetComponent<Tracer>();
            tracer.speed = 30f;

            var lineRenderer = voidLink.GetComponent<LineRenderer>();
            lineRenderer.widthMultiplier = 1f;
            lineRenderer.startWidth = 0.5f;
            lineRenderer.endWidth = 0.5f;
            lineRenderer.numCapVertices = 1;

            var newTracerMat = new Material(Paths.Material.matVoidSurvivorBeamTrail);
            newTracerMat.SetTexture("_RemapTex", Paths.Texture2D.texRampAncientWisp);

            lineRenderer.material = newTracerMat;

            var animateShaderAlpha = voidLink.GetComponent<AnimateShaderAlpha>();
            animateShaderAlpha.timeMax = 7f;

            ContentAddition.AddEffect(voidLink);

            decal = PrefabAPI.InstantiateClone(Paths.GameObject.WPDirtDecalRadial, "Corrupted Teleporter Decal", false);
            var decalComponent = decal.GetComponent<Decal>();
            decalComponent.Material = Paths.Material.matVoidCampDecal;
            decalComponent.RenderMode = Decal.DecalRenderMode.Deferred;
            decalComponent.Fade = 1f;
            decal.transform.localScale = Vector3.one * 100f;

            def = new()
            {
                buildCostString = delegate (CostTypeDef def, BuildCostStringContext c)
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

                payCost = delegate (PayCostContext context, PayCostResults results)
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
            areYouFuckingKiddingMe.material.shader = Paths.Shader.HopooGamesDeferredStandard;
            var areYouFuckingKiddngMe2 = mdl.transform.Find("shrineRuinInner");
            var mat = areYouFuckingKiddngMe2.GetComponent<MeshRenderer>().material;
            mat.shader = Paths.Shader.HopooGamesDeferredStandard;
            mat.SetColor("_Color", new Color32(255, 0, 173, 255));
            mat.SetFloat("_EmPower", 5f);

            var to = mdl.AddComponent<BoxCollider>();
            to.center = Vector3.zero; to.size = Vector3.one * 0.04f;
            prefab.GetComponent<DitherModel>().bounds = to;
            prefab.GetComponent<DitherModel>().renderers[0] = mdl.GetComponent<MeshRenderer>();
            prefab.GetComponent<Highlight>().targetRenderer = mdl.GetComponent<MeshRenderer>();

            var light = mdl.AddComponent<Light>();
            light.intensity = 12f;
            light.range = 20f;
            light.color = new Color32(170, 0, 255, 255);

            var symbol = prefab.transform.Find("Symbol");
            symbol.localPosition = new(-4, 8, -2);
            var meshRenderer = symbol.GetComponent<MeshRenderer>();
            meshRenderer.material.mainTexture = Main.prodAssets.LoadAsset<Texture2D>("assets/sandswept/shrineruinicon.png");
            meshRenderer.material.SetColor("_TintColor", new Color32(255, 255, 255, 255));

            shrineVFX = PrefabAPI.InstantiateClone(Utils.Assets.GameObject.ShrineUseEffect, "Shrine of Ruin VFX", false);
            shrineVFX.GetComponent<EffectComponent>().soundName = string.Empty;
            ContentAddition.AddEffect(shrineVFX);

            var purchaseInteraction = prefab.GetComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = "SANDSWEPT_VOID_SHRINE_RUIN_NAME";
            purchaseInteraction.contextToken = "SANDSWEPT_VOID_SHRINE_RUIN_CONTEXT";
            purchaseInteraction.Networkavailable = true;
            purchaseInteraction.costType = costTypeIndex;
            purchaseInteraction.cost = whiteItemCost;

            var genericDisplayNameProvider = prefab.GetComponent<GenericDisplayNameProvider>();
            genericDisplayNameProvider.displayToken = "SANDSWEPT_VOID_SHRINE_RUIN_NAME";

            UnityEngine.Object.DestroyImmediate(prefab.GetComponent<ShrineBloodBehavior>()); // kill yourself

            prefab.AddComponent<ShrineOfRuinController>();

            prefab.AddComponent<UnityIsAFuckingPieceOfShit2>();

            modelBase.Find("mdlShrineHealing").gameObject.SetActive(false);
            mdl.transform.parent = modelBase;

            var expansionRequirementComponent2 = prefab.AddComponent<ExpansionRequirementComponent>();
            expansionRequirementComponent2.requiredExpansion = Utils.Assets.ExpansionDef.DLC1;

            PrefabAPI.RegisterNetworkPrefab(prefab);

            LanguageAPI.Add("SANDSWEPT_VOID_SHRINE_RUIN_NAME", "Shrine of Ruin");
            LanguageAPI.Add("SANDSWEPT_VOID_SHRINE_RUIN_CONTEXT", "Offer to Shrine of Ruin");

            LanguageAPI.Add("SANDSWEPT_VOID_SHRINE_RUIN_USE_MESSAGE_2P", "<style=cShrine>The corruption spreads.</color>");
            LanguageAPI.Add("SANDSWEPT_VOID_SHRINE_RUIN_USE_MESSAGE", "<style=cShrine>The corruption spreads.</color>");

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
            SceneManager.activeSceneChanged += SpawnAndModifySimulacrumDirectors;
            On.RoR2.SceneExitController.Begin += SetNextSceneToSimulacrum;
            On.RoR2.Run.PickNextStageSceneFromCurrentSceneDestinations += HandleSceneAndCache;
            On.RoR2.SceneDirector.Start += Gyatttttt;
            On.RoR2.BasicPickupDropTable.GeneratePickupPreReplacement += OnGeneratePickup;
            On.RoR2.Run.Start += ResetShrineOfRuin;

            PostInit();
        }

        private UniquePickup OnGeneratePickup(On.RoR2.BasicPickupDropTable.orig_GeneratePickupPreReplacement orig, BasicPickupDropTable self, Xoroshiro128Plus rng)
        {
            if (shouldReplaceDrops && self.bossWeight == 0f && self.equipmentWeight < 1f)
            {
                VoidedPickupTable table = new(self, rng);
                return table.GenerateDrop();
            }

            return orig(self, rng);
        }

        private void ResetShrineOfRuin(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);
            shouldReplaceDrops = false;
            shouldCorruptNextStage = false;
            for (int i = 0; i < teleporterLanguageOverlays.Count; i++)
            {
                var languageOverlay = teleporterLanguageOverlays[i];
                languageOverlay.Remove();
            }

            teleporterLanguageOverlays.Clear();

            Language.SetCurrentLanguage(Language.currentLanguageName);
        }

        [ConCommand(commandName = "sandswept_add_ruin_stack", helpText = "Forcefully triggers Shrine of Ruin effect.", flags = ConVarFlags.SenderMustBeServer)]
        public static void CC_AddRuinStack(ConCommandArgs args)
        {
            if (!Run.instance)
            {
                Debug.Log("sandswept_add_ruin_stack requires an active run!");
                Main.ModLogger.LogError("sandswept_add_ruin_stack requires an active run!");
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
                if (Run.instance && (Run.instance.stageClearCount + 1) % 5 == 0)
                {
                    self.teleporterSpawnCard = Paths.InteractableSpawnCard.iscLunarTeleporter;
                }

                var multiplier = 1f + 0.5f * (Run.instance.participatingPlayerCount - 1);
                ClassicStageInfo.instance.sceneDirectorInteractibleCredits = Convert.ToInt32(sceneDirectorInteractableCredits * multiplier);

                for (int i = 0; i < teleporterLanguageOverlays.Count; i++)
                {
                    var languageOverlay = teleporterLanguageOverlays[i];
                    languageOverlay.Remove();
                }

                teleporterLanguageOverlays.Clear();

                Language.SetCurrentLanguage(Language.currentLanguageName);
            }

            orig(self);
        }

        private void HandleSceneAndCache(On.RoR2.Run.orig_PickNextStageSceneFromCurrentSceneDestinations orig, Run self)
        {
            if (shouldCorruptNextStage)
            {
                WeightedSelection<SceneDef> weightedSelection = new();
                SceneCatalog.mostRecentSceneDef.AddDestinationsToWeightedSelection(weightedSelection, (x) =>
                {
                    SceneDef simulacrumScene = SceneCatalog.FindSceneDef("it" + x.cachedName);
                    if (x.cachedName == "dampcavesimple")
                    {
                        simulacrumScene = SceneCatalog.FindSceneDef("itdampcave");
                    }

                    return simulacrumScene && self.CanPickStage(x);
                });

                if (weightedSelection.choices.Length > 0)
                {
                    new CallVFXCoroutine().Send(NetworkDestination.Clients);
                    self.PickNextStageScene(weightedSelection);
                    var kurwaRunnerKurwa = globalKurwaTracker.GetComponent<KurwaRunnerKurwa>();
                    kurwaRunnerKurwa.StartCoroutine(kurwaRunnerKurwa.CorruptTeleporter());
                    kurwaRunnerKurwa.StartCoroutine(kurwaRunnerKurwa.SpawnProps());
                    return;
                }
            }

            if (shouldReplaceDrops)
            {
                string sceneName = SceneCatalog.mostRecentSceneDef.cachedName.Substring(2);
                if (sceneName == "dampcave")
                {
                    sceneName = "dampcavesimple";
                }
                SceneDef scene = SceneCatalog.FindSceneDef(sceneName);
                WeightedSelection<SceneDef> weightedSelection = new();
                scene.AddDestinationsToWeightedSelection(weightedSelection, (x) =>
                {
                    return self.CanPickStage(x);
                });

                self.PickNextStageScene(weightedSelection);
                return;
            }

            orig(self);
        }

        private void SetNextSceneToSimulacrum(On.RoR2.SceneExitController.orig_Begin orig, SceneExitController self)
        {
            if (shouldCorruptNextStage)
            {
                Run.instance.PickNextStageSceneFromCurrentSceneDestinations();

                SceneDef originalScene = self.useRunNextStageScene ? Run.instance.nextStageScene : self.destinationScene;

                SceneDef simulacrumScene = SceneCatalog.FindSceneDef("it" + originalScene.cachedName);
                if (originalScene.cachedName == "dampcavesimple")
                {
                    simulacrumScene = SceneCatalog.FindSceneDef("itdampcave");
                }

                if (simulacrumScene)
                {
                    self.destinationScene = simulacrumScene;
                    Run.instance.nextStageScene = simulacrumScene;
                }
            }

            orig(self);
        }

        private void SpawnAndModifySimulacrumDirectors(Scene oldScene, Scene newScene)
        {
            if (shouldCorruptNextStage)
            {
                if (!newScene.name.StartsWith("it"))
                {
                    return;
                }

                shouldReplaceDrops = true;

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

            public UniquePickup GenerateDrop()
            {
                var pickupIndex = TierSelection.Evaluate(rng.nextNormalizedFloat);
                return new UniquePickup()
                {
                    pickupIndex = pickupIndex,
                    decayValue = 0f,
                    upgradeValue = 0
                };
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
        public int faggot = 1;

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
                if (refreshTimer <= 0f && purchaseCount < faggot)
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
                // Main.ModLogger.LogError("AddShrineStack() called on client");
                return;
            }

            // Main.ModLogger.LogError("Trying to send INetMessage to clients");
            new CallVFXCoroutine(interactor.GetComponent<NetworkIdentity>().netId).Send(NetworkDestination.Clients);

            // Main.ModLogger.LogError("Interactor net id is " + interactor.GetComponent<NetworkIdentity>().netId);

            var kurwaRunnerKurwa = ShrineOfRuin.globalKurwaTracker.GetComponent<KurwaRunnerKurwa>();
            kurwaRunnerKurwa.StartCoroutine(kurwaRunnerKurwa.CorruptTeleporter());
            kurwaRunnerKurwa.StartCoroutine(kurwaRunnerKurwa.SpawnProps());
            // Main.ModLogger.LogError("Running Coroutines on host");

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
                if (def.tier != ItemTier.Tier1 /*|| def.ContainsTag(ItemTag.Scrap)*/) continue;
                var count = inventory.GetItemCount(def);
                itemsToRemove.AddChoice(item, count); numItems += count;
            }

            if (numItems < ShrineOfRuin.whiteItemCost)
            {
                return;
            }

            StartCoroutine(RemoveItems(itemsToRemove, interactorBody, inventory));

            if (Run.instance)
            {
                ShrineOfRuin.shouldCorruptNextStage = true;
            }

            Transform origin = TeleporterInteraction.instance.transform;
            var baseMesh = TeleporterInteraction.instance.transform.Find("BaseMesh");
            if (baseMesh)
            {
                var surfaceHeight = baseMesh.Find("SurfaceHeight");
                if (surfaceHeight)
                {
                    origin = surfaceHeight;
                }
            }

            EffectManager.SpawnEffect(ShrineOfRuin.voidLink, new EffectData
            {
                start = gameObject.transform.position + new Vector3(0f, 3f, 0f),
                origin = origin.position
            }, true);

            if (interactorBody)
            {
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = interactorBody,
                    baseToken = "SANDSWEPT_VOID_SHRINE_RUIN_USE_MESSAGE",
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
            if (purchaseCount >= faggot)
            {
                symbolTransform.gameObject.SetActive(false);
                CallRpcSetPingable(false);
                purchaseInteraction.SetAvailable(false);
            }
        }

        public IEnumerator RemoveItems(WeightedSelection<ItemIndex> itemsToRemove, CharacterBody interactorBody, Inventory inventory)
        {
            for (int i = 0; i < ShrineOfRuin.whiteItemCost; i++)
            {
                // Main.ModLogger.LogError("runnning remove items for loop #" + i);

                var idx = itemsToRemove.EvaluateToChoiceIndex(Run.instance.treasureRng.nextNormalizedFloat);
                var choice = itemsToRemove.GetChoice(idx);

                PurchaseInteraction.CreateItemTakenOrb(interactorBody.corePosition, gameObject, choice.value);

                inventory.RemoveItem(ItemCatalog.GetItemDef(choice.value));

                Util.PlaySound("Play_voidJailer_m1_impact", gameObject);

                if (choice.weight <= 1)
                {
                    itemsToRemove.RemoveChoice(idx);
                }
                else
                {
                    itemsToRemove.ModifyChoiceWeight(idx, choice.weight - 1);
                }
                yield return new WaitForSeconds(0.5f / ShrineOfRuin.whiteItemCost);
            }

            var materialPropertyBlock = new MaterialPropertyBlock();

            materialPropertyBlock.SetColor("_EmColor", new Color32(215, 0, 255, 255));

            var model = gameObject.GetComponent<ModelLocator>().modelTransform;

            model.GetComponent<Light>().range = 60f;

            model.Find("shrineRuinInner").GetComponent<MeshRenderer>().SetPropertyBlock(materialPropertyBlock);

            yield return null;
        }

        public IEnumerator TheVoices()
        {
            Util.PlaySound("Play_voidRaid_fog_explode", gameObject);

            Util.PlaySound("Play_voidRaid_fog_affectPlayer", gameObject);
            Util.PlaySound("Play_voidRaid_fog_affectPlayer", gameObject);

            yield return new WaitForSeconds(5f);

            Util.PlaySound("Stop_voidRaid_fog_affectPlayer", gameObject);
            Util.PlaySound("Stop_voidRaid_fog_affectPlayer", gameObject);

            gameObject.SetActive(false);
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
                if (def.tier != ItemTier.Tier1 /*|| def.ContainsTag(ItemTag.Scrap)*/) continue;
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

    public class KurwaRunnerKurwa : MonoBehaviour
    {
        public IEnumerator CorruptTeleporter()
        {
            // Main.ModLogger.LogError("Running CorruptTeleporter() Coroutine");
            if (!TeleporterInteraction.instance)
            {
                // Main.ModLogger.LogError("Could not find TeleporterInteraction instance");
                yield break;
            }

            yield return new WaitForSeconds(0.03f);
            var overlay1 = LanguageAPI.AddOverlay("TELEPORTER_NAME", "T?eleporter?");
            yield return new WaitForSeconds(0.03f);
            var overlay2 = LanguageAPI.AddOverlay("TELEPORTER_BEGIN_CONTEXT", "Activate T?eleporter..?");
            yield return new WaitForSeconds(0.03f);
            var overlay3 = LanguageAPI.AddOverlay("TELEPORTER_END_CONTEXT", "Enter T?eleporter?");

            yield return new WaitForSeconds(0.03f);
            var overlay4 = LanguageAPI.AddOverlay("LUNAR_TELEPORTER_NAME", "...Pr??imordial T?eleporter?");
            yield return new WaitForSeconds(0.03f);
            var overlay5 = LanguageAPI.AddOverlay("LUNAR_TELEPORTER_BEGIN_CONTEXT", "Activate ...Pr??imordial T?eleporter..?");
            yield return new WaitForSeconds(0.03f);
            var overlay6 = LanguageAPI.AddOverlay("LUNAR_TELEPORTER_END_CONTEXT", "Enter ...Pr??imordial T?eleporter?");

            yield return new WaitForSeconds(0.03f);
            var overlay7 = LanguageAPI.AddOverlay("OBJECTIVE_FIND_TELEPORTER", "Find and activate the <style=cIsVoid>T?eleporter? <sprite name=\"TP\" tint=1></style>");
            yield return new WaitForSeconds(0.03f);
            var overlay8 = LanguageAPI.AddOverlay("OBJECTIVE_CHARGE_TELEPORTER", "Charge the <style=cIsVoid>T?eleporter? <sprite name=\"TP\" tint=1></style> ({0}%)");
            yield return new WaitForSeconds(0.03f);
            var overlay9 = LanguageAPI.AddOverlay("OBJECTIVE_CHARGE_TELEPORTER_OOB", "Enter the <style=cIsVoid>T?eleporter? zone!</style> ({0}%)");
            yield return new WaitForSeconds(0.03f);
            var overlay10 = LanguageAPI.AddOverlay("OBJECTIVE_FINISH_TELEPORTER", "Proceed through the <style=cIsVoid>T?eleporter? <sprite name=\"TP\" tint=1></style>");

            yield return new WaitForSeconds(0.03f);
            var overlay11 = LanguageAPI.AddOverlay("PLAYER_ACTIVATED_TELEPORTER_2P", "<style=cEvent>You activated the <style=cIsVoid>T?eleporter? <sprite name=\"TP\" tint=1></style>.</style>");
            yield return new WaitForSeconds(0.03f);
            var overlay12 = LanguageAPI.AddOverlay("PLAYER_ACTIVATED_TELEPORTER_", "<style=cEvent>{0} activated the <style=cIsVoid>T?eleporter? <sprite name=\"TP\" tint=1></style>.</style>");

            ShrineOfRuin.teleporterLanguageOverlays.Add(overlay1);
            ShrineOfRuin.teleporterLanguageOverlays.Add(overlay2);
            ShrineOfRuin.teleporterLanguageOverlays.Add(overlay3);

            ShrineOfRuin.teleporterLanguageOverlays.Add(overlay4);
            ShrineOfRuin.teleporterLanguageOverlays.Add(overlay5);
            ShrineOfRuin.teleporterLanguageOverlays.Add(overlay6);

            ShrineOfRuin.teleporterLanguageOverlays.Add(overlay7);
            ShrineOfRuin.teleporterLanguageOverlays.Add(overlay8);
            ShrineOfRuin.teleporterLanguageOverlays.Add(overlay9);
            ShrineOfRuin.teleporterLanguageOverlays.Add(overlay10);

            ShrineOfRuin.teleporterLanguageOverlays.Add(overlay11);
            ShrineOfRuin.teleporterLanguageOverlays.Add(overlay12);

            Language.SetCurrentLanguage(Language.currentLanguageName);

            foreach (HUD hud in HUD.instancesList)
            {
                var infoPanel = hud.gameModeUiRoot.Find("ClassicRunInfoHudPanel(Clone)");
                if (infoPanel)
                {
                    var rightInfoBar = infoPanel.Find("RightInfoBar");
                    if (rightInfoBar)
                    {
                        var objectivePanel = rightInfoBar.Find("ObjectivePanel");
                        if (objectivePanel)
                        {
                            var objectivePanelController = objectivePanel.GetComponent<ObjectivePanelController>();
                            if (objectivePanelController)
                            {
                                /*
                                var fuckingH = objectivePanelController.objectiveTrackers.Where(x => x.baseToken == "OBJECTIVE_FIND_TELEPORTER").FirstOrDefault();
                                if (fuckingH != null)
                                {
                                    fuckingH.label.text = fuckingH.GenerateString();
                                }
                                */
                                // of course it doesnt work
                                //

                                var fuckingH = objectivePanelController.objectiveTrackers.Where(x => x.baseToken == "OBJECTIVE_FIND_TELEPORTER").FirstOrDefault();
                                if (fuckingH != null)
                                {
                                    fuckingH.cachedString = null;
                                }
                                objectivePanelController.RefreshObjectiveTrackers();
                            }
                        }
                    }
                }
            }

            // stupid fuck

            var voidColor = new Color32(108, 0, 241, 255);

            var holdoutZoneController = TeleporterInteraction.instance.GetComponent<HoldoutZoneController>();
            yield return new WaitForSeconds(0.03f);
            holdoutZoneController.calcColor += HoldoutZoneController_calcColor;

            var teleporterTransform = TeleporterInteraction.instance.transform;
            var baseMesh = teleporterTransform.Find("TeleporterBaseMesh");
            if (baseMesh)
            {
                yield return new WaitForSeconds(0.03f);
                Object.Instantiate(ShrineOfRuin.decal, baseMesh.position, Quaternion.identity);

                var baseMr = baseMesh.GetComponent<MeshRenderer>();
                var materials = new Material[2];
                materials[0] = ShrineOfRuin.corruptedTeleporterMaterial;
                materials[1] = ShrineOfRuin.corruptedTeleporterFresnelMaterial;

                yield return new WaitForSeconds(0.03f);
                baseMr.SetMaterialArray(materials);

                var teleporterProngMesh = baseMesh.Find("TeleporterProngMesh");
                if (teleporterProngMesh)
                {
                    var teleporterProngMeshMr = teleporterProngMesh.GetComponent<MeshRenderer>();
                    yield return new WaitForSeconds(0.03f);
                    teleporterProngMeshMr.SetMaterialArray(materials);
                }

                var prongContainer = baseMesh.Find("ProngContainer");
                if (prongContainer)
                {
                    yield return new WaitForSeconds(0.03f);
                    baseMr.material = ShrineOfRuin.corruptedTeleporterProngMaterial;
                    var lunarTeleporter = prongContainer.Find("LunarTeleporterProngs(Clone)/ModelBase/lunar teleporter");
                    if (lunarTeleporter)
                    {
                        var water = lunarTeleporter.Find("Water");
                        if (water)
                        {
                            yield return new WaitForSeconds(0.03f);
                            water.GetComponent<MeshRenderer>().material = ShrineOfRuin.corruptedTeleporterWaterMaterial;
                        }

                        var lunarTeleporterProngSkinned = lunarTeleporter.Find("LunarTeleporterProngSkinned");
                        if (lunarTeleporterProngSkinned)
                        {
                            yield return new WaitForSeconds(0.03f);
                            lunarTeleporterProngSkinned.GetComponent<SkinnedMeshRenderer>().material = ShrineOfRuin.corruptedTeleporterProngMaterial;
                        }
                    }
                }

                // Main.ModLogger.LogError("found base mesh");
                var builtInEffects = baseMesh.Find("BuiltInEffects");
                if (builtInEffects)
                {
                    // Main.ModLogger.LogError("found built in effects");
                    var passiveParticleSphere = builtInEffects.Find("PassiveParticle, Sphere");
                    if (passiveParticleSphere)
                    {
                        // Main.ModLogger.LogError("found passive particle sphere");
                        var passiveParticleSphereMain = passiveParticleSphere.GetComponent<ParticleSystem>().main;
                        yield return new WaitForSeconds(0.03f);
                        passiveParticleSphereMain.startColor = new ParticleSystem.MinMaxGradient(voidColor);
                    }
                    var passiveParticleCenter = builtInEffects.Find("PassiveParticle, Center");
                    if (passiveParticleCenter)
                    {
                        // Main.ModLogger.LogError("found passive particle center");
                        var passiveParticleCenterMain = passiveParticleCenter.GetComponent<ParticleSystem>().main;
                        var passiveParticleStartColor = passiveParticleCenterMain.startColor;
                        yield return new WaitForSeconds(0.03f);
                        passiveParticleCenterMain.startColor = new ParticleSystem.MinMaxGradient(voidColor);
                    }

                    var pp = builtInEffects.Find("PP");
                    if (pp)
                    {
                        var postProcessVolume = pp.GetComponent<PostProcessVolume>();
                        var profile = postProcessVolume.profile;
                        var colorGrading = profile.GetSetting<ColorGrading>();
                        colorGrading.SetAllOverridesTo(false);
                        colorGrading.temperature.overrideState = true;
                        colorGrading.temperature.value = 20f;
                        colorGrading.tint.overrideState = true;
                        colorGrading.tint.value = 100f;
                        colorGrading.colorFilter.overrideState = true;
                        colorGrading.colorFilter.value = new Color32(255, 79, 141, 255);
                    }

                    var chargingEffect = builtInEffects.Find("ChargingEffect");
                    if (chargingEffect)
                    {
                        var betweenProngs = chargingEffect.Find("BetweenProngs");
                        if (betweenProngs)
                        {
                            var loop = betweenProngs.Find("Loop");
                            if (loop)
                            {
                                var pointLight = loop.Find("Point light");
                                if (pointLight)
                                {
                                    var light = pointLight.GetComponent<Light>();
                                    yield return new WaitForSeconds(0.03f);
                                    light.range = 100f;
                                    yield return new WaitForSeconds(0.03f);
                                    light.color = new Color32(187, 96, 255, 255);
                                }

                                var core = loop.Find("Core");
                                if (core)
                                {
                                    yield return new WaitForSeconds(0.03f);
                                    core.transform.localScale = new Vector3(2f, 4f, 2f);
                                    var coreParticleSystemRenderer = core.GetComponent<ParticleSystemRenderer>();
                                    yield return new WaitForSeconds(0.03f);
                                    coreParticleSystemRenderer.material = ShrineOfRuin.corruptedTeleporterFireMaterial;
                                }

                                var beam = loop.Find("Beam");
                                if (beam)
                                {
                                    yield return new WaitForSeconds(0.03f);
                                    beam.transform.localScale = new Vector3(3f, 50f, 3f);
                                    var beamParticleSystemRenderer = beam.GetComponent<ParticleSystemRenderer>();
                                    yield return new WaitForSeconds(0.03f);
                                    beamParticleSystemRenderer.material = ShrineOfRuin.corruptedTeleporterBeamMaterial;
                                }
                            }
                        }
                    }

                    var chargedEffect = builtInEffects.Find("ChargedEffect");
                    if (chargedEffect)
                    {
                        var ring = chargedEffect.Find("Ring");
                        if (ring)
                        {
                            yield return new WaitForSeconds(0.03f);
                            ring.GetComponent<ParticleSystemRenderer>().material = ShrineOfRuin.corruptedTeleporterRingMaterial;
                        }

                        var betweenProngs = chargedEffect.Find("BetweenProngs");
                        if (betweenProngs)
                        {
                            var initialBurst = betweenProngs.Find("InitialBurst");
                            if (initialBurst)
                            {
                                var pointLight = initialBurst.Find("Point light");
                                if (pointLight)
                                {
                                    yield return new WaitForSeconds(0.03f);
                                    var light = pointLight.GetComponent<Light>();
                                    light.color = new Color32(197, 96, 255, 255);
                                    light.range = 100f;
                                }

                                var gradient = new Gradient();

                                var colors = new GradientColorKey[2];
                                colors[0] = new GradientColorKey(Color.white, 0f);
                                colors[1] = new GradientColorKey(new Color32(164, 19, 236, 255), 0.117f);

                                var alphas = new GradientAlphaKey[2];
                                alphas[0] = new GradientAlphaKey(1f, 0.362f);
                                alphas[1] = new GradientAlphaKey(0f, 1f);

                                gradient.SetKeys(colors, alphas);

                                var flash = initialBurst.Find("Flash");
                                if (flash)
                                {
                                    var flashColorOverLifetime = flash.GetComponent<ParticleSystem>().colorOverLifetime;
                                    yield return new WaitForSeconds(0.03f);
                                    flashColorOverLifetime.color = gradient;
                                }

                                var flashLines = initialBurst.Find("Flash Lines");
                                if (flashLines)
                                {
                                    var flashLinesColorOverLifetime = flashLines.GetComponent<ParticleSystem>().colorOverLifetime;
                                    yield return new WaitForSeconds(0.03f);
                                    flashLinesColorOverLifetime.color = gradient;
                                }

                                var flashLinesVertical = initialBurst.Find("Flash Lines, Vertical");
                                if (flashLinesVertical)
                                {
                                    var flashLinesVerticalColorOverLifetime = flashLinesVertical.GetComponent<ParticleSystem>().colorOverLifetime;
                                    yield return new WaitForSeconds(0.03f);
                                    flashLinesVerticalColorOverLifetime.color = gradient;
                                }
                            }
                        }

                        var lightningAlongProngs = chargedEffect.Find("LightningAlongProngs");
                        if (lightningAlongProngs)
                        {
                            yield return new WaitForSeconds(0.03f);
                            lightningAlongProngs.GetComponent<ParticleSystemRenderer>().material = ShrineOfRuin.corruptedTeleporterLightningMaterial;
                        }
                    }
                }
            }

        }
        private void HoldoutZoneController_calcColor(ref Color color)
        {
            color = new Color(0.25f, 0f, 1f, 1f) * 2f;
        }

        public IEnumerator SpawnProps()
        {
            // Main.ModLogger.LogError("Running SpawnProps() Coroutine");
            if (!DirectorCore.instance)
            {
                // Main.ModLogger.LogError("Could not find DirectorCore instance");
                yield break;
            }

            if (!TeleporterInteraction.instance)
            {
                // Main.ModLogger.LogError("Could not find TeleporterInteraction instance in SpawnProps()");
                yield break;
            }

            yield return new WaitForSeconds(0.99f);

            var directorPlacementRule = new DirectorPlacementRule()
            {
                minDistance = 16f,
                maxDistance = 32f,
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                position = TeleporterInteraction.instance.transform.position,
                preventOverhead = false,
            };

            for (int j = 0; j < 2; j++)
            {
                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(Paths.SpawnCard.scVoidCampGrass, directorPlacementRule, Run.instance.spawnRng));
                yield return new WaitForSeconds(0.1f);
                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(Paths.SpawnCard.scVoidCampTallGrassCluster1, directorPlacementRule, Run.instance.spawnRng));
                yield return new WaitForSeconds(0.1f);
                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(Paths.SpawnCard.scVoidCampTallGrassCluster2, directorPlacementRule, Run.instance.spawnRng));
                yield return new WaitForSeconds(0.1f);
                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(Paths.SpawnCard.scVoidCampTallGrassCluster3, directorPlacementRule, Run.instance.spawnRng));
                yield return new WaitForSeconds(0.1f);
                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(Paths.SpawnCard.scVoidCampKelp, directorPlacementRule, Run.instance.spawnRng));
                yield return new WaitForSeconds(0.1f);
                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(Paths.SpawnCard.scVoidCampXYZ, directorPlacementRule, Run.instance.spawnRng));
                yield return new WaitForSeconds(0.1f);
                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(Paths.SpawnCard.scVoidCampXYZOpen, directorPlacementRule, Run.instance.spawnRng));
            }
        }
    }

    public class CallVFXCoroutine : INetMessage
    {
        public NetworkInstanceId objID;

        public CallVFXCoroutine()
        {
        }

        public CallVFXCoroutine(NetworkInstanceId objID)
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

            var kurwaRunnerKurwa = ShrineOfRuin.globalKurwaTracker.GetComponent<KurwaRunnerKurwa>();
            kurwaRunnerKurwa.StartCoroutine(kurwaRunnerKurwa.CorruptTeleporter());
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(objID);
        }
    }
}
