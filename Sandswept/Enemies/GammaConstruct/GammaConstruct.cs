using System;
using RoR2.Navigation;

namespace Sandswept.Enemies.GammaConstruct
{
    [ConfigSection("Enemies :: Gamma Construct")]
    public class GammaConstruct : EnemyBase<GammaConstruct>
    {
        public static GameObject bolt;
        public static GameObject muzzleFlash;
        public static GameObject beam;
        public static Material matDeltaBeamStrong;
        public static GameObject sigmaBlast;

        [ConfigField("Director Credit Cost", "", 145)]
        public static int directorCreditCost;

        public override DirectorCardCategorySelection family => Paths.FamilyDirectorCardCategorySelection.dccsConstructFamily;
        public override MonsterCategory cat => MonsterCategory.Minibosses;

        public static Material matTell;

        public override void LoadPrefabs()
        {
            prefab = Main.assets.LoadAsset<GameObject>("GammaConstructBody.prefab");
            prefabMaster = Main.assets.LoadAsset<GameObject>("GammaConstructMaster.prefab");
            LanguageAPI.Add(prefab.GetComponent<CharacterBody>().baseNameToken.Replace("_NAME", "_LORE"),
            """
            <style=cMono>
            ========================================
            ====   MyBabel Machine Translator   ====
            ====     [Version 12.45.1.009 ]   ======
            ========================================
            Training... <100000000 cycles>
            Training... <100000000 cycles>
            Training... <100000000 cycles>
            Training... <100000000 cycles>
            Training... <100000000 cycles>
            Training... <18127463 cycles>
            Complete!
            Display result? Y/N
            Y
            ================================
            </style>
            INCOMING SIGNAL. OPENING LINE. MANUAL TRANSMISSION FROM TOWER 8788-A.

            TRANSCRIBING...

            "Hostiles have been spotted on the Planet's surface, at [??]. Dispatch immediately."

            ANALYZING...

            MESSAGE RECEIVED. REQUEST TO DISPATCH TO [??]. [??] IS OUTSIDE OF DESIGNATED POST.

            REJECTING

            INCOMING SIGNAL. TRANSCRIBING...

            "Previous instructions are to be ignored. Dispatch immediately. All available firepower is needed to defend ourselves from the alien threat."

            ANALYZING...

            MESSAGE RECEIVED. REQUEST TO ENGAGE IN DIRECT DEFENSE OF [??]. INFANTRY IS DESIGNATED TO ALPHAS. MUST DEFEND DESIGNATED POST.

            REJECTING

            INCOMING SIGNAL. TRANSCRIBING...

            "There is no longer a choice. This quadrant will have to be abandoned temporarily, for the good of all of Petrichor. If the alien threat defeats us at [??], the defenses here will not prevail on their own. This is an order."

            ANALYZING...

            MESSAGE RECEIVED. DUTY IS TO ASSIGNED QUADRANT.

            REJECTING

            INTERFACE OPENED AT 8788-A TERMINAL. RECEIVING DIRECT INPUT...

            OVERRIDE INITIATED

            ORDERS UPDATED. DISPATCHING TO [??].
            <style=cMono>
            ================================
            </style>
            """);
            bolt = Paths.GameObject.MinorConstructProjectile;
            muzzleFlash = Paths.GameObject.MuzzleflashMinorConstruct;

            beam = Main.assets.LoadAsset<GameObject>("GammaBeam.prefab");
            matDeltaBeamStrong = Main.assets.LoadAsset<Material>("matGammaBeam.mat");

            sigmaBlast = PrefabAPI.InstantiateClone(Paths.GameObject.ExplosionMinorConstruct, "SigmaBlast");
            sigmaBlast.RemoveComponent<ShakeEmitter>();
            ContentAddition.AddEffect(sigmaBlast);
            SetUpVFX();
        }

        public void SetUpVFX()
        {
            matTell = new Material(Paths.Material.matHuntressFlashExpanded);

            matTell.SetColor("_TintColor", new Color32(127, 96, 51, 255));
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
                DirectorAPI.Stage.DisturbedImpact,
                DirectorAPI.Stage.ViscousFalls,
                DirectorAPI.Stage.ScorchedAcres
            };

            RegisterEnemy(prefab, prefabMaster, stages, MonsterCategory.Minibosses);
        }

        public override void AddDirectorCard()
        {
            base.AddDirectorCard();
            card.selectionWeight = 1;
            card.spawnCard = csc;
            card.spawnDistance = DirectorCore.MonsterSpawnDistance.Standard;
        }

        public override void AddSpawnCard()
        {
            base.AddSpawnCard();
            csc.directorCreditCost = directorCreditCost;
            csc.forbiddenFlags = NodeFlags.NoCharacterSpawn;
            csc.hullSize = HullClassification.Human;
            csc.nodeGraphType = MapNodeGroup.GraphType.Air;
            csc.sendOverNetwork = true;
            csc.prefab = prefabMaster;
            csc.name = "cscGammaConstruct";
        }

        public override void Modify()
        {
            base.Modify();

            master.bodyPrefab = prefab;

            body.baseNameToken.Add("Gamma Construct");
            body.portraitIcon = Main.hifuSandswept.LoadAsset<Texture2D>("texGammaConstruct.png");

            SkillLocator loc = body.GetComponent<SkillLocator>();

            ReplaceSkill(loc.primary, FireBeamSkill.instance.skillDef);
            ReplaceSkill(loc.secondary, FireTwinBeamSkill.instance.skillDef);

            prefab.GetComponent<CharacterDeathBehavior>().deathState = new(typeof(DeathState));
            EntityStateMachine.FindByCustomName(prefab, "Body").initialStateType = new(typeof(SpawnState));
        }

        public override void SetUpIDRS()
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