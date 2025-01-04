using System;

namespace Sandswept.Survivors.Renegade.States {
    public class Charge : CoolerBasicMeleeAttack {
        public float speed = 50f;
        private Vector3 vel;
        private bool shouldExitImmediately = false;
        public override float BaseDuration => 90000f;

        public override float DamageCoefficient => 7f;

        public override string HitboxName => "Punch";

        public override GameObject HitEffectPrefab => ImpactEffect;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.15f;

        public override GameObject SwingEffectPrefab => null;

        public override string MuzzleString => "SwingRight";
        private static LazyAddressable<GameObject> ImpactEffect = new(() => Paths.GameObject.OmniImpactVFXLoaderLightning);
        private bool triggeredComboAlready = false;
        private RenegadeComboController combo;

        public override void OnEnter()
        {
            pushAwayForce = 150f;
            forceVector = Vector3.up * 40f;
            base.OnEnter();
            vel = base.inputBank.aimDirection * speed;
            vel.y = 0;
            combo = GetComponent<RenegadeComboController>();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if ((((base.fixedAge >= 0.4f) && !base.inputBank.skill3.down)) || shouldExitImmediately) {
                outer.SetNextStateToMain();
            }

            base.characterMotor.rootMotion = vel * Time.fixedDeltaTime;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }

        public override void OnExit()
        {
            base.OnExit();

            string animationStateName = "SwingFistRight";
            float num = 0.8f;
            PlayCrossfade("Gesture, Additive", animationStateName, "SwingFist.playbackRate", num, 0.1f);
            PlayCrossfade("Gesture, Override", animationStateName, "SwingFist.playbackRate", num, 0.1f);
        }

        public override void OnMeleeHitAuthority()
        {
            base.OnMeleeHitAuthority();

            if (hitResults.Count > 0) {
                combo.HitTargets(hitResults.ToArray());

                for (int i = 0; i < hitResults.Count; i++) {
                    if (!triggeredComboAlready) {
                        triggeredComboAlready = true;
                        combo.UpdateCombo(DamageSource.Utility);
                    }

                    HurtBox box = hitResults[i];

                    if (box.healthComponent) {
                        Rigidbody rb = box.healthComponent.body.GetComponent<Rigidbody>();

                        if (rb.mass >= 300) {
                            shouldExitImmediately = true;
                        }
                    }
                }
            }
        }
    }
}