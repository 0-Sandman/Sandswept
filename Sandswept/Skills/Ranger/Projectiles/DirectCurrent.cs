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

            On.RoR2.GlobalEventManager.OnHitAll += GlobalEventManager_OnHitAll;
        }

        private static void GlobalEventManager_OnHitAll(On.RoR2.GlobalEventManager.orig_OnHitAll orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            var attacker = damageInfo.attacker;
            if (attacker)
            {
                var attackerBody = attacker.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    Main.ModLogger.LogError(hitObject);

                    var buffCount = attackerBody.GetBuffCount(Buffs.Charge.instance.BuffDef);

                    var shouldDeductCharge = hitObject.GetComponent<HealthComponent>() == null;

                    if (shouldDeductCharge && attackerBody.baseNameToken == "SS_RANGER_BODY_NAME")
                    {
                        Main.ModLogger.LogError("should deduct charge and is ranger");
                        attackerBody.SetBuffCount(Buffs.Charge.instance.BuffDef.buffIndex, Mathf.Max(0, buffCount - 1));
                    }

                    if (damageInfo.HasModdedDamageType(chargeOnHit))
                    {
                        Main.ModLogger.LogError("has modded damage type and should gain charge");
                        attackerBody.SetBuffCount(Buffs.Charge.instance.BuffDef.buffIndex, Math.Min(maxCharge, buffCount + 2));
                    }

                    // schizo but so is modded damage type for some reason? like if I hit a wall then there's no damage type, otherwise it's chargeonhit lol
                    // also I hate sdifjosdiofjsdimfvoisdjmiojviosfdv
                    // hitting an airborne enemy works just fine, gives two charge
                    // hitting any enemy on the ground will just give 1 net charge unless you hit above the ground which is guh
                }
            }

            orig(self, damageInfo, hitObject);
        }

        public static int maxCharge = 20;
    }
}