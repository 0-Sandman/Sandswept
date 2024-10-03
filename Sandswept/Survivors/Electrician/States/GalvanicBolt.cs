using System;

namespace Sandswept.Survivors.Electrician.States
{
    public class GalvanicBolt : BaseSkillState
    {
        public float baseDuration = 1.1f;
        public float duration;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;

            if (isAuthority)
            {
                FireProjectileInfo info = MiscUtils.GetProjectile(Electrician.GalvanicBolt, 2f, characterBody);
                ProjectileManager.instance.FireProjectile(info);
            }
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