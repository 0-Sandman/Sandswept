﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.Projectiles
{
    public static class TheFuckingBFG
    {
        public static GameObject prefabDefault;

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
            projectileProximityBeamController.procCoefficient = 0.5f;
            projectileProximityBeamController.damageCoefficient = 0.5f;
            projectileProximityBeamController.attackRange = 35f;

            PrefabAPI.RegisterNetworkPrefab(prefabDefault);
            ContentAddition.AddProjectile(prefabDefault);
        }
    }
}