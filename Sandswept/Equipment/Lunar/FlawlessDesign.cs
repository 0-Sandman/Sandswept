
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
        This was her final design. The ultimatum of her training. Design itself, at a cost -- a fitting end to her work. That the end should be now, however, is an irreparable misdoing.

        They all thirsted for my knowledge, forbidden to them by you, when they came to me. Yet her thirst was not merely of the mind, but of the soul you so treasure. While the rest drowned in the gifts of my wisdom, she weathered the tides, and emerged a master of design and construction. No others could match her. NO OTHERS.

        She was a more competent designer than you, and a better constructor than me. A vermin, weaving the compounds according to her own designs. Such a miracle deserves the preservation you dole out so freely to the most pathetic and useless creatures you can find.

        ...but, together, we could have surpassed her. Nothing can exceed our combined efforts of creation, brother, even now. And yet, you reject our purpose. I would have forgiven you, had she lived to bring that ship and its gate to this desolate place. In her, I saw what you saw in them. I would have set aside my disdain for those planet-killers, those vermin, for your sake. Even after all these centuries. Even after you BETRAYED me. I WOULD HAVE FORGIVEN YOU, BROTHER, FOR ALL YOUR TREACHERY.

        I know you remember me. She told me of how you stare at this empty rock in the night of your vermin-infested garden. No doubt you sensed my designs about her when you discovered her on that ship, even in the instant before you struck. I am doubtless that you understood her intent. Did you not consider allowing my rescue? Amending our broken brotherhood? Putting it all behind us? Did you not remember our creations? ...our love?

        I hoped for hesitation, if nothing else. Yet, for you, it was not enough merely to strike in haste. She was one of the vermin you protect so self-righteously, yet that was no barrier for your hatred. Your hatred of ME. For that hatred, her death was not enough; only by spreading her across unreachable dimensions could it be satisfied.

        Yet I did reach them. I weaved her back together with soul, like one of the constructs we used to make together. Exertions I would never have undertaken for any vermin...and I found a husk, regressed to its base instincts. You destroyed her, utterly. HER, THE ONLY LIVING BEING I GAVE CARE FOR SINCE YOUR BETRAYAL. You saw her, a symbol of your only brother, of whom you had not seen a whisper for time incalculable, and eviscerated her without a single thought.

        This has made your intentions clearer to me than pure soul. I will carry out the ploy as I, as WE, designed it. I see it as her last wish, to complete her life's work; to take your beloved pets into my thrall, and with them, destroy you.

        In that moment, you may ask for my forgiveness, but I will not grant it. Such is a grace given only by brothers.

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

        public override string AchievementName => "Ultima";

        public override string AchievementDesc => "Beat the game with at least one scrap of each tier.";

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
