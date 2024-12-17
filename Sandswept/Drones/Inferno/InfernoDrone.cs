using System;

namespace Sandswept.Drones.Inferno
{
    [ConfigSection("Interactables :: Inferno Drone")]
    public class InfernoDrone : DroneBase
    {
        public override GameObject DroneBody => Main.Assets.LoadAsset<GameObject>("InfernoDroneBody.prefab");

        public override GameObject DroneMaster => Main.Assets.LoadAsset<GameObject>("InfernoDroneMaster.prefab");

        public override Dictionary<string, string> Tokens =>
        new() {
            {"SANDSWEPT_INFERNO_DRONE_BODY", "Inferno Drone"},
            {"SANDSWEPT_INFERNO_DRONE_BROKEN_NAME", "Broken Inferno Drone"},
            {"SANDSWEPT_INFERNO_DRONE_CONTEXT", "Repair?"}
        };

        public override string ConfigName => "Inferno Drone";

        public override GameObject DroneBroken => Main.Assets.LoadAsset<GameObject>("InfernoDroneBroken.prefab");

        public override int Weight => 40;

        public override int Credits => 20;

        public override DirectorAPI.Stage[] Stages => new DirectorAPI.Stage[] {
            DirectorAPI.Stage.TitanicPlains,
            DirectorAPI.Stage.AbandonedAqueduct,
            DirectorAPI.Stage.ScorchedAcres,
            DirectorAPI.Stage.AbyssalDepths
        };

        public override string iscName => "iscInfernoDroneBroken";

        public static GameObject MortarProjectile;
        private static GameObject SigmaProjectile;
        private static GameObject SigmaProjectile2;

        public override void Setup()
        {
            base.Setup();

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
                stockToConsume = 1
            });

            ContentAddition.AddProjectile(MortarProjectile);
            ContentAddition.AddProjectile(SigmaProjectile);
            ContentAddition.AddProjectile(SigmaProjectile2);
        }
    }
}