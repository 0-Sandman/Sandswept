using IL.RoR2.Items;
using RoR2.ContentManagement;
using Sandswept.Utils.Components;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine.PlayerLoop;
using UnityEngine.VFX;

namespace Sandswept.Equipment.Lunar
{
    [ConfigSection("Equipment :: Corrupted Catalyst")]
    public class CorruptedCatalyst : EquipmentBase
    {
        public override string EquipmentName => "Corrupted Catalyst";

        public override string EquipmentLangTokenName => "CORRUPTED_CATALYST";

        public override string EquipmentPickupDesc => "Create an aura that buffs all allies and enemies alike.";

        public override string EquipmentFullDescription => ("Create a $su" + buffRadius + "m$se aura that gives all allies and enemies a $surandom buff$se for $su" + buffDuration + "seconds$se.").AutoFormat();

        public override string EquipmentLore => "TBD";

        public override GameObject EquipmentModel => Paths.GameObject.GenericPickup;
        public override bool IsLunar => true;
        public override Sprite EquipmentIcon => Paths.Sprite.texEquipmentBGIcon;
        public override float Cooldown => 35f;

        [ConfigField("Buff Duration", "", 15f)]
        public static float buffDuration;

        [ConfigField("Buff Radius", "", 30f)]
        public static float buffRadius;

        public static List<BuffDef> moddedBuffWhitelist;
        public static List<BuffDef> buffBlacklist;

        public static List<BuffDef> availableBuffs = new();
        public static bool buffDefsSetup = false;

        public static readonly SphereSearch sphereSearch = new();

        public static GameObject wardReference;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            //throw new NotImplementedException();
            return new ItemDisplayRuleDict();
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            LoadAssets();

            CreateEquipment();
            //SetupBuffDefLists();
            Hooks();
        }

        private void LoadAssets()
        {
            wardReference = Main.dgoslingAssets.LoadAsset<GameObject>("MushroomLunarWard");
            wardReference.GetComponentInChildren<Renderer>().AddComponent<MaterialControllerComponents.HGIntersectionController>();

            wardReference.RegisterNetworkPrefab();
        }

        public override void Hooks()
        {
            base.Hooks();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            RoR2Application.onLoad += SetupBuffDefLists;
        }

        public static void SetupBuffDefLists()
        {
            moddedBuffWhitelist = new List<BuffDef>
            {
            };

            buffBlacklist = new List<BuffDef>
            {
                RoR2Content.Buffs.AffixBlue,
                RoR2Content.Buffs.AffixEcho,
                RoR2Content.Buffs.AffixHaunted,
                RoR2Content.Buffs.AffixLunar,
                RoR2Content.Buffs.AffixPoison,
                RoR2Content.Buffs.AffixRed,
                RoR2Content.Buffs.AffixWhite,
                DLC1Content.Buffs.EliteVoid,
                DLC1Content.Buffs.EliteEarth,
                DLC2Content.Buffs.EliteAurelionite,
                DLC2Content.Buffs.EliteBead,
                RoR2Content.Buffs.HiddenInvincibility,
                DLC2Content.Buffs.CookingRolling,
                DLC2Content.Buffs.CookingSearing,
                DLC2Content.Buffs.CorruptionFesters,
                DLC2Content.Buffs.GeodeBuff,
                DLC2Content.Buffs.KnockUpHitEnemies,
                DLC2Content.Buffs.SojournHealing,
                DLC2Content.Buffs.SojournVehicle,
                DLC1Content.Buffs.VoidSurvivorCorruptMode,
                DLC2Content.Buffs.WeakenedBeating,
                DLC2Content.Buffs.boostedFireEffect,
                DLC1Content.Buffs.ImmuneToDebuffCooldown,
                RoR2Content.Buffs.ElementalRingsCooldown,
                DLC1Content.Buffs.BearVoidCooldown,
                DLC1Content.Buffs.ElementalRingVoidCooldown,
                DLC2Content.Buffs.TeleportOnLowHealthCooldown,
                JunkContent.Buffs.LightningShield,
                JunkContent.Buffs.EngiTeamShield,
                JunkContent.Buffs.Deafened,
                JunkContent.Buffs.LoaderOvercharged,
                JunkContent.Buffs.MeatRegenBoost,
                JunkContent.Buffs.BodyArmor,
                JunkContent.Buffs.EnrageAncientWisp,
                JunkContent.Buffs.GoldEmpowered,
                JunkContent.Buffs.LoaderPylonPowered,
                JunkContent.Buffs.Slow30
            };

            PropertyInfo[] propertyInfos = typeof(Paths.BuffDef).GetProperties();

            foreach (PropertyInfo item in propertyInfos)
            {
                if (buffBlacklist.Contains((BuffDef)item.GetValue(item)))
                {
                    availableBuffs.Add((BuffDef)item.GetValue(item));
                }
            }

            if (moddedBuffWhitelist.Count > 0)
            {
                availableBuffs.AddRange(moddedBuffWhitelist);
            }

            if (availableBuffs.Count > 0)
            {
                buffDefsSetup = true;
                // Main.ModLogger.LogInfo("finished settingup buffdefs");
            }
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            body.AddItemBehavior<CorruptedCatalystController>(body.inventory.GetEquipment(body.inventory.activeEquipmentSlot).equipmentDef == EquipmentDef ? 1 : 0);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!slot.characterBody)
            {
                return false;
            }

            BuffDef randomBuff = availableBuffs[Run.instance.stageRng.RangeInt(0, availableBuffs.Count - 1)];

            if (slot.characterBody.TryGetComponent<CorruptedCatalystController>(out var CorruptedCatalystController))
            {
                if (CorruptedCatalystController.buffDef != null && CorruptedCatalystController.ward == null)
                {
                    CorruptedCatalystController.buffDef = null;
                }

                CorruptedCatalystController.buffDef = randomBuff;
                CorruptedCatalystController.shouldRun = true;
            }

            return true;
        }
    }

    public class CorruptedCatalystController : CharacterBody.ItemBehavior
    {
        public GameObject ward;

        public BuffDef buffDef;
        public bool shouldRun = false;

        private void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            bool what = stack > 0;
            if (ward != what && shouldRun && buffDef)
            {
                if (what)
                {
                    ward = Instantiate(CorruptedCatalyst.wardReference);
                    var buffWard = ward.GetComponent<BuffWard>();
                    buffWard.Networkradius = CorruptedCatalyst.buffRadius + body.radius;
                    buffWard.invertTeamFilter = true;
                    buffWard.buffDef = buffDef;
                    ward.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject);
                    shouldRun = false;
                }
                else
                {
                    Destroy(ward);
                    // shouldnt it be NetworkServer.Destroy?
                    ward = null;
                    buffDef = null;
                    shouldRun = false;
                }
            }
        }

        private void OnDisable()
        {
            if (ward)
            {
                Destroy(ward);
                // shouldnt it be NetworkServer.Destroy?
                buffDef = null;
                shouldRun = false;
            }
        }
    }
}