using System;

namespace Sandswept.Utils {
    public abstract class CoolerBasicMeleeAttack : BasicMeleeAttack {
        public abstract float BaseDuration { get; }
        public abstract float DamageCoefficient { get; }
        public abstract string HitboxName { get; }
        public abstract GameObject HitEffectPrefab { get; }
        public abstract float ProcCoefficient { get; }
        public abstract float HitPauseDuration { get; }
        public abstract GameObject SwingEffectPrefab { get; }
        public abstract string MuzzleString { get; }
        public virtual string MechanimHitboxParameter { get; }
        public virtual bool ScaleHitPauseDurationWithAttackSpeed { get; } = true;
        public virtual Func<bool> AlternateActiveParameter { get; } = () => { return true; };
        public override void OnEnter()
        {
            base.baseDuration = BaseDuration;
            base.damageCoefficient = DamageCoefficient;
            base.hitBoxGroupName = HitboxName;
            base.hitEffectPrefab = HitEffectPrefab;
            base.procCoefficient = ProcCoefficient;
            base.hitPauseDuration = HitPauseDuration;
            base.swingEffectPrefab = SwingEffectPrefab;
            base.swingEffectMuzzleString = MuzzleString;
            if (MechanimHitboxParameter != null) base.mecanimHitboxActiveParameter = MechanimHitboxParameter;
            base.scaleHitPauseDurationAndVelocityWithAttackSpeed = ScaleHitPauseDurationWithAttackSpeed;
            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.fixedAge += Time.fixedDeltaTime;

            if (string.IsNullOrEmpty(MechanimHitboxParameter) && AlternateActiveParameter()) {
                base.BeginMeleeAttackEffect();
            }
            else if (animator.GetFloat(MechanimHitboxParameter) >= 0.5f) {
                base.BeginMeleeAttackEffect();
            }

            if (base.isAuthority) {
                base.AuthorityFixedUpdate();
            }
        }
    }
}