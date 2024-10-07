using System;

namespace Sandswept.Survivors.Electrician.States
{
    public class GalvanicBolt : BaseSkillState
    {
        public float duration = 1.1f;

        public override void OnEnter()
        {
            base.OnEnter();

            duration /= attackSpeedStat;

            if (isAuthority)
            {
                FireProjectileInfo info = MiscUtils.GetProjectile(Electrician.GalvanicBolt, 2f, characterBody);
                ProjectileManager.instance.FireProjectile(info);
            }

            // base.StartAimMode(duration * 0.5f);

            PlayAnimation("Gesture, Override", "ShootLeft", "Generic.playbackRate", duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}