using System;

namespace Sandswept.Survivors.Electrician.States
{
    public class GalvanicBolt : BaseSkillState
    {
        public float baseDuration = 1.9f;
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

            // base.StartAimMode(duration * 0.5f);

            PlayAnimation("Gesture, Override", "ShootLeft", "Generic.playbackRate", duration / 3f);

            Util.PlaySound("Play_loader_R_shock", base.gameObject);
            Util.PlayAttackSpeedSound("Play_voidman_m1_shoot", base.gameObject, 0.7f);
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