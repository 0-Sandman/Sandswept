using System;

namespace Sandswept.Survivors.Renegade.States {
    public class CrushCombo : CoolerBasicMeleeAttack, SteppedSkillDef.IStepSetter
    {
        public override float BaseDuration => 0.6f;

        public override float DamageCoefficient => 3.8f;

        public override string HitboxName => "Punch";

        public override GameObject HitEffectPrefab => ImpactEffect;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.1f;

        public override GameObject SwingEffectPrefab => SwingEffect;
        private static LazyAddressable<GameObject> SwingEffect => new(() => Paths.GameObject.LoaderSwingBasic);
        private static LazyAddressable<GameObject> ImpactEffect => new(() => Paths.GameObject.OmniImpactVFXLoader);

        public override string MuzzleString => "SwingRight";
        private int step;
        private RenegadeComboController combo;

        public override void OnEnter()
        {
            base.OnEnter();
            swingEffectMuzzleString = (step == 1) ? "SwingRight" : "SwingLeft";
            combo = GetComponent<RenegadeComboController>();
        }

        public override void PlayAnimation()
        {
            string animationStateName = ((step == 1) ? "SwingFistRight" : "SwingFistLeft");
            float num = Mathf.Max(duration, 0.2f);
            PlayCrossfade("Gesture, Additive", animationStateName, "SwingFist.playbackRate", num, 0.1f);
            PlayCrossfade("Gesture, Override", animationStateName, "SwingFist.playbackRate", num, 0.1f);
        }

        public override void OnMeleeHitAuthority()
        {
            if (hitResults.Count > 0) {
                base.skillLocator.secondary.RunRecharge(base.skillLocator.secondary.baseRechargeInterval * 0.85f);
            }
            
            if (combo.HitTargets(hitResults.ToArray())) {
                combo.UpdateCombo(DamageSource.Secondary);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public void SetStep(int i)
        {
            step = i;
        }
    }
}