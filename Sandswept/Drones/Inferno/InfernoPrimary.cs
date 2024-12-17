using System;

namespace Sandswept.Drones.Inferno
{
    public class InfernoPrimary : BaseSkillState
    {
        public float baseDuration = 0.6f;
        public float duration;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;

            if (isAuthority)
            {
                FireProjectileInfo info = MiscUtils.GetProjectile(InfernoDrone.MortarProjectile, 3f, characterBody);
                ProjectileManager.instance.FireProjectile(info);
            }

            base.StartAimMode(duration);

            PlayAnimation("Gesture, Override", "ShootLeft", "Generic.playbackRate", duration / 3f);

            Util.PlaySound("Play_MULT_m1_grenade_launcher_shoot", base.gameObject);
            Util.PlaySound("Play_MULT_m1_grenade_launcher_explo", base.gameObject);
            Util.PlaySound("Play_MULT_m1_grenade_launcher_beep", base.gameObject);

            EffectManager.SimpleMuzzleFlash(Paths.GameObject.MagmaOrbExplosion, base.gameObject, "Muzzle", false);
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