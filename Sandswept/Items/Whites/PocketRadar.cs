/*using LookingGlass.ItemStatsNameSpace;

namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Pocket Radar")]
    internal class PocketRadar : ItemBase<PocketRadar>
    {
        public override string ItemName => "Pocket Radar";

        public override string ItemLangTokenName => "POCKET_RADAR";

        public override string ItemPickupDesc => "Cloaked chests are easier to see and appear more frequently.";

        public override string ItemFullDescription => $"Cloaked chests are $sueasier to see$se and are $suguaranteed$se to spawn $su{baseExtraCloakedChests}$se $ss(+{stackExtraCloakedChests} per stack)$se times each stage.".AutoFormat();

        public override string ItemLore =>
        """

        """;
        public override string AchievementName => "Gloomy Flash";

        public override string AchievementDesc => "Open a Cloaked Chest.";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => null;
        public override Sprite ItemIcon => null;

        public override ItemTag[] ItemTags => [ItemTag.InteractableRelated, ItemTag.Utility, ItemTag.OnStageBeginEffect, ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.BrotherBlacklist];

        [ConfigField("Base Extra Cloaked Chests", "", 1)]
        public static int baseExtraCloakedChests;

        [ConfigField("Stack Extra Cloaked Chests", "", 1)]
        public static int stackExtraCloakedChests;

        public static Material newCloakedChestMaterial;

        public static InteractableSpawnCard interactableSpawnCard;

        public override void Init()
        {
            base.Init();
            SetUpISC();
            SetUpVFX();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Extra Cloaked Chests: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Utility);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
            itemStatsDef.calculateValues = (master, stack) =>
            {
                List<float> values = new()
                {
                    baseExtraCloakedChests + stackExtraCloakedChests * (stack - 1)
                };

                return values;
            };

            return itemStatsDef;
        }

        public void SetUpISC()
        {
            interactableSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            interactableSpawnCard.prefab = Paths.GameObject.Chest1StealthedVariant;
            interactableSpawnCard.sendOverNetwork = true;
            interactableSpawnCard.hullSize = HullClassification.Human;
            interactableSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            interactableSpawnCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
            interactableSpawnCard.directorCreditCost = 0;
            interactableSpawnCard.occupyPosition = true;
            interactableSpawnCard.eliteRules = SpawnCard.EliteRules.Default;
            interactableSpawnCard.orientToFloor = true;
            interactableSpawnCard.slightlyRandomizeOrientation = true;
            interactableSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = true;
            interactableSpawnCard.weightScalarWhenSacrificeArtifactEnabled = 1;
            interactableSpawnCard.skipSpawnWhenDevotionArtifactEnabled = false;
            interactableSpawnCard.maxSpawnsPerStage = -1;
            interactableSpawnCard.prismaticTrialSpawnChance = 1f;
            interactableSpawnCard.name = "iscCloakedChestPocketRadar";
        }

        public void SetUpVFX()
        {
            newCloakedChestMaterial = new Material(Paths.Material.matCloakedEffect);
            newCloakedChestMaterial.SetFloat("_Magnitude", 0.3f); // up from 0.105
            newCloakedChestMaterial.SetFloat("_InvFade", 2f); // up from 0.1
        }

        public override void Hooks()
        {
            SceneDirector.onPrePopulateSceneServer += OnPrePopulateSceneServer;
        }

        private void OnPrePopulateSceneServer(SceneDirector director)
        {
            if (!SceneInfo.instance.countsAsStage && !SceneInfo.instance.sceneDef.allowItemsToSpawnObjects)
            {
                return;
            }

            int stack = 0;

            for (int i = 0; i < PlayerCharacterMasterController._instancesReadOnly.Count; i++)
            {
                var pcmc = PlayerCharacterMasterController._instancesReadOnly[i];
                stack += GetCount(pcmc);
            }

            if (stack <= 0)
            {
                return;
            }

            var finalCount = baseExtraCloakedChests + stackExtraCloakedChests * (stack - 1);

            for (int j = 0; j < finalCount; j++)
            {
                // Main.ModLogger.LogError("trying to place cloaked chest");

                var directorPlacementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                };

                var cloakedChestInstance = DirectorCore.instance.TrySpawnObject(
                    new DirectorSpawnRequest(
                        interactableSpawnCard, directorPlacementRule,
                        Run.instance.runRNG));

                if (cloakedChestInstance)
                {
                    if (cloakedChestInstance.TryGetComponent<ModelLocator>(out var modelLocator))
                    {
                        var modelTransform = modelLocator.modelTransform;
                        var chestSMR = modelTransform.Find("Cube.001").GetComponent<SkinnedMeshRenderer>();
                        chestSMR.material = newCloakedChestMaterial;

                        VFXUtils.AddLight(chestSMR.gameObject, Color.white, 0.4f, 6f);
                    }
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}*/