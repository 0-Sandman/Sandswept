using System;

namespace Sandswept.Survivors.Electrician
{
    public class Electrician : SurvivorBase<Electrician>
    {
        public override string Name => "Electrician";

        public override string Description => "TBD";

        public override string Subtitle => "TBD";

        public override string Outro => "TBD";

        public override string Failure => "TBD";
        public static GameObject GalvanicBolt;
        public static GameObject TempestSphere;
        public override void LoadAssets()
        {
            base.LoadAssets();

            Body = Main.Assets.LoadAsset<GameObject>("ElectricianBody.prefab");

            Body.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpStandard;

            var cb = Body.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Paths.GameObject.StandardCrosshair;
            cb.preferredPodPrefab = Paths.GameObject.RoboCratePod;

            SurvivorDef = Main.Assets.LoadAsset<SurvivorDef>("sdElectrician.asset");
            SurvivorDef.cachedName = "Electrician"; // for eclipse fix

            Master = PrefabAPI.InstantiateClone(Paths.GameObject.EngiMonsterMaster, "ElectricianMaster");

            SkillLocator locator = Body.GetComponent<SkillLocator>();
            ReplaceSkills(locator.primary, new SkillDef[] { SkillsDefs.Primary.GalvanicBolt.instance });
            ReplaceSkills(locator.secondary, new SkillDef[] { Skills.TempestSphere.instance });
            ReplaceSkills(locator.utility, new SkillDef[] { Skills.StaticSnare.instance });
            ReplaceSkills(locator.special, new SkillDef[] { Skills.SignalOverload.instance });

            "SANDSWEPT_ELECTR_PASSIVE_NAME".Add("Volatile Shields");
            "SANDSWEPT_ELECTR_PASSIVE_DESC".Add("Discharge yo <style=cIsUtility>balls</style> when struck by enemy <style=cDeath>jaws</style>.");

            GalvanicBolt = Main.Assets.LoadAsset<GameObject>("GalvanicBallProjectile.prefab");
            ContentAddition.AddProjectile(GalvanicBolt);

            TempestSphere = Main.Assets.LoadAsset<GameObject>("TempestSphereProjectile.prefab");
            TempestSphere.transform.Find("AreaIndicator").GetComponent<MeshRenderer>().sharedMaterial = Paths.Material.matGrandparentGravArea;
            ContentAddition.AddProjectile(TempestSphere);
        }
    }

    public class TempestBallController : MonoBehaviour {
        public float ticksPerSecond = 10;
        public SphereCollider sphere;
        private float stopwatch = 0f;
        private float delay;
        private ProjectileController controller;
        private ProjectileDamage damage;
        private BlastAttack attack;

        public void Start() {
            controller = GetComponent<ProjectileController>();
            damage = GetComponent<ProjectileDamage>();

            delay = 1f / ticksPerSecond;

            attack = new();
            attack.radius = sphere.radius;
            attack.attacker = controller.owner;
            attack.baseDamage = damage.damage / ticksPerSecond;
            attack.crit = damage.crit;
            attack.damageType = damage.damageType;
            attack.procCoefficient = 1f;
            attack.teamIndex = controller.teamFilter.teamIndex;
            attack.losType = BlastAttack.LoSType.None;
            attack.falloffModel = BlastAttack.FalloffModel.None;
        }

        public void FixedUpdate() {
            if (NetworkServer.active) {
                stopwatch += Time.fixedDeltaTime;

                if (stopwatch >= delay) {
                    stopwatch = 0f;

                    attack.position = base.transform.position;
                    attack.Fire();
                }
            }
        }
    }
}