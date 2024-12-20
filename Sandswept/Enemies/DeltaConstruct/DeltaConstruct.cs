using System;
using RoR2.Navigation;

namespace Sandswept.Enemies.DeltaConstruct
{
    public class DeltaConstruct : EnemyBase<DeltaConstruct>
    {
        public static GameObject bolt;
        public static GameObject muzzleFlash;
        public static GameObject beam;
        public static Material matDeltaBeamStrong;
        public static GameObject DeltaBurnyTrail;

        public override void LoadPrefabs()
        {
            prefab = Main.Assets.LoadAsset<GameObject>("DeltaConstructBody.prefab");
            prefabMaster = Main.Assets.LoadAsset<GameObject>("DeltaConstructMaster.prefab");

            bolt = Paths.GameObject.MinorConstructProjectile;
            muzzleFlash = Paths.GameObject.MuzzleflashMinorConstruct;

            beam = Main.Assets.LoadAsset<GameObject>("DeltaBeam.prefab");
            matDeltaBeamStrong = Main.Assets.LoadAsset<Material>("matDeltaBeamStrong.mat");

            DeltaBurnyTrail = Main.Assets.LoadAsset<GameObject>("DeltaBurnyTrail.prefab");
            ContentAddition.AddNetworkedObject(DeltaBurnyTrail);
        }

        public override void PostCreation()
        {
            base.PostCreation();

            List<Stage> stages = new List<DirectorAPI.Stage> {
                Stage.SkyMeadow,
                Stage.SkyMeadowSimulacrum,
                DirectorAPI.Stage.SulfurPools,
                DirectorAPI.Stage.TreebornColony,
                DirectorAPI.Stage.GoldenDieback,
                DirectorAPI.Stage.ArtifactReliquary_AphelianSanctuary_Theme,
                DirectorAPI.Stage.RallypointDelta,
                DirectorAPI.Stage.HelminthHatchery,
                DirectorAPI.Stage.DisturbedImpact
            };

            RegisterEnemy(prefab, prefabMaster, stages, MonsterCategory.Minibosses);

            // prefab.GetComponent<ModelLocator>()._modelTransform.AddComponent<ShittyDebugComp>();
        }

        public override void AddDirectorCard()
        {
            base.AddDirectorCard();
            card.selectionWeight = 1;
            card.spawnCard = isc;
            card.spawnDistance = DirectorCore.MonsterSpawnDistance.Standard;
        }

        public override void AddSpawnCard()
        {
            base.AddSpawnCard();
            isc.directorCreditCost = 150;
            isc.forbiddenFlags = NodeFlags.NoCharacterSpawn;
            isc.hullSize = HullClassification.Human;
            isc.nodeGraphType = MapNodeGroup.GraphType.Ground;
            isc.sendOverNetwork = true;
            isc.prefab = prefabMaster;
        }

        public override void Modify()
        {
            base.Modify();

            master.bodyPrefab = prefab;

            body.baseNameToken.Add("Delta Construct");
            body.portraitIcon = Main.hifuSandswept.LoadAsset<Texture>("texDeltaConstruct.png");

            SkillLocator loc = body.GetComponent<SkillLocator>();

            ReplaceSkill(loc.primary, FireBoltsSkill.instance.skillDef);
            ReplaceSkill(loc.secondary, SkystrikeSkill.instance.skillDef);

            prefab.GetComponent<CharacterDeathBehavior>().deathState = new(typeof(BaseConstructDeath));
            EntityStateMachine.FindByCustomName(prefab, "Body").initialStateType = new(typeof(BaseConstructSpawn));
        }

        public override void SetupIDRS()
        {
            AddDisplayRule(Paths.EquipmentDef.EliteFireEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, -0.03616F, -0.40235F),
                localAngles = new Vector3(10.15459F, 337.802F, 194.5243F),
                localScale = new Vector3(0.34135F, 0.34135F, 0.34135F),
                followerPrefab = Paths.GameObject.DisplayEliteHorn,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteIceEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, -0.03569F, -0.37111F),
                localAngles = new Vector3(10.42402F, 183.3698F, 183.9226F),
                localScale = new Vector3(0.09205F, 0.09205F, 0.09205F),
                followerPrefab = Paths.GameObject.DisplayEliteIceCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteEarthEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 1.06064F, 0.06114F),
                localAngles = new Vector3(309.1414F, 0F, 270F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F),
                followerPrefab = Paths.GameObject.DisplayEliteMendingAntlers,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteAurelioniteEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 0.40959F, -0.26543F),
                localAngles = new Vector3(280.9711F, 180.0002F, 179.9999F),
                localScale = new Vector3(1F, 1F, 1F),
                followerPrefab = Paths.GameObject.DisplayEliteAurelioniteEquipment,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.ElitePoisonEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, -0.03104F, -0.36065F),
                localAngles = new Vector3(0F, 180F, 0F),
                localScale = new Vector3(0.17335F, 0.17335F, 0.17335F),
                followerPrefab = Paths.GameObject.DisplayEliteUrchinCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteHauntedEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, -0.03241F, -0.26001F),
                localAngles = new Vector3(0F, 0F, 180F),
                localScale = new Vector3(0.19985F, 0.19985F, 0.19985F),
                followerPrefab = Paths.GameObject.DisplayEliteStealthCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteLunarEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 0.74635F, -0.07608F),
                localAngles = new Vector3(90F, 0F, 0F),
                localScale = new Vector3(0.29435F, 0.29378F, 0.29435F),
                followerPrefab = Paths.GameObject.DisplayEliteLunarEye,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteLightningEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 0.4868F, -0.35906F),
                localAngles = new Vector3(333.8828F, 173.3047F, 177.03F),
                localScale = new Vector3(1F, 1F, 1F),
                followerPrefab = Paths.GameObject.DisplayEliteRhinoHorn,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteVoidEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(-0.65868F, 3.12402F, 0.06114F),
                localAngles = new Vector3(359.9704F, 287.4572F, 359.9907F),
                localScale = new Vector3(1.85743F, 1.85743F, 1.85743F),
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteBeadEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(-0.16075F, 0.10642F, -0.43811F),
                localAngles = new Vector3(0.01025F, 99.63512F, 292.3816F),
                localScale = new Vector3(0.05F, 0.05F, 0.05F),
                followerPrefab = Paths.GameObject.DisplayEliteBeadSpike,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteEarthEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 0.21976F, 0.12895F),
                localAngles = new Vector3(63.21782F, 10.4108F, 189.3193F),
                localScale = new Vector3(2F, 2F, 2F),
                followerPrefab = Paths.GameObject.DisplayEliteMendingAntlers,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Elites.Osmium.Instance.EliteEquipmentDef, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 0.65735F, 0.02518F),
                localAngles = new Vector3(0F, 0F, 270F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F),
                followerPrefab = Elites.Osmium.crownModel,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Elites.Motivating.Instance.EliteEquipmentDef, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0.00427F, -0.25201F, 0.05359F),
                localAngles = new Vector3(270F, 0F, 0F),
                localScale = new Vector3(2.31024F, 2.31024F, 2.31024F),
                followerPrefab = Elites.Motivating.Crown,
                limbMask = LimbFlags.None
            });

            CollapseIDRS();
        }

        public class ShittyDebugComp : MonoBehaviour
        {
            public Animator anim;

            public void Start()
            {
                anim = GetComponent<Animator>();
            }

            public void Update()
            {
                Debug.Log("yaw: " + anim.GetFloat("aimYawCycle"));
                Debug.Log("pitch: " + anim.GetFloat("aimPitchCycle"));
            }
        }
    }
}