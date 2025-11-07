using System;
using RoR2.CharacterAI;
using RoR2.Navigation;
using Sandswept.Enemies.ArdentWisp;
using Sandswept.Enemies.ArdentWisp.States;

namespace Sandswept.Enemies.ArdentWisp
{
    [ConfigSection("Enemies :: Ardent Wisp")]
    public class ArdentWisp : EnemyBase<ArdentWisp>
    {
        public static GameObject JellyCoreProjectile;

        [ConfigField("Director Credit Cost", "", 300)]
        public static int directorCreditCost;

        public override DirectorCardCategorySelection family => Paths.FamilyDirectorCardCategorySelection.dccsWispFamily;
        public override MonsterCategory cat => MonsterCategory.Minibosses;

        public static GameObject ArdentChargeLine;
        public static GameObject ArdentExplosion;
        public static GameObject ArdentFireball;
        public static GameObject ArdentBombProjectile;
        public static Material IndicatorArdentWisp;

        public override void Create()
        {
            return;
        }

        public override void LoadPrefabs()
        {
            prefab = Main.assets.LoadAsset<GameObject>("ArdentWispBody.prefab");
            prefabMaster = Main.assets.LoadAsset<GameObject>("ArdentWispMaster.prefab");

            IndicatorArdentWisp = Object.Instantiate(Paths.Material.matNullBombAreaIndicator);
            IndicatorArdentWisp.SetFloat("_RimStrength", 0f);
            IndicatorArdentWisp.SetFloat("_InvFade", 1.1416f);
            IndicatorArdentWisp.SetFloat("_SoftPower", 2.6864f);
            IndicatorArdentWisp.SetFloat("_Boost", 1.611f);
            IndicatorArdentWisp.SetFloat("_RimPower", 10.6f);
            IndicatorArdentWisp.SetFloat("_AlphaBoost", 2.66f);
            IndicatorArdentWisp.SetFloat("_IntersectionStrength", 0.3f);
            IndicatorArdentWisp.SetTexture("_RemapTex", Paths.Texture2D.texRampAncientWisp);
            Object.DontDestroyOnLoad(IndicatorArdentWisp);

            ArdentChargeLine = Main.assets.LoadAsset<GameObject>("ArdentStrikeEffect.prefab");
            ArdentChargeLine.AddComponent<VFXAttributes>((x) =>
            {
                x.DoNotPool = true;
                x.DoNotCullPool = true;
            });
            ArdentChargeLine.GetComponent<ArdentFlareCharge>().radius.sharedMaterial = IndicatorArdentWisp;

            ContentAddition.AddEffect(ArdentChargeLine);

            ArdentExplosion = Main.assets.LoadAsset<GameObject>("ArdentExplosionEffect.prefab");
            ArdentExplosion.GetComponent<VFXAttributes>().DoNotPool = true;

            ContentAddition.AddEffect(ArdentExplosion);

            ArdentFireball = Main.assets.LoadAsset<GameObject>("ArdentFlameProjectile.prefab");
            ContentAddition.AddEffect(ArdentFireball);

            ArdentBombProjectile = Main.assets.LoadAsset<GameObject>("ArdentBombProjectile.prefab");
            ArdentBombProjectile.EditComponent<ArdentBombProjectile>((x) =>
            {
                x.outerRadius.GetComponent<MeshRenderer>().sharedMaterial = IndicatorArdentWisp;
                x.innerRadius.GetComponent<MeshRenderer>().sharedMaterial = Paths.Material.matNullBombAreaIndicator;
            });
            ArdentBombProjectile.GetComponent<ProjectileExplosion>().explosionEffect = Paths.GameObject.ExplosionGolem;

            ContentAddition.AddProjectile(ArdentBombProjectile);
        }

        public override void PostCreation()
        {
            base.PostCreation();
            var stages = new List<Stage>() { Stage.AbandonedAqueduct, DirectorAPI.Stage.AbandonedAqueductSimulacrum, DirectorAPI.Stage.AphelianSanctuary, DirectorAPI.Stage.ArtifactReliquary, DirectorAPI.Stage.ArtifactReliquary_AbandonedAqueduct_Theme, DirectorAPI.Stage.ArtifactReliquary_ScorchedAcres_Theme, DirectorAPI.Stage.ScorchedAcres, DirectorAPI.Stage.DisturbedImpact, DirectorAPI.Stage.GoldenDieback, DirectorAPI.Stage.HelminthHatchery, DirectorAPI.Stage.TitanicPlains, DirectorAPI.Stage.TitanicPlainsSimulacrum, DirectorAPI.Stage.SkyMeadow, DirectorAPI.Stage.SkyMeadowSimulacrum, DirectorAPI.Stage.TreebornColony, DirectorAPI.Stage.SunderedGrove, DirectorAPI.Stage.ViscousFalls };
            RegisterEnemy(prefab, prefabMaster, stages, MonsterCategory.BasicMonsters);
        }

        public override void Modify()
        {
            base.Modify();

            master.bodyPrefab = prefab;
            body.baseNameToken.Add("Ardent Wisp");

            var locator = body.GetComponent<SkillLocator>();

            ReplaceSkill(locator.primary, States.ArdentBombSkill.instance.skillDef);
            ReplaceSkill(locator.secondary, States.CarpetFireSkill.instance.skillDef);

            body.GetComponent<CharacterDeathBehavior>().deathState = new(typeof(ArdentWispDeath));
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
            csc.name = "cscArdentWisp";
        }

        public override void SetUpIDRS()
        {
            CollapseIDRS();
        }
    }
}