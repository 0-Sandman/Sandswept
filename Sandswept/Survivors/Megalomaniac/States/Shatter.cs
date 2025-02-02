using System;

namespace Sandswept.Survivors.Megalomaniac.States {
    public class ShatterWind : BaseSkillState {
        public float duration = 0.35f;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Override", "PrepGatling", "PrepGatling.playbackRate", duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration) {
                outer.SetNextState(new ShatterFiring());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
    public class ShatterFiring : BaseSkillState {
        public static float damageCoeff = 2f;
        public float fireRate = 1f / 4f;
        public int currentMuzzle = 1;
        public float stopwatch = 0f;
        public static LazyAddressable<GameObject> MuzzleFlash = new(() => Paths.GameObject.MuzzleflashLunarNeedle);

        public override void OnEnter()
        {
            base.OnEnter();

            fireRate /= base.attackSpeedStat;

            PlayAnimation("Gesture, Override", "FireGatling", "Generic.playbackRate", 2f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;

            if (stopwatch >= fireRate) {
                stopwatch = 0f;

                if (base.isAuthority) {
                    FireProjectileInfo info = MiscUtils.GetProjectile(Megalomaniac.MegaloStickyOrb, damageCoeff, base.characterBody, DamageTypeCombo.GenericPrimary);
                    info.position = base.inputBank.aimOrigin;
                    ProjectileManager.instance.FireProjectile(info);
                }

                string muzzle = currentMuzzle == 1 ? "MuzzleB" : "MuzzleT";
                currentMuzzle++;
                if (currentMuzzle >= 3) {
                    currentMuzzle = 1;
                }

                EffectManager.SimpleMuzzleFlash(MuzzleFlash, base.gameObject, muzzle, false);
                AkSoundEngine.PostEvent(Events.Play_lunar_golem_attack1_launch, base.gameObject);
            }

            if (base.fixedAge >= 0.35f && !base.inputBank.skill1.down) {
                outer.SetNextState(new ShatterExit());
            }
        }
    }

    public class ShatterExit : BaseSkillState {
        public float duration = 0.35f;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Override", "StopGatling", "PrepGatling.playbackRate", duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}