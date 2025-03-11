using System;

namespace Sandswept.Drones.Voltaic
{
    public class VoltaicPrimary : BaseSkillState
    {
        public float baseDuration = 0.6f;
        public float duration;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;

            base.StartAimMode(0.2f);

            if (isAuthority)
            {
                FireProjectileInfo info = MiscUtils.GetProjectile(VoltaicDrone.SpikeProjectile, 2f, characterBody);
                ProjectileManager.instance.FireProjectile(info);
            }

            base.StartAimMode(duration);

            PlayAnimation("Gesture, Override", "Blast", "Generic.playbackRate", duration / 3f);

            Util.PlaySound("Play_MULT_m1_grenade_launcher_shoot", base.gameObject);
            Util.PlaySound("Play_MULT_m1_grenade_launcher_explo", base.gameObject);
            Util.PlaySound("Play_MULT_m1_grenade_launcher_beep", base.gameObject);

            EffectManager.SimpleMuzzleFlash(Paths.GameObject.MuzzleflashRoboBall, base.gameObject, "MuzzleCannon", false);
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