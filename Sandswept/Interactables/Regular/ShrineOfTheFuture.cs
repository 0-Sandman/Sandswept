using IL.RoR2.UI;
using Newtonsoft.Json.Utilities;
using RoR2.ExpansionManagement;
using Sandswept.Enemies.DeltaConstruct;
using Sandswept.Enemies.GammaConstruct;
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
        public override int CreditCost => directorCreditCost;

        public override HullClassification Size => HullClassification.Golem;

        //public override int MinimumStageToAppearOn => 3;
        public override int MinimumStageToAppearOn => 3;

        //public override int SpawnWeight => 1;
        public override int SpawnWeight => 1;

        public override bool SlightlyRandomizeOrientation => false;
        public override bool OrientToFloor => false;

        public override string inspectInfoDescription => $"When activated by a survivor, the Shrine of The Future spawns a random strong elite miniboss for each player, that when defeated, drops {itemCount} item potentials per player.";

        public GameObject prefab;

        public static GameObject shrineVFX;

        [ConfigField("Base Enemy Count", "", 1)]
        public static int baseEnemyCount;

        [ConfigField("Enemy Count Per Player", "", 1)]
        public static int enemyCountPerPlayer;

        [ConfigField("Director Credit Cost", "", 35)]
        public static int directorCreditCost;

        [ConfigField("Item Count Per Player", "", 3)]
        public static int itemCount;

        [ConfigField("Enemy Stats Multiplier", "", 0.5f)]
        public static float enemyStatsMultiplier;

        [ConfigField("Max Stage To Spawn On", "", 8)]
        public static int maxStageToSpawnOn;

        public override void Init()
        {
            base.Init();
            prefab = PrefabAPI.InstantiateClone(Paths.GameObject.ShrineCombat, "Shrine of the Future", true);
            var modelBase = prefab.transform.Find("Base");
            modelBase.transform.localPosition = new(0, 1.1f, 0);
            var mdl = Main.prodAssets.LoadAsset<GameObject>("assets/sandswept/shrinefuture.fbx");
            prefab.GetComponent<ModelLocator>().modelTransform = mdl.transform;
            Object.Destroy(prefab.GetComponent<Highlight>());
            var hightlight = prefab.AddComponent<MultiHighlight>();
            hightlight.targetRenderer = mdl.GetComponent<Renderer>();
            hightlight.others = [mdl.transform.Find("Stem").GetComponent<Renderer>(), mdl.transform.Find("Stem/Crystal").GetComponent<Renderer>()];
            mdl.name = "mdlShrineOfTheFuture";
            mdl.transform.localScale = Vector3.one * 70;
            mdl.AddComponent<EntityLocator>().entity = prefab;
            mdl.AddComponent<ChildLocator>().transformPairs = new ChildLocator.NameTransformPair[] { new() { name = "FireworkOrigin", transform = prefab.transform.Find("Symbol") } };

            var areYouFuckingKiddingMe = mdl.GetComponent<MeshRenderer>();
            areYouFuckingKiddingMe.material.shader = Paths.Shader.HopooGamesDeferredStandard;

            var from = modelBase.Find("mdlShrineCombat").GetComponent<BoxCollider>();
            var to = mdl.AddComponent<BoxCollider>();
            to.center = Vector3.zero; to.size = Vector3.one * 0.04f;
            var stem = mdl.transform.Find("Stem");
            stem.AddComponent<BoxCollider>().size = Vector3.zero;

            var stemMeshRenderer = stem.GetComponent<MeshRenderer>();
            var mat = stemMeshRenderer.material;
            mat.shader = Paths.Shader.HopooGamesDeferredStandard;
            mat.SetColor("_TintColor", Color.white);
            mat.SetColor("_EmColor", new Color32(15, 19, 38, 255));
            mat.SetTexture("_NormalTex", Paths.Texture2D.texNormalBumpyRock);
            mat.SetFloat("_NormalStrength", 1f);

            areYouFuckingKiddingMe.material = stemMeshRenderer.material;

            mdl.transform.Find("Stem/Crystal").AddComponent<BoxCollider>().size = Vector3.zero;
            mdl.transform.Find("Stem/Crystal").GetComponent<MeshRenderer>().material = Main.hifuSandswept.LoadAsset<Material>("assets/sandswept/interactables/shrineofthefuture/matshrineofthefuturediamonddiffuse2.mat");
            modelBase.Find("mdlShrineCombat").gameObject.SetActive(false);
            mdl.transform.parent = modelBase;

            var symbol = prefab.transform.Find("Symbol");
            symbol.localPosition = new(0, 9, 0);
            var symbolMeshRenderer = symbol.GetComponent<MeshRenderer>();
            var symbolMaterial = symbolMeshRenderer.material;
            symbolMaterial.SetColor("_TintColor", new Color32(0, 72, 255, 255));
            symbolMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            symbolMaterial.SetFloat("_AlphaBoost", 3f);
            symbolMaterial.mainTexture = Main.prodAssets.LoadAsset<Texture2D>("assets/sandswept/shrinefutureicon.png");

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

            var spawnInfos = new ScriptedCombatEncounter.SpawnInfo[10];
            spawnInfos[0] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscBeetleGuard };
            spawnInfos[1] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscBell };
            spawnInfos[2] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscClayGrenadier };
            spawnInfos[3] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscClayBruiser };
            spawnInfos[4] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscLemurianBruiser };
            spawnInfos[5] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscGreaterWisp };
            spawnInfos[6] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscParent };
            spawnInfos[7] = new() { cullChance = 0f, spawnCard = Utils.Assets.CharacterSpawnCard.cscGolem };
            spawnInfos[8] = new() { cullChance = 0f, spawnCard = DeltaConstruct.Instance.csc };
            spawnInfos[9] = new() { cullChance = 0f, spawnCard = GammaConstruct.Instance.csc };

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

            LanguageAPI.Add("SANDSWEPT_SHRINE_FUTURE_USE_MESSAGE_2P", "<style=cShrine>Time has shifted.</color>");
            LanguageAPI.Add("SANDSWEPT_SHRINE_FUTURE_USE_MESSAGE", "<style=cShrine>Time has warped.</color>");

            shrineVFX = PrefabAPI.InstantiateClone(Utils.Assets.GameObject.ShrineUseEffect, "Shrine of The Future VFX", false);
            shrineVFX.GetComponent<EffectComponent>().soundName = "Play_ui_obj_nullWard_complete";
            ContentAddition.AddEffect(shrineVFX);

            On.RoR2.ClassicStageInfo.RebuildCards += ClassicStageInfo_RebuildCards;

            PostInit();
        }

        private void ClassicStageInfo_RebuildCards(On.RoR2.ClassicStageInfo.orig_RebuildCards orig, ClassicStageInfo self, DirectorCardCategorySelection forcedMonsterCategory, DirectorCardCategorySelection forcedInteractableCategory)
        {
            orig(self, forcedMonsterCategory, forcedInteractableCategory);
            if (Run.instance && Run.instance.stageClearCount >= maxStageToSpawnOn)
            {
                self.interactableCategories.RemoveCardsThatFailFilter(x => x.spawnCard != interactableSpawnCard);
            }
        }
    }

    public class MultiHighlight : Highlight
    { public Renderer[] others = new Renderer[] { }; }

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

        public PurchaseInteraction purchaseInteraction;

        public int purchaseCount = 0;

        public float refreshTimer = 2f;

        public const float refreshDuration = 2f;

        public bool waitingForRefresh;

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

            var finalCount = ShrineOfTheFuture.baseEnemyCount + (ShrineOfTheFuture.enemyCountPerPlayer * Run.instance.participatingPlayerCount);

            List<ScriptedCombatEncounter.SpawnInfo> spawns = new();
            for (int i = 0; i < finalCount; i++)
            {
                spawns.Add(scriptedCombatEncounter.spawns[Run.instance.spawnRng.RangeInt(0, scriptedCombatEncounter.spawns.Length)]);
            }

            scriptedCombatEncounter.spawns = spawns.ToArray();
        }

        public Dictionary<EliteDef, EquipmentIndex> GetRandomT2EliteDefToEquipmentIndexPair()
        {
            var tier2EliteTypes = EliteAPI.VanillaEliteTiers[5].eliteTypes;

            // var tier2ElitesExceptFuckOffTwisted = tier2EliteTypes.Where(x => x.eliteEquipmentDef != Paths.EquipmentDef.EliteBeadEquipment).ToList();
            // twisted got fixed

            var randomElite = tier2EliteTypes[Run.instance.runRNG.RangeInt(0, tier2EliteTypes.Length)];
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
            if (inventory.GetItemCount(RoR2Content.Items.UseAmbientLevel) <= 0)
            {
                inventory.GiveItem(RoR2Content.Items.UseAmbientLevel);
            }
            // just in case fix lol
            inventory.GiveItem(RoR2Content.Items.BoostHp, Mathf.Max(0, Mathf.RoundToInt(((eliteDef.healthBoostCoefficient * ShrineOfTheFuture.enemyStatsMultiplier) - 1f) * 10f)));
            inventory.GiveItem(RoR2Content.Items.BoostDamage, Mathf.Max(0, Mathf.RoundToInt(((eliteDef.damageBoostCoefficient * ShrineOfTheFuture.enemyStatsMultiplier) - 1f) * 10f)));
        }

        private void CombatSquad_onDefeatedServer()
        {
            SpawnRewards();
            if (purchaseCount >= maxPurchaseCount)
            {
                symbolTransform.gameObject.SetActive(false);
                CallRpcSetPingable(false);
            }
            else
            {
                purchaseInteraction.SetAvailable(true);
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

            EffectManager.SpawnEffect(ShrineOfTheFuture.shrineVFX, new EffectData
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

            symbolTransform.gameObject.SetActive(false);
            CallRpcSetPingable(false);
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
                pickupIndex = Run.instance.availableTier1DropList[Run.instance.treasureRng.RangeInt(0, Run.instance.availableTier1DropList.Count)]
            };

            PickupPickerController.Option green = new()
            {
                available = true,
                pickupIndex = Run.instance.availableTier2DropList[Run.instance.treasureRng.RangeInt(0, Run.instance.availableTier2DropList.Count)]
            };

            return new PickupPickerController.Option[] { white, green };
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