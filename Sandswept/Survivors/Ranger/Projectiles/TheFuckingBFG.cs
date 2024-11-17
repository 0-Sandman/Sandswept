using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.Projectiles
{
    public static class TheFuckingBFG
    {
        public static GameObject prefabDefault;
        public static GameObject SigmaProjectile2;
        public static GameObject SigmaProjectile;
        public static GameObject SigmaSauce;

        public static void Init()
        {
            prefabDefault = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BFG/BeamSphere.prefab").WaitForCompletion(), "Galvanize Projectile", true);

            var projectileSimple = prefabDefault.GetComponent<ProjectileSimple>();
            projectileSimple.lifetime = 3f;
            projectileSimple.desiredForwardSpeed = 30f;
            projectileSimple.enableVelocityOverLifetime = true;
            projectileSimple.velocityOverLifetime = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.5f, 0.5f), new Keyframe(1f, 0f));

            var projectileImpactExplosion = prefabDefault.GetComponent<ProjectileImpactExplosion>();
            projectileImpactExplosion.blastRadius = 0f;
            projectileImpactExplosion.falloffModel = BlastAttack.FalloffModel.None;
            projectileImpactExplosion.blastDamageCoefficient = 0f;
            projectileImpactExplosion.blastProcCoefficient = 0f;
            projectileImpactExplosion.childrenCount = 0;
            projectileImpactExplosion.lifetime = 3f;
            projectileImpactExplosion.enabled = false;

            var projectileProximityBeamController = prefabDefault.GetComponent<ProjectileProximityBeamController>();
            projectileProximityBeamController.attackInterval = 0.5f;
            projectileProximityBeamController.listClearInterval = 0.5f;
            projectileProximityBeamController.procCoefficient = 0.7f;
            projectileProximityBeamController.damageCoefficient = 0.5f;
            projectileProximityBeamController.attackRange = 20f;

            PrefabAPI.RegisterNetworkPrefab(prefabDefault);
            ContentAddition.AddProjectile(prefabDefault);
            
            SigmaProjectile = PrefabAPI.InstantiateClone(Paths.GameObject.ToolbotGrenadeLauncherProjectile, "BIG FUCKING LAVA BALL !!!");
            SigmaProjectile2 = PrefabAPI.InstantiateClone(Paths.GameObject.MolotovSingleProjectile, "joe sigma");
            SigmaProjectile2.GetComponent<ProjectileController>().ghostPrefab = Paths.GameObject.FireballGhost;
            SigmaProjectile.GetComponent<ProjectileImpactExplosion>().lifetime = 10f;
            SigmaProjectile2.GetComponent<ProjectileSimple>().desiredForwardSpeed = 0;
            var explo = SigmaProjectile.GetComponent<ProjectileImpactExplosion>();
            explo.fireChildren = true;
            explo.childrenDamageCoefficient = 1f;
            explo.childrenCount = 1;
            explo.childrenProjectilePrefab = SigmaProjectile2;

            SigmaProjectile.GetComponent<ProjectileImpactExplosion>().blastDamageCoefficient = 1f;
            SigmaProjectile.GetComponent<ProjectileImpactExplosion>().blastRadius = 13f;

            SigmaProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = 90f;

            SigmaProjectile.GetComponent<Rigidbody>().useGravity = true;
            SigmaProjectile.GetComponent<ProjectileController>().ghostPrefab = Paths.GameObject.FireballGhost;

            SigmaProjectile.transform.localScale *= 3f;
            SigmaProjectile.GetComponent<BoxCollider>().size *= 0.3f;

            SigmaSauce = PrefabAPI.InstantiateClone(Paths.GameObject.MolotovProjectileDotZone, "Sigma Sauce");
            SigmaSauce.transform.localScale *= 3f;
            SigmaProjectile2.GetComponent<ProjectileImpactExplosion>().childrenProjectilePrefab = SigmaSauce;

            ContentAddition.AddProjectile(SigmaSauce);
            ContentAddition.AddProjectile(SigmaProjectile);
            ContentAddition.AddProjectile(SigmaProjectile2);
        }
    }
}