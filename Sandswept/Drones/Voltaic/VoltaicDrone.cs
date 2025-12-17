using RoR2.ExpansionManagement;
using System;
using System.Collections;
using static Rewired.UI.ControlMapper.ControlMapper;

namespace Sandswept.Drones.Voltaic
{
    [ConfigSection("Interactables :: Voltaic Drone")]
    public class VoltaicDrone : DroneBase<VoltaicDrone>
    {
        public override GameObject DroneBody => Main.assets.LoadAsset<GameObject>("VoltaicDroneBody.prefab");

        public override GameObject DroneMaster => CopyDrone1MasterIfDoesntExist();

        public override Dictionary<string, string> Tokens =>
        new() {
            {"SANDSWEPT_VOLTAIC_DRONE_BODY", "Voltaic Drone"},
            {"SANDSWEPT_VOLTAIC_DRONE_BROKEN_NAME", "Broken Voltaic Drone"},
            {"SANDSWEPT_VOLTAIC_DRONE_CONTEXT", "Repair?"},
            {"SANDSWEPT_VOLTAIC_DRONE_DESC", "sampel description Expansión"},
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

        public override Texture2D icon => Main.assets.LoadAsset<Texture2D>("texVoltaicDroneBody.png");

        public static GameObject SpikeProjectile;

        public override void Setup()
        {
            base.Setup();

            SpikeProjectile = Main.assets.LoadAsset<GameObject>("VoltaicSpikeProjectile.prefab");
            SpikeProjectile.GetComponent<ProjectileImpactExplosion>().impactEffect = Paths.GameObject.LoaderGroundSlam;
            SpikeProjectile.GetComponent<ProjectileSimple>().lifetimeExpiredEffect = Paths.GameObject.RoboCratePodGroundImpact;
            SpikeProjectile.AddComponent<VoltaicRodController>();
            // idfk im disabling this for now
            SpikeProjectile.layer = LayerIndex.debris.intVal;
            SpikeProjectile.GetComponent<ProjectileStickOnImpact>().ignoreCharacters = true;
            ContentAddition.AddProjectile(SpikeProjectile);

            SkillLocator loc = DroneBody.GetComponent<SkillLocator>();
            AssignIfExists(loc.primary, new SkillInfo()
            {
                type = new(typeof(VoltaicPrimary)),
                cooldown = 6f,
                stockToConsume = 1,
                icon = Main.assets.LoadAsset<Sprite>("Jumpstart.png"),
                nameToken = "SANDSWEPT_VOLTAIC_PRIM_NAME",
                descToken = "SANDSWEPT_VOLTAIC_PRIME_DESC",
                name = "Jumpstart",
                desc = "Fire an embedding rod that <style=cIsUtility>zaps</style> targets for <style=cIsDamage>200% damage</style> repeatedly."
            });

            ContentAddition.AddEntityState(typeof(VoltaicPrimary), out _);
        }

        public override void PostCreation()
        {
            base.PostCreation();

            AddOperatorSkill(new SkillInfo()
            {
                type = new(typeof(CommandVoltaicShot)),
                descToken = "SANDSWEPT_VOLTAIC_OPERATOR_DESC",
                desc = "Fire an embedding rod that <style=cIsUtility>shocks</style> targets for <style=cIsDamage>600% damage</style> repeatedly, locking them in place."
            }, 250f, DroneCommandReceiver.TargetType.Ground);
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
            def.droneBrokenSpawnCard = iscBroken;
            def.nameToken = def.bodyPrefab.GetComponent<CharacterBody>().baseNameToken;
            def.descriptionToken = "SANDSWEPT_VOLTAIC_DESC".Add("Blasts embedding rods that <style=cIsUtility>arc</style> an electric current to nearby targets for <style=cIsDamage>200% damage</style>.");
            def.pickupToken = "SANDSWEPT_VOLTAIC_PICKUP".Add("Blasts embedding rods that zap targets.");
            def.skillDescriptionToken = "SANDSWEPT_VOLTAIC_DESC";
            def.remoteOpCost = 40;
            def.remoteOpBody = DroneBody;
            def.droneType = DroneType.Combat;
            return def;
        }

        public class VoltaicRodController : MonoBehaviour {
            public ProjectileStickOnImpact stick;
            public Rigidbody rb;
            public bool hasStuck = false;
            public Vector3 forward;

            public void Start() {
                stick = GetComponent<ProjectileStickOnImpact>();
                rb = GetComponent<Rigidbody>();
                stick.stickEvent.AddListener(OnStick);
            }

            public void OnStick() {
                StartCoroutine(OnStickHandler());
            }

            public IEnumerator OnStickHandler() {
                base.gameObject.layer = LayerIndex.noCollision.intVal;
                yield return new WaitForEndOfFrame();
                if (stick.stuckBody && stick.stuckBody.mainHurtBox) {
                    stick.stuckTransform = stick.stuckBody.mainHurtBox.transform;
                    stick.NetworklocalPosition = Vector3.zero;
                }
                hasStuck = true;
            }

            public void FixedUpdate() {
                if (hasStuck && !stick.stuckTransform) {
                    GameObject.Destroy(base.gameObject);
                }

                if (rb.velocity != Vector3.zero && !hasStuck) {
                    base.transform.forward = rb.velocity.normalized;
                    forward = base.transform.forward;
                }
                else if (hasStuck) {
                    base.transform.forward = forward;
                }
            }
        }
    }
}