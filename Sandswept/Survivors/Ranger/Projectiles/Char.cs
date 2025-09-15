using Sandswept.Survivors.Ranger.VFX;
using static R2API.DamageAPI;
using static Rewired.UI.ControlMapper.ControlMapper;

namespace Sandswept.Survivors.Ranger.Projectiles
{
    public static class Char
    {
        public static GameObject prefabDefault;
        public static GameObject prefabMajor;
        public static GameObject prefabRenegade;
        public static GameObject prefabMileZero;
        public static GameObject prefabRacecar;
        public static GameObject prefabSandswept;

        public static GameObject child;

        public static void Init()
        {
            child = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Molotov/MolotovProjectileDotZone.prefab").WaitForCompletion(), "Char Child Projectile");

            PrefabAPI.RegisterNetworkPrefab(child);
            ContentAddition.AddProjectile(child);

            prefabDefault = CreateProjectileRecolor("Default", DirectCurrentVFX.ghostPrefabDefault, DirectCurrentVFX.impactPrefabDefault);
            /*
            prefabMajor = CreateProjectileRecolor("Major", DirectCurrentVFX.ghostPrefabMajor, DirectCurrentVFX.impactPrefabMajor);
            prefabRenegade = CreateProjectileRecolor("Renegade", DirectCurrentVFX.ghostPrefabRenegade, DirectCurrentVFX.impactPrefabRenegade);
            prefabMileZero = CreateProjectileRecolor("Mile Zero", DirectCurrentVFX.ghostPrefabMileZero, DirectCurrentVFX.impactPrefabMileZero);
            prefabRacecar = CreateProjectileRecolor("Racecar", DirectCurrentVFX.ghostPrefabRacecar, DirectCurrentVFX.impactPrefabRacecar);
            prefabSandswept = CreateProjectileRecolor("Sandswept", DirectCurrentVFX.ghostPrefabSandswept, DirectCurrentVFX.impactPrefabSandswept);
            */
        }

        public static GameObject CreateProjectileRecolor(string name, GameObject tracerPrefab, GameObject impactPrefab)
        {
            var prefab = Paths.GameObject.MageLightningBombProjectile.InstantiateClone("Char " + name + " Projectile", true);

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

            var projectileImpactExplosion = prefab.GetComponent<ProjectileImpactExplosion>();
            projectileImpactExplosion.falloffModel = BlastAttack.FalloffModel.None;
            projectileImpactExplosion.blastRadius = 2.5f; // easier to hit
            projectileImpactExplosion.bonusBlastForce = Vector3.zero;
            projectileImpactExplosion.lifetime = 5f;
            projectileImpactExplosion.impactEffect = impactPrefab;
            projectileImpactExplosion.childrenCount = 1;
            projectileImpactExplosion.childrenProjectilePrefab = child;

            var projectileSimple = prefab.GetComponent<ProjectileSimple>();
            projectileSimple.lifetime = 5f;
            projectileSimple.desiredForwardSpeed = 170f;

            var antiGravityForce = prefab.GetComponent<AntiGravityForce>();
            antiGravityForce.antiGravityCoefficient = -0.2f;

            var projectileController = prefab.GetComponent<ProjectileController>();

            var newGhost = tracerPrefab;
            // Main.ModLogger.LogError("direct current vfx ghost prefab is " + DirectCurrentVFX.ghostPrefab); exists

            projectileController.ghostPrefab = newGhost;

            prefab.RegisterNetworkPrefab();
            ContentAddition.AddProjectile(prefab);

            return prefab;
        }
    }
}