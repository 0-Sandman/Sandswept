using System;
using System.Collections;
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

        [ConfigField("Item Count Per Player", "", 4)]
        public static int itemCount;

        public override void Init()
        {
            base.Init();
            prefab = PrefabAPI.InstantiateClone(Paths.GameObject.ShrineCombat, "Shrine of the Future", true);

            var modelBase = prefab.transform.Find("Base");
            modelBase.localPosition = new Vector3(0f, 6f, 0f);

            var mdl = modelBase.Find("mdlShrineCombat").gameObject;
            mdl.name = "mdlShrineOfTheFuture";
            mdl.transform.localScale = Vector3.one * 400f;

            mdl.RemoveComponent<EntityLocator>();
            var newEntityLocator = modelBase.AddComponent<EntityLocator>();
            newEntityLocator.entity = prefab;

            var collision = mdl.transform.Find("Collision");
            collision.localScale = Vector3.one / 400f;

            var shrineMat = Main.hifuSandswept.LoadAsset<Material>("matShrineOfTheFuture.mat");
            shrineMat.SetTexture("_FlowHeightRamp", Paths.Texture2D.texRampDeathmark);

            mdl.GetComponent<MeshFilter>().sharedMesh = Main.hifuSandswept.LoadAsset<Mesh>("mdlShrineOfTheFuture.fbx");
            mdl.GetComponent<MeshRenderer>().sharedMaterial = shrineMat;

            var symbol = prefab.transform.Find("Symbol");
            // symbol.localPosition = new(0, 6, 0);
            var meshRenderer = symbol.GetComponent<MeshRenderer>();
            meshRenderer.material.mainTexture = Main.prodAssets.LoadAsset<Texture2D>("assets/sandswept/shrinesacrificeicon.png");
            meshRenderer.material.SetColor("_TintColor", new Color32(20, 74, 96, 255));

            var purchaseInteraction = prefab.GetComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = "SANDSWEPT_SHRINE_FUTURE_NAME";
            purchaseInteraction.contextToken = "SANDSWEPT_SHRINE_FUTURE_CONTEXT";
            purchaseInteraction.Networkavailable = true;
            purchaseInteraction.costType = CostTypeIndex.None;
            purchaseInteraction.cost = 0;

            var genericDisplayNameProvider = prefab.GetComponent<GenericDisplayNameProvider>();
            genericDisplayNameProvider.displayToken = "SANDSWEPT_SHRINE_FUTURE_NAME";

            prefab.RemoveComponent<ShrineCombatBehavior>();
            prefab.RemoveComponent<CombatDirector>();

            prefab.AddComponent<ShrineOfTheFutureController>();

            prefab.AddComponent<UnityIsAFuckingPieceOfShit3>();

            interactableSpawnCard.prefab = prefab;

            var combatSquad = prefab.GetComponent<CombatSquad>();
            combatSquad.grantBonusHealthInMultiplayer = true;

            var spawnInfos = new ScriptedCombatEncounter.SpawnInfo[8];
            spawnInfos[0] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscBeetleGuard };
            spawnInfos[1] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscBell };
            spawnInfos[2] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscClayGrenadier };
            spawnInfos[3] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscClayBruiser };
            spawnInfos[4] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscLemurianBruiser };
            spawnInfos[5] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscGreaterWisp };
            spawnInfos[6] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscParent };
            spawnInfos[7] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscGolem };

            // sorry for hardcoding :<

            var scriptedCombatEncounter = prefab.AddComponent<ScriptedCombatEncounter>();
            scriptedCombatEncounter.grantUniqueBonusScaling = false;
            scriptedCombatEncounter.spawns = spawnInfos;
            scriptedCombatEncounter.teamIndex = TeamIndex.Monster;
            scriptedCombatEncounter.randomizeSeed = true;
            scriptedCombatEncounter.spawnOnStart = false;

            PrefabAPI.RegisterNetworkPrefab(prefab);

            LanguageAPI.Add("SANDSWEPT_SHRINE_FUTURE_NAME", "Shrine of the Future");
            LanguageAPI.Add("SANDSWEPT_SHRINE_FUTURE_CONTEXT", "Offer to Shrine of The Future");

            var inspectDef = ScriptableObject.CreateInstance<InspectDef>();
            var inspectInfo = inspectDef.Info = new()
            {
                TitleToken = genericDisplayNameProvider.displayToken,
                DescriptionToken = "SANDSWEPT_SHRINE_FUTURE_DESCRIPTION",
                FlavorToken = "Nonbinary Sex #Sandswept",
                isConsumedItem = false,
                Visual = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texShrineIconOutlined.png").WaitForCompletion(),
                TitleColor = Color.white
            };
            // add this to base later tbh?
            LanguageAPI.Add("SANDSWEPT_SHRINE_FUTURE_DESCRIPTION", "When activated by a survivor, the Shrine of The Future spawns a random strong elite miniboss that when defeated, drops " + itemCount + " items per player.");

            LanguageAPI.Add("SANDSWEPT_SHRINE_FUTURE_USE_MESSAGE_2P", "<style=cShrine>Time has shifted.</color>");
            LanguageAPI.Add("SANDSWEPT_SHRINE_FUTURE_USE_MESSAGE", "<style=cShrine>Time has warped.</color>");

            prefab.GetComponent<GenericInspectInfoProvider>().InspectInfo.Info = inspectInfo;

            On.RoR2.ClassicStageInfo.RebuildCards += ClassicStageInfo_RebuildCards;

            PostInit();
        }

        private void ClassicStageInfo_RebuildCards(On.RoR2.ClassicStageInfo.orig_RebuildCards orig, ClassicStageInfo self, DirectorCardCategorySelection forcedMonsterCategory, DirectorCardCategorySelection forcedInteractableCategory)
        {
            orig(self, forcedMonsterCategory, forcedInteractableCategory);
            if (Run.instance.stageClearCount >= 6)
            {
                self.interactableCategories.RemoveCardsThatFailFilter(x => x.spawnCard != interactableSpawnCard);
            }
        }
    }

    public class UnityIsAFuckingPieceOfShit3 : MonoBehaviour
    {
        public PurchaseInteraction purchaseInteraction;
        public ShrineOfTheFutureController shrineOfTheFutureController;

        public void Start()
        {
            shrineOfTheFutureController = GetComponent<ShrineOfTheFutureController>();
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            purchaseInteraction.onPurchase.AddListener(SoTrue);
        }

        public void SoTrue(Interactor interactor)
        {
            shrineOfTheFutureController.AddShrineStack(interactor);
        }
    }

    public class ShrineOfTheFutureController : ShrineBehavior
    {
        public int maxPurchaseCount = 1;

        public float costMultiplierPerPurchase;

        public Transform symbolTransform;

        private PurchaseInteraction purchaseInteraction;

        private int purchaseCount;

        private float refreshTimer;

        private const float refreshDuration = 2f;

        private bool waitingForRefresh;

        public ScriptedCombatEncounter scriptedCombatEncounter;
        public CombatSquad combatSquad;

        public override int GetNetworkChannel()
        {
            return RoR2.Networking.QosChannelIndex.defaultReliable.intVal;
        }

        private void Start()
        {
            // Main.ModLogger.LogError("shrine sacrifice behavior start");
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            symbolTransform = transform.Find("Symbol");

            scriptedCombatEncounter = GetComponent<ScriptedCombatEncounter>();

            combatSquad = GetComponent<CombatSquad>();
            combatSquad.onMemberAddedServer += CombatSquad_onMemberAddedServer;
            combatSquad.onDefeatedServer += CombatSquad_onDefeatedServer;

            var randomIndex1 = scriptedCombatEncounter.spawns[Run.instance.stageRng.RangeInt(0, scriptedCombatEncounter.spawns.Length)];
            // var randomIndex2 = scriptedCombatEncounter.spawns[Run.instance.stageRng.RangeInt(0, scriptedCombatEncounter.spawns.Length)];

            // ScriptedCombatEncounter.SpawnInfo[] randomSpawn = new ScriptedCombatEncounter.SpawnInfo[2] { randomIndex1, randomIndex2 };
            ScriptedCombatEncounter.SpawnInfo[] randomSpawn = new ScriptedCombatEncounter.SpawnInfo[1] { randomIndex1 };

            scriptedCombatEncounter.spawns = randomSpawn;
        }

        public Dictionary<EliteDef, EquipmentIndex> GetRandomT2EliteDefToEquipmentIndexPair()
        {
            var tier2EliteTypes = EliteAPI.VanillaEliteTiers[4].eliteTypes;

            var tier2ElitesExceptFuckOffTwisted = tier2EliteTypes.Where(x => x.eliteEquipmentDef != Paths.EquipmentDef.EliteBeadEquipment).ToList();

            var randomElite = tier2ElitesExceptFuckOffTwisted[Run.instance.runRNG.RangeInt(0, tier2ElitesExceptFuckOffTwisted.Count)];
            return new Dictionary<EliteDef, EquipmentIndex>()
            {
                { randomElite, randomElite.eliteEquipmentDef.equipmentIndex }
            };
        }

        private void CombatSquad_onMemberAddedServer(CharacterMaster master)
        {
            var inventory = master.inventory;
            if (!inventory)
            {
                return;
            }

            var randomPair = GetRandomT2EliteDefToEquipmentIndexPair();
            var equipmentIndex = randomPair.Values.First();
            var eliteDef = randomPair.Keys.First();

            inventory.SetEquipmentIndex(equipmentIndex);
            inventory.GiveItem(RoR2Content.Items.BoostHp, Mathf.Max(0, Mathf.RoundToInt(((eliteDef.healthBoostCoefficient * 0.5f) - 1f) * 10f)));
            inventory.GiveItem(RoR2Content.Items.BoostDamage, Mathf.Max(0, Mathf.RoundToInt(((eliteDef.damageBoostCoefficient * 0.5f) - 1f) * 10f)));
        }

        private void CombatSquad_onDefeatedServer()
        {
            SpawnRewards();
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

            // Main.ModLogger.LogError("interactor body is " + interactorBody);

            StartCoroutine(SpawnEnemies());

            if (interactorBody)
            {
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = interactorBody,
                    baseToken = "SANDSWEPT_SHRINE_FUTURE_USE_MESSAGE",
                });
            }

            EffectManager.SpawnEffect(ShrineOfSacrifice.shrineVFX, new EffectData
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
            }
        }

        public IEnumerator SpawnEnemies()
        {
            yield return new WaitForSeconds(1f);
            scriptedCombatEncounter.BeginEncounter();
        }

        public void SpawnRewards()
        {
            int itemCount = Run.instance.participatingPlayerCount * ShrineOfTheFuture.itemCount;
            float angle = 360f / itemCount;
            Vector3 vector = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);

            for (int i = 0; i < itemCount; i++)
            {
                GenericPickupController.CreatePickupInfo info = new()
                {
                    position = transform.position + new Vector3(0, 8f, 0),
                    prefabOverride = Paths.GameObject.OptionPickup,
                    rotation = Quaternion.identity,
                    pickupIndex = PickupCatalog.FindPickupIndex(ItemTier.Tier2),
                    pickerOptions = GenerateOptions(),
                };

                PickupDropletController.CreatePickupDroplet(info, transform.position + new Vector3(0, 8f, 0), vector);
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