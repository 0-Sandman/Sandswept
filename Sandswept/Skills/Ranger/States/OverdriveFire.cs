using Sandswept.Skills.Ranger.VFX;
using System;

namespace Sandswept.States.Ranger
{
    public class OverdriveFire : BaseState
    {
        public static int ShotsPerSecond = 5;
        public static float SelfDamageCoeff = 0.11f;
        public static float ProcCoeff = 1f;
        public static GameObject TracerEffect => GunGoShootVFX.tracerPrefab; // beef this up later
        private float damageCoeff = 0f;
        private float shots;
        private float shotDelay => 1f / shots;
        private float stopwatch = 0f;
        private Components.RangerHeatManager heat;

        public override void OnEnter()
        {
            base.OnEnter();

            heat = GetComponent<Components.RangerHeatManager>();
            heat.isFiring = true;

            damageCoeff = 1.5f + 0.0075f * heat.CurrentHeat;
            shots = ShotsPerSecond * attackSpeedStat;
        }

        public void Exit()
        {
            heat.isFiring = false;
            outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;

            if (inputBank.skill1.down && stopwatch >= shotDelay)
            {
                stopwatch = 0f;
                FireShot();
            }

            if (!inputBank.skill1.down)
            {
                Exit();
            }
        }

        public void FireShot()
        {
            AkSoundEngine.PostEvent(Events.Play_commando_M2, gameObject);

            if (heat.IsOverheating)
            {
                DamageInfo info = new()
                {
                    attacker = gameObject,
                    procCoefficient = 0,
                    damage = damageStat * SelfDamageCoeff,
                    crit = false,
                    position = transform.position,
                    damageColorIndex = DamageColorIndex.Bleed,
                    damageType = DamageType.BypassArmor // makes rap fake and cheesing guh
                };

                if (NetworkServer.active)
                {
                    healthComponent.TakeDamage(info);
                }

                AkSoundEngine.PostEvent(Events.Play_item_proc_igniteOnKill, gameObject);
            }

            PlayAnimation("Gesture, Override", "OverdriveFire");

            if (!isAuthority)
            {
                return;
            }

            BulletAttack attack = new()
            {
                aimVector = GetAimRay().direction,
                falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                damage = damageStat * damageCoeff,
                isCrit = RollCrit(),
                damageType = DamageType.Generic,
                owner = gameObject,
                muzzleName = "Muzzle",
                origin = GetAimRay().origin,
                tracerEffectPrefab = TracerEffect,
                procCoefficient = ProcCoeff,
                spreadPitchScale = 1f + (0.01f * heat.CurrentHeat),
                spreadYawScale = 1f + (0.01f * heat.CurrentHeat)
            };

            attack.Fire();
        }
    }
}