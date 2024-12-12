using System;
using RoR2.CharacterAI;
using RoR2.Navigation;

namespace Sandswept.Enemies.CannonballJellyfish
{
    public class CannonballJellyfish : EnemyBase<CannonballJellyfish>
    {
        public static GameObject JellyCoreProjectile;

        public override void LoadPrefabs()
        {
            prefab = Main.Assets.LoadAsset<GameObject>("CannonJellyBody.prefab");
            prefabMaster = PrefabAPI.InstantiateClone(Paths.GameObject.JellyfishMaster, "CannonJellyfishMaster");

            JellyCoreProjectile = Main.Assets.LoadAsset<GameObject>("JellyCoreProjectile.prefab");
            JellyCoreProjectile.GetComponent<ProjectileImpactExplosion>().explosionEffect = Paths.GameObject.SojournExplosionVFX;
            ContentAddition.AddProjectile(JellyCoreProjectile);
        }

        public override void PostCreation()
        {
            base.PostCreation();
            var stages = new List<Stage>() { Stage.AbandonedAqueduct, DirectorAPI.Stage.AbandonedAqueductSimulacrum, DirectorAPI.Stage.AphelianSanctuary, DirectorAPI.Stage.ArtifactReliquary, DirectorAPI.Stage.ArtifactReliquary_AbandonedAqueduct_Theme, DirectorAPI.Stage.ArtifactReliquary_ScorchedAcres_Theme, DirectorAPI.Stage.ScorchedAcres, DirectorAPI.Stage.DisturbedImpact, DirectorAPI.Stage.GoldenDieback, DirectorAPI.Stage.HelminthHatchery, DirectorAPI.Stage.TitanicPlains, DirectorAPI.Stage.TitanicPlainsSimulacrum, DirectorAPI.Stage.SkyMeadow, DirectorAPI.Stage.SkyMeadowSimulacrum, DirectorAPI.Stage.TreebornColony, DirectorAPI.Stage.SunderedGrove, DirectorAPI.Stage.ViscousFalls };
            RegisterEnemy(prefab, prefabMaster, stages);
        }

        public override void Modify()
        {
            base.Modify();

            master.bodyPrefab = prefab;
            body.baseNameToken.Add("Cannonball Jellyfish");
            body.portraitIcon = Main.hifuSandswept.LoadAsset<Texture>("texCannonballJellyfish.png");

            WipeAllDrivers(master.gameObject);
            AddNewDriver(master.gameObject, "JellyCharge", AISkillDriver.AimType.AtCurrentEnemy, AISkillDriver.MovementType.ChaseMoveTarget, AISkillDriver.TargetType.CurrentEnemy, 10f, 60f, SkillSlot.Primary);
            AddNewDriver(master.gameObject, "JellyFlee", AISkillDriver.AimType.AtCurrentEnemy, AISkillDriver.MovementType.FleeMoveTarget, AISkillDriver.TargetType.CurrentEnemy, 0f, 10f, SkillSlot.None);
            AddNewDriver(master.gameObject, "Strafe", AISkillDriver.AimType.AtCurrentEnemy, AISkillDriver.MovementType.StrafeMovetarget, AISkillDriver.TargetType.CurrentEnemy, 10f, 60f, SkillSlot.None);
            AddNewDriver(master.gameObject, "Chase", AISkillDriver.AimType.AtCurrentEnemy, AISkillDriver.MovementType.ChaseMoveTarget, AISkillDriver.TargetType.CurrentEnemy, 60f, float.PositiveInfinity, SkillSlot.None);

            var locator = body.GetComponent<SkillLocator>();

            ReplaceSkill(locator.primary, SkillDefs.JellyDash.instance.skillDef);

            master.GetComponent<BaseAI>().aimVectorMaxSpeed = 40000f;
            master.GetComponent<BaseAI>().aimVectorDampTime = 0.1f;

            body.GetComponent<CharacterDeathBehavior>().deathState = new(typeof(JellyDeath));
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
            isc.directorCreditCost = 70;
            isc.forbiddenFlags = NodeFlags.NoCharacterSpawn;
            isc.hullSize = HullClassification.Human;
            isc.nodeGraphType = MapNodeGroup.GraphType.Air;
            isc.sendOverNetwork = true;
            isc.prefab = prefabMaster;
        }
    }
}