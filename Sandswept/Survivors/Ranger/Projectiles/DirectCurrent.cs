using Sandswept.Survivors.Ranger.VFX;
using static R2API.DamageAPI;
using static Rewired.UI.ControlMapper.ControlMapper;

namespace Sandswept.Survivors.Ranger.Projectiles
{
    public static class DirectCurrent
    {
        public static GameObject prefabDefault;
        public static GameObject prefabMajor;
        public static GameObject prefabRenegade;
        public static GameObject prefabMileZero;
        public static GameObject prefabRacecar;
        public static GameObject prefabSandswept;
        public static ModdedDamageType chargeOnHit = ReserveDamageType();
        public static ModdedDamageType chargeOnHitDash = ReserveDamageType();

        public static void Init()
        {
            prefabDefault = CreateProjectileRecolor("Default", DirectCurrentVFX.ghostPrefabDefault, DirectCurrentVFX.impactPrefabDefault);
            prefabMajor = CreateProjectileRecolor("Major", DirectCurrentVFX.ghostPrefabMajor, DirectCurrentVFX.impactPrefabMajor);
            prefabRenegade = CreateProjectileRecolor("Renegade", DirectCurrentVFX.ghostPrefabRenegade, DirectCurrentVFX.impactPrefabRenegade);
            prefabMileZero = CreateProjectileRecolor("Mile Zero", DirectCurrentVFX.ghostPrefabMileZero, DirectCurrentVFX.impactPrefabMileZero);
            prefabRacecar = CreateProjectileRecolor("Racecar", DirectCurrentVFX.ghostPrefabRacecar, DirectCurrentVFX.impactPrefabRacecar);
            prefabSandswept = CreateProjectileRecolor("Sandswept", DirectCurrentVFX.ghostPrefabSandswept, DirectCurrentVFX.impactPrefabSandswept);
        }

        public static GameObject CreateProjectileRecolor(string name, GameObject tracerPrefab, GameObject impactPrefab)
        {
            var prefab = Paths.GameObject.MageLightningBombProjectile.InstantiateClone("Direct Current " + name + " Projectile", true);

            var proximityDetonator = prefab.transform.GetChild(0).GetComponent<SphereCollider>();
            proximityDetonator.radius = 0.6f;

            prefab.RemoveComponent<ProjectileProximityBeamController>();

            prefab.RemoveComponent<AkEvent>();
            prefab.RemoveComponent<AkGameObj>();

            var sphereCollider = prefab.GetComponent<SphereCollider>();
            // sphereCollider.material = Paths.PhysicMaterial.physmatEngiGrenade;
            sphereCollider.radius = 0.5f;
            prefab.layer = LayerIndex.projectile.intVal;

            prefab.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var projectileDamage = prefab.GetComponent<ProjectileDamage>();
            projectileDamage.damageType = DamageType.Generic;

            var holder = prefab.AddComponent<ModdedDamageTypeHolderComponent>();
            holder.Add(chargeOnHit);

            var projectileImpactExplosion = prefab.GetComponent<ProjectileImpactExplosion>();
            projectileImpactExplosion.falloffModel = BlastAttack.FalloffModel.None;
            projectileImpactExplosion.blastRadius = 2.5f; // easier to hit
            projectileImpactExplosion.bonusBlastForce = Vector3.zero;
            projectileImpactExplosion.lifetime = 5f;
            projectileImpactExplosion.impactEffect = impactPrefab;

            var projectileSimple = prefab.GetComponent<ProjectileSimple>();
            projectileSimple.lifetime = 5f;
            projectileSimple.desiredForwardSpeed = 170f;

            var antiGravityForce = prefab.GetComponent<AntiGravityForce>();
            antiGravityForce.antiGravityCoefficient = -0.2f;

            var projectileController = prefab.GetComponent<ProjectileController>();

            var newGhost = tracerPrefab;
            // Main.ModLogger.LogError("direct current vfx ghost prefab is " + DirectCurrentVFX.ghostPrefab); exists

            projectileController.ghostPrefab = newGhost;

            prefab.AddComponent<InverseFalloffProjectile>().DollarStoreConstructor(50f, 0.5f, 1.3f, 0.75f, 1.5f);

            prefab.RegisterNetworkPrefab();
            ContentAddition.AddProjectile(prefab);

            return prefab;
        }

        public static int maxCharge = 10;

        public class InverseFalloffProjectile : MonoBehaviour {
            public Vector3 initialPosition;
            public float baseDistance = 60f;
            public float minDamage = 0.4f;
            public float minRadius = 0.75f;
            public float maxDamage = 1.4f;
            public float maxRadius = 1.4f;
            public int stages = 0;
            public ProjectileExplosion explosion;
            public ProjectileDamage damage;
            public float originalDamage;
            public float originalRadius;
            public float[] thresholds;

            public void Start() {
                damage = GetComponent<ProjectileDamage>();
                explosion = GetComponent<ProjectileExplosion>();
                originalDamage = damage.damage;

                if (explosion) {
                    originalRadius = explosion.blastRadius;
                }

                initialPosition = base.transform.position;

                thresholds = new float[] {
                    0f, baseDistance, baseDistance * 1.5f
                };
            }

            public void DollarStoreConstructor(float dist, float minD, float maxD, float minR, float maxR) {
                this.baseDistance = dist;
                this.minDamage = minD;
                this.maxDamage = maxD;
                this.minRadius = minR;
                this.maxRadius = maxR;
            }

            public void FixedUpdate() {
                float distance = Vector3.Distance(base.transform.position, initialPosition);

                if (distance < thresholds[1]) {
                    stages = 1;

                    damage.damage = Util.Remap(distance, 0.5f, thresholds[1], originalDamage * minDamage, originalDamage);
                    if (explosion) explosion.blastRadius = Util.Remap(distance, 0f, thresholds[1], originalRadius * minRadius, originalRadius);
                }

                if (distance > thresholds[1]) {
                    stages = 2;

                    damage.damage = Util.Remap(distance, thresholds[1], thresholds[2], originalDamage, originalDamage * maxDamage);
                    if (explosion) explosion.blastRadius = Util.Remap(distance, thresholds[1], thresholds[2], originalRadius, originalRadius * maxRadius);
                }

                if (distance > thresholds[2]) {
                    stages = 3;
                }
            }
        }
    }
}