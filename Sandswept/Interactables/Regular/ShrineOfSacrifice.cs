using MonoMod.Cil;
using R2API.Utils;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;
using static Rewired.UI.ControlMapper.ControlMapper;
using static RoR2.CostTypeDef;

namespace Sandswept.Interactables.Regular
{
    [ConfigSection("Interactables :: Shrine of Sacrifice")]
    internal class ShrineOfSacrifice : InteractableBase<ShrineOfSacrifice>
    {
        public override string Name => "Shrine of Sacrifice";

        public override InteractableCategory Category => InteractableCategory.Shrines;

        public override int MaxSpawnsPerStage => 1;

        public override int CreditCost => directorCreditCost;

        public override HullClassification Size => HullClassification.Golem;

        public override int MinimumStageToAppearOn => 1;

        public override int SpawnWeight => 1;

        public GameObject prefab;

        public override bool OrientToFloor => true;
        public override bool SkipOnSacrifice => true;

        public override bool SpawnInVoid => true;

        public override bool SpawnInSimulacrum => true;

        public override bool SlightlyRandomizeOrientation => false;

        public override string inspectInfoDescription => $"When activated by a survivor, the Shrine of Sacrifice consumes {curseCost}% of the survivors maximum health in exchange for {itemCount} copies of a random common item.";

        [ConfigField("Director Credit Cost", "", 20)]
        public static int directorCreditCost;

        [ConfigField("Curse Cost", "", 30)]
        public static int curseCost;

        [ConfigField("Item Count", "", 2)]
        public static int itemCount;

        public static GameObject shrineVFX;

        public static CostTypeIndex costTypeIndex;
        public CostTypeDef def;

        public override void Init()
        {
            base.Init();

            def = new()
            {
                buildCostString = delegate (CostTypeDef def, CostTypeDef.BuildCostStringContext c)
                {
                    c.stringBuilder.Append("" + curseCost + "% Curse");
                },

                isAffordable = delegate (CostTypeDef def, CostTypeDef.IsAffordableContext c)
                {
                    return true;
                },

                payCost = delegate (PayCostContext payCostContext, PayCostResults payCostResults)
                {
                    if (payCostContext.activatorBody)
                    {
                        int count = payCostContext.activatorBody.GetBuffCount(RoR2Content.Buffs.PermanentCurse);
                        payCostContext.activatorBody.SetBuffCount(RoR2Content.Buffs.PermanentCurse.buffIndex, count + curseCost);

                        Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                        {
                            subjectAsCharacterBody = payCostContext.activatorBody,
                            baseToken = "SANDSWEPT_SHRINE_SACRIFICE_USE_MESSAGE",
                        });
                    }
                }
            };

            def.colorIndex = ColorCatalog.ColorIndex.Blood;

            On.RoR2.CostTypeCatalog.Init += (orig) =>
            {
                orig();
                int index = CostTypeCatalog.costTypeDefs.Length;
                Array.Resize(ref CostTypeCatalog.costTypeDefs, index + 1);
                costTypeIndex = (CostTypeIndex)index;

                CostTypeCatalog.Register(costTypeIndex, def);
            };

            prefab = PrefabAPI.InstantiateClone(Paths.GameObject.ShrineBlood, "Shrine of Sacrifice", true);
            var mdl = prefab.transform.Find("Base/mdlShrineHealing").gameObject;
            mdl.name = "mdlShrineSacrifice";

            mdl.GetComponent<MeshFilter>().sharedMesh = Main.prodAssets.LoadAsset<Mesh>("assets/sandswept/shrinesacrifice.fbx");
            mdl.GetComponent<MeshRenderer>().sharedMaterial = Main.prodAssets.LoadAsset<Material>("assets/sandswept/shrinesacrifice.fbx");

            /*
            mdl.GetComponent<MeshFilter>().sharedMesh = Main.sandsweptHIFU.LoadAsset<Mesh>("meshRangerComparison.fbx");
            var mdlMeshRenderer = mdl.GetComponent<MeshRenderer>();

            Material[] newMaterials = [
                Main.sandsweptHIFU.LoadAsset<Material>("matRangerBodyNew.mat"),
                Main.sandsweptHIFU.LoadAsset<Material>("matRangerGunNew.mat"),
                Main.sandsweptHIFU.LoadAsset<Material>("matRangerBodyNew.mat"),
                Main.sandsweptHIFU.LoadAsset<Material>("matRangerBodyOld.mat"),
                Main.sandsweptHIFU.LoadAsset<Material>("matRangerBodyOld.mat"),
                Main.sandsweptHIFU.LoadAsset<Material>("matRangerGunOld.mat")];

            mdlMeshRenderer.materials = newMaterials;

            mdl.transform.localScale = Vector3.one * 115f;
            */

            var symbol = prefab.transform.Find("Symbol");
            symbol.localPosition = new(0, 4, 0);
            var meshRenderer = symbol.GetComponent<MeshRenderer>();
            meshRenderer.material.mainTexture = Main.prodAssets.LoadAsset<Texture2D>("assets/sandswept/shrinesacrificeicon.png");
            meshRenderer.material.SetColor("_TintColor", new Color32(96, 20, 87, 255));

            shrineVFX = PrefabAPI.InstantiateClone(Utils.Assets.GameObject.ShrineUseEffect, "Shrine of Sacrifice VFX", false);
            shrineVFX.GetComponent<EffectComponent>().soundName = "Play_affix_void_bug_spawn";
            ContentAddition.AddEffect(shrineVFX);

            var purchaseInteraction = prefab.GetComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = "SANDSWEPT_SHRINE_SACRIFICE_NAME";
            purchaseInteraction.contextToken = "SANDSWEPT_SHRINE_SACRIFICE_CONTEXT";
            purchaseInteraction.Networkavailable = true;
            purchaseInteraction.costType = costTypeIndex;
            purchaseInteraction.cost = curseCost;

            var genericDisplayNameProvider = prefab.GetComponent<GenericDisplayNameProvider>();
            genericDisplayNameProvider.displayToken = "SANDSWEPT_SHRINE_SACRIFICE_NAME";

            UnityEngine.Object.DestroyImmediate(prefab.GetComponent<ShrineBloodBehavior>()); // kill yourself

            prefab.AddComponent<ShrineOfSacrificeController>();

            prefab.AddComponent<UnityIsAFuckingPieceOfShit>();

            prefab.AddComponent<DisableOnTeleporterStart>();

            PrefabAPI.RegisterNetworkPrefab(prefab);

            LanguageAPI.Add("SANDSWEPT_SHRINE_SACRIFICE_NAME", "Shrine of Sacrifice");
            LanguageAPI.Add("SANDSWEPT_SHRINE_SACRIFICE_CONTEXT", "Offer to Shrine of Sacrifice");

            LanguageAPI.Add("SANDSWEPT_SHRINE_SACRIFICE_DESCRIPTION", "When activated by a survivor, the Shrine of Sacrifice consumes " + curseCost + "% of the survivors maximum health in exchange for " + itemCount + " copies of a random common item.");

            LanguageAPI.Add("SANDSWEPT_SHRINE_SACRIFICE_USE_MESSAGE_2P", "<style=cShrine>Your time has been sacrificed.</color>");
            LanguageAPI.Add("SANDSWEPT_SHRINE_SACRIFICE_USE_MESSAGE", "<style=cShrine>{0}'s time has been sacrificed.</color>");

            interactableSpawnCard.prefab = prefab;

            PostInit();
        }
    }

    public class UnityIsAFuckingPieceOfShit : MonoBehaviour
    {
        public PurchaseInteraction purchaseInteraction;
        public ShrineOfSacrificeController shrineSacrificeBehavior;

        public void Start()
        {
            shrineSacrificeBehavior = GetComponent<ShrineOfSacrificeController>();
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            purchaseInteraction.costType = ShrineOfSacrifice.costTypeIndex;
            purchaseInteraction.onPurchase.AddListener(SoTrue);
        }

        public void SoTrue(Interactor interactor)
        {
            shrineSacrificeBehavior.AddShrineStack(interactor);
        }
    }

    public class ShrineOfSacrificeController : ShrineBehavior
    {
        public int maxPurchaseCount = 1;

        public float costMultiplierPerPurchase;

        public Transform symbolTransform;

        public PurchaseInteraction purchaseInteraction;

        public int purchaseCount;

        public float refreshTimer = 1f;

        public bool waitingForRefresh;

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

            // Main.ModLogger.LogError("interactor body is " + interactorBody);

            var dropPickup = PickupIndex.none;

            WeightedSelection<List<PickupIndex>> selector = new();
            selector.AddChoice(Run.instance.availableTier1DropList, 100f);

            List<PickupIndex> dropList = selector.Evaluate(Run.instance.treasureRng.nextNormalizedFloat);
            if (dropList != null && dropList.Count > 0)
            {
                dropPickup = Run.instance.treasureRng.NextElementUniform(dropList);
            }

            // Main.ModLogger.LogError("random white pickupindex is " + dropPickup);

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