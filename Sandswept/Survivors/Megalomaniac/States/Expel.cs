using System;
namespace Sandswept.Survivors.Megalomaniac.States {
    public class Expel : AimThrowableBase {
        public static LazyAddressable<GameObject> SpawnEffect = new(() => Paths.GameObject.ExplosionLunarSun);
        private bool hasPlayedSpawn = false;
        private Animator anim;
        public override void OnEnter()
        {
            base.maxDistance = 120f;
            base.arcVisualizerPrefab = Paths.GameObject.BasicThrowableVisualizer;
            base.endpointVisualizerPrefab = Paths.GameObject.HuntressArrowRainIndicator;
            base.endpointVisualizerRadiusScale = 6;
            base.baseMinimumDuration = 0.3f;
            base.projectileBaseSpeed = 120f;
            base.useGravity = true;
            base.projectilePrefab = Megalomaniac.MegaloHeadProjectile;
            base.damageCoefficient = 6f;
            base.OnEnter();

            PlayAnimation("Gesture, Override", "GrabHead", "Generic.playbackRate", 2f);

            anim = GetModelAnimator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            StartAimMode(0.2f);

            if (!hasPlayedSpawn && anim.GetFloat("headTaken") > 0.5f) {
                hasPlayedSpawn = true;
                
                EffectManager.SpawnEffect(SpawnEffect, new EffectData {
                    origin = FindModelChild("Head").transform.position,
                    scale = 2f
                }, false);

                AkSoundEngine.PostEvent(Events.Play_lunar_wisp_attack2_explode, base.gameObject);
            }
        }

        public override void FireProjectile()
        {
            base.FireProjectile();

            PlayAnimation("Gesture, Override", "ThrowHead", "Generic.playbackRate", 2f);

            AkSoundEngine.PostEvent(Events.Play_lunar_wisp_attack2_launch, base.gameObject);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }
    }
}