﻿using Sandswept.Skills.Ranger.VFX;
using System;
using System.Collections.Generic;
using System.Text;
using static R2API.DamageAPI;

namespace Sandswept.Skills.Ranger.Projectiles
{
    public static class DirectCurrent
    {
        public static GameObject prefab;
        public static ModdedDamageType chargeOnHit = ReserveDamageType();

        public static void Init()
        {
            prefab = PrefabAPI.InstantiateClone(Assets.GameObject.MageLightningBombProjectile, "Direct Current Projectile");
            prefab.transform.localScale = Vector3.one * 1.5f; // easier to hit

            prefab.RemoveComponent<ProjectileProximityBeamController>();

            prefab.RemoveComponent<AkEvent>();
            prefab.RemoveComponent<AkGameObj>();

            var projectileDamage = prefab.GetComponent<ProjectileDamage>();
            projectileDamage.damageType = DamageType.Generic;

            var holder = prefab.AddComponent<ModdedDamageTypeHolderComponent>();
            holder.Add(chargeOnHit);

            var projectileImpactExplosion = prefab.GetComponent<ProjectileImpactExplosion>();
            projectileImpactExplosion.falloffModel = BlastAttack.FalloffModel.None;
            projectileImpactExplosion.blastRadius = 2f; // easier to hit
            projectileImpactExplosion.bonusBlastForce = Vector3.zero;
            projectileImpactExplosion.lifetime = 5f;

            var newImpact = PrefabAPI.InstantiateClone(Assets.GameObject.OmniImpactVFXLightningMage, "Direct Current Explosion", false);

            var effectComponent = newImpact.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_engi_M1_explo";

            for (int i = 0; i < newImpact.transform.childCount; i++)
            {
                var trans = newImpact.transform.GetChild(i);
                trans.localScale *= 0.14285714285f;
            }

            ContentAddition.AddEffect(newImpact);

            projectileImpactExplosion.impactEffect = newImpact;

            var projectileSimple = prefab.GetComponent<ProjectileSimple>();
            projectileSimple.lifetime = 5f;
            projectileSimple.desiredForwardSpeed = 140f;

            var antiGravityForce = prefab.GetComponent<AntiGravityForce>();
            antiGravityForce.antiGravityCoefficient = 0.9f;

            var projectileController = prefab.GetComponent<ProjectileController>();

            var newGhost = DirectCurrentVFX.ghostPrefab;
            // Main.ModLogger.LogError("direct current vfx ghost prefab is " + DirectCurrentVFX.ghostPrefab); exists

            projectileController.ghostPrefab = newGhost;

            PrefabAPI.RegisterNetworkPrefab(prefab);

            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private static void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            if (damageInfo.HasModdedDamageType(chargeOnHit) && attackerBody.GetBuffCount(Buffs.Charged.instance.BuffDef) <= 9)
            {
                attackerBody.AddBuff(Buffs.Charged.instance.BuffDef);
            }
        }
    }
}