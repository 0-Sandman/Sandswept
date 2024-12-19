using Newtonsoft.Json.Utilities;
using RoR2.ExpansionManagement;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static RoR2.CombatDirector;

namespace Sandswept.Interactables.Regular
{
    [ConfigSection("Interactables :: Shrine of Death")]
    internal class ShrineOfDeath : InteractableBase<ShrineOfDeath>
    {
        public override string Name => "Shrine of Death";

        public override DirectorAPI.InteractableCategory Category => InteractableCategory.Misc;

        //public override int MaxSpawnsPerStage => 1;
        public override int MaxSpawnsPerStage => 0;

        //public override int CreditCost => 20;
        public override int CreditCost => int.MaxValue;

        public override HullClassification Size => HullClassification.Golem;

        //public override int MinimumStageToAppearOn => 3;
        public override int MinimumStageToAppearOn => 1;

        //public override int SpawnWeight => 1;
        public override int SpawnWeight => 0;

        public override bool SlightlyRandomizeOrientation => false;
        public override bool OrientToFloor => false;

        public override List<Stage> Stages => null;

        public GameObject prefab;

        public static GameObject shrineVFX;

        [ConfigField("Percent Health Cost", "", 20)]
        public static int percentHealthCost;

        [ConfigField("Healing Disable Duration", "", 2f)]
        public static float healingDisableDuration;

        [ConfigField("Cooldown", "", 3f)]
        public static float cooldown;

        [ConfigField("Max Use Count", "", 2147483647)]
        public static int maxUseCount;

        public static GameObject fmpPrefab;

        public List<Vector3> possibleCommencementSpots = new();

        public override void Init()
        {
            base.Init();
            fmpPrefab = Paths.GameObject.DeathProjectile;

            prefab = PrefabAPI.InstantiateClone(Paths.GameObject.ShrineCombat, "Shrine of Death", true);
            Main.ModLogger.LogError("prefab right after instantiating is " + prefab);

            var purchaseInteraction = prefab.GetComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = "SANDSWEPT_SHRINE_DEATH_NAME";
            purchaseInteraction.contextToken = "SANDSWEPT_SHRINE_DEATH_CONTEXT";
            purchaseInteraction.Networkavailable = true;
            purchaseInteraction.costType = CostTypeIndex.PercentHealth;
            purchaseInteraction.cost = percentHealthCost;

            var genericDisplayNameProvider = prefab.GetComponent<GenericDisplayNameProvider>();
            genericDisplayNameProvider.displayToken = "SANDSWEPT_SHRINE_DEATH_NAME";

            prefab.RemoveComponent<ShrineCombatBehavior>();
            prefab.RemoveComponent<CombatDirector>();
            prefab.AddComponent<ShrineOfDeathController>();
            prefab.AddComponent<UnityIsAFuckingPieceOfShit4>();

            var expansionRequirementComponent = prefab.AddComponent<ExpansionRequirementComponent>();
            expansionRequirementComponent.requiredExpansion = Main.SandsweptExpansionDef;

            PrefabAPI.RegisterNetworkPrefab(prefab);
            Main.ModLogger.LogError("prefab right after registering is " + prefab);

            LanguageAPI.Add("SANDSWEPT_SHRINE_DEATH_NAME", "Shrine of Death");
            LanguageAPI.Add("SANDSWEPT_SHRINE_DEATH_CONTEXT", "Offer to Shrine of Death");

            var inspectDef = ScriptableObject.CreateInstance<InspectDef>();
            var inspectInfo = inspectDef.Info = new()
            {
                TitleToken = genericDisplayNameProvider.displayToken,
                DescriptionToken = "SANDSWEPT_SHRINE_DEATH_DESCRIPTION",
                FlavorToken = "A Sex #Sandswept",
                isConsumedItem = false,
                Visual = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texShrineIconOutlined.png").WaitForCompletion(),
                TitleColor = Color.white
            };
            // add this to base later tbh?
            LanguageAPI.Add("SANDSWEPT_SHRINE_DEATH_DESCRIPTION", "When activated by a survivor, the Shrine of Death aaaaa");

            prefab.GetComponent<GenericInspectInfoProvider>().InspectInfo = Object.Instantiate(prefab.GetComponent<GenericInspectInfoProvider>().InspectInfo);
            prefab.GetComponent<GenericInspectInfoProvider>().InspectInfo.Info = inspectInfo;

            shrineVFX = PrefabAPI.InstantiateClone(Utils.Assets.GameObject.ShrineUseEffect, "Shrine of The Death VFX", false);
            shrineVFX.GetComponent<EffectComponent>().soundName = "Play_ui_obj_eradicator_open";
            ContentAddition.AddEffect(shrineVFX);

            possibleCommencementSpots.Add(new Vector3(-116.9045f, 491.6998f, -29.86444f)); // close range
            possibleCommencementSpots.Add(new Vector3(-37.01875f, 491.6998f, 0.1641912f)); // medium range
            possibleCommencementSpots.Add(new Vector3(-116.7904f, 491.6998f, 69.67595f)); // medium-far range

            interactableSpawnCard.prefab = prefab;

            On.RoR2.Stage.Start += Stage_Start;

            PostInit();
        }

        private IEnumerator Stage_Start(On.RoR2.Stage.orig_Start orig, RoR2.Stage self)
        {
            yield return orig(self);
            var sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "moon2")
            {
                var randomPos = possibleCommencementSpots[Run.instance.stageRng.RangeInt(0, possibleCommencementSpots.Count)];

                DirectorPlacementRule directorPlacementRule = new()
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Direct,
                    position = randomPos
                };
                var shrineOfDeathInstance = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(Instance.interactableSpawnCard, directorPlacementRule, Run.instance.stageRng));
            }
        }
    }

    public class UnityIsAFuckingPieceOfShit4 : MonoBehaviour
    {
        public PurchaseInteraction purchaseInteraction;
        public ShrineOfDeathController shrineOfDeathController;

        public void Start()
        {
            shrineOfDeathController = GetComponent<ShrineOfDeathController>();
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            purchaseInteraction.onPurchase.AddListener(SoTrue);
        }

        public void SoTrue(Interactor interactor)
        {
            shrineOfDeathController.AddShrineStack(interactor);
        }
    }

    public class ShrineOfDeathController : ShrineBehavior
    {
        public int maxPurchaseCount = ShrineOfDeath.maxUseCount;

        public float costMultiplierPerPurchase;

        public Transform symbolTransform;

        private PurchaseInteraction purchaseInteraction;

        private int purchaseCount;

        private float refreshTimer;

        private float refreshDuration = ShrineOfDeath.cooldown;

        private bool waitingForRefresh;

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
                    purchaseInteraction.Networkcost = ShrineOfDeath.percentHealthCost;
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

            StartCoroutine(ProcOnKills(interactorBody));

            /*
            if (interactorBody)
            {
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = interactorBody,
                    baseToken = "SANDSWEPT_SHRINE_DEATH_USE_MESSAGE",
                });
            }
            */

            EffectManager.SpawnEffect(ShrineOfDeath.shrineVFX, new EffectData
            {
                origin = base.transform.position,
                rotation = Quaternion.identity,
                scale = 1.5f,
                color = new Color32(96, 20, 87, 255)
            }, true);

            Util.PlaySound("Play_deathProjectile_pulse", gameObject);

            purchaseCount++;
            refreshTimer = ShrineOfDeath.cooldown;
            if (purchaseCount >= maxPurchaseCount)
            {
                symbolTransform.gameObject.SetActive(false);
                CallRpcSetPingable(false);
            }
        }

        public IEnumerator ProcOnKills(CharacterBody interactorBody)
        {
            yield return new WaitForSeconds(0.1f);

            interactorBody.AddTimedBuff(RoR2Content.Buffs.HealingDisabled, ShrineOfDeath.healingDisableDuration);

            var ghostFMP = Object.Instantiate<GameObject>(ShrineOfDeath.fmpPrefab, /*new Vector3(0f, -200f, 0f)*/interactorBody.footPosition, Quaternion.identity);
            ghostFMP.transform.localScale = new Vector3(0f, 0f, 0f);
            ghostFMP.GetComponent<DeathProjectile>().baseDuration = 1.1f;
            Object.Destroy(ghostFMP.GetComponent<DestroyOnTimer>());
            Object.Destroy(ghostFMP.GetComponent<DeathProjectile>());
            Object.Destroy(ghostFMP.GetComponent<ApplyTorqueOnStart>());
            Object.Destroy(ghostFMP.GetComponent<ProjectileDeployToOwner>());
            Object.Destroy(ghostFMP.GetComponent<Deployable>());
            Object.Destroy(ghostFMP.GetComponent<ProjectileStickOnImpact>());
            Object.Destroy(ghostFMP.GetComponent<ProjectileController>());

            ghostFMP.transform.position = interactorBody.footPosition;

            var hc = ghostFMP.GetComponent<HealthComponent>();

            if (NetworkServer.active)
            {
                DamageInfo damageInfo = new()
                {
                    attacker = interactorBody.gameObject,
                    crit = interactorBody.RollCrit(),
                    damage = interactorBody.baseDamage,
                    position = interactorBody.footPosition,
                    procCoefficient = 0f,
                    damageType = DamageType.Generic,
                    damageColorIndex = DamageColorIndex.Item
                };
                var damageReport = new DamageReport(damageInfo, hc, damageInfo.damage, hc.combinedHealth);
                GlobalEventManager.instance.OnCharacterDeath(damageReport);
            }
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