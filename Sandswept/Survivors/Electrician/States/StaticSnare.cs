using System;

namespace Sandswept.Survivors.Electrician.States
{
    public class StaticSnare : BaseSkillState
    {
        public float duration = 0.5f;

        public override void OnEnter()
        {
            base.OnEnter();

            duration /= base.attackSpeedStat;

            PlayAnimation("Gesture, Override", "Throw", "Generic.playbackRate", duration);

            if (base.isAuthority)
            {
                FireProjectileInfo info = MiscUtils.GetProjectile(Electrician.StaticSnare, 1f, base.characterBody);
                ProjectileManager.instance.FireProjectile(info);
            }

            AkSoundEngine.PostEvent(Events.Play_MULT_m2_throw, base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}