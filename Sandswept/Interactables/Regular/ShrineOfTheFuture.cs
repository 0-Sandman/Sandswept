using System;
using System.Linq;
using static RoR2.CombatDirector;

namespace Sandswept.Interactables.Regular
{
    [ConfigSection("Interactables :: Shrine of the Future")]
    internal class ShrineOfTheFuture : InteractableBase<ShrineOfTheFuture>
    {
        public override string Name => "Shrine of the Future";

        public override DirectorAPI.InteractableCategory Category => InteractableCategory.Shrines;

        //public override int MaxSpawnsPerStage => 1;
        public override int MaxSpawnsPerStage => 1;

        //public override int CreditCost => 20;
        public override int CreditCost => 40;

        public override HullClassification Size => HullClassification.BeetleQueen;

        //public override int MinimumStageToAppearOn => 3;
        public override int MinimumStageToAppearOn => 3;

        //public override int SpawnWeight => 1;
        public override int SpawnWeight => 1;

        public override bool SlightlyRandomizeOrientation => false;
        public override bool OrientToFloor => false;

        public GameObject prefab;

        public static bool isCorrectDirector = false;

        public override void Init()
        {
            base.Init();
            prefab = PrefabAPI.InstantiateClone(Assets.GameObject.ShrineCombat, "Shrine of the Future", true);

            var purchaseInteraction = prefab.GetComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = "SANDSWEPT_SHRINE_FUTURE_NAME";
            purchaseInteraction.contextToken = "SANDSWEPT_SHRINE_FUTURE_CONTEXT";
            purchaseInteraction.Networkavailable = true;

            var genericDisplayNameProvider = prefab.GetComponent<GenericDisplayNameProvider>();
            genericDisplayNameProvider.displayToken = "SANDSWEPT_SHRINE_FUTURE_NAME";

            var shrineCombatBehavior = prefab.GetComponent<ShrineCombatBehavior>();
            shrineCombatBehavior.baseMonsterCredit = 150; // the higher end of miniboss cost
            shrineCombatBehavior.shrineEffectColor = new Color32(69, 71, 238, 255);

            var combatDirector = prefab.GetComponent<CombatDirector>();
            combatDirector.maximumNumberToSpawnBeforeSkipping = 1;
            combatDirector.skipSpawnIfTooCheap = false;
            combatDirector.shouldSpawnOneWave = true;
            combatDirector.spawnDistanceMultiplier = 0.66f;
            combatDirector.goldRewardCoefficient = 0f;
            combatDirector.customName = "ShrineOfTheFutureDirector";
            combatDirector.minRerollSpawnInterval = 0.01f;
            combatDirector.maxRerollSpawnInterval = 0.03f;

            interactableSpawnCard.prefab = prefab;

            var combatSquad = prefab.GetComponent<CombatSquad>();
            combatSquad.onDefeatedServerLogicEvent.AddListener(delegate { SpawnRewards(combatSquad.gameObject); });

            PrefabAPI.RegisterNetworkPrefab(prefab);

            LanguageAPI.Add("SANDSWEPT_SHRINE_FUTURE_NAME", "Shrine of the Future");
            LanguageAPI.Add("SANDSWEPT_SHRINE_FUTURE_CONTEXT", "Defy");

            On.RoR2.CombatDirector.Spawn += CombatDirector_Spawn;
            On.RoR2.DirectorCore.TrySpawnObject += DirectorCore_TrySpawnObject;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
            On.RoR2.ClassicStageInfo.RebuildCards += ClassicStageInfo_RebuildCards;

            PostInit();
        }

        private bool CombatDirector_Spawn(On.RoR2.CombatDirector.orig_Spawn orig, CombatDirector self, SpawnCard spawnCard, EliteDef eliteDef, Transform spawnTarget, DirectorCore.MonsterSpawnDistance spawnDistance, bool preventOverhead, float valueMultiplier, DirectorPlacementRule.PlacementMode placementMode)
        {
            isCorrectDirector = false;
            if (spawnCard.prefab && spawnCard.prefab.TryGetComponent<CharacterMaster>(out var master) && master.bodyPrefab && master.bodyPrefab.TryGetComponent<CharacterBody>(out var body))
            {
                if (body.isChampion)
                {
                    spawnCard = null;
                }

                if (self.customName == "ShrineOfTheFutureDirector" && spawnCard != null)
                {
                    isCorrectDirector = true;
                }
            }

            return orig(self, spawnCard, eliteDef, spawnTarget, spawnDistance, preventOverhead, valueMultiplier, placementMode);
        }

        private GameObject DirectorCore_TrySpawnObject(On.RoR2.DirectorCore.orig_TrySpawnObject orig, DirectorCore self, DirectorSpawnRequest directorSpawnRequest)
        {
            // run only if director is shrine of the future directorrrr :sob:
            if (isCorrectDirector)
            {
                var randomPair = GetRandomT2EliteDefToEquipmentIndexPair();
                var equipmentIndex = randomPair.Values.First();
                var eliteDef = randomPair.Keys.First();

                directorSpawnRequest.onSpawnedServer = (spawnResult) =>
                {
                    var instance = spawnResult.spawnedInstance;
                    var master = instance.GetComponent<CharacterMaster>();
                    master.inventory.SetEquipmentIndex(equipmentIndex);
                    master.inventory.GiveItem(RoR2Content.Items.BoostHp, Mathf.RoundToInt((eliteDef.healthBoostCoefficient - 1f) * 10f));
                    master.inventory.GiveItem(RoR2Content.Items.BoostDamage, Mathf.RoundToInt((eliteDef.damageBoostCoefficient - 1f) * 10f));
                };
            }

            return orig(self, directorSpawnRequest);
        }

        private void ClassicStageInfo_RebuildCards(On.RoR2.ClassicStageInfo.orig_RebuildCards orig, ClassicStageInfo self)
        {
            orig(self);

            if (Run.instance.loopClearCount > 0)
            {
                self.interactableCategories.RemoveCardsThatFailFilter(x => x.spawnCard != Instance.interactableSpawnCard);
            }
        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            var shrineCombatBehaviors = GameObject.FindObjectsOfType<ShrineCombatBehavior>();
            foreach (ShrineCombatBehavior shrineCombatBehavior in shrineCombatBehaviors)
            {
                Main.ModLogger.LogError("found shrine " + shrineCombatBehavior.name);
            }
        }

        public static void SpawnRewards(GameObject shrine)
        {
            int itemCount = Run.instance.participatingPlayerCount * 4;
            float angle = 360f / itemCount;
            Vector3 vector = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);

            for (int i = 0; i < itemCount; i++)
            {
                GenericPickupController.CreatePickupInfo info = new()
                {
                    position = shrine.transform.position + new Vector3(0, 3f, 0),
                    prefabOverride = Assets.GameObject.OptionPickup,
                    rotation = Quaternion.identity,
                    pickupIndex = PickupCatalog.FindPickupIndex(ItemTier.Lunar),
                    pickerOptions = GenerateOptions()
                };

                PickupDropletController.CreatePickupDroplet(info, shrine.transform.position + new Vector3(0, 3f, 0), vector);
                vector = quaternion * vector;
            }
        }

        public static PickupPickerController.Option[] GenerateOptions()
        {
            PickupPickerController.Option white = new()
            {
                available = true,
                pickupIndex = Run.instance.availableTier1DropList[Run.instance.treasureRng.RangeInt(0, Run.instance.availableTier1DropList.Count - 1)]
            };

            PickupPickerController.Option green = new()
            {
                available = true,
                pickupIndex = Run.instance.availableTier2DropList[Run.instance.treasureRng.RangeInt(0, Run.instance.availableTier2DropList.Count - 1)]
            };

            return new PickupPickerController.Option[] { white, green };
        }

        public Dictionary<EliteDef, EquipmentIndex> GetRandomT2EliteDefToEquipmentIndexPair()
        {
            var tier2Elites = EliteAPI.VanillaEliteTiers[3].eliteTypes;
            var randomElite = tier2Elites[Run.instance.runRNG.RangeInt(0, tier2Elites.Length)];
            return new Dictionary<EliteDef, EquipmentIndex>()
            {
                { randomElite, randomElite.eliteEquipmentDef.equipmentIndex }
            };
        }
    }
}