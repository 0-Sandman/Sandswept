using System;
using EntityStates.Drone.Command;

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

    public class CommandVoltaicShot : BaseCommandState {
        public float duration = 1.5f;
        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Override", "Blast", "Generic.playbackRate", duration / 3f);

            Util.PlaySound("Play_MULT_m1_grenade_launcher_shoot", base.gameObject);
            Util.PlaySound("Play_MULT_m1_grenade_launcher_explo", base.gameObject);
            Util.PlaySound("Play_MULT_m1_grenade_launcher_beep", base.gameObject);

            EffectManager.SimpleMuzzleFlash(Paths.GameObject.NanoPistolMuzzleFlashVFX, base.gameObject, "MuzzleCannon", false);

            if (isAuthority)
            {
                FireProjectileInfo info = MiscUtils.GetProjectile(VoltaicDrone.SpikeProjectile, 6f, GetAttackerBody());
                info.damageTypeOverride = DamageTypeCombo.GenericSecondary | DamageType.Shock5s;
                info.position = GetAttackerBody().aimOrigin;
                info.rotation = Util.QuaternionSafeLookRotation((GetTargetPosition() - GetAttackerBody().aimOrigin).normalized);
                ProjectileManager.instance.FireProjectile(info);
            }

            outer.SetNextStateToMain();
        }
    } 
}