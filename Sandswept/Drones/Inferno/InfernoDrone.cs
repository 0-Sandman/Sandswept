using RoR2.ExpansionManagement;
using System;
using static Rewired.UI.ControlMapper.ControlMapper;

namespace Sandswept.Drones.Inferno
{
    [ConfigSection("Interactables :: Inferno Drone")]
    public class InfernoDrone : DroneBase<InfernoDrone>
    {
        public override GameObject DroneBody => Main.assets.LoadAsset<GameObject>("InfernoDroneBody.prefab");

        public override GameObject DroneMaster => CopyDrone1MasterIfDoesntExist();

        public override Dictionary<string, string> Tokens =>
        new() {
            {"SANDSWEPT_INFERNO_DRONE_BODY", "Inferno Drone"},
            {"SANDSWEPT_INFERNO_DRONE_BROKEN_NAME", "Broken Inferno Drone"},
            {"SANDSWEPT_INFERNO_DRONE_CONTEXT", "Repair?"}
        };

        public override GameObject DroneBroken => Main.assets.LoadAsset<GameObject>("InfernoDroneBroken.prefab");

        [ConfigField("Director Credit Cost", "", 35)]
        public static int directorCreditCost;

        public override int Weight => 7;

        public override int Credits => directorCreditCost;

        public override DirectorAPI.Stage[] Stages => new DirectorAPI.Stage[] {
            DirectorAPI.Stage.TitanicPlains,
            DirectorAPI.Stage.AbandonedAqueduct,
            DirectorAPI.Stage.ScorchedAcres,
            DirectorAPI.Stage.AbyssalDepths
        };

        public override string iscName => "iscInfernoDroneBroken";

        public override string inspectInfoDescription => "A companion bought with gold that will follow the survivor at a close distance shooting out molotov cocktails.";

        public override Texture2D icon => Main.assets.LoadAsset<Texture2D>("texInfernoDroneBody.png");

        public static GameObject MortarProjectile;
        private static GameObject SigmaProjectile;
        public static GameObject SigmaProjectile2;

        public override void Setup()
        {
            base.Setup();

            DroneBody.GetComponent<CharacterBody>().portraitIcon = icon;

            MortarProjectile = PrefabAPI.InstantiateClone(Paths.GameObject.ToolbotGrenadeLauncherProjectile, "InfernoMortar");
            SigmaProjectile = PrefabAPI.InstantiateClone(Paths.GameObject.MolotovClusterProjectile, "InfernoShell");
            SigmaProjectile2 = PrefabAPI.InstantiateClone(Paths.GameObject.MolotovSingleProjectile, "InfernoShellSingle");
            SigmaProjectile2.GetComponent<ProjectileController>().ghostPrefab = Paths.GameObject.FireballGhost;
            SigmaProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = 0f;
            SigmaProjectile.GetComponent<ProjectileImpactExplosion>().explodeOnLifeTimeExpiration = true;
            SigmaProjectile.GetComponent<ProjectileImpactExplosion>().lifetime = 0.5f;
            SigmaProjectile.GetComponent<ProjectileImpactExplosion>().childrenProjectilePrefab = SigmaProjectile2;
            SigmaProjectile.GetComponent<ProjectileImpactExplosion>().childrenCount = 3;
            var explo = MortarProjectile.GetComponent<ProjectileImpactExplosion>();
            explo.fireChildren = true;
            explo.childrenDamageCoefficient = 1f;
            explo.childrenCount = 1;
            explo.childrenProjectilePrefab = SigmaProjectile;

            MortarProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = 200f;

            MortarProjectile.GetComponent<Rigidbody>().useGravity = false;

            MortarProjectile.transform.localScale *= 1.5f;

            SkillLocator loc = DroneBody.GetComponent<SkillLocator>();
            AssignIfExists(loc.primary, new SkillInfo()
            {
                type = new(typeof(InfernoPrimary)),
                cooldown = 6f,
                stockToConsume = 1,
                nameToken = "SANDSWEPT_INFERNO_PRIM_NAME",
                descToken = "SANDSWEPT_INFERNO_PRIM_DESC",
                name = "Scorch",
                desc = "Shoot a rocket that leaves <style=cIsDamage>napalm pools</style> for <style=cIsDamage>3x300% damage per second</style>.",
                icon = Main.assets.LoadAsset<Sprite>("Scorch.png")
            });

            var trans = DroneBody.GetComponent<ModelLocator>()._modelTransform;
            var meshSMR = trans.Find("InfernoDroneMesh").GetComponent<SkinnedMeshRenderer>();
            meshSMR.material.SetColor("_EmColor", new Color32(255, 88, 0, 255));
            meshSMR.material.SetFloat("_EmPower", 5f);

            ContentAddition.AddProjectile(MortarProjectile);
            ContentAddition.AddProjectile(SigmaProjectile);
            ContentAddition.AddProjectile(SigmaProjectile2);

            ContentAddition.AddEntityState(typeof(InfernoPrimary), out _);
            ContentAddition.AddEntityState(typeof(DeathState), out _);
        }

        public override void PostCreation()
        {
            base.PostCreation();

            AddOperatorSkill(new SkillInfo()
            {
                type = new(typeof(CommandInfernoTrail)),
                descToken = "SANDSWEPT_INFERNO_OPERATOR_DESC",
                desc = "Charge forward and carpet the ground, leaving behind napalm pools for <style=cIsUtility>7x300% damage</style>. Scales with attack speed."
            }, 50f, DroneCommandReceiver.TargetType.Enemy | DroneCommandReceiver.TargetType.Ground);
        }

        public override DroneDef GetDroneDef()
        {
            DroneDef def = ScriptableObject.CreateInstance<DroneDef>();
            def._masterPrefab = DroneMaster;
            def.bodyPrefab = DroneBody;
            def.canCombine = true;
            def.canDrop = true;
            def.canScrap = true;
            def.tier = ItemTier.Tier2;
            def.remoteOpBody = DroneBody;
            def.droneBrokenSpawnCard = iscBroken;
            def.nameToken = def.bodyPrefab.GetComponent<CharacterBody>().baseNameToken;
            def.descriptionToken = "SANDSWEPT_INFERNO_DESC".Add("Fires rockets at targets that create <style=cIsDamage>napalm pools<lstyle> for <style=cIsDamage>300% damage per second</style>.");
            def.skillDescriptionToken = "SANDSWEPT_INFERNO_DESC";
            def.pickupToken = "SANDSWEPT_INFERNO_PICKUP".Add("Fires rockets at targets that leave behind napalm pools.");
            def.remoteOpCost = 40;
            def.droneType = DroneType.Combat;
            def.iconSprite = Main.assets.LoadAsset<Sprite>("texInfernoDroneBody.png");
            return def;
        }
    }
}