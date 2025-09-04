using RoR2.ExpansionManagement;
using System;
using static Rewired.UI.ControlMapper.ControlMapper;

namespace Sandswept.Drones.Inferno
{
    [ConfigSection("Interactables :: Inferno Drone")]
    public class InfernoDrone : DroneBase<InfernoDrone>
    {
        public override GameObject DroneBody => Main.assets.LoadAsset<GameObject>("InfernoDroneBody.prefab");

        public override GameObject DroneMaster => Main.assets.LoadAsset<GameObject>("InfernoDroneMaster.prefab");

        public override Dictionary<string, string> Tokens =>
        new() {
            {"SANDSWEPT_INFERNO_DRONE_BODY", "Inferno Drone"},
            {"SANDSWEPT_INFERNO_DRONE_BROKEN_NAME", "Broken Inferno Drone"},
            {"SANDSWEPT_INFERNO_DRONE_CONTEXT", "Repair?"}
        };

        public override string ConfigName => "Inferno Drone";

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

            var trans = DroneBody.GetComponent<ModelLocator>()._modelTransform;
            var meshSMR = trans.Find("InfernoDroneMesh").GetComponent<SkinnedMeshRenderer>();
            meshSMR.material.SetColor("_EmColor", new Color32(255, 88, 0, 255));
            meshSMR.material.SetFloat("_EmPower", 5f);

            DroneBody.GetComponent<CharacterBody>().portraitIcon = Main.hifuSandswept.LoadAsset<Texture2D>("texInfernoDrone.png");

            ContentAddition.AddProjectile(MortarProjectile);
            ContentAddition.AddProjectile(SigmaProjectile);
            ContentAddition.AddProjectile(SigmaProjectile2);

            ContentAddition.AddEntityState(typeof(InfernoPrimary), out _);
            ContentAddition.AddEntityState(typeof(DeathState), out _);
        }
    }
}