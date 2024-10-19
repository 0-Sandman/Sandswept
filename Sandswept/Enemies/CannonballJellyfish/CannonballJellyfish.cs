using System;
using RoR2.CharacterAI;
using RoR2.Navigation;

namespace Sandswept.Enemies.CannonballJellyfish
{
    public class CannonballJellyfish : EnemyBase<CannonballJellyfish>
    {
        public override void LoadPrefabs()
        {
            prefab = PrefabAPI.InstantiateClone(Paths.GameObject.JellyfishBody, "CannonJellyfishBody");
            prefabMaster = PrefabAPI.InstantiateClone(Paths.GameObject.JellyfishMaster, "CannonJellyfishMaster");
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
            body.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            body.baseNameToken = "SANDSWEPT_ENEMY_CBJ_NAME";
            body.baseNameToken.Add("Cannonball Jellyfish");

            SwapStats(prefab, 12, 0, 10, 40, 0, 80, 0);
            WipeAllDrivers(master.gameObject);
            AddNewDriver(master.gameObject, "JellyCharge", AISkillDriver.AimType.AtCurrentEnemy, AISkillDriver.MovementType.ChaseMoveTarget, AISkillDriver.TargetType.CurrentEnemy, 10f, 40f, SkillSlot.Secondary);
            AddNewDriver(master.gameObject, "Strafe", AISkillDriver.AimType.AtCurrentEnemy, AISkillDriver.MovementType.StrafeMovetarget, AISkillDriver.TargetType.CurrentEnemy, 0f, 90f, SkillSlot.None);
            SwapMaterials(prefab, Paths.Material.matVoidBarnacleBullet, true);

            SkillLocator locator = prefab.GetComponent<SkillLocator>();
            ModelLocator modelLocator = prefab.GetComponent<ModelLocator>();

            GameObject hitbox = new GameObject("ChargeHitbox");
            hitbox.AddComponent<HitBox>();
            hitbox.transform.localScale = new(2f, 2f, 2f);

            Transform model = modelLocator.modelTransform;
            hitbox.transform.SetParent(model);
            hitbox.transform.position = Vector3.zero;
            hitbox.transform.localPosition = Vector3.zero;

            HitBoxGroup group = model.AddComponent<HitBoxGroup>();
            group.groupName = "HBCharge";
            group.hitBoxes = new HitBox[] { hitbox.GetComponent<HitBox>() };

            ReplaceSkill(locator.secondary, SkillDefs.JellyDash.instance.skillDef);

            master.GetComponent<BaseAI>().aimVectorMaxSpeed = 40000f;
            master.GetComponent<BaseAI>().aimVectorDampTime = 0.1f;
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
            isc.directorCreditCost = 40;
            isc.forbiddenFlags = NodeFlags.NoCharacterSpawn;
            isc.hullSize = HullClassification.Human;
            isc.nodeGraphType = MapNodeGroup.GraphType.Air;
            isc.sendOverNetwork = true;
            isc.prefab = prefabMaster;
        }
    }
}