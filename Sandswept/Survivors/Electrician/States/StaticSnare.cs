using System;

namespace Sandswept.Survivors.Electrician.States
{
    public class StaticSnare : BaseSkillState
    {
        public float duration = 2f;

        public override void OnEnter()
        {
            base.OnEnter();

            duration /= base.attackSpeedStat;

            if (base.isAuthority)
            {
                FireProjectileInfo info = MiscUtils.GetProjectile(Electrician.StaticSnare, 1f, base.characterBody);
                ProjectileManager.instance.FireProjectile(info);
            }
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