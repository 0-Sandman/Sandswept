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

        public override string EquipmentPickupDesc => "<style=cDeath>Permanently sacrifice maximum health</style> to <style=cIsUtility>duplicate items>/style>.";

        public override string EquipmentFullDescription => $"Spend <style=cIsDeath>{BaseHealthCost}%</style> <style=cHealth>maximum health</style> <style=cDeath>PERMANENTLY</style> to <style=cIsUtility>duplicate a targeted item</style>. <style=cDeath>Health cost increases with item rarity</style>".AutoFormat();
        public override string EquipmentLore => "your boats floated";
        public override GameObject EquipmentModel => null;
        public override bool IsLunar => true;
        public override Sprite EquipmentIcon => null;
        public override float Cooldown => 45f;

        [ConfigField("Base Health Cost", "", 10)]
        public static int BaseHealthCost;

        public static GameObject DesignIndicator;
        public static LazyAddressable<GameObject> DuplicationEffect = new(() => Paths.GameObject.ExplosionLunarSun);

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            Hooks();

            DesignIndicator = PrefabAPI.InstantiateClone(Paths.GameObject.RecyclerIndicator, "DesignPickupIndicator");
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

            Main.ModLogger.LogError("flawless design oninventorychanged called");

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
            float cost = BaseHealthCost;

            if (EquipmentCatalog.GetEquipmentDef(def.equipmentIndex))
            {
                cost *= 1.5f;
                return (int)cost;
            }

            switch (def.itemTier)
            {
                case ItemTier.VoidTier2:
                case ItemTier.Tier2:
                    cost *= 2f;
                    break;

                case ItemTier.VoidTier3:
                case ItemTier.Tier3:
                    cost *= 4f;
                    break;

                case ItemTier.VoidBoss:
                case ItemTier.Boss:
                    cost *= 3f;
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