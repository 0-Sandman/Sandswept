using RoR2.ContentManagement;
using Sandswept.Utils.Components;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine.PlayerLoop;

namespace Sandswept.Equipment
{
    [ConfigSection("Equipment :: Mushroom lunar")]
    public class MushroomLunar : EquipmentBase
    {
        public override string EquipmentName => "Mushroom lunar";

        public override string EquipmentLangTokenName => "MUSHROOM_LUNAR";

        public override string EquipmentPickupDesc => "";

        public override string EquipmentFullDescription => "";

        public override string EquipmentLore => "";

        public override GameObject EquipmentModel => Utils.Assets.GameObject.GenericPickup;
        public override bool IsLunar => true;
        public override Sprite EquipmentIcon => Utils.Assets.Sprite.texEquipmentBGIcon;
        public override float Cooldown => 35f;
        [ConfigField("Aura Length", "", 15f)]
        public static float AuraLength;
        [ConfigField("Aura Radius","",30f)]
        public static float AuraRadius;
        public static List<BuffDef> moddedWhitelist;
        public static List<BuffDef> buffBlacklist;
        public static List<BuffDef> availibleDefs = new List<BuffDef>();
        public static readonly SphereSearch sphereSearch = new SphereSearch();
        public static GameObject wardReference;
        public static bool buffDefsSetup = false;
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            //throw new NotImplementedException();
            return new ItemDisplayRuleDict();
        }
        void LoadAssets()
        {
            wardReference = Main.dgoslingAssets.LoadAsset<GameObject>("MushroomLunarWard");
            wardReference.GetComponentInChildren<Renderer>().AddComponent<MaterialControllerComponents.HGIntersectionController>();

            PrefabAPI.RegisterNetworkPrefab(wardReference);
        }
        public static void SetupBuffDefLists()
        {
            moddedWhitelist = new List<BuffDef>
            {


            };

            buffBlacklist = new List<BuffDef> {
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

            PropertyInfo[] propertyInfos = typeof(Utils.Assets.BuffDef).GetProperties();

            foreach (PropertyInfo item in propertyInfos)
            {
                if (buffBlacklist.Contains((BuffDef)item.GetValue(item)))
                    availibleDefs.Add((BuffDef)item.GetValue(item));
            }

            if (moddedWhitelist.Count > 0) 
                availibleDefs.AddRange(moddedWhitelist);
            if(availibleDefs.Count > 0)
            {
                buffDefsSetup = true;
                Main.ModLogger.LogInfo("finished settingup buffdefs");
            }
                
        }
        public override void Hooks()
        {
            base.Hooks();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            RoR2Application.onLoad += () =>
            {
                SetupBuffDefLists();
            };
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody obj)
        {
            obj.AddItemBehavior<MushroomLunarController>((obj.inventory.GetEquipment(obj.inventory.activeEquipmentSlot).equipmentDef == this.EquipmentDef) ? 1 : 0);
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

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
           if(slot.characterBody == null) return false;
           

            BuffDef rand = availibleDefs[RoR2.Run.instance.stageRng.RangeInt(0, availibleDefs.Count - 1)];
            MushroomLunarController mushroomLunarController = slot.characterBody.GetComponentInChildren<MushroomLunarController>();
            if (mushroomLunarController)
            {
                if (mushroomLunarController.buffDef != null && mushroomLunarController.ward == null)
                    mushroomLunarController.buffDef = null;
                mushroomLunarController.buffDef = rand;
                mushroomLunarController.run = true;
                //if(mushroomLunarController.ward!=null)
            }
           

            return true;
        }
    }
    public class MushroomLunarController : CharacterBody.ItemBehavior
    {
        public GameObject ward;

        public BuffDef buffDef;
        public bool run = false;
        void FixedUpdate()
        {
            if (!NetworkServer.active)
                return;
            bool flag = stack > 0;
            if (ward != flag&&(run&&buffDef))
            {


                if (flag)
                {
                    ward = Object.Instantiate(MushroomLunar.wardReference);
                    ward.GetComponent<BuffWard>().Networkradius += body.radius;
                    ward.GetComponent<BuffWard>().invertTeamFilter = true;
                    ward.GetComponent<BuffWard>().buffDef = buffDef;
                        ward.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject);
                    run = false;
                }
                else
                {
                    Object.Destroy(ward);
                    ward = null;
                    buffDef = null;
                    run = false;
                }
                    
            }
        }

        void OnDisable()
        {
            if (ward)
            {
                Object.Destroy(ward);
                buffDef = null;
                run=false;
            }
        }
    }
}
