using System;

namespace Sandswept.Survivors.Electrician.States
{
    public class TempestSphereCharge : BaseSkillState
    {
        public float chargeDuration = 0.05f;

        public override void OnEnter()
        {
            base.OnEnter();

            chargeDuration /= base.attackSpeedStat;

            PlayAnimation("Gesture, Override", "ChargeOrb", "Generic.playbackRate", chargeDuration);
            GetModelAnimator().SetBool("chargingOrb", true);
            Util.PlaySound("Play_vagrant_attack2_charge", gameObject);
            Util.PlaySound("Play_loader_shift_charge_loop", gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= chargeDuration)
            {
                outer.SetNextState(new TempestSphereFire());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }

    public class TempestSphereFire : BaseSkillState
    {
        public float damageCoeff = 5f;
        public float recoilTime = 0.3f;
        public bool locked = false;

        public override void OnEnter()
        {
            base.OnEnter();

            recoilTime /= base.attackSpeedStat;

            if (base.isAuthority)
            {
                FireProjectileInfo info = MiscUtils.GetProjectile(Electrician.TempestSphere, damageCoeff, base.characterBody);
                ProjectileManager.instance.FireProjectile(info);
                Util.PlaySound("Play_vagrant_attack2_charge", gameObject);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= recoilTime && !base.inputBank.skill2.down)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            GetModelAnimator().SetBool("chargingOrb", false);

            if (!locked)
            {
                Util.PlaySound("Stop_loader_shift_charge_loop", gameObject);
                TempestBallController.LockAllOrbs(base.characterBody);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}