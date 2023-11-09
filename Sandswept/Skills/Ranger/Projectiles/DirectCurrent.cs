using Sandswept.Skills.Ranger.VFX;
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

            var newImpact = PrefabAPI.InstantiateClone(Assets.GameObject.OmniImpactVFXLightningMage, "Direct Current Explosion", false);

            var effectComponent = newImpact.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_engi_M1_explo";

            var sphereExpanding = newImpact.transform.Find("Sphere, Expanding").GetComponent<ParticleSystemRenderer>();

            var newMat = Object.Instantiate(Assets.Material.matLightningSphere);

            newMat.SetColor("_TintColor", new Color32(17, 17, 17, 255));
            newMat.SetTexture("_RemapTex", Main.hifuSandswept.LoadAsset<Texture2D>("Assets/Sandswept/texRampDirectCurrentImpact.png"));

            sphereExpanding.material = newMat;

            for (int i = 0; i < newImpact.transform.childCount; i++)
            {
                var trans = newImpact.transform.GetChild(i);
                trans.localScale *= 0.1785714285f; // 1/14 * 2.5m radius
            }

            ContentAddition.AddEffect(newImpact);

            projectileImpactExplosion.impactEffect = newImpact;

            var projectileSimple = prefab.GetComponent<ProjectileSimple>();
            projectileSimple.lifetime = 5f;
            projectileSimple.desiredForwardSpeed = 170f;

            var antiGravityForce = prefab.GetComponent<AntiGravityForce>();
            antiGravityForce.antiGravityCoefficient = -0.2f;

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

            if (damageInfo.HasModdedDamageType(chargeOnHit) && attackerBody.GetBuffCount(Buffs.Charge.instance.BuffDef) <= 9)
            {
                attackerBody.AddBuff(Buffs.Charge.instance.BuffDef);
            }
        }
    }
}