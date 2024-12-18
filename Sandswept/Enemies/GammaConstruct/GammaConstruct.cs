using System;
using RoR2.Navigation;

namespace Sandswept.Enemies.GammaConstruct
{
    public class GammaConstruct : EnemyBase<GammaConstruct>
    {
        public static GameObject bolt;
        public static GameObject muzzleFlash;
        public static GameObject beam;
        public static Material matDeltaBeamStrong;
        public static GameObject sigmaBlast;

        public override void LoadPrefabs()
        {
            prefab = Main.Assets.LoadAsset<GameObject>("GammaConstructBody.prefab");
            prefabMaster = Main.Assets.LoadAsset<GameObject>("GammaConstructMaster.prefab");

            bolt = Paths.GameObject.MinorConstructProjectile;
            muzzleFlash = Paths.GameObject.MuzzleflashMinorConstruct;

            beam = Main.Assets.LoadAsset<GameObject>("DeltaBeam.prefab");
            matDeltaBeamStrong = Main.Assets.LoadAsset<Material>("matDeltaBeamStrong.mat");

            sigmaBlast = PrefabAPI.InstantiateClone(Paths.GameObject.ExplosionMinorConstruct, "SigmaBlast");
            sigmaBlast.RemoveComponent<ShakeEmitter>();
            ContentAddition.AddEffect(sigmaBlast);
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
            card.spawnCard = isc;
            card.spawnDistance = DirectorCore.MonsterSpawnDistance.Standard;
        }

        public override void AddSpawnCard()
        {
            base.AddSpawnCard();
            isc.directorCreditCost = 185;
            isc.forbiddenFlags = NodeFlags.NoCharacterSpawn;
            isc.hullSize = HullClassification.Human;
            isc.nodeGraphType = MapNodeGroup.GraphType.Air;
            isc.sendOverNetwork = true;
            isc.prefab = prefabMaster;
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

            prefab.GetComponent<CharacterDeathBehavior>().deathState = new(typeof(BaseConstructDeath));
            EntityStateMachine.FindByCustomName(prefab, "Body").initialStateType = new(typeof(BaseConstructSpawn));
        }
    }
}