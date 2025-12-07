using System;
using System.Linq;
using RoR2.Navigation;

namespace Sandswept.Enemies.DeltaConstruct
{
    [ConfigSection("Enemies :: Delta Construct")]
    public class DeltaConstruct : EnemyBase<DeltaConstruct>
    {
        public static GameObject bolt;
        public static GameObject muzzleFlash;
        public static GameObject beam;
        public static Material matDeltaBeamStrong;
        public static GameObject DeltaBurnyTrail;

        [ConfigField("Director Credit Cost", "", 130)]
        public static int directorCreditCost;

        public override DirectorCardCategorySelection family => Paths.FamilyDirectorCardCategorySelection.dccsConstructFamily;
        public override MonsterCategory cat => MonsterCategory.Minibosses;

        public static Material matTell;

        public override void LoadPrefabs()
        {
            prefab = Main.assets.LoadAsset<GameObject>("DeltaConstructBody.prefab");
            prefab.GetComponent<CharacterBody>().bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            prefabMaster = Main.assets.LoadAsset<GameObject>("DeltaConstructMaster.prefab");
            LanguageAPI.Add(prefab.GetComponent<CharacterBody>().baseNameToken.Replace("_NAME", "_LORE"),
            """
            Brother, watch as I create my own guardian.
            First, I will take mass, shape it into pyramids, as you taught me with the Alpha Constructs. Eight pyramids, to be exact.
            Next, I will imbue these with blood and soul. Blood, for heat, and soul, for compassion and empathy.
             - No. These ratios are improper. They are supposed to be loyal, not emotional. There is too much soul in these creations.
            Brother? Why is soul unnecessary?
             - Watch as I fill this knurl with excess soul. Its form becomes gold, a poor material for endurance.
            Is that it?
             - No, brother, that is not all. If I were to hit it, it would want to retaliate. Soul brings life to our constructs. It is unstable, unreliable, and it dampens the other compounds, offsetting the ratios.
            If we were to treat our constructs properly, wouldn't giving them life be a good thing?
             - Giving something soul gives it free will; the free will to decide we are not the construct's supreme creators. Our constructs do not need to make that decision, only us.
            """);
            matDeltaBeamStrong = Main.assets.LoadAsset<Material>("matDeltaBeamStrong.mat");
            matDeltaBeamStrong.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            matDeltaBeamStrong.SetColor("_TintColor", Color.red);
            matDeltaBeamStrong.SetFloat("_Boost", 5.388472f);
            matDeltaBeamStrong.SetFloat("_AlphaBoost", 5.388472f);
            matDeltaBeamStrong.SetFloat("_AlphaBias", 1f);
            matDeltaBeamStrong.SetFloat("_InvFade", 0.5f);
            matDeltaBeamStrong.SetInt("_ZTest", 4);
            matDeltaBeamStrong.SetInt("_DstBlend", 1);
            matDeltaBeamStrong.SetInt("_SrcBlend", 1);

            bolt = PrefabAPI.InstantiateClone(Paths.GameObject.MinorConstructProjectile, "Delta Construct Bolt Projectile");
            GameObject boltGhost = PrefabAPI.InstantiateClone(Paths.GameObject.MinorConstructProjectileGhost, "Delta Construct Bolt Ghost", false);

            VFXUtils.RecolorMaterialsAndLights(boltGhost.transform.Find("Trail").gameObject, new Color32(255, 40, 40, 255), new Color32(255, 40, 40, 255), true);

            boltGhost.transform.Find("SoftGlow").gameObject.SetActive(false);

            var pyramid = boltGhost.transform.Find("Pyramid");
            VFXUtils.RecolorMaterialsAndLights(pyramid.gameObject, Color.red, Color.red, true);
            var pyramidPSR = pyramid.GetComponent<ParticleSystemRenderer>();
            pyramidPSR.material.SetFloat("_Boost", 7.208117f);
            pyramidPSR.material.SetFloat("_AlphaBoost", 2.51821f);
            pyramidPSR.material.SetFloat("_AlphaBias", 0f);

            bolt.GetComponent<ProjectileController>().ghostPrefab = boltGhost;
            ContentAddition.AddProjectile(bolt);

            muzzleFlash = PrefabAPI.InstantiateClone(Paths.GameObject.MuzzleflashMinorConstruct, "Delta Construct Muzzle Flash", false);
            muzzleFlash.transform.Find("Chunks").gameObject.SetActive(false); // shitass broken shader that I can't see therefore I don't know how to recolor it
            VFXUtils.RecolorMaterialsAndLights(muzzleFlash.gameObject, new Color32(201, 20, 20, 255), new Color32(201, 20, 20, 255), true);

            ContentAddition.AddEffect(muzzleFlash);

            beam = Main.assets.LoadAsset<GameObject>("DeltaBeam.prefab");
            var faggotDispersers = beam.transform.Find("Particle System, Sparks (1)").GetComponent<ParticleSystemRenderer>();
            VFXUtils.RecolorMaterialsAndLights(faggotDispersers.gameObject, Color.red, Color.red, true);
            faggotDispersers.material.SetFloat("_Boost", 12.0718f);
            faggotDispersers.material.SetFloat("_AlphaBoost", 2.420291f);
            faggotDispersers.material.SetFloat("_AlphaBias", 0.044f);
            // zjebana pizda pierdolona dziwka kurwa co nadal jest pomaranczowa po recolorze

            DeltaBurnyTrail = Main.assets.LoadAsset<GameObject>("DeltaBurnyTrail.prefab");

            var destroyOnTimer = DeltaBurnyTrail.GetComponent<DestroyOnTimer>();
            destroyOnTimer.duration = 16f;

            var particleSystemMain = DeltaBurnyTrail.GetComponent<ParticleSystem>().main;
            particleSystemMain.duration = 16f;
            particleSystemMain.startLifetime = 16f;

            // visuals fade bumped up to ~8s due to invfade

            var particleSystemRenderer = DeltaBurnyTrail.GetComponent<ParticleSystemRenderer>();
            var newTrailMat = new Material(particleSystemRenderer.material);

            newTrailMat.SetColor("_TintColor", new Color32(255, 0, 0, 27));
            // newTrailMat.SetFloat("_InvFade", 0f);
            newTrailMat.SetFloat("_Boost", 1f);
            newTrailMat.SetFloat("_AlphaBoost", 11.21f);

            particleSystemRenderer.material = newTrailMat;

            var newImpactEffect = PrefabAPI.InstantiateClone(Paths.GameObject.OmniExplosionVFXMinorConstruct, "Delta Construct Impact VFX", false);
            VFXUtils.RecolorMaterialsAndLights(newImpactEffect, Color.red, Color.red, true);
            newImpactEffect.transform.Find("Fire").GetComponent<ParticleSystemRenderer>().material.SetFloat("_AlphaBias", 0f);

            ContentAddition.AddEffect(newImpactEffect);

            bolt.GetComponent<ProjectileSingleTargetImpact>().impactEffect = newImpactEffect;

            // how the fuck does increasing all this to 10s make it last 5s???
            // ContentAddition.AddNetworkedObject(DeltaBurnyTrail);
            // commented this out in hopes of it not breaking anything, since the error was annoying
            SetUpVFX();
        }

        public void SetUpVFX()
        {
            matTell = new Material(Paths.Material.matHuntressFlashExpanded);

            matTell.SetColor("_TintColor", new Color32(127, 0, 4, 255));
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
            card.spawnCard = csc;
            card.spawnDistance = DirectorCore.MonsterSpawnDistance.Standard;
        }

        public override void AddSpawnCard()
        {
            base.AddSpawnCard();
            csc.directorCreditCost = directorCreditCost;
            csc.forbiddenFlags = NodeFlags.NoCharacterSpawn;
            csc.hullSize = HullClassification.Human;
            csc.nodeGraphType = MapNodeGroup.GraphType.Ground;
            csc.sendOverNetwork = true;
            csc.prefab = prefabMaster;
            csc.name = "cscDeltaConstruct";
        }

        public override void Modify()
        {
            base.Modify();

            master.bodyPrefab = prefab;

            body.baseNameToken.Add("Delta Construct");
            body.portraitIcon = Main.sandsweptHIFU.LoadAsset<Texture>("texDeltaConstruct.png");

            SkillLocator loc = body.GetComponent<SkillLocator>();

            ReplaceSkill(loc.primary, FireBoltsSkill.instance.skillDef);
            ReplaceSkill(loc.secondary, SkystrikeSkill.instance.skillDef);

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
                localAngles = new Vector3(10.15459F, 337.802F, 194.5243F),
                localScale = new Vector3(0.34135F, 0.34135F, 0.34135F),
                followerPrefab = Paths.GameObject.DisplayEliteHorn,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteIceEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, -0.03569F, -0.37111F),
                localAngles = new Vector3(10.42402F, 183.3698F, 183.9226F),
                localScale = new Vector3(0.09205F, 0.09205F, 0.09205F),
                followerPrefab = Paths.GameObject.DisplayEliteIceCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteEarthEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 1.06064F, 0.06114F),
                localAngles = new Vector3(309.1414F, 0F, 270F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F),
                followerPrefab = Paths.GameObject.DisplayEliteMendingAntlers,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteAurelioniteEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 0.40959F, -0.26543F),
                localAngles = new Vector3(280.9711F, 180.0002F, 179.9999F),
                localScale = new Vector3(1F, 1F, 1F),
                followerPrefab = Paths.GameObject.DisplayEliteAurelioniteEquipment,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.ElitePoisonEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, -0.03104F, -0.36065F),
                localAngles = new Vector3(0F, 180F, 0F),
                localScale = new Vector3(0.17335F, 0.17335F, 0.17335F),
                followerPrefab = Paths.GameObject.DisplayEliteUrchinCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteHauntedEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, -0.03241F, -0.26001F),
                localAngles = new Vector3(0F, 0F, 180F),
                localScale = new Vector3(0.19985F, 0.19985F, 0.19985F),
                followerPrefab = Paths.GameObject.DisplayEliteStealthCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteLunarEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 0.74635F, -0.07608F),
                localAngles = new Vector3(90F, 0F, 0F),
                localScale = new Vector3(0.29435F, 0.29378F, 0.29435F),
                followerPrefab = Paths.GameObject.DisplayEliteLunarEye,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteLightningEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 0.4868F, -0.35906F),
                localAngles = new Vector3(333.8828F, 173.3047F, 177.03F),
                localScale = new Vector3(1F, 1F, 1F),
                followerPrefab = Paths.GameObject.DisplayEliteRhinoHorn,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteBeadEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(-0.16075F, 0.10642F, -0.43811F),
                localAngles = new Vector3(0.01025F, 99.63512F, 292.3816F),
                localScale = new Vector3(0.05F, 0.05F, 0.05F),
                followerPrefab = Paths.GameObject.DisplayEliteBeadSpike,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteEarthEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 0.21976F, 0.12895F),
                localAngles = new Vector3(63.21782F, 10.4108F, 189.3193F),
                localScale = new Vector3(2F, 2F, 2F),
                followerPrefab = Paths.GameObject.DisplayEliteMendingAntlers,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Elites.Osmium.Instance.EliteEquipmentDef, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 0.65735F, 0.02518F),
                localAngles = new Vector3(0F, 0F, 270F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F),
                followerPrefab = Elites.Osmium.crownModel,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Elites.Motivating.Instance.EliteEquipmentDef, new()
            {
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
                Main.ModLogger.LogError("yaw: " + anim.GetFloat("aimYawCycle"));
                Main.ModLogger.LogError("pitch: " + anim.GetFloat("aimPitchCycle"));
            }
        }
    }
}