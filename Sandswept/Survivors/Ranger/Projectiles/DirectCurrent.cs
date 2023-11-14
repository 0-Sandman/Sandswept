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
        public static ModdedDamageType chargeOnHit = ReserveDamageType();
        public static ModdedDamageType chargeOnHitDash = ReserveDamageType();

        public static void Init()
        {
            prefabDefault = CreateProjectileRecolor("Default", DirectCurrentVFX.ghostPrefabDefault, DirectCurrentVFX.impactPrefabDefault);
            prefabMajor = CreateProjectileRecolor("Major", DirectCurrentVFX.ghostPrefabMajor, DirectCurrentVFX.impactPrefabMajor);
            prefabRenegade = CreateProjectileRecolor("Renegade", DirectCurrentVFX.ghostPrefabRenegade, DirectCurrentVFX.impactPrefabRenegade);
            prefabMileZero = CreateProjectileRecolor("Mile Zero", DirectCurrentVFX.ghostPrefabMileZero, DirectCurrentVFX.impactPrefabMileZero);

            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        public static GameObject CreateProjectileRecolor(string name, GameObject tracerPrefab, GameObject impactPrefab)
        {
            var prefab = Assets.GameObject.MageLightningBombProjectile.InstantiateClone("Direct Current " + name + " Projectile", true);

            var proximityDetonator = prefab.transform.GetChild(0).GetComponent<SphereCollider>();
            proximityDetonator.radius = 0.6f;

            prefab.RemoveComponent<ProjectileProximityBeamController>();

            prefab.RemoveComponent<AkEvent>();
            prefab.RemoveComponent<AkGameObj>();

            var sphereCollider = prefab.GetComponent<SphereCollider>();
            // sphereCollider.material = Assets.PhysicMaterial.physmatEngiGrenade;
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

            prefab.RegisterNetworkPrefab();

            return prefab;
        }

        public static int maxCharge = 10;

        private static void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var buffCount = attackerBody.GetBuffCount(Buffs.Charge.instance.BuffDef);

            if (damageInfo.HasModdedDamageType(chargeOnHit) && buffCount <= 9)
            {
                attackerBody.AddBuff(Buffs.Charge.instance.BuffDef);
            }

            if (damageInfo.HasModdedDamageType(chargeOnHitDash))
            {
                attackerBody.SetBuffCount(Buffs.Charge.instance.BuffDef.buffIndex, Mathf.Min(10, buffCount + 3));
            }
        }
    }
}