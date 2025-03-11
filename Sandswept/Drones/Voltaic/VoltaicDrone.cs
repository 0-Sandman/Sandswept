using RoR2.ExpansionManagement;
using System;
using static Rewired.UI.ControlMapper.ControlMapper;

namespace Sandswept.Drones.Voltaic
{
    [ConfigSection("Interactables :: Voltaic Drone")]
    public class VoltaicDrone : DroneBase<VoltaicDrone>
    {
        public override GameObject DroneBody => Main.Assets.LoadAsset<GameObject>("VoltaicDroneBody.prefab");

        public override GameObject DroneMaster => Main.Assets.LoadAsset<GameObject>("VoltaicDroneMaster.prefab");

        public override Dictionary<string, string> Tokens =>
        new() {
            {"SANDSWEPT_VOLTAIC_DRONE_BODY", "Voltaic Drone"},
            {"SANDSWEPT_VOLTAIC_DRONE_BROKEN_NAME", "Broken Voltaic Drone"},
            {"SANDSWEPT_VOLTAIC_DRONE_CONTEXT", "Repair?"}
        };

        public override string ConfigName => "Voltaic Drone";

        public override GameObject DroneBroken => Main.Assets.LoadAsset<GameObject>("VoltaicDroneBroken.prefab");

        public override int Weight => 7;

        public override int Credits => 35;

        public override DirectorAPI.Stage[] Stages => new DirectorAPI.Stage[] {
            DirectorAPI.Stage.SirensCall,
            DirectorAPI.Stage.WetlandAspect,
            DirectorAPI.Stage.DistantRoost,
            DirectorAPI.Stage.SkyMeadow
        };

        public override string iscName => "iscVoltaicDroneBroken";

        public static GameObject SpikeProjectile;

        public override void Setup()
        {
            base.Setup();

            SpikeProjectile = Main.Assets.LoadAsset<GameObject>("VoltaicSpikeProjectile.prefab");
            ContentAddition.AddProjectile(SpikeProjectile);

            SkillLocator loc = DroneBody.GetComponent<SkillLocator>();
            AssignIfExists(loc.primary, new SkillInfo()
            {
                type = new(typeof(VoltaicPrimary)),
                cooldown = 6f,
                stockToConsume = 1
            });
        }
    }
}