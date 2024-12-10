using System;

namespace Sandswept.Enemies.CannonballJellyfish {
    public class JellyDeath : BaseState
    {
        private static readonly float bodyPreservationDuration = 1f;

        private static readonly float hardCutoffDuration = 10f;

        private static readonly float maxFallDuration = 4f;

        private static readonly float minTimeToKeepBodyForNetworkMessages = 0.5f;

        public static GameObject voidDeathEffect;

        private float restStopwatch;

        private float fallingStopwatch;

        private bool bodyMarkedForDestructionServer;

        private CameraTargetParams.AimRequest aimRequest;

        protected Transform cachedModelTransform { get; private set; }

        protected bool isBrittle { get; private set; }

        protected bool isVoidDeath { get; private set; }

        protected bool isPlayerDeath { get; private set; }

        protected virtual bool shouldAutoDestroy => true;

        protected virtual float GetDeathAnimationCrossFadeDuration()
        {
            return 0.1f;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            bodyMarkedForDestructionServer = false;
            cachedModelTransform = (base.modelLocator ? base.modelLocator.modelTransform : null);
            isBrittle = (bool)base.characterBody && base.characterBody.isGlass;
            isVoidDeath = (bool)base.healthComponent && (base.healthComponent.killingDamageType & DamageType.VoidDeath) != 0;
            isPlayerDeath = (bool)base.characterBody.master && base.characterBody.master.GetComponent<PlayerCharacterMasterController>() != null;
            if (isVoidDeath)
            {
                if ((bool)base.characterBody && base.isAuthority)
                {
                    EffectManager.SpawnEffect(voidDeathEffect, new EffectData
                    {
                        origin = base.characterBody.corePosition,
                        scale = base.characterBody.bestFitRadius
                    }, transmit: true);
                }
                if ((bool)cachedModelTransform)
                {
                    EntityState.Destroy(cachedModelTransform.gameObject);
                    cachedModelTransform = null;
                }
            }

            else {
                Destroy(this.GetModelTransform().gameObject);
                EffectManager.SpawnEffect(Paths.GameObject.ExplosivePotExplosion, new EffectData {
                    origin = base.characterBody.corePosition,
                    scale = base.characterBody.bestFitRadius * 2f,
                }, false);

                if (base.isAuthority) {
                    var proj = MiscUtils.GetProjectile(CannonballJellyfish.JellyCoreProjectile, 4f, base.characterBody);
                    ProjectileManager.instance.FireProjectile(proj);
                }
            }
        
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}