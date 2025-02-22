using System;
using RoR2.Navigation;
using Sandswept.Enemies.StonePillar.States;

namespace Sandswept.Enemies.StonePillar
{
    public class StonePillar : EnemyBase<StonePillar>
    {
        public override DirectorCardCategorySelection family => null;
        public override MonsterCategory cat => MonsterCategory.Champions;

        public override void LoadPrefabs()
        {
            prefab = Main.Assets.LoadAsset<GameObject>("StonePillarBody.prefab");
            prefabMaster = Main.Assets.LoadAsset<GameObject>("StonePillarMaster.prefab");
        }

        public override void PostCreation()
        {
            base.PostCreation();

            ContentAddition.AddBody(prefab);
            ContentAddition.AddMaster(prefabMaster);
        }

        public override void AddDirectorCard()
        {
            base.AddDirectorCard();
        }

        public override void AddSpawnCard()
        {
            base.AddSpawnCard();
        }

        public override void Modify()
        {
            base.Modify();

            master.bodyPrefab = prefab;

            body.baseNameToken.Add("Nu Construct");
            body.portraitIcon = null;

            SkillLocator loc = body.GetComponent<SkillLocator>();

            ReplaceSkill(loc.primary, Skills.Slam.instance);
            // ReplaceSkill(loc.secondary, FireTwinBeamSkill.instance.skillDef);

            prefab.GetComponent<CharacterDeathBehavior>().deathState = new(typeof(BaseConstructDeath));
            EntityStateMachine.FindByCustomName(prefab, "Body").initialStateType = new(typeof(BaseConstructSpawn));
            EntityStateMachine.FindByCustomName(prefab, "Body").mainStateType = new(typeof(PillarMainState));
        }

        public override void SetupIDRS()
        {
            AddDisplayRule(Paths.EquipmentDef.EliteFireEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, -0.03616F, -0.40235F),
                localAngles = new Vector3(12.96796F, 256.5957F, 78.47137F),
                localScale = new Vector3(0.53449F, 0.53449F, 0.53449F),
                followerPrefab = Paths.GameObject.DisplayEliteHorn,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteIceEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0.33496F, -0.92779F, 0F),
                localAngles = new Vector3(21.33943F, 90.01358F, 183.3155F),
                localScale = new Vector3(0.20691F, 0.3167F, 0.20691F),
                followerPrefab = Paths.GameObject.DisplayEliteIceCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteAurelioniteEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(1.07206F, 0.66943F, 0F),
                localAngles = new Vector3(270F, 270F, 0F),
                localScale = new Vector3(2F, 2F, 2F),
                followerPrefab = Paths.GameObject.DisplayEliteAurelioniteEquipment,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.ElitePoisonEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0.6929F, -0.0446F, -0.04675F),
                localAngles = new Vector3(1.11274F, 82.99651F, 358.7424F),
                localScale = new Vector3(0.24801F, 0.24801F, 0.24801F),
                followerPrefab = Paths.GameObject.DisplayEliteUrchinCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteHauntedEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0.538F, -0.01053F, 0F),
                localAngles = new Vector3(16.82266F, 90.80365F, 181.3537F),
                localScale = new Vector3(0.38189F, 0.38189F, 0.38189F),
                followerPrefab = Paths.GameObject.DisplayEliteStealthCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteLunarEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 0.74635F, -0.07608F),
                localAngles = new Vector3(90F, 0F, 0F),
                localScale = new Vector3(1.28393F, 1.28145F, 1.28393F),
                followerPrefab = Paths.GameObject.DisplayEliteLunarEye,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteLightningEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(-0.82796F, -0.27011F, 0F),
                localAngles = new Vector3(353.0671F, 268.2247F, 359.734F),
                localScale = new Vector3(1.98965F, 1.98965F, 1.98965F),
                followerPrefab = Paths.GameObject.DisplayEliteRhinoHorn,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteBeadEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0.73504F, 0.08889F, -0.43811F),
                localAngles = new Vector3(55.15301F, 71.07037F, 325.3015F),
                localScale = new Vector3(0.09869F, 0.09869F, 0.09869F),
                followerPrefab = Paths.GameObject.DisplayEliteBeadSpike,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteEarthEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0.54282F, 0.43117F, 0.12895F),
                localAngles = new Vector3(61.90358F, 283.1268F, 191.1874F),
                localScale = new Vector3(3.36659F, 3.36659F, 3.36659F),
                followerPrefab = Paths.GameObject.DisplayEliteMendingAntlers,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Elites.Osmium.Instance.EliteEquipmentDef, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0.52083F, 1.48547F, 0.02518F),
                localAngles = new Vector3(351.7859F, 270.0001F, 270F),
                localScale = new Vector3(0.92859F, 0.92859F, 0.92859F),
                followerPrefab = Elites.Osmium.crownModel,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Elites.Motivating.Instance.EliteEquipmentDef, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(-0.10456F, -0.24988F, 0.00999F),
                localAngles = new Vector3(55.71718F, 269.9998F, 179.9999F),
                localScale = new Vector3(5.08253F, 5.08253F, 5.08253F),
                followerPrefab = Elites.Motivating.Crown,
                limbMask = LimbFlags.None
            });

            CollapseIDRS();
        }
    }
}