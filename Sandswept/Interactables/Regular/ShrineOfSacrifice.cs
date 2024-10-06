using R2API.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;
using static Rewired.UI.ControlMapper.ControlMapper;

namespace Sandswept.Interactables.Regular
{
    [ConfigSection("Interactables :: Shrine of Sacrifice")]
    internal class ShrineOfSacrifice : InteractableBase<ShrineOfSacrifice>
    {
        public override string Name => "Shrine of Sacrifice";

        public override InteractableCategory Category => InteractableCategory.Shrines;

        public override int MaxSpawnsPerStage => 1;

        public override int CreditCost => 30;

        public override HullClassification Size => HullClassification.Golem;

        public override int MinimumStageToAppearOn => 1;

        public override int SpawnWeight => 1;

        public GameObject prefab;

        public override bool OrientToFloor => true;
        public override bool SkipOnSacrifice => true;

        public override bool SpawnInVoid => true;

        public override bool SpawnInSimulacrum => true;

        public override bool SlightlyRandomizeOrientation => false;

        public override void Init()
        {
            base.Init();

            prefab = PrefabAPI.InstantiateClone(Paths.GameObject.ShrineBlood, "Shrine of Sacrifice", true);
            var mdl = prefab.transform.Find("Base/mdlShrineHealing").gameObject;
            mdl.name = "mdlShrineSacrifice";
            mdl.GetComponent<MeshFilter>().sharedMesh = Main.prodAssets.LoadAsset<Mesh>("assets/sandswept/shrinesacrifice.fbx");
            mdl.GetComponent<MeshRenderer>().sharedMaterial = Main.prodAssets.LoadAsset<Material>("assets/sandswept/shrinesacrifice.fbx");
            prefab.transform.Find("Symbol").localPosition = new(0, 4, 0);
            prefab.transform.Find("Symbol").GetComponent<MeshRenderer>().material.mainTexture = Main.prodAssets.LoadAsset<Texture2D>("assets/sandswept/shrinesacrificeicon.png");
            prefab.transform.Find("Symbol").GetComponent<MeshRenderer>().material.SetColor("_TintColor", new Color(96 / 255f, 20 / 255f, 87 / 255f, 255 / 255f));

            var purchaseInteraction = prefab.GetComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = "SANDSWEPT_SHRINE_SACRIFICE_NAME";
            purchaseInteraction.contextToken = "SANDSWEPT_SHRINE_SACRIFICE_CONTEXT";
            purchaseInteraction.Networkavailable = true;
            purchaseInteraction.costType = CostTypeIndex.SoulCost;
            purchaseInteraction.cost = 20;

            var genericDisplayNameProvider = prefab.GetComponent<GenericDisplayNameProvider>();
            genericDisplayNameProvider.displayToken = "SANDSWEPT_SHRINE_SACRIFICE_NAME";

            UnityEngine.Object.DestroyImmediate(prefab.GetComponent<ShrineBloodBehavior>()); // kill yourself

            prefab.AddComponent<ShrineSacrificeBehavior>();

            prefab.AddComponent<UnityIsAFuckingPieceOfShit>();

            PrefabAPI.RegisterNetworkPrefab(prefab);

            LanguageAPI.Add("SANDSWEPT_SHRINE_SACRIFICE_NAME", "Shrine of Sacrifice");
            LanguageAPI.Add("SANDSWEPT_SHRINE_SACRIFICE_CONTEXT", "Offer to Shrine of Sacrifice");

            var inspectDef = ScriptableObject.CreateInstance<InspectDef>();
            var inspectInfo = inspectDef.Info = new()
            {
                TitleToken = genericDisplayNameProvider.displayToken,
                DescriptionToken = "SANDSWEPT_SHRINE_SACRIFICE_DESCRIPTION",
                FlavorToken = "Gay Sex #Sandswept",
                isConsumedItem = false,
                Visual = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texShrineIconOutlined.png").WaitForCompletion(),
                TitleColor = Color.white
            };
            // add this to base later tbh?
            LanguageAPI.Add("SANDSWEPT_SHRINE_SACRIFICE_DESCRIPTION", "When activated by a survivor the Shrine of Sacrifice consumes a percentage of the survivors maximum health in exchange for two items.");

            LanguageAPI.Add("SANDSWEPT_SHRINE_SACRIFICE_USE_MESSAGE_2P", "<style=cShrine>You have sacrificed time and have been rewarded.</color>");
            LanguageAPI.Add("SANDSWEPT_SHRINE_SACRIFICE_USE_MESSAGE", "<style=cShrine>{0} has sacrificed time and has been rewarded.</color>");

            prefab.GetComponent<GenericInspectInfoProvider>().InspectInfo.Info = inspectInfo;

            interactableSpawnCard.prefab = prefab;

            PostInit();
        }
    }

    public class UnityIsAFuckingPieceOfShit : MonoBehaviour
    {
        public PurchaseInteraction purchaseInteraction;
        public ShrineSacrificeBehavior shrineSacrificeBehavior;

        public void Start()
        {
            shrineSacrificeBehavior = GetComponent<ShrineSacrificeBehavior>();
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            purchaseInteraction.onPurchase.AddListener(SoTrue);
        }

        public void SoTrue(Interactor interactor)
        {
            shrineSacrificeBehavior.AddShrineStack(interactor);
        }
    }

    public class ShrineSacrificeBehavior : ShrineBehavior
    {
        public int maxPurchaseCount = 1;

        public float costMultiplierPerPurchase;

        public Transform symbolTransform;

        private PurchaseInteraction purchaseInteraction;

        private int purchaseCount;

        private float refreshTimer;

        private const float refreshDuration = 2f;

        private bool waitingForRefresh;

        public int itemCount = 2;

        public override int GetNetworkChannel()
        {
            return RoR2.Networking.QosChannelIndex.defaultReliable.intVal;
        }

        private void Start()
        {
            Main.ModLogger.LogError("shrine sacrifice behavior start");
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
                    purchaseInteraction.Networkcost = 20;
                    waitingForRefresh = false;
                }
            }
        }

        public void AddShrineStack(Interactor interactor)
        {
            Main.ModLogger.LogError("trying to run add shrine stack");
            if (!NetworkServer.active)
            {
                Main.ModLogger.LogError("NETWORK SERVER NOT ACTRIVE EEEE ");
                // Debug.LogWarning("[Server] function 'System.Void RoR2.ShrineBloodBehavior::AddShrineStack(RoR2.Interactor)' called on client");
                return;
            }
            waitingForRefresh = true;
            var interactorBody = interactor.GetComponent<CharacterBody>();

            Main.ModLogger.LogError("interactor body is " + interactorBody);

            var dropPickup = PickupIndex.none;

            WeightedSelection<List<PickupIndex>> selector = new();
            selector.AddChoice(Run.instance.availableTier1DropList.ToList(), 100f);

            List<PickupIndex> dropList = selector.Evaluate(Run.instance.treasureRng.nextNormalizedFloat);
            if (dropList != null && dropList.Count > 0)
            {
                dropPickup = Run.instance.treasureRng.NextElementUniform(dropList);
            }

            Main.ModLogger.LogError("random white pickupindex is " + dropPickup);

            float angle = 360f / itemCount;
            Vector3 vector = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);

            for (int i = 0; i < itemCount; i++)
            {
                GenericPickupController.CreatePickupInfo info = new()
                {
                    position = transform.position + new Vector3(0, 3f, 0),
                    rotation = Quaternion.identity,
                    pickupIndex = dropPickup
                };

                PickupDropletController.CreatePickupDroplet(info, transform.position + new Vector3(0, 3f, 0), vector);
                vector = quaternion * vector;
            }

            if (interactorBody)
            {
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = interactorBody,
                    baseToken = "SANDSWEPT_SHRINE_SACRIFICE_USE_MESSAGE",
                });
            }

            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
            {
                origin = base.transform.position,
                rotation = Quaternion.identity,
                scale = 1f,
                color = Color.red
            }, true);

            purchaseCount++;
            refreshTimer = 2f;
            if (purchaseCount >= maxPurchaseCount)
            {
                symbolTransform.gameObject.SetActive(false);
                CallRpcSetPingable(false);
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