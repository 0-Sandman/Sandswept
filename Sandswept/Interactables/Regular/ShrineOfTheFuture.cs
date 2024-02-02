using static RoR2.CombatDirector;

namespace Sandswept.Interactables.Regular
{
    [ConfigSection("Interactables :: Shrine of the Void")]
    internal class ShrineOfTheFuture : InteractableBase
    {
        public override string Name => "Shrine of the Void";

        public override DirectorAPI.InteractableCategory Category => InteractableCategory.Shrines;

        //public override int MaxSpawnsPerStage => 1;
        public override int MaxSpawnsPerStage => 10;

        //public override int CreditCost => 20;
        public override int CreditCost => 0;

        public override HullClassification Size => HullClassification.BeetleQueen;

        //public override int MinimumStageToAppearOn => 3;
        public override int MinimumStageToAppearOn => 1;

        //public override int SpawnWeight => 1;
        public override int SpawnWeight => 100000;

        public override bool SlightlyRandomizeOrientation => false;
        public override bool OrientToFloor => false;

        public GameObject prefab;

        public override void Init()
        {
            base.Init();
            prefab = PrefabAPI.InstantiateClone(Assets.GameObject.ShrineCombat, "Shrine of the Future");

            var purchaseInteraction = prefab.GetComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = "SANDSWEPT_SHRINE_FUTURE_NAME";
            purchaseInteraction.contextToken = "SANDSWEPT_SHRINE_FUTURE_CONTEXT";
            // purchaseInteraction.onPurchase.AddListener(delegate { EnableCombatDirector(prefab); });

            var genericDisplayNameProvider = prefab.GetComponent<GenericDisplayNameProvider>();
            genericDisplayNameProvider.displayToken = "SANDSWEPT_SHRINE_FUTURE_NAME";

            var shrineCombatBehavior = prefab.GetComponent<ShrineCombatBehavior>();
            shrineCombatBehavior.baseMonsterCredit = 150; // the higher end of miniboss cost
            shrineCombatBehavior.shrineEffectColor = new Color32(69, 71, 238, 255);

            var combatDirector = prefab.GetComponent<CombatDirector>();
            combatDirector.maximumNumberToSpawnBeforeSkipping = 1;
            combatDirector.skipSpawnIfTooCheap = true;
            combatDirector.shouldSpawnOneWave = true;
            combatDirector.spawnDistanceMultiplier = 0.66f;
            combatDirector.goldRewardCoefficient = 0f;
            combatDirector.customName = "ShrineOfTheFutureDirector";

            interactableSpawnCard.prefab = prefab;

            var combatSquad = prefab.GetComponent<CombatSquad>();
            combatSquad.onDefeatedServerLogicEvent.AddListener(delegate { SpawnRewards(prefab); });

            PrefabAPI.RegisterNetworkPrefab(prefab);

            LanguageAPI.Add("SANDSWEPT_SHRINE_FUTURE_NAME", "Shrine of the Future");
            LanguageAPI.Add("SANDSWEPT_SHRINE_FUTURE_CONTEXT", "Defy");

            On.RoR2.CombatDirector.Spawn += CombatDirector_Spawn;

            PostInit();
        }

        public static void EnableCombatDirector(GameObject shrine)
        {
            shrine.GetComponent<CombatDirector>().enabled = true;
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

        private bool CombatDirector_Spawn(On.RoR2.CombatDirector.orig_Spawn orig, CombatDirector self, SpawnCard spawnCard, EliteDef eliteDef, Transform spawnTarget, DirectorCore.MonsterSpawnDistance spawnDistance, bool preventOverhead, float valueMultiplier, DirectorPlacementRule.PlacementMode placementMode)
        {
            // we're gonna cheat a bit hehe
            if (self.customName == "ShrineOfTheFutureDirector")
            {
                Main.ModLogger.LogError("elite tier at index 0 is " + eliteTiers[0]);
                Main.ModLogger.LogError("elite tier at index 1 is " + eliteTiers[1]);
                Main.ModLogger.LogError("elite tier at index 2 is " + eliteTiers[2]);
                Main.ModLogger.LogError("random elite from index 0 is " + eliteTiers[0].GetRandomAvailableEliteDef(self.rng));
                Main.ModLogger.LogError("random elite from index 1 is " + eliteTiers[1].GetRandomAvailableEliteDef(self.rng));
                Main.ModLogger.LogError("random elite from index 2 is " + eliteTiers[2].GetRandomAvailableEliteDef(self.rng));
                eliteDef = eliteTiers[2].GetRandomAvailableEliteDef(self.rng); // always t2
            }
            return orig(self, spawnCard, eliteDef, spawnTarget, spawnDistance, preventOverhead, valueMultiplier, placementMode);
        }
    }
}