using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;

namespace Sandswept.Elites
{
    [ConfigSection("Elites :: Osmium")]
    internal class Osmium : EliteEquipmentBase<Osmium>
    {
        public override string EliteEquipmentName => "Artificial Void";

        public override string EliteAffixToken => "OSMIUM";

        public override string EliteEquipmentPickupDesc => "uzJ7tC6fFnk Bec'om`e a\"n as/pect o/f/ sing|ul&arity._ 3yd4myHf81E";

        public override string EliteEquipmentFullDescription => "uzJ7tC6fFnk Bec'om`e a\"n as/pect o/f/ sing|ul&arity._ 3yd4myHf81E";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Osmium";

        public override GameObject EliteEquipmentModel => CreateAffixModel(new Color32(110, 64, 255, 255));

        public override Sprite EliteEquipmentIcon => Main.hifuSandswept.LoadAsset<Sprite>("texOsmiumAffix.png");

        public override Sprite EliteBuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("texOsmiumBuff.png");

        public override Texture2D EliteRampTexture => Main.hifuSandswept.LoadAsset<Texture2D>("texRampOsmium.png");

        public override float DamageMultiplier => damageMultiplier;
        public override float HealthMultiplier => healthMultiplier;

        public static GameObject warbanner;
        public static GameObject crownModel;
        public override EliteTier Tier => EliteTier.Tier2;

        public override Color EliteBuffColor => Color.white;

        public static GameObject aura;
        public static BuffDef outsideAuraBuff;
        public static BuffDef insideAuraBuff;
        public static BuffDef jumpDisabledBuff;
        public static GameObject groundVFX;
        public static GameObject distortionVFX;

        [ConfigField("Damage Multiplier", "Decimal.", 6f)]
        public static float damageMultiplier;

        [ConfigField("Health Multiplier", "Decimal.", 18f)]
        public static float healthMultiplier;

        [ConfigField("Outside Damage Taken Multiplier", "Decimal.", 0.15f)]
        public static float outsideDamageTakenMultiplier;

        [ConfigField("Player Outside Damage Taken Multiplier", "Only works if a player has the aspect. Decimal.", 0.75f)]
        public static float playerOutsideDamageTakenMultiplier;

        [ConfigField("Outside Proc Coefficient Multiplier", "Decimal.", 0.5f)]
        public static float outsideProcCoefficientMultiplier;

        [ConfigField("Inside Damage Taken Multiplier", "Decimal.", 1.33f)]
        public static float insideDamageTakenMultiplier;

        [ConfigField("Minimum Aura Radius", "Scales with Base Max Health.", 13f)]
        public static float minimumAuraRadius;

        [ConfigField("Maximum Aura Radius", "Scales with Base Max Health.", 40f)]
        public static float maximumAuraRadius;

        [ConfigField("Aura Grounding Interval", "", 1.25f)]
        public static float auraGroundingInterval;

        public static DamageColorIndex outsideColor = DamageColourHelper.RegisterDamageColor(new Color32(100, 100, 100, 255));

        public override bool CheckEliteRequirement(SpawnCard card)
        {
            return card.nodeGraphType == RoR2.Navigation.MapNodeGroup.GraphType.Ground;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            crownModel = Main.dgoslingAssets.LoadAsset<GameObject>("DisplayosmiumCrown.prefab");
            crownModel.AddComponent<ItemDisplay>();
            crownModel.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(crownModel);
            ItemDisplayRuleDict dict = new ItemDisplayRuleDict(Array.Empty<ItemDisplayRule>());
            //ItemDisplayRule[] tmpRules = Utils.Assets.ItemDisplayRuleSet.idrsAcidLarva.FindDisplayRuleGroup(RoR2Content.Equipment.AffixWhite).rules;
            //tmpRules[0].followerPrefab = crownModel;
            //tmpRules[1].followerPrefab = crownModel;
            //dict.Add("AcidLarva", tmpRules);
            // tmpRules = Utils.Assets.ItemDisplayRuleSet.idrsAssassin2.FindDisplayRuleGroup(RoR2Content.Equipment.AffixWhite).rules;
            //tmpRules[0].followerPrefab = crownModel;
            //dict.Add("mdlAssassin",tmpRules);
            //tmpRules = Utils.Assets.ItemDisplayRuleSet.idrsBandit2.FindDisplayRuleGroup(RoR2Content.Equipment.AffixWhite).rules;
            //tmpRules[0].followerPrefab= crownModel;
            //dict.Add("mdlBandit2", tmpRules);
            //tmpRules = Utils.Assets.ItemDisplayRuleSet.idrsBeetle.FindDisplayRuleGroup(RoR2Content.Equipment.AffixWhite).rules;
            //tmpRules[0].followerPrefab = crownModel;
            dict.Add("mdlBeetle", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(0F, 1.06064F, 0.06114F),
                localAngles = new Vector3(309.1414F, 0F, 270F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F),
                followerPrefab = crownModel,
                followerPrefabAddress = new("useless"),
                limbMask = LimbFlags.None
            });

            //tmpRules = Utils.Assets.ItemDisplayRuleSet.idrsBeetleGuard.FindDisplayRuleGroup(RoR2Content.Equipment.AffixWhite).rules;
            //tmpRules[0].followerPrefab = crownModel;
            //dict.Add("mdlBeetleGuard",tmpRules);
            //tmpRules = Utils.Assets.ItemDisplayRuleSet.idrsBeetleQueen.FindDisplayRuleGroup(RoR2Content.Equipment.AffixWhite).rules;
            //tmpRules[0].followerPrefab = crownModel;
            //dict.Add("mdlBeetleQueen", tmpRules);
            dict.Add("mdlCommandoDualies", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),

            childName = "Head",
localPos = new Vector3(0F, 0.43946F, 0.19302F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.15F, 0.15F, 0.15F)
        }
      });
            dict.Add("mdlHuntress", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.00158F, 0.44266F, 0.03309F),
localAngles = new Vector3(283.7986F, 185.5439F, 261.9359F),
localScale = new Vector3(0.125F, 0.125F, 0.125F)
        }
            });
            dict.Add("mdlToolbot", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "HeadCenter",
localPos = new Vector3(0.0817F, 2.00278F, 1.21422F),
localAngles = new Vector3(350.1546F, 2.09847F, 272.2529F),
localScale = new Vector3(0.8F, 0.8F, 0.8F)
        }
            });
            dict.Add("mdlTreebot", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "FlowerBase",
localPos = new Vector3(0.0322F, 1.54428F, 0.75568F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.7F, 0.7F, 0.7F)
        }
            });

            dict.Add("mdlEngi", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "HeadCenter",
localPos = new Vector3(0F, 0.17388F, 0.1733F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F)
        }
            });
            dict.Add("mdlMage", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.17269F, 0.12011F),
localAngles = new Vector3(272.3778F, 180.0004F, 269.9996F),
localScale = new Vector3(0.075F, 0.075F, 0.075F)
        }
            });
            dict.Add("mdlMerc", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.25684F, 0.18109F),
localAngles = new Vector3(273F, 0F, 90F),
localScale = new Vector3(0.1F, 0.1F, 0.1F)
        }
            });
            dict.Add("mdlLoader", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.27065F, 0.15073F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F)
        }
            });
            dict.Add("mdlCroco", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0.00001F, 1.98247F, 2.39918F),
localAngles = new Vector3(0F, 0F, 270F),
localScale = new Vector3(1F, 1F, 1F)
        }
            });
            dict.Add("mdlCaptain", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.29509F, 0.1712F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.12F, 0.12F, 0.12F)
        }
            });
            dict.Add("mdlBandit2", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Hat",
localPos = new Vector3(-0.00006F, 0.18337F, 0.1098F),
localAngles = new Vector3(299.3236F, 180F, 270F),
localScale = new Vector3(0.125F, 0.125F, 0.125F)
//childName = "Hat",
//localPos = new Vector3(-0.00005F, 0.06572F, -0.02642F),
//localAngles = new Vector3(335F, 0F, 0F),
//localScale = new Vector3(0.6F, 0.6F, 0.6F)
        }
            });
            //    dict.Add("mdlEquipmentDrone", new ItemDisplayRule[1]
            //    {
            //new ItemDisplayRule()
            //{
            //  ruleType = ItemDisplayRuleType.ParentedPrefab,
            //  followerPrefab = crownModel, followerPrefabAddress = new("useless"),
            //  childName = "HeadCenter",
            //  localPos = new Vector3(),
            //  localAngles = new Vector3(),
            //  localScale = new Vector3(),
            //}
            //    });
            dict.Add("mdlWarframeWisp(Clone)", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
          localPos = new Vector3(),
          localAngles = new Vector3(),
          localScale = new Vector3(),
        }
            });
            dict.Add("mdlRailGunner", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.17331F, 0.14733F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.075F, 0.075F, 0.075F)
        }
            });
            dict.Add("mdlSeeker", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0.00001F, 0.23621F, 0.14362F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.08F, 0.08F, 0.08F)
        }
            });
            dict.Add("mdlFalseSon", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0.00429F, 0.52612F, 0.21166F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.2F, 0.2F, 0.2F)
        }
            });
            dict.Add("mdlChef", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.44373F, 0.22475F, -0.00004F),
localAngles = new Vector3(0F, 270F, 270F),
localScale = new Vector3(0.15F, 0.15F, 0.15F)
        }
            });
            dict.Add("Ranger", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0.01794F, 0.23586F, 0.20082F),
localAngles = new Vector3(279.4937F, 90F, 0F),
localScale = new Vector3(0.08F, 0.08F, 0.08F)
        }
            });
            dict.Add("mdlVoidSurvivor", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0.00001F, 0.13581F, 0.2011F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.125F, 0.125F, 0.125F)
        }
            });
            //    dict.Add("mdlHeretic", new ItemDisplayRule[1]
            //    {
            //new ItemDisplayRule()
            //{
            //  ruleType = ItemDisplayRuleType.ParentedPrefab,
            //  followerPrefab = crownModel, followerPrefabAddress = new("useless"),
            //  childName = "Head",
            //  localPos = new Vector3(),
            //  localAngles = new Vector3(),
            //  localScale = new Vector3(),
            //}
            //    });

            dict.Add("AcidLarva", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "BodyBase",
localPos = new Vector3(0F, 5.31923F, 0.92007F),
localAngles = new Vector3(284.4718F, 180F, 270F),
localScale = new Vector3(1F, 1F, 1F)
        }
            });
            dict.Add("mdlBeetleGuard", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.15591F, 1.68506F),
localAngles = new Vector3(38.77916F, 0F, 270F),
localScale = new Vector3(0.6F, 0.6F, 0.6F)
        }
            });
            dict.Add("mdlBeetleQueen", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0.00003F, 3.98681F, -0.70338F),
localAngles = new Vector3(325.5302F, 0F, 270F),
localScale = new Vector3(2.1F, 2.1F, 2.1F)
        }
            });
            dict.Add("mdlBell", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Chain",
localPos = new Vector3(-1.365F, -0.1481F, -0.85294F),
localAngles = new Vector3(90F, 328F, 0F),
localScale = new Vector3(1.1F, 1.1F, 1.1F)
        }
            });
            dict.Add("mdlBison", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.00001F, 0.26844F, 0.75293F),
localAngles = new Vector3(5F, 0F, 270F),
localScale = new Vector3(0.2F, 0.2F, 0.2F)
        }
            });
            dict.Add("mdlBrother", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0.00003F, 0.30442F, 0.27633F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.2F, 0.2F, 0.2F)
        }
            });
            dict.Add("mdlClayBoss", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "PotLidTop",
localPos = new Vector3(0F, 1.14754F, 2.75167F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(1F, 1F, 1F)
        }
            });
            dict.Add("mdlClayBruiser", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.00003F, 0.49679F, 0.35615F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.2F, 0.2F, 0.2F)
        }
            });
            dict.Add("mdlClayGrenadier", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Torso",
localPos = new Vector3(0.02096F, 0.45886F, 0.32552F),
localAngles = new Vector3(273F, 90F, 0F),
localScale = new Vector3(0.2F, 0.2F, 0.2F)
        }
            });
            dict.Add("mdlMagmaWorm", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "LowerJaw",
localPos = new Vector3(0F, 1.05185F, 1.23493F),
localAngles = new Vector3(0F, 0F, 270F),
localScale = new Vector3(1F, 1F, 1F)
        }
            });
            dict.Add("mdlFlyingVermin", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Body",
localPos = new Vector3(0F, 1.36907F, 0.90859F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.6F, 0.6F, 0.6F)
        }
            });
            dict.Add("mdlGolem", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 1.273F, 0.46027F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.4F, 0.4F, 0.4F)
        }
            });
            dict.Add("mdlGrandparent", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0.00001F, 7.50781F, 0.41562F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(4F, 4F, 4F)
        }
            });
            dict.Add("mdlGravekeeper", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0.00023F, 4.91397F, 2.60214F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(2F, 2F, 2F)
        }
            });
            dict.Add("mdlGreaterWisp", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "MaskBase",
localPos = new Vector3(0F, 1.17681F, 0.87427F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.7F, 0.7F, 0.7F)
        }
            });
            dict.Add("mdlGup", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "MainBody2",
localPos = new Vector3(0F, 1.15732F, 0.69748F),
localAngles = new Vector3(298.3305F, 180F, 270F),
localScale = new Vector3(0.7F, 0.7F, 0.7F)
        }
            });
            dict.Add("mdlHermitCrab", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.13612F, 1.34704F, 0.3413F),
localAngles = new Vector3(270F, 68.25634F, 0F),
localScale = new Vector3(0.4F, 0.4F, 0.4F)
        }
            });
            dict.Add("mdlImp", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Neck",
localPos = new Vector3(0F, 0.16215F, -0.20313F),
localAngles = new Vector3(270F, 270F, 0F),
localScale = new Vector3(0.2F, 0.2F, 0.2F)
        }
            });
            dict.Add("mdlImpBoss", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Neck",
localPos = new Vector3(0F, 0.77942F, -0.7296F),
localAngles = new Vector3(283.7287F, 0F, 270F),
localScale = new Vector3(1F, 1F, 1F)
        }
            });
            dict.Add("mdlJellyfish", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Hull2",
localPos = new Vector3(0.01242F, 0.97758F, 1.17747F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.8F, 0.8F, 0.8F)
        }
            });
            dict.Add("mdlLemurian", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.00002F, 1.67104F, -2.85597F),
localAngles = new Vector3(0F, 180F, 270F),
localScale = new Vector3(1.5F, 1.5F, 1.5F)
        }
            });
            dict.Add("mdlChild", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0.00002F, 0.48007F, 0.26979F),
localAngles = new Vector3(270.3357F, 179.9988F, 270.0012F),
localScale = new Vector3(0.15F, 0.15F, 0.15F)
        }
            });
            dict.Add("mdlLemurianBruiser", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 2.81765F, 2.14676F),
localAngles = new Vector3(0F, 0F, 270F),
localScale = new Vector3(1.5F, 1.5F, 1.5F)
        }
            });
            dict.Add("mdlMiniMushroom", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.50541F, -1.21665F, 0.00092F),
localAngles = new Vector3(0F, 270F, 90F),
localScale = new Vector3(0.6F, 0.6F, 0.6F)
        }
            });
            dict.Add("mdlMinorConstruct", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "CapTop",
localPos = new Vector3(0F, 1.04299F, 0.34622F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.5F, 0.5F, 0.5F)
        }
            });
            dict.Add("mdlNullifier", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Muzzle",
localPos = new Vector3(0F, 1.0435F, 2.37762F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(1F, 1F, 1F)
        }
            });
            dict.Add("mdlParent", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-36.3644F, 158.6059F, -0.00013F),
localAngles = new Vector3(320.7383F, 270F, 270F),
localScale = new Vector3(30F, 30F, 30F)
        }
            });
            dict.Add("mdlRoboBallBoss", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Center",
localPos = new Vector3(0F, 1.11979F, 0.91924F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.5F, 0.5F, 0.5F)
        }
            });
            dict.Add("mdlRoboBallMini", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Root",
localPos = new Vector3(0F, 1.11968F, 0.88305F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.6F, 0.6F, 0.6F)
        }
            });
            dict.Add("mdlScav", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.77904F, -1.59193F, -13.17959F),
localAngles = new Vector3(23.34232F, 180F, 270F),
localScale = new Vector3(4.5F, 4.5F, 4.5F)
        }
            });
            dict.Add("mdlScorchling", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0.16215F, 0.83455F, 0.012F),
localAngles = new Vector3(278.2771F, 270F, 270F),
localScale = new Vector3(0.5F, 0.5F, 0.5F)
        }
            });
            dict.Add("TitanBody", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.00001F, 6.28477F, 2.34205F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(2F, 2F, 2F)
        }
            });
            dict.Add("TitanGoldBody", new ItemDisplayRule[1]
            {
                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = crownModel, followerPrefabAddress = new("useless"),
                    childName = "Head",
localPos = new Vector3(0.00001F, 5.25809F, 2.07683F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(2.5F, 2.5F, 2.5F)
                }
            });
            dict.Add("mdlVagrant", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Hull",
localPos = new Vector3(0.00014F, 1.47516F, 1.49021F),
localAngles = new Vector3(274.0579F, 180F, 270F),
localScale = new Vector3(1F, 1F, 1F)
        }
            });
            dict.Add("mdlVermin", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.03629F, -0.45339F, -0.50791F),
localAngles = new Vector3(84.59605F, 0F, 90F),
localScale = new Vector3(0.6F, 0.6F, 0.6F)
        }
            });
            dict.Add("mdlVoidBarnacle", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.59149F, -0.00004F, -1.0096F),
localAngles = new Vector3(0F, 180F, 180F),
localScale = new Vector3(0.5F, 0.5F, 0.5F)
        }
            });
            dict.Add("mdlVoidJailer", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.97354F, 0F, 0.26005F),
localAngles = new Vector3(0.00001F, 248.7224F, 180F),
localScale = new Vector3(0.3F, 0.3F, 0.3F)
        }
            });
            dict.Add("mdlVoidMegaCrab", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "BodyBase",
localPos = new Vector3(0F, 10.15379F, 4.9236F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(4F, 4F, 4F)
        }
            });
            dict.Add("mdlVulture", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.00013F, 2.07661F, -2.41388F),
localAngles = new Vector3(0F, 180F, 270F),
localScale = new Vector3(1.5F, 1.5F, 1.5F)
        }
            });
            dict.Add("mdlWisp1Mouth", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = crownModel, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.62259F, 0.44798F),
localAngles = new Vector3(0F, 0F, 270F),
localScale = new Vector3(0.5F, 0.5F, 0.5F)
        }
            });
            return dict;
        }

        public override void Hooks()
        {
            distortionVFX = PrefabAPI.InstantiateClone(Paths.GameObject.TreebotShockwavePullEffect, "Osmium Pull Down Distortion VFX", false);

            var transform = distortionVFX.transform;
            var pollenSingle = transform.GetChild(1);
            var pollenDust = transform.GetChild(2);
            var pollenRadial = transform.GetChild(3);
            var pollenSingle2 = transform.GetChild(4);
            var distortionWave2 = transform.GetChild(7).GetComponent<ParticleSystem>().main.startColor;
            pollenSingle.gameObject.SetActive(false);
            pollenDust.gameObject.SetActive(false);
            pollenRadial.gameObject.SetActive(false);
            pollenSingle2.gameObject.SetActive(false);
            distortionWave2.color = new Color32(110, 64, 255, 255);

            ContentAddition.AddEffect(distortionVFX);

            groundVFX = PrefabAPI.InstantiateClone(Paths.GameObject.PurchaseLockVoid, "Osmium Pull Down VFX", false);
            // groundVFX.RemoveComponent<NetworkIdentity>();

            var sphere2 = groundVFX.transform.GetChild(0).GetComponent<MeshRenderer>();

            var newMat = Object.Instantiate(Paths.Material.matVoidCampLock);
            newMat.SetColor("_TintColor", new Color32(62, 24, 211, 196));

            sphere2.material = newMat;

            var effectComponent = groundVFX.AddComponent<EffectComponent>();
            effectComponent.applyScale = true;
            effectComponent.positionAtReferencedTransform = true;
            effectComponent.parentToReferencedTransform = true;

            var VFXAttributes = effectComponent.AddComponent<VFXAttributes>();

            var destroyOnTimer = groundVFX.AddComponent<DestroyOnTimer>();
            destroyOnTimer.duration = 0.5f;

            ContentAddition.AddEffect(groundVFX);

            jumpDisabledBuff = ScriptableObject.CreateInstance<BuffDef>();
            jumpDisabledBuff.isDebuff = false;
            jumpDisabledBuff.isCooldown = false;
            jumpDisabledBuff.canStack = false;
            jumpDisabledBuff.isHidden = false;
            jumpDisabledBuff.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffOsmiumGravity.png");
            jumpDisabledBuff.buffColor = new Color32(110, 64, 255, 255);
            jumpDisabledBuff.name = "Osmium - Jump Disabled";

            outsideAuraBuff = ScriptableObject.CreateInstance<BuffDef>();
            outsideAuraBuff.isDebuff = false;
            outsideAuraBuff.isCooldown = false;
            outsideAuraBuff.canStack = false;
            outsideAuraBuff.isHidden = true;
            outsideAuraBuff.buffColor = Color.blue;
            outsideAuraBuff.iconSprite = Paths.BuffDef.bdArmorBoost.iconSprite;
            outsideAuraBuff.name = "Outside Osmium Aura (Lesbian Sex)";

            insideAuraBuff = ScriptableObject.CreateInstance<BuffDef>();
            insideAuraBuff.isDebuff = false;
            insideAuraBuff.isCooldown = false;
            insideAuraBuff.canStack = false;
            insideAuraBuff.isHidden = true;
            insideAuraBuff.buffColor = Color.red;
            insideAuraBuff.iconSprite = Paths.BuffDef.bdAttackSpeedOnCrit.iconSprite;
            insideAuraBuff.name = "Inside Osmium Aura (Gay Sex)";

            ContentAddition.AddBuffDef(jumpDisabledBuff);
            ContentAddition.AddBuffDef(outsideAuraBuff);
            ContentAddition.AddBuffDef(insideAuraBuff);

            aura = PrefabAPI.InstantiateClone(Paths.GameObject.RailgunnerMineAltDetonated, "Osmium Aura");
            aura.RemoveComponent<SlowDownProjectiles>();

            aura.transform.localPosition = Vector3.zero;
            aura.transform.localEulerAngles = Vector3.zero;

            var areaIndicator = aura.transform.Find("AreaIndicator");
            var softGlow = areaIndicator.Find("SoftGlow");
            var sphere = areaIndicator.Find("Sphere");
            sphere.transform.localScale = Vector3.one;
            var light = areaIndicator.Find("Point Light").GetComponent<Light>();
            var core = areaIndicator.Find("Core");

            softGlow.gameObject.SetActive(false);
            core.gameObject.SetActive(false);

            light.intensity = 10f;
            light.range = 24f;
            light.color = new Color32(204, 0, 255, 255);

            var chargeIn = areaIndicator.Find("ChargeIn");
            var psr = chargeIn.GetComponent<ParticleSystemRenderer>();
            var ps = chargeIn.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.45f;

            var emission = ps.emission;
            emission.rateOverTime = 150f;

            var newMat2 = Object.Instantiate(Paths.Material.matRailgunTracerHead1);
            newMat2.SetColor("_TintColor", Color.black);

            psr.material = newMat2;

            var newRadiusMat = Object.Instantiate(Paths.Material.matMoonbatteryCrippleRadius);
            newRadiusMat.SetTexture("_RemapTex", Paths.Texture2D.texRampBeetleBreath);
            newRadiusMat.SetColor("_TintColor", new Color32(60, 0, 255, 255));
            newRadiusMat.SetFloat("_SoftFactor", 0.6503434f);
            newRadiusMat.SetFloat("_SoftPower", 0.5117764f);
            newRadiusMat.SetFloat("_Boost", 5f);
            newRadiusMat.SetFloat("_RimPower", 3.624829f);
            newRadiusMat.SetFloat("_RimStrength", 0.4449388f);
            newRadiusMat.SetFloat("_AlphaBoost", 2.27f);
            newRadiusMat.SetFloat("_IntersectionStrength", 1.38f);

            var newIndicatorMat = Object.Instantiate(Paths.Material.matCrippleSphereIndicator);
            newIndicatorMat.SetTexture("_RemapTex", Paths.Texture2D.texRampFogDebug);
            newIndicatorMat.SetColor("_TintColor", new Color32(6, 0, 255, 7));
            newIndicatorMat.SetFloat("_SoftFactor", 3.3f);
            newIndicatorMat.SetFloat("_SoftPower", 1f);
            newIndicatorMat.SetFloat("_Boost", 0.34f);
            newIndicatorMat.SetFloat("_RimPower", 3.830029f);
            newIndicatorMat.SetFloat("_RimStrength", 0.9263985f);
            newIndicatorMat.SetFloat("_AlphaBoost", 2.27f);
            newIndicatorMat.SetFloat("_IntersectionStrength", 20f);

            var meshRenderer = sphere.GetComponent<MeshRenderer>();
            Material[] sharedMaterials = meshRenderer.sharedMaterials;
            sharedMaterials[0] = newRadiusMat;
            sharedMaterials[1] = newIndicatorMat;
            meshRenderer.SetSharedMaterials(sharedMaterials, 2);

            var buffWard = aura.GetComponent<BuffWard>();
            buffWard.buffDef = insideAuraBuff;
            buffWard.expires = false;
            buffWard.invertTeamFilter = true;
            buffWard.buffDuration = 0.2f;
            buffWard.radius = 20f;
            buffWard.interval = 0.1f;

            var teamFilter = aura.GetComponent<TeamFilter>();
            teamFilter.defaultTeam = TeamIndex.None;

            PrefabAPI.RegisterNetworkPrefab(aura);

            // On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.HealthComponent.TakeDamageForce_Vector3_bool_bool += HealthComponent_TakeDamageForce_Vector3_bool_bool;
            On.RoR2.HealthComponent.TakeDamageForce_DamageInfo_bool_bool += HealthComponent_TakeDamageForce_DamageInfo_bool_bool;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            // CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            // IL.EntityStates.GenericCharacterMain.ProcessJump += GenericCharacterMain_ProcessJump;
        }

        private void GenericCharacterMain_ProcessJump(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<GenericCharacterMain>("jumpInputReceived")))
            {
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, GenericCharacterMain, bool>>((orig, self) =>
                {
                    var body = self.characterBody;
                    if (body && body.HasBuff(jumpDisabledBuff))
                    {
                        return false;
                    }

                    return orig;
                });
            }
            else
            {
                Main.ModLogger.LogError("Failed to apply Jump Hook");
            }

            c.Index = 0;

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<EntityState>("get_characterBody"),
                x => x.MatchLdloc(out _)))
            {
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, GenericCharacterMain, float>>((orig, self) =>
                {
                    var body = self.characterBody;
                    if (body && body.HasBuff(jumpDisabledBuff))
                    {
                        return 0f;
                    }
                    return orig;
                });
            }
            else
            {
                Main.ModLogger.LogError("Failed to apply Jump Speed Hook");
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var attacker = damageInfo.attacker;
            if (attacker)
            {
                var attackerBody = attacker.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    var victimBody = self.body;
                    if (victimBody)
                    {
                        if (victimBody.HasBuff(insideAuraBuff) || victimBody.HasBuff(Instance.EliteBuffDef))
                        {
                            if (attackerBody.HasBuff(outsideAuraBuff))
                            {
                                if (victimBody.isPlayerControlled)
                                {
                                    damageInfo.damage *= playerOutsideDamageTakenMultiplier;
                                }
                                else
                                {
                                    damageInfo.damage *= outsideDamageTakenMultiplier;
                                }

                                damageInfo.procCoefficient *= outsideProcCoefficientMultiplier;

                                // damageInfo.damageColorIndex = DamageColorIndex.; // fall damage color??? wtf hopooo where the fuck
                                damageInfo.damageColorIndex = outsideColor;
                            }
                            else if (attackerBody.HasBuff(insideAuraBuff))
                            {
                                damageInfo.damage *= insideDamageTakenMultiplier;
                                damageInfo.damageColorIndex = DamageColorIndex.Nearby;
                            }
                        }
                    }
                }
            }

            orig(self, damageInfo);
        }

        private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == insideAuraBuff)
            {
                self.AddBuff(outsideAuraBuff);
            }
            if (buffDef == Instance.EliteBuffDef)
            {
                self.RemoveComponent<OsmiumController>();
            }
        }

        private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == insideAuraBuff)
            {
                self.RemoveBuff(outsideAuraBuff);
            }
            if (buffDef == Instance.EliteBuffDef)
            {
                self.gameObject.AddComponent<OsmiumController>();
            }
        }

        private void HealthComponent_TakeDamageForce_DamageInfo_bool_bool(On.RoR2.HealthComponent.orig_TakeDamageForce_DamageInfo_bool_bool orig, HealthComponent self, DamageInfo damageInfo, bool alwaysApply, bool disableAirControlUntilCollision)
        {
            if (self.body && self.body.HasBuff(EliteBuffDef))
            {
                damageInfo.force = Vector3.zero;
            }
            orig(self, damageInfo, alwaysApply, disableAirControlUntilCollision);
        }

        private void HealthComponent_TakeDamageForce_Vector3_bool_bool(On.RoR2.HealthComponent.orig_TakeDamageForce_Vector3_bool_bool orig, HealthComponent self, Vector3 force, bool alwaysApply, bool disableAirControlUntilCollision)
        {
            if (self.body && self.body.HasBuff(EliteBuffDef))
            {
                force = Vector3.zero;
            }
            orig(self, force, alwaysApply, disableAirControlUntilCollision);
        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }
    }

    public class OsmiumController : MonoBehaviour
    {
        public CharacterBody body;
        public HealthComponent healthComponent;
        public GameObject wardInstance;
        public Vector3 pullDownStrength = new(0f, -30f, 0f);
        public float timer;
        public float pullDownInterval = Osmium.auraGroundingInterval;
        public Vector3 myPosition;
        public float radius;

        public void Start()
        {
            healthComponent = GetComponent<HealthComponent>();
            body = healthComponent.body;
            radius = Util.Remap(body.baseMaxHealth, 0f, 1125f, Osmium.minimumAuraRadius, Osmium.maximumAuraRadius);
            wardInstance = Instantiate(Osmium.aura, body.transform);
            wardInstance.GetComponent<BuffWard>().Networkradius = radius;
            wardInstance.GetComponent<TeamFilter>().teamIndex = TeamIndex.None;
            wardInstance.transform.Find("AreaIndicator/Sphere").localScale = Vector3.one * 2f;

            /*
            var sphere = wardInstance.transform.Find("AreaIndicator/Sphere");
            var modelScale = model.localScale;
            Vector3 idealModelScale = Vector3.one * 2f;

            if (modelScale.sqrMagnitude <= idealModelScale.sqrMagnitude)
            {
                idealModelScale -= modelScale;
            }

            sphere.localScale = Vector3.Scale(sphere.localScale, idealModelScale);
            */
            NetworkServer.Spawn(wardInstance);
        }

        public void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (!healthComponent.alive && NetworkServer.active)
            {
                Destroy(this);
            }

            if (wardInstance)
            {
                wardInstance.transform.localPosition = Vector3.zero;
                wardInstance.transform.localEulerAngles = Vector3.zero;
                wardInstance.transform.position = body.transform.position;
                wardInstance.transform.eulerAngles = Vector3.zero;
                // I HATE THIS WHY DOES IT GET OFFSET OVER TIME
            }

            if (timer >= pullDownInterval)
            {
                // Main.ModLogger.LogWarning("checking distance");
                StartCoroutine(CheckDistance());
                timer = 0f;
            }
        }

        public IEnumerator CheckDistance()
        {
            yield return new WaitForSeconds(0.1f);

            bool anyEnemies = false;

            for (int i = 0; i < CharacterBody.instancesList.Count; i++)
            {
                var enemyBody = CharacterBody.instancesList[i];
                if (!enemyBody)
                {
                    continue;
                }

                var enemyMotor = enemyBody.characterMotor;
                if (!enemyMotor)
                {
                    continue;
                }

                if (enemyMotor.isGrounded)
                {
                    continue;
                }

                if (enemyBody.teamComponent.teamIndex == body.teamComponent.teamIndex)
                {
                    continue;
                }

                myPosition = body.corePosition;
                var enemyPosition = enemyBody.corePosition;
                if (Vector3.Distance(enemyPosition, myPosition) < radius)
                {
                    anyEnemies = true;
                    // Main.ModLogger.LogError("enemy within radius, trying to pull down");
                    PullDown(enemyBody, enemyMotor);
                }
            }

            if (anyEnemies)
            {
                Util.PlaySound("Play_artifactBoss_attack1_explode", gameObject);
            }

            yield return null;
        }

        public void PullDown(CharacterBody characterBody, CharacterMotor characterMotor)
        {
            var effectData = new EffectData { origin = characterBody.corePosition, rotation = Quaternion.identity, scale = characterBody.radius * 1.5f };
            effectData.SetNetworkedObjectReference(characterBody.gameObject);
            EffectManager.SpawnEffect(Osmium.groundVFX, effectData, true);

            if (!NetworkServer.active)
            {
                return;
            }

            characterBody.AddTimedBuff(Osmium.jumpDisabledBuff, 0.5f);

            var damageInfo = new DamageInfo()
            {
                attacker = gameObject,
                canRejectForce = false,
                crit = false,
                damage = 0,
                force = pullDownStrength * characterMotor.mass,
                inflictor = gameObject,
                position = characterBody.corePosition,
                procCoefficient = 0,
                damageType = DamageType.BypassBlock
            };

            characterBody.healthComponent.TakeDamageForce(damageInfo);
        }

        public void OnDestroy()
        {
            NetworkServer.Destroy(wardInstance);
        }
    }
}