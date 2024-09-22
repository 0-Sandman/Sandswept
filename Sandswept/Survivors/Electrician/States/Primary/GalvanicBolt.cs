using System;

namespace Sandswept.Survivors.Electrician.States.Primary
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