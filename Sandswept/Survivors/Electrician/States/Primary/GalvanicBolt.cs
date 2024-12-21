using System;

namespace Sandswept.Survivors.Electrician.States
{
    public class GalvanicBolt : BaseSkillState
    {
        public float baseDuration = 0.6f;
        public float duration;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;

            if (isAuthority)
            {
                FireProjectileInfo info = MiscUtils.GetProjectile(Electrician.GalvanicBolt, 1.5f, characterBody, DamageTypeCombo.GenericPrimary);
                ProjectileManager.instance.FireProjectile(info);
            }

            characterBody.SetSpreadBloom(12f, true);

            // base.StartAimMode(duration * 0.5f);

            PlayAnimation("Gesture, Override", "ShootLeft", "Generic.playbackRate", duration / 3f);

            // Util.PlaySound("Play_loader_R_shock", base.gameObject);
            // Util.PlayAttackSpeedSound("Play_voidman_m1_shoot", base.gameObject, 0.7f);

            Util.PlaySound("Play_elec_m1_shoot", gameObject);

            EffectManager.SimpleMuzzleFlash(Electrician.ElecMuzzleFlash, gameObject, "MuzzleCannon", false);
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