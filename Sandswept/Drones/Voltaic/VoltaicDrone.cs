using RoR2.ExpansionManagement;
using System;
using static Rewired.UI.ControlMapper.ControlMapper;

namespace Sandswept.Drones.Voltaic
{
    [ConfigSection("Interactables :: Voltaic Drone")]
    public class VoltaicDrone : DroneBase<VoltaicDrone>
    {
        public override GameObject DroneBody => Main.assets.LoadAsset<GameObject>("VoltaicDroneBody.prefab");

        public override GameObject DroneMaster => Main.assets.LoadAsset<GameObject>("VoltaicDroneMaster.prefab");

        public override Dictionary<string, string> Tokens =>
        new() {
            {"SANDSWEPT_VOLTAIC_DRONE_BODY", "Voltaic Drone"},
            {"SANDSWEPT_VOLTAIC_DRONE_BROKEN_NAME", "Broken Voltaic Drone"},
            {"SANDSWEPT_VOLTAIC_DRONE_CONTEXT", "Repair?"}
        };

        public override GameObject DroneBroken => Main.assets.LoadAsset<GameObject>("VoltaicDroneBroken.prefab");

        [ConfigField("Director Credit Cost", "", 35)]
        public static int directorCreditCost;

        public override int Weight => 7;

        public override int Credits => directorCreditCost;

        public override DirectorAPI.Stage[] Stages => new DirectorAPI.Stage[] {
            DirectorAPI.Stage.SirensCall,
            DirectorAPI.Stage.WetlandAspect,
            DirectorAPI.Stage.DistantRoost,
            DirectorAPI.Stage.SkyMeadow
        };

        public override string iscName => "iscVoltaicDroneBroken";

        public override string inspectInfoDescription => "A companion bought with gold that will follow the survivor at a close distance shooting out a bolt that zaps nearby targets.";

        public static GameObject SpikeProjectile;

        public override void Setup()
        {
            base.Setup();

            SpikeProjectile = Main.assets.LoadAsset<GameObject>("VoltaicSpikeProjectile.prefab");
            ContentAddition.AddProjectile(SpikeProjectile);

            SkillLocator loc = DroneBody.GetComponent<SkillLocator>();
            AssignIfExists(loc.primary, new SkillInfo()
            {
                type = new(typeof(VoltaicPrimary)),
                cooldown = 6f,
                stockToConsume = 1
            });

            ContentAddition.AddEntityState(typeof(VoltaicPrimary), out _);
        }
    }
}