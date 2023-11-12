using R2API.Networking.Interfaces;
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
            prefab = PrefabAPI.InstantiateClone(Assets.GameObject.MageLightningBombProjectile, "Direct Current Projectile", true);

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

            prefab.RemoveComponent<ProjectileImpactExplosion>();

            var directCurrentExplosion = prefab.AddComponent<DirectCurrentExplosion>();
            directCurrentExplosion.falloffModel = BlastAttack.FalloffModel.None;
            directCurrentExplosion.blastRadius = 2.5f; // easier to hit
            directCurrentExplosion.blastDamageCoefficient = 1f;
            directCurrentExplosion.blastProcCoefficient = 1f;
            directCurrentExplosion.blastAttackerFiltering = AttackerFiltering.Default;
            directCurrentExplosion.bonusBlastForce = Vector3.zero;
            directCurrentExplosion.canRejectForce = true;
            directCurrentExplosion.projectileHealthComponent = null;
            directCurrentExplosion.explosionEffect = null;
            directCurrentExplosion.fireChildren = false;
            directCurrentExplosion.applyDot = false;
            directCurrentExplosion.impactEffect = DirectCurrentVFX.impactPrefab;
            directCurrentExplosion.lifetimeExpiredSound = null;
            directCurrentExplosion.offsetForLifetimeExpiredSound = 0f;
            directCurrentExplosion.destroyOnEnemy = true;
            directCurrentExplosion.destroyOnWorld = true;
            directCurrentExplosion.impactOnWorld = true;
            directCurrentExplosion.timerAfterImpact = false;
            directCurrentExplosion.lifetime = 5f;
            directCurrentExplosion.transformSpace = ProjectileImpactExplosion.TransformSpace.World;

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
        }

        public static int maxCharge = 20;
    }

    public class DirectCurrentExplosion : ProjectileImpactExplosion
    {
        public override void OnBlastAttackResult(BlastAttack blastAttack, BlastAttack.Result result)
        {
            base.OnBlastAttackResult(blastAttack, result);
            // Main.ModLogger.LogError("right after base call");
            // Main.ModLogger.LogError("blast attack is " + blastAttack);
            // Main.ModLogger.LogError("result is " + result);

            var attacker = blastAttack.attacker;
            if (attacker)
            {
                // Main.ModLogger.LogError("attacker exists");
                var attackerBody = attacker.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    // Main.ModLogger.LogError("attacker body exists");
                    var buffCount = attackerBody.GetBuffCount(Buffs.Charge.instance.BuffDef);

                    // new SyncChargeOnHit(attacker.GetComponent<NetworkIdentity>().netId, 2, 1).Send(R2API.Networking.NetworkDestination.Clients);

                    var hitPoints = result.hitPoints;
                    // Main.ModLogger.LogError("hitpoints are " + hitPoints);
                    if (hitPoints.Length > 0)
                    {
                        // Main.ModLogger.LogError("more than zero hitpoints");

                        for (int i = 0; i < hitPoints.Length; i++)
                        {
                            // Main.ModLogger.LogError("looping through all hitpoints");

                            var hitPoint = hitPoints[i];
                            if (hitPoint.hurtBox)
                            {
                                // Main.ModLogger.LogError("hitbox exists, trying to set add 2 charge");
                                attackerBody.SetBuffCount(Buffs.Charge.instance.BuffDef.buffIndex, Math.Min(DirectCurrent.maxCharge, buffCount + 2));
                            }
                        }
                    }
                    else
                    {
                        // Main.ModLogger.LogError("zero hitpoints, trying to remove 1 charge");
                        attackerBody.SetBuffCount(Buffs.Charge.instance.BuffDef.buffIndex, Mathf.Max(0, buffCount - 1));
                    }
                }
            }
        }
    }

    /*
    public class SyncChargeOnHit : INetMessage
    {
        private NetworkInstanceId objID;
        private HurtBoxReference hurtBoxReference;

        public SyncChargeOnHit()
        {
        }

        public SyncChargeOnHit(NetworkInstanceId objID, HurtBoxReference hurtBoxReference)
        {
            this.objID = objID;
            this.hurtBoxReference = hurtBoxReference;
        }

        public void Deserialize(NetworkReader reader)
        {
            objID = reader.ReadNetworkId();
            hurtBoxReference = reader.ReadHurtBoxReference();
        }

        public void OnReceived()
        {
            if (NetworkServer.active) return;
            var obj = Util.FindNetworkObject(objID);
            if (obj)
            {
                var body = obj.GetComponent<CharacterBody>();
                if (body)
                {
                    body.SetBuffCount(Buffs.Charge.instance.BuffDef.buffIndex, Math.Min(DirectCurrent.maxCharge, buffCount + 2));
                }
            }
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(objID);
            writer.Write(chargeGain);
            writer.Write(chargeLoss);
        }
    }
    */
}