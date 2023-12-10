using HG;
using System.Linq;

namespace Sandswept.Elites
{
    internal class Phasing : EliteEquipmentBase<Phasing>
    {
        public override string EliteEquipmentName => "John Hopoo";

        public override string EliteAffixToken => "PHASING";

        public override string EliteEquipmentPickupDesc => "Become an aspect of singularity.";

        public override string EliteEquipmentFullDescription => "Become an aspect of singularity.";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Phasing";

        public override GameObject EliteEquipmentModel => CreateAffixModel(new Color32(104, 25, 200, 255));

        public override Sprite EliteEquipmentIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texPhasingAffix.png");

        public override Sprite EliteBuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texPhasingBuff.png");

        public override Texture2D EliteRampTexture => Main.hifuSandswept.LoadAsset<Texture2D>("Assets/Sandswept/texRampPhasing.png");

        public override float DamageMultiplier => 2f;
        public override float HealthMultiplier => 4f;

        public static GameObject warbanner;

        public override CombatDirector.EliteTierDef[] CanAppearInEliteTiers => EliteAPI.GetCombatDirectorEliteTiers().Where(x => x.eliteTypes.Contains(Addressables.LoadAssetAsync<EliteDef>("RoR2/Base/EliteFire/edFire.asset").WaitForCompletion())).ToArray();

        public override Color EliteBuffColor => new Color32(104, 25, 200, 255);

        public static float cooldown = 7f;
        public static BuffDef blackHoleCooldown;
        public static GameObject blackHole;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            CreateEliteTiers();
            CreateElite();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
        }

        private void CreateEliteTiers()
        {
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            blackHoleCooldown = ScriptableObject.CreateInstance<BuffDef>();
            blackHoleCooldown.isHidden = true;
            blackHoleCooldown.isDebuff = false;
            blackHoleCooldown.isCooldown = false;
            blackHoleCooldown.canStack = false;

            ContentAddition.AddBuffDef(blackHoleCooldown);

            blackHole = PrefabAPI.InstantiateClone(Assets.GameObject.GravSphere, "Phasing Black Hole");

            var projectileSimple = blackHole.GetComponent<ProjectileSimple>();
            projectileSimple.lifetime = 5f;
            projectileSimple.desiredForwardSpeed = 10f;

            var radialForce = blackHole.GetComponent<RadialForce>();
            radialForce.radius = 13f;
            radialForce.forceMagnitude = -1000f;

            blackHole.AddComponent<PullStrengthController>();

            blackHole.AddComponent<ProjectileTargetComponent>();

            var projectileSteerTowardTarget = blackHole.AddComponent<ProjectileSteerTowardTarget>();
            projectileSteerTowardTarget.rotationSpeed = 10f;

            var projectileDirectionalTargetFinder = blackHole.AddComponent<ProjectileDirectionalTargetFinder>();
            projectileDirectionalTargetFinder.lookRange = 39f;
            projectileDirectionalTargetFinder.lookCone = 180f;
            projectileDirectionalTargetFinder.targetSearchInterval = 0.1f;
            projectileDirectionalTargetFinder.onlySearchIfNoTarget = true;
            projectileDirectionalTargetFinder.allowTargetLoss = false;
            projectileDirectionalTargetFinder.testLoS = false;
            projectileDirectionalTargetFinder.ignoreAir = false;

            PrefabAPI.RegisterNetworkPrefab(blackHole);

            // On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.HealthComponent.TakeDamageForce_Vector3_bool_bool += HealthComponent_TakeDamageForce_Vector3_bool_bool;
            On.RoR2.HealthComponent.TakeDamageForce_DamageInfo_bool_bool += HealthComponent_TakeDamageForce_DamageInfo_bool_bool;
            On.RoR2.GlobalEventManager.OnHitAll += GlobalEventManager_OnHitAll;
        }

        private void GlobalEventManager_OnHitAll(On.RoR2.GlobalEventManager.orig_OnHitAll orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            var attacker = damageInfo.attacker;
            if (attacker && hitObject)
            {
                var attackerBody = attacker.GetComponent<CharacterBody>();
                if (attackerBody && attackerBody.HasBuff(EliteBuffDef) && !attackerBody.HasBuff(blackHoleCooldown))
                {
                    var aimDirection = attackerBody.equipmentSlot.GetAimRay().direction;
                    var fpi = new FireProjectileInfo()
                    {
                        crit = attackerBody.RollCrit(),
                        damage = 0,
                        damageColorIndex = DamageColorIndex.Void,
                        owner = attacker,
                        projectilePrefab = blackHole,
                        force = 0,
                        position = damageInfo.position,
                        rotation = Util.QuaternionSafeLookRotation(aimDirection),
                        procChainMask = default
                    };

                    ProjectileManager.instance.FireProjectile(fpi);

                    attackerBody.AddTimedBuffAuthority(blackHoleCooldown.buffIndex, cooldown);
                }
            }
            orig(self, damageInfo, hitObject);
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.attacker)
            {
                var body = damageInfo.attacker.GetComponent<CharacterBody>();
                if (body && body.equipmentSlot && body.HasBuff(EliteBuffDef))
                {
                    var aimDirection = body.equipmentSlot.GetAimRay().direction;
                    float force = -4000f * damageInfo.procCoefficient;
                    damageInfo.force = Vector3.Scale(damageInfo.force, -Vector3.one);
                    damageInfo.force += aimDirection * force;
                    damageInfo.canRejectForce = false;
                    damageInfo.damageType |= DamageType.BypassBlock;
                    EffectManager.SpawnEffect(VFX.Phasing.prefab, new EffectData { origin = damageInfo.position, rotation = Util.QuaternionSafeLookRotation(aimDirection) }, true);
                }
            }
            orig(self, damageInfo);
        }

        private void HealthComponent_TakeDamageForce_DamageInfo_bool_bool(On.RoR2.HealthComponent.orig_TakeDamageForce_DamageInfo_bool_bool orig, HealthComponent self, DamageInfo damageInfo, bool alwaysApply, bool disableAirControlUntilCollision)
        {
            if (self.body && self.body.HasBuff(EliteBuffDef))
            {
                damageInfo.force = Vector3.zero;
            }
            orig(self, damageInfo, alwaysApply, disableAirControlUntilCollision);
        }

        private void HealthComponent_TakeDamageForce_Vector3_bool_bool(On.RoR2.HealthComponent.orig_TakeDamageForce_Vector3_bool_bool orig, HealthComponent self, Vector3 force, bool alwaysApply, bool disableAirControlUntilCollision)
        {
            if (self.body && self.body.HasBuff(EliteBuffDef))
            {
                force = Vector3.zero;
            }
            orig(self, force, alwaysApply, disableAirControlUntilCollision);
        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }
    }

    public class PullStrengthController : MonoBehaviour
    {
        public ProjectileSimple projectileSimple;
        public RadialForce radialForce;
        public float dampingMultiplier = 0.5f;
        // results in -500 pull strength at 5s of lifetime

        public void Start()
        {
            projectileSimple = GetComponent<ProjectileSimple>();
            radialForce = GetComponent<RadialForce>();
        }

        public void FixedUpdate()
        {
            radialForce.damping = Util.Remap(projectileSimple.stopwatch, 0f, 1f, 0f * (1f - dampingMultiplier), 1f * (1f - dampingMultiplier)) / 2.5f; // results in 1 damping at max projectile lifetime and 0 at min
        }
    }
}