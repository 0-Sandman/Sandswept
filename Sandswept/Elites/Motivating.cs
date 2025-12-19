using HG;
using System.Linq;

namespace Sandswept.Elites
{
    [ConfigSection("Elites :: Motivating")]
    internal class Motivating : EliteEquipmentBase<Motivating>
    {
        public override string EliteEquipmentName => "John Hopoo";

        public override string EliteAffixToken => "MOTIVATING";

        public override string EliteEquipmentPickupDesc => "Omg OwO <3 hiii :3 x3 hiiii heyyy :3 :3 :3 UwU meow mrrraow OwO";

        public override string EliteEquipmentFullDescription => "Omg OwO <3 hiii :3 x3 hiiii heyyy :3 :3 :3 UwU meow mrrraow OwO";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Motivating";

        public override GameObject EliteEquipmentModel => CreateAffixModel(new Color32(255, 131, 20, 255));

        public override Sprite EliteEquipmentIcon => Main.hifuSandswept.LoadAsset<Sprite>("texMotivatorAffix.png");

        public override Sprite EliteBuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("texMotivatorBuff2.png");

        public override Texture2D EliteRampTexture => Main.hifuSandswept.LoadAsset<Texture2D>("texRampMotivator.png");

        public override float DamageMultiplier => damageMultiplier;
        public override float HealthMultiplier => healthMultiplier;

        public static GameObject warbanner;
        public static GameObject Crown;
        public override EliteTier Tier => EliteTier.Tier1;

        public override Color EliteBuffColor => Color.white; /*new Color32(200, 101, 105, 255);*/

        public override GameObject EliteCrownModel => Main.hifuSandswept.LoadAsset<GameObject>("EliteMotivatingCrownHolder.prefab");

        public static ItemDisplayRule copiedBlazingIDRS = new();

        public static BuffDef wrbnnerBuff;
        public static BuffDef warcryBuff;

        [ConfigField("Damage Multiplier", "Decimal.", 2f)]
        public static float damageMultiplier;

        [ConfigField("Health Multiplier", "Decimal.", 4f)]
        public static float healthMultiplier;

        [ConfigField("Passive Attack Speed Buff", "Decimal.", 0.3f)]
        public static float passiveAttackSpeedBuff;

        [ConfigField("Passive Movement Speed Buff", "Decimal.", 0.3f)]
        public static float passiveMovementSpeedBuff;

        [ConfigField("On Hit Attack Speed Buff", "Decimal.", 0.3f)]
        public static float onHitAttackSpeedBuff;

        [ConfigField("On Hit Attack Speed Buff Duration", "", 4f)]
        public static float onHitAttackSpeedBuffDuration;

        [ConfigField("Passive Buff Radius", "", 20f)]
        public static float passiveBuffRadius;

        [ConfigField("On Hit Buff Radius", "", 20f)]
        public static float onHitBuffRadius;

        private void CreateCrown()
        {
            Crown = PrefabAPI.InstantiateClone(Utils.Assets.GameObject.DisplayColossusItem, "Motivating Crown", false);
            /*
            var crownModel = Crown.transform.Find("meshColossusBuffCrown").GetComponent<MeshRenderer>();

            var newCrownMat = new Material(Utils.Assets.Material.matColossusItem);
            newCrownMat.SetColor("_EmColor", new Color32(215, 0, 29, 255));
            newCrownMat.SetFloat("_EmPower", 0.1f);
            newCrownMat.SetFloat("_SpecularStrength", 0.1648568f);
            newCrownMat.SetFloat("_SpecularExponent", 0.8268503f);

            crownModel.material = newCrownMat;
            */
            var crownGlow = Crown.transform.Find("meshColossusBuffCrownGlow").GetComponent<MeshRenderer>();

            var newGlowMat = new Material(Utils.Assets.Material.matColossusItemGlow);
            newGlowMat.SetColor("_TintColor", new Color32(255, 115, 126, 255));

            crownGlow.material = newGlowMat;
            Crown.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", Main.dgoslingAssets.LoadAsset<Texture2D>("texColossusItemDiffuseMot.png"));
            Crown.transform.localScale = new Vector3(10, 10, 10);
            Crown.transform.localRotation = Quaternion.Euler(90, 0, 0);
            Crown.transform.Find("meshColossusBuffCrownGlow").GetComponent<MeshRenderer>().material = Utils.Assets.Material.matColossusItemGlow;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            CreateCrown();
            Crown.AddComponent<ItemDisplay>();
            Crown.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(Crown);
            ItemDisplayRuleDict dict = new ItemDisplayRuleDict(Array.Empty<ItemDisplayRule>());
            //ItemDisplayRule[] tmpRules = Utils.Assets.ItemDisplayRuleSet.idrsAcidLarva.FindDisplayRuleGroup(RoR2Content.Equipment.AffixWhite).rules;
            //tmpRules[0].followerPrefab = Crown;
            //tmpRules[1].followerPrefab = Crown;
            //dict.Add("AcidLarva", tmpRules);
            // tmpRules = Utils.Assets.ItemDisplayRuleSet.idrsAssassin2.FindDisplayRuleGroup(RoR2Content.Equipment.AffixWhite).rules;
            //tmpRules[0].followerPrefab = Crown;
            //dict.Add("mdlAssassin",tmpRules);
            //tmpRules = Utils.Assets.ItemDisplayRuleSet.idrsBandit2.FindDisplayRuleGroup(RoR2Content.Equipment.AffixWhite).rules;
            //tmpRules[0].followerPrefab= Crown;
            //dict.Add("mdlBandit2", tmpRules);
            //tmpRules = Utils.Assets.ItemDisplayRuleSet.idrsBeetle.FindDisplayRuleGroup(RoR2Content.Equipment.AffixWhite).rules;
            //tmpRules[0].followerPrefab = Crown;
            dict.Add("mdlBeetle", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(0F, 0.32926F, 0.6006F),
                localAngles = new Vector3(326.3988F, 180F, 0F),
                localScale = new Vector3(2.5F, 2.5F, 2.5F),
                followerPrefab = Crown,
                followerPrefabAddress = new("useless"),
                limbMask = LimbFlags.None
            });

            //tmpRules = Utils.Assets.ItemDisplayRuleSet.idrsBeetleGuard.FindDisplayRuleGroup(RoR2Content.Equipment.AffixWhite).rules;
            //tmpRules[0].followerPrefab = Crown;
            //dict.Add("mdlBeetleGuard",tmpRules);
            //tmpRules = Utils.Assets.ItemDisplayRuleSet.idrsBeetleQueen.FindDisplayRuleGroup(RoR2Content.Equipment.AffixWhite).rules;
            //tmpRules[0].followerPrefab = Crown;
            //dict.Add("mdlBeetleQueen", tmpRules);
            dict.Add("mdlCommandoDualies", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),

            childName = "Head",
            localPos = new Vector3(-0.00065F, 0.19587F, 0.01865F),
            localAngles = new Vector3(6.12111F, 0F, 0F),
            localScale = new Vector3(1F, 1F, 1F)
        }
      });
            dict.Add("mdlHuntress", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.00162F, 0.19032F, -0.03598F),
localAngles = new Vector3(343.7789F, 0F, 0F),
localScale = new Vector3(0.75F, 0.75F, 0.75F)
        }
            });
            dict.Add("mdlToolbot", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "HeadCenter",
localPos = new Vector3(0.12113F, 0.66826F, 0.93F),
localAngles = new Vector3(54.30067F, 0F, 0F),
localScale = new Vector3(5.5F, 5.5F, 5.5F)
        }
            });
            dict.Add("mdlTreebot", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "FlowerBase",
localPos = new Vector3(0F, 1.21606F, -0.0031F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(2.5F, 2.5F, 2.5F)
        }
            });

            dict.Add("mdlEngi", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Chest",
localPos = new Vector3(0F, 0.51827F, 0.03488F),
localAngles = new Vector3(56.93467F, 180F, 180F),
localScale = new Vector3(0.85F, 0.85F, 0.85F)
        }
            });
            dict.Add("mdlMage", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.04525F, 0.00195F),
localAngles = new Vector3(18.72627F, 0F, 0F),
localScale = new Vector3(0.9F, 0.9F, 0.9F)
        }
            });
            dict.Add("mdlMerc", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.10737F, 0.01007F),
localAngles = new Vector3(2.94553F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
        }
            });
            dict.Add("mdlLoader", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.11864F, 0.01685F),
localAngles = new Vector3(9.6236F, 0F, 0F),
localScale = new Vector3(0.8F, 0.8F, 0.8F)
        }
            });
            dict.Add("mdlCroco", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.29673F, -0.23156F),
localAngles = new Vector3(83.55682F, 180F, 180F),
localScale = new Vector3(10.5F, 10.5F, 10.5F)
        }
            });
            dict.Add("mdlCaptain", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.13914F, 0.0176F),
localAngles = new Vector3(4.4224F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
        }
            });
            dict.Add("mdlBandit2", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Hat",
localPos = new Vector3(-0.00004F, 0.01713F, -0.00378F),
localAngles = new Vector3(335F, 0F, 0F),
localScale = new Vector3(0.6F, 0.6F, 0.6F)
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
            //  followerPrefab = Crown, followerPrefabAddress = new("useless"),
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
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
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
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.11616F, 0.01647F),
localAngles = new Vector3(14.89151F, 0F, 0F),
localScale = new Vector3(0.6F, 0.6F, 0.6F)
        }
            });
            dict.Add("mdlSeeker", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.13553F, 0.01291F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.8F, 0.8F, 0.8F)
        }
            });
            dict.Add("mdlFalseSon", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0.00429F, 0.23886F, -0.01F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1.25F, 1.25F, 1.25F)
        }
            });
            dict.Add("mdlChef", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.7005F, -0.02469F, -0.00003F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(1.5F, 1.5F, 1.5F)
        }
            });
            dict.Add("Ranger", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.12856F, 0.05948F),
localAngles = new Vector3(9.49379F, 0F, 0F),
localScale = new Vector3(0.8F, 0.8F, 0.8F)
        }
            });
            dict.Add("mdlVoidSurvivor", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.02375F, 0.00002F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1.1F, 1.1F, 1.1F)
        }
            });
            //    dict.Add("mdlHeretic", new ItemDisplayRule[1]
            //    {
            //new ItemDisplayRule()
            //{
            //  ruleType = ItemDisplayRuleType.ParentedPrefab,
            //  followerPrefab = Crown, followerPrefabAddress = new("useless"),
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
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "BodyBase",
localPos = new Vector3(0F, 5.0664F, -3.21779F),
localAngles = new Vector3(348.773F, 0F, 0F),
localScale = new Vector3(5F, 5F, 5F)
        }
            });
            dict.Add("mdlBeetleGuard", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.00001F, 0.38995F, 0.5643F),
localAngles = new Vector3(85.00005F, 180F, 180F),
localScale = new Vector3(5F, 5F, 5F)
        }
            });
            dict.Add("mdlBeetleQueen", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0.00005F, 3.5F, 0.68988F),
localAngles = new Vector3(354.3496F, 0F, 0F),
localScale = new Vector3(6F, 6F, 6F)
        }
            });
            dict.Add("mdlBell", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Chain",
localPos = new Vector3(0F, -0.25873F, 0F),
localAngles = new Vector3(0F, 239.4491F, 180F),
localScale = new Vector3(5F, 5F, 5F)
        }
            });
            dict.Add("mdlBison", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.33377F, 0.39525F),
localAngles = new Vector3(84.8304F, 180F, 180F),
localScale = new Vector3(2F, 2F, 2F)
        }
            });
            dict.Add("mdlBrother", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0.00001F, 0.17994F, 0.02854F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1.1F, 1.1F, 1.1F)
        }
            });
            dict.Add("mdlClayBoss", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "PotLidTop",
localPos = new Vector3(0F, 0.61259F, 1.01702F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(3F, 3F, 3F)
        }
            });
            dict.Add("mdlClayBruiser", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.00001F, 0.45847F, 0.08446F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1.2F, 1.2F, 1.2F)
        }
            });
            dict.Add("mdlClayGrenadier", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Torso",
localPos = new Vector3(0F, 0.42413F, 0.02598F),
localAngles = new Vector3(3F, 0F, 0F),
localScale = new Vector3(0.75F, 0.75F, 0.75F)
        }
            });
            dict.Add("mdlMagmaWorm", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "LowerJaw",
localPos = new Vector3(0F, 0F, -0.32798F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(5F, 5F, 5F)
        }
            });
            dict.Add("mdlFlyingVermin", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Body",
localPos = new Vector3(0F, 1.34924F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(3F, 3F, 3F)
        }
            });
            dict.Add("mdlGolem", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 1.28836F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(2F, 2F, 2F)
        }
            });
            dict.Add("mdlGrandparent", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 7.02552F, -0.12349F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(10F, 10F, 10F)
        }
            });
            dict.Add("mdlGravekeeper", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 7.02552F, -0.12349F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(10F, 10F, 10F)
        }
            });
            dict.Add("mdlGreaterWisp", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "MaskBase",
localPos = new Vector3(0F, 1.05182F, 0.06994F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(3F, 3F, 3F)
        }
            });
            dict.Add("mdlGup", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "MainBody2",
localPos = new Vector3(0F, 1.05103F, -0.10032F),
localAngles = new Vector3(352.8643F, 0F, 0F),
localScale = new Vector3(1.5F, 1.5F, 1.5F)
        }
            });
            dict.Add("mdlHermitCrab", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 1.82328F, 0F),
localAngles = new Vector3(0F, 328.9559F, 0F),
localScale = new Vector3(1.5F, 1.5F, 1.5F)
        }
            });
            dict.Add("mdlImp", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Neck",
localPos = new Vector3(0F, 0.09156F, 0.00001F),
localAngles = new Vector3(0F, 180F, 0F),
localScale = new Vector3(1.1F, 1.1F, 1.1F)
        }
            });
            dict.Add("mdlImpBoss", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Neck",
localPos = new Vector3(0F, 0.41799F, 0F),
localAngles = new Vector3(0F, 180F, 0F),
localScale = new Vector3(1.3F, 1.3F, 1.3F)
        }
            });
            dict.Add("mdlJellyfish", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Hull2",
localPos = new Vector3(0.01431F, 1.13175F, 0.02581F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(3F, 3F, 3F)
        }
            });
            dict.Add("mdlLemurian", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.00001F, 0.26014F, -0.07975F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(6F, 6F, 6F)
        }
            });
            dict.Add("mdlChild", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0.00002F, 0.20758F, 0.01853F),
localAngles = new Vector3(5.10126F, 0.00389F, -0.00481F),
localScale = new Vector3(1.5F, 1.5F, 1.5F)
        }
            });
            dict.Add("mdlLemurianBruiser", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 0.98952F, 0.78549F),
localAngles = new Vector3(270F, 180F, 0F),
localScale = new Vector3(6F, 6F, 6F)
        }
            });
            dict.Add("mdlMiniMushroom", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.44661F, 0F, 0.00092F),
localAngles = new Vector3(90F, 270F, 0F),
localScale = new Vector3(2F, 2F, 2F)
        }
            });
            dict.Add("mdlMinorConstruct", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "CapTop",
localPos = new Vector3(0F, 0.52153F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(2F, 2F, 2F)
        }
            });
            dict.Add("mdlNullifier", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Muzzle",
localPos = new Vector3(0F, 1.2162F, 0.49685F),
localAngles = new Vector3(16.76504F, 0F, 0F),
localScale = new Vector3(3F, 3F, 3F)
        }
            });
            dict.Add("mdlParent", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-63.81822F, 114.8034F, -0.00011F),
localAngles = new Vector3(316.5069F, 90F, 0F),
localScale = new Vector3(200F, 200F, 200F)
        }
            });
            dict.Add("mdlRoboBallBoss", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "MainEyeMuzzle",
localPos = new Vector3(0F, 0F, -0.13112F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
        }
            });
            dict.Add("mdlRoboBallMini", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Root",
localPos = new Vector3(0F, 0F, 1.02261F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
        }
            });
            dict.Add("mdlScav", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-3.68493F, -7.12428F, -9.26112F),
localAngles = new Vector3(290.0912F, 180F, 197.6753F),
localScale = new Vector3(15F, 15F, 15F)
        }
            });
            dict.Add("mdlScorchling", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.1471F, 0.42499F, 0.012F),
localAngles = new Vector3(12.19688F, 90F, 0F),
localScale = new Vector3(2F, 2F, 2F)
        }
            });
            dict.Add("mdlTitan", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, 5.25825F, 0.39198F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(10F, 10F, 10F)
        }
            });
            dict.Add("mdlVagrant", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Hull",
localPos = new Vector3(0F, 1.78913F, 0.02596F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(3F, 3F, 3F)
        }
            });
            dict.Add("mdlVermin", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.03629F, -0.42449F, -0.3745F),
localAngles = new Vector3(332.5934F, 180F, 180F),
localScale = new Vector3(2F, 2F, 2F)
        }
            });
            dict.Add("mdlVoidBarnacle", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.69995F, -0.00004F, 0.22737F),
localAngles = new Vector3(0F, 270F, 90F),
localScale = new Vector3(0.5F, 0.5F, 0.5F)
        }
            });
            dict.Add("mdlVoidJailer", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.81369F, 0.00001F, -0.23035F),
localAngles = new Vector3(0F, 0F, 90F),
localScale = new Vector3(1.5F, 1.5F, 1.5F)
        }
            });
            dict.Add("mdlVoidMegaCrab", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "BodyBase",
localPos = new Vector3(0F, 9.8578F, -0.00002F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(20F, 20F, 20F)
        }
            });
            dict.Add("mdlVulture", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(-0.0001F, 0.27165F, -0.81025F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(5F, 5F, 5F)
        }
            });
            dict.Add("mdlWisp1Mouth", new ItemDisplayRule[1]
            {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = Crown, followerPrefabAddress = new("useless"),
          childName = "Head",
localPos = new Vector3(0F, -0.12267F, 0.17931F),
localAngles = new Vector3(270F, 180F, 0F),
localScale = new Vector3(3.25F, 3.25F, 3.25F)
        }
            });

            return dict;
        }

        public override void Hooks()
        {
            wrbnnerBuff = ScriptableObject.CreateInstance<BuffDef>();
            wrbnnerBuff.isHidden = false;
            wrbnnerBuff.canStack = false;
            wrbnnerBuff.isCooldown = false;
            wrbnnerBuff.isDebuff = false;
            wrbnnerBuff.iconSprite = Paths.BuffDef.bdWarbanner.iconSprite;
            wrbnnerBuff.buffColor = new Color32(240, 35, 89, 255);
            wrbnnerBuff.name = "Motivating Aura";

            warcryBuff = ScriptableObject.CreateInstance<BuffDef>();
            warcryBuff.isHidden = false;
            warcryBuff.canStack = false;
            warcryBuff.isCooldown = false;
            warcryBuff.isDebuff = false;
            warcryBuff.iconSprite = Paths.BuffDef.bdTeamWarCry.iconSprite;
            warcryBuff.buffColor = new Color32(240, 35, 89, 255);
            warcryBuff.name = "Motivating On Hit Buff";

            ContentAddition.AddBuffDef(wrbnnerBuff);
            ContentAddition.AddBuffDef(warcryBuff);

            /*
            foreach (ItemDisplayRuleSet itemDisplayRuleSet in ItemDisplayRuleSet.instancesList)
            {
                var keyAssetRuleGroupArray = itemDisplayRuleSet.keyAssetRuleGroups;
                for (int i = 0; i < keyAssetRuleGroupArray.Length; i++)
                {
                    var index = keyAssetRuleGroupArray[i];
                    var keyAsset = index.keyAsset;

                    Main.ModLogger.LogError("==============");
                    Main.ModLogger.LogError(keyAsset);

                    var rules = index.displayRuleGroup.rules;
                    for (int j = 0; j < rules.Length; j++)
                    {
                        var rule = rules[j];

                        Main.ModLogger.LogError(rule);
                        Main.ModLogger.LogError(rule.childName);
                        Main.ModLogger.LogError(rule.followerPrefab);
                        Main.ModLogger.LogError(rule.limbMask);
                        Main.ModLogger.LogError(rule.localAngles);
                        Main.ModLogger.LogError(rule.localPos);
                        Main.ModLogger.LogError(rule.localScale);
                        Main.ModLogger.LogError(rule.ruleType);

                        if (keyAsset == Paths.EquipmentDef.EliteFireEquipment)
                        {
                            copiedBlazingIDRS.childName = rule.childName;
                            copiedBlazingIDRS.followerPrefab = rule.followerPrefab;
                            copiedBlazingIDRS.limbMask = rule.limbMask;
                            copiedBlazingIDRS.localAngles = rule.localAngles;
                            copiedBlazingIDRS.localPos = rule.localPos;
                            copiedBlazingIDRS.localScale = rule.localScale;
                            copiedBlazingIDRS.ruleType = rule.ruleType;
                            break;
                        }
                    }

                    Main.ModLogger.LogError("==============");
                }
            }
*/

            warbanner = PrefabAPI.InstantiateClone(Paths.GameObject.WarbannerWard, "Motivator Warbanner");
            var mdlWarbanner = warbanner.transform.GetChild(1);
            mdlWarbanner.transform.localPosition = Vector3.zero;
            mdlWarbanner.RemoveComponent<ObjectScaleCurve>();

            var buffWard = warbanner.GetComponent<BuffWard>();
            buffWard.buffDef = wrbnnerBuff;

            var cylinder = mdlWarbanner.GetChild(0).GetComponent<MeshRenderer>();
            var newMat = Object.Instantiate(Paths.Material.matWarbannerPole);
            newMat.SetColor("_TintColor", new Color32(160, 79, 60, 255));

            cylinder.material = newMat;

            var plane = mdlWarbanner.GetChild(1).GetComponent<SkinnedMeshRenderer>();
            var newMat2 = Object.Instantiate(Paths.Material.matWarbannerFlag);
            newMat2.SetTexture("_MainTex", Main.hifuSandswept.LoadAsset<Texture2D>("texMotivatorWarbanner.png"));

            plane.material = newMat2;
            // plane.GetComponent<Cloth>().enabled = false;

            var indicator = warbanner.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();

            var newMat3 = Object.Instantiate(Paths.Material.matWarbannerSphereIndicator2);
            newMat3.SetColor("_TintColor", new Color32(255, 0, 28, 255));
            newMat3.SetFloat("_InvFade", 1.622365f);
            newMat3.SetFloat("_RimStrength", 0.2729147f);
            newMat3.SetFloat("_IntersectionStrength", 1.563318f);
            newMat3.SetTexture("_RemapTex", Paths.Texture2D.texRampBeamLightning);

            indicator.material = newMat3;

            PrefabAPI.RegisterNetworkPrefab(warbanner);

            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
        }

        private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == Instance.EliteBuffDef)
            {
                self.RemoveComponent<MotivatorController>();
            }
        }

        private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == Instance.EliteBuffDef)
            {
                self.gameObject.AddComponent<MotivatorController>();
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(wrbnnerBuff))
            {
                args.moveSpeedMultAdd += passiveMovementSpeedBuff;
                args.baseAttackSpeedAdd += passiveAttackSpeedBuff;
            }
            if (sender.HasBuff(warcryBuff))
            {
                args.baseAttackSpeedAdd += onHitAttackSpeedBuff;
            }
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var info = report.damageInfo;
            if (!NetworkServer.active)
            {
                return;
            }

            if (!info.attacker)
            {
                return;
            }

            var motivatorController = info.attacker.GetComponent<MotivatorController>();
            if (!motivatorController)
            {
                return;
            }
            if (Util.CheckRoll(100f * info.procCoefficient))
            {
                if (Util.HasEffectiveAuthority(info.attacker))
                {
                    motivatorController.Proc();
                }
            }
        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }

        public static float GetOffset(GameObject prefab)
        {
            var capsuleCollider = prefab.GetComponent<CapsuleCollider>();
            if (capsuleCollider)
            {
                return capsuleCollider.height * 0.5f + capsuleCollider.center.y;
            }
            return 0f;
        }
    }

    public class MotivatorController : MonoBehaviour
    {
        public GameObject warbannerPrefab = Motivating.warbanner;
        public GameObject warbannerInstance;
        public float warbannerRadius = Motivating.passiveBuffRadius;
        public float onHitRadius = Motivating.onHitBuffRadius;
        public CharacterBody body;
        public Transform modelTransform;
        public HealthComponent healthComponent;
        public TeamIndex team;
        public static readonly SphereSearch sphereSearch = new();
        public static readonly List<HurtBox> hurtBoxBuffer = new();
        public static List<MotivatorController> motivatorControllers = new();
        public GameObject warbannerParent = new("Motivator Warbanner Parent");
        public Transform mdlWarbanner;

        public void Start()
        {
            body = GetComponent<CharacterBody>();
            healthComponent = body?.healthComponent;
            modelTransform = body?.modelLocator?.modelTransform;
            if (body)
            {
                warbannerRadius += body.radius;
                onHitRadius += body.radius;
            }
            if (modelTransform)
            {
                warbannerParent.transform.parent = modelTransform;
                warbannerParent.transform.localPosition = Vector3.zero;
                warbannerParent.transform.eulerAngles = Vector3.zero;

                warbannerInstance = Instantiate(warbannerPrefab, modelTransform.position, Quaternion.identity);
                warbannerInstance.transform.parent = warbannerParent.transform;

                warbannerInstance.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
                warbannerInstance.GetComponent<BuffWard>().Networkradius = warbannerRadius;

                NetworkServer.Spawn(warbannerInstance);

                mdlWarbanner = warbannerInstance.transform.GetChild(1);
                if (body)
                {
                    // mdlWarbanner.localScale = Vector3.one * body.radius * 0.3f;
                    // if (body.isPlayerControlled)
                    mdlWarbanner.gameObject.SetActive(false);
                }
            }

            /*
            Main.ModLogger.LogError("motivator warbanner is " + Motivator.warbanner);
            Main.ModLogger.LogError("warbanner instance is " + warbannerInstance);
            Main.ModLogger.LogError("warbanner instance networked body attaachment is " + warbannerInstance.GetComponent<NetworkedBodyAttachment>());
            * none null
            * */

            /*
            var networkedBodyAttachment = warbannerInstance.GetComponent<NetworkedBodyAttachment>();
            networkedBodyAttachment.AttachToGameObjectAndSpawn(warbannerOffset);

            warbannerInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(warbannerOffset);
            */

            motivatorControllers.Add(this);
        }

        public void FixedUpdate()
        {
            if (warbannerParent && modelTransform)
            {
                warbannerParent.transform.eulerAngles = modelTransform.eulerAngles;
            }

            if (warbannerInstance)
            {
                warbannerInstance.transform.localPosition = Vector3.zero;
                warbannerInstance.transform.localEulerAngles = Vector3.zero;
            }
        }

        public void Proc()
        {
            if (!body)
            {
                return;
            }

            if (!healthComponent)
            {
                return;
            }

            if (!healthComponent.alive)
            {
                return;
            }

            Vector3 corePosition = body.corePosition;
            sphereSearch.origin = corePosition;
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.radius = onHitRadius;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            sphereSearch.OrderCandidatesByDistance();
            sphereSearch.GetHurtBoxes(hurtBoxBuffer);
            sphereSearch.ClearCandidates();

            for (int i = 0; i < hurtBoxBuffer.Count; i++)
            {
                var hurtBox = hurtBoxBuffer[i];
                if (hurtBox.healthComponent)
                {
                    var targetBody = hurtBox.healthComponent.body;
                    if (targetBody && !targetBody.HasBuff(Motivating.Instance.EliteBuffDef) && targetBody.teamComponent.teamIndex == body.teamComponent.teamIndex)
                    {
                        targetBody.AddTimedBuff(Motivating.warcryBuff, Motivating.onHitAttackSpeedBuffDuration);
                    }
                }
            }

            hurtBoxBuffer.Clear();

            // Someone Else make vfx later and uncomment
            /*
            EffectManager.SpawnEffect(buffVFX, new EffectData
            {
                origin = corePosition,
                scale = radius
            }, true);
            */
        }

        public void OnDestroy()
        {
            NetworkServer.Destroy(warbannerInstance);
            motivatorControllers.Remove(this);
        }
    }
}