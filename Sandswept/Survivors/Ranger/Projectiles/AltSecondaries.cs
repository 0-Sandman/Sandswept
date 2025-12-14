using System;
using System.Collections.Generic;
using System.Text;
using R2API.Utils;

namespace Sandswept.Survivors.Ranger.Projectiles
{
    public static class AltSecondaries
    {
        public static GameObject galvanizeProjectile;
        public static GameObject charBallProjectile;
        public static GameObject charPoolSpawnerProjectile;
        public static GameObject charPoolProjectile;

        public static void Init()
        {
            galvanizeProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BFG/BeamSphere.prefab").WaitForCompletion(), "Galvanize Projectile", true);

            var projectileSimple = galvanizeProjectile.GetComponent<ProjectileSimple>();
            projectileSimple.lifetime = 3f;
            projectileSimple.desiredForwardSpeed = 30f;
            projectileSimple.enableVelocityOverLifetime = true;
            projectileSimple.velocityOverLifetime = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.5f, 0.5f), new Keyframe(1f, 0f));

            var projectileImpactExplosion = galvanizeProjectile.GetComponent<ProjectileImpactExplosion>();
            projectileImpactExplosion.blastRadius = 0f;
            projectileImpactExplosion.falloffModel = BlastAttack.FalloffModel.None;
            projectileImpactExplosion.blastDamageCoefficient = 0f;
            projectileImpactExplosion.blastProcCoefficient = 0f;
            projectileImpactExplosion.childrenCount = 0;
            projectileImpactExplosion.lifetime = 3f;
            projectileImpactExplosion.enabled = false;

            var projectileProximityBeamController = galvanizeProjectile.GetComponent<ProjectileProximityBeamController>();
            projectileProximityBeamController.attackInterval = 0.5f;
            projectileProximityBeamController.listClearInterval = 0.5f;
            projectileProximityBeamController.procCoefficient = 0.7f;
            projectileProximityBeamController.damageCoefficient = 0.5f;
            projectileProximityBeamController.attackRange = 20f;

            var holder = galvanizeProjectile.AddComponent<R2API.DamageAPI.ModdedDamageTypeHolderComponent>();
            holder.Add(Electrician.Electrician.LIGHTNING);

            PrefabAPI.RegisterNetworkPrefab(galvanizeProjectile);
            ContentAddition.AddProjectile(galvanizeProjectile);

            charBallProjectile = PrefabAPI.InstantiateClone(Paths.GameObject.ToolbotGrenadeLauncherProjectile, "Char Ball Projectile");
            charBallProjectile.GetComponent<ProjectileImpactExplosion>().lifetime = 10f;

            charBallProjectile.RemoveComponent<AntiGravityForce>();
            var antiGravityForce = charBallProjectile.AddComponent<CoolerAntiGravityForce>((x) =>
            {
                x.antiGravityCoefficient = -3.5f;
                x.rampTime = 0.2f;
            });

            charPoolSpawnerProjectile = PrefabAPI.InstantiateClone(Paths.GameObject.MolotovSingleProjectile, "Char Pool Spawner Projectile");
            charPoolSpawnerProjectile.GetComponent<ProjectileController>().ghostPrefab = Paths.GameObject.FireballGhost;
            charPoolSpawnerProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = 0;

            charBallProjectile.GetComponent<ProjectileImpactExplosion>((x) =>
            {
                x.fireChildren = true;
                x.blastDamageCoefficient = 1f;
                x.childrenDamageCoefficient = 1f;
                x.childrenCount = 1;
                x.childrenProjectilePrefab = charPoolSpawnerProjectile;
                x.blastRadius = 13f;
            });

            charBallProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = 240f;
            charBallProjectile.GetComponent<Rigidbody>().useGravity = false;
            charBallProjectile.GetComponent<ProjectileController>().ghostPrefab = Paths.GameObject.FireballGhost;
            charBallProjectile.transform.localScale *= 3f;
            charBallProjectile.GetComponent<BoxCollider>().size *= 0.3f;

            charPoolProjectile = PrefabAPI.InstantiateClone(Paths.GameObject.MolotovProjectileDotZone, "Char Pool Projectile");
            charPoolProjectile.transform.localScale *= 3f;

            /*
            charPoolProjectile.GetComponent<ProjectileDamage>((x) =>
            {
                x.damage = 0.06667f; // 200% dps at 3 ticks per second
            });
            */
            // nevermind, of course it still does the fucking full% damage per tick

            charPoolProjectile.GetComponent<ProjectileDotZone>((x) =>
            {
                x.fireFrequency = 1f;
                x.resetFrequency = 3f;
            });

            charPoolSpawnerProjectile.GetComponent<ProjectileImpactExplosion>((x) =>
            {
                x.blastDamageCoefficient = 0f;
                x.childrenDamageCoefficient = 1f;
                x.childrenProjectilePrefab = charPoolProjectile;
                x.childrenCount = 1;
            });

            ContentAddition.AddProjectile(charBallProjectile);
            ContentAddition.AddProjectile(charPoolProjectile);
            ContentAddition.AddProjectile(charPoolSpawnerProjectile);
        }
    }
}