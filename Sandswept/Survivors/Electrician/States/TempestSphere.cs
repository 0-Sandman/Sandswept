using System;

namespace Sandswept.Survivors.Electrician.States {
    public class TempestSphereCharge : BaseSkillState {
        public float chargeDuration = 1.4f;

        public override void OnEnter()
        {
            base.OnEnter();

            chargeDuration /= base.attackSpeedStat;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= chargeDuration && !base.inputBank.skill2.down) {
                outer.SetNextState(new TempestSphereFire());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }

    public class TempestSphereFire : BaseSkillState {
        public float damageCoeff = 8f;
        public float recoilTime = 0.8f;
        public override void OnEnter()
        {
            base.OnEnter();

            recoilTime /= base.attackSpeedStat;

            if (base.isAuthority) {
                FireProjectileInfo info = MiscUtils.GetProjectile(Electrician.TempestSphere, damageCoeff, base.characterBody);
                ProjectileManager.instance.FireProjectile(info);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= recoilTime) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}