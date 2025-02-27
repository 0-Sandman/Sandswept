using EntityStates;
using EntityStates.Drone;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Sandswept.Drones {
    public class DeathState : GenericCharacterDeath
    {
        public class RigidbodyCollisionListener : MonoBehaviour
        {
            public DeathState deathState;

            private void OnCollisionEnter(Collision collision)
            {
                deathState.OnImpactServer(collision.GetContact(0).point);
                deathState.Explode();
            }
        }

        public static GameObject initialExplosionEffect = new LazyAddressable<GameObject>(() => Paths.GameObject.ExplosionDroneInitial);
        public static GameObject deathExplosionEffect = new LazyAddressable<GameObject>(() => Paths.GameObject.ExplosionDroneDeath);
        public float deathEffectRadius = 5f;
        public float forceAmount = 20f;
        public float deathDuration = 2f;
        public bool destroyOnImpact = true;
        public static Dictionary<string, InteractableSpawnCard> droneCards = new();

        private RigidbodyCollisionListener rigidbodyCollisionListener;

        public override void OnEnter()
        {
            base.OnEnter();
            AkSoundEngine.PostEvent(Events.Play_drone_deathpt1, base.gameObject);

            if (base.rigidbodyMotor)
            {
                base.rigidbodyMotor.forcePID.enabled = false;
                base.rigidbodyMotor.rigid.useGravity = true;
                base.rigidbodyMotor.rigid.AddForce(Vector3.up * forceAmount, ForceMode.Force);
                base.rigidbodyMotor.rigid.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
            if (base.rigidbodyDirection)
            {
                base.rigidbodyDirection.enabled = false;
            }
            if (initialExplosionEffect)
            {
                EffectManager.SpawnEffect(deathExplosionEffect, new EffectData
                {
                    origin = base.characterBody.corePosition,
                    scale = base.characterBody.radius + deathEffectRadius
                }, transmit: false);
            }
            if (base.isAuthority && destroyOnImpact)
            {
                rigidbodyCollisionListener = base.gameObject.AddComponent<RigidbodyCollisionListener>();
                rigidbodyCollisionListener.deathState = this;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && base.fixedAge > deathDuration)
            {
                Explode();
            }
        }

        public void Explode()
        {
            if (base.modelLocator && base.modelLocator.modelTransform) {
                EntityState.Destroy(base.modelLocator.modelTransform.gameObject);
            }
            EntityState.Destroy(base.gameObject);
        }

        public virtual void OnImpactServer(Vector3 contactPoint)
        {
            string bodyName = BodyCatalog.GetBodyName(base.characterBody.bodyIndex);

            SpawnCard spawnCard = droneCards[bodyName];
            
            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                position = contactPoint
            };

            GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, new Xoroshiro128Plus(0uL)));
            
            if (gameObject)
            {
                PurchaseInteraction component = gameObject.GetComponent<PurchaseInteraction>();
                if (component && component.costType == CostTypeIndex.Money)
                {
                    component.Networkcost = Run.instance.GetDifficultyScaledCost(component.cost);
                }
            }
        }

        public override void OnExit()
        {
            if (deathExplosionEffect)
            {
                EffectManager.SpawnEffect(deathExplosionEffect, new EffectData
                {
                    origin = base.characterBody.corePosition,
                    scale = base.characterBody.radius + deathEffectRadius
                }, transmit: false);
            }
            if (rigidbodyCollisionListener)
            {
                EntityState.Destroy(rigidbodyCollisionListener);
            }
            
            AkSoundEngine.PostEvent(Events.Play_drone_deathpt2, base.gameObject);

            base.OnExit();
        }
    }
}