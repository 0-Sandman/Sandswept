
using IL.RoR2.Items;
using RoR2.ContentManagement;
using Sandswept.Items.VoidGreens;
using Sandswept.Utils.Components;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine.PlayerLoop;
using UnityEngine.VFX;

namespace Sandswept.Equipment.Lunar
{
    [ConfigSection("Equipment :: Flawless Design")]
    public class FlawlessDesign : EquipmentBase
    {
        public override string EquipmentName => "Flawless Design";

        public override string EquipmentLangTokenName => "FLAWLESS_DESIGN";

        public override string EquipmentPickupDesc => "Permanently sacrifice $srmaximum health$se to $suduplicate$se items.".AutoFormat();

        public override string EquipmentFullDescription => $"$srPermanently$se sacrifice $sr{baseHealthCost}%$se of your $srmaximum health$se to $suduplicate$se a targeted item. $srHealth cost increases with item rarity$se.".AutoFormat();
        public override string EquipmentLore =>
        """
        A furrified freestyle, lyrics of furry
        My third eye make me shine like jewelry
        You're just a rent-a-sweeper, your rhymes are minute-maid
        I'll be here when it fades, to watch you flip like a Renegade
        ( no lore yet )
        """;
        public override GameObject EquipmentModel => Main.hifuSandswept.LoadAsset<GameObject>("SacrificialBandHolder.prefab");
        public override bool IsLunar => true;
        public override Sprite EquipmentIcon => Main.sandsweptHIFU.LoadAsset<Sprite>("texObama.png");
        public override float Cooldown => 45f;

        [ConfigField("Base Health Cost", "", 10)]
        public static int baseHealthCost;

        [ConfigField("Equipment Cost Multiplier", "", 1.5f)]
        public static float equipmentCostMultiplier;

        [ConfigField("Green and Void Green Cost Multiplier", "", 2f)]
        public static float greenAndVoidGreenCostMultiplier;

        [ConfigField("Red and Void Red Cost Multiplier", "", 4f)]
        public static float redAndVoidRedCostMultiplier;

        [ConfigField("Yellow and Void Yellow Cost Multiplier", "", 3f)]
        public static float yellowAndVoidYellowCostMultiplier;

        [ConfigField("Lunar Cost Multiplier", "", 2f)]
        public static float lunarCostMultiplier;

        public static GameObject DesignIndicator;
        public static LazyAddressable<GameObject> DuplicationEffect = new(() => Paths.GameObject.ExplosionLunarSun);

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Init()
        {
            base.Init();
            SetUpIndicator();
        }

        public void SetUpIndicator()
        {
            DesignIndicator = PrefabAPI.InstantiateClone(Paths.GameObject.RecyclerIndicator, "DesignPickupIndicator", false);
        }

        public override void Hooks()
        {
            base.Hooks();
            On.RoR2.GenericPickupController.CreatePickup += OnCreatePickup;
            On.RoR2.CharacterMaster.OnInventoryChanged += OnInventoryChanged;
        }

        private void OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
        {
            orig(self);

            // Main.ModLogger.LogError("flawless design oninventorychanged called");

            if (self.inventory)
            {
                GameObject body = self.bodyInstanceObject;
                if (!body) return;

                EquipmentDef def = EquipmentCatalog.GetEquipmentDef(self.inventory.GetEquipmentIndex());

                if (def == EquipmentDef && !body.GetComponent<FlawlessDesignTracker>())
                {
                    body.AddComponent<FlawlessDesignTracker>();
                }

                if (def != EquipmentDef && body.GetComponent<FlawlessDesignTracker>())
                {
                    body.RemoveComponent<FlawlessDesignTracker>();
                }
            }
        }

        private GenericPickupController OnCreatePickup(On.RoR2.GenericPickupController.orig_CreatePickup orig, ref GenericPickupController.CreatePickupInfo createPickupInfo)
        {
            var pickup = orig(ref createPickupInfo);

            if (pickup && createPickupInfo.delusionItemIndex == Items.NoTier.TwistedWound.instance.ItemDef.itemIndex)
            {
                pickup.AddComponent<FlawlessDesignMarker>();
            }

            return pickup;
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!slot.characterBody)
            {
                return false;
            }

            if (slot.characterBody.TryGetComponent<FlawlessDesignTracker>(out var designTracker))
            {
                if (!designTracker.target)
                {
                    return false;
                }

                GenericPickupController pickup = designTracker.target.GetComponent<GenericPickupController>();

                int cost = GetCurseCostForPickup(pickup.pickupIndex);
                pickup.AddComponent<FlawlessDesignMarker>();

                GenericPickupController.CreatePickupInfo info = new()
                {
                    position = pickup.transform.position,
                    delusionItemIndex = Items.NoTier.TwistedWound.instance.ItemDef.itemIndex, // this is really jank but i cant think of a better way to send this information
                    pickupIndex = pickup.pickupIndex
                };

                PickupDropletController.CreatePickupDroplet(info, info.position, Random.onUnitSphere * 15f);

                EffectManager.SpawnEffect(DuplicationEffect, new EffectData
                {
                    origin = pickup.transform.position,
                    scale = 3f
                }, true);

                slot.characterBody.inventory.GiveItem(Items.NoTier.TwistedWound.instance.ItemDef, cost);
                CharacterMasterNotificationQueue.PushPickupNotification(slot.characterBody.master, PickupCatalog.FindPickupIndex(Items.NoTier.TwistedWound.instance.ItemDef.itemIndex));
            }

            return true;
        }

        public static int GetCurseCostForPickup(PickupIndex index)
        {
            PickupDef def = PickupCatalog.GetPickupDef(index);
            float cost = baseHealthCost;

            if (EquipmentCatalog.GetEquipmentDef(def.equipmentIndex))
            {
                cost *= equipmentCostMultiplier;
                return (int)cost;
            }

            switch (def.itemTier)
            {
                case ItemTier.VoidTier2:
                case ItemTier.Tier2:
                    cost *= greenAndVoidGreenCostMultiplier;
                    break;

                case ItemTier.VoidTier3:
                case ItemTier.Tier3:
                    cost *= redAndVoidRedCostMultiplier;
                    break;

                case ItemTier.VoidBoss:
                case ItemTier.Boss:
                    cost *= yellowAndVoidYellowCostMultiplier;
                    break;

                case ItemTier.Lunar:
                    cost *= lunarCostMultiplier;
                    break;

                default:
                    break;
            }

            return (int)cost;
        }

        public class FlawlessDesignTracker : ComponentTracker<GenericPickupController>
        {
            public override void Start()
            {
                base.maxSearchAngle = 45f;
                base.maxSearchDistance = 30f;
                base.searchDelay = 0.1f;
                base.isActiveCallback = () =>
                {
                    return base.body && base.body.isEquipmentActivationAllowed;
                };
                base.validFilter = (x) =>
                {
                    return !x.GetComponent<FlawlessDesignMarker>();
                };
                base.targetingIndicatorPrefab = DesignIndicator;
                base.Start();
            }
        }

        public class FlawlessDesignMarker : MonoBehaviour
        {
            private bool slur; // having a class with nothing in it causes omnisharp to bug out
        }
    }
}
