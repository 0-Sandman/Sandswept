using Sandswept.Skills.Ranger.VFX;
using System;

namespace Sandswept.States.Ranger
{
    public class OverdriveFire : BaseState
    {
        public static int ShotsPerSecond = 5;
        public static float DamageCoeff = 2f;
        public static float SelfDamageCoeff = 0.1f;
        public static float ProcCoeff = 1f;
        public static GameObject TracerEffect => GunGoShootVFX.tracerPrefab; // beef this up later
        private float shots;
        private float shotDelay => 1f / shots;
        private float stopwatch = 0f;
        private Components.RangerHeatManager heat;

        public override void OnEnter()
        {
            base.OnEnter();
            shots = ShotsPerSecond * base.attackSpeedStat;
            heat = GetComponent<Components.RangerHeatManager>();
            heat.isFiring = true;
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

            if (base.inputBank.skill1.down && stopwatch >= shotDelay)
            {
                stopwatch = 0f;
                FireShot();
            }

            if (!base.inputBank.skill1.down)
            {
                Exit();
            }
        }

        public void FireShot()
        {
            AkSoundEngine.PostEvent(Events.Play_commando_M2, base.gameObject);

            if (heat.IsOverheating)
            {
                DamageInfo info = new()
                {
                    attacker = base.gameObject,
                    procCoefficient = 0,
                    damage = base.damageStat * SelfDamageCoeff,
                    crit = false,
                    position = base.transform.position,
                    damageColorIndex = DamageColorIndex.Bleed
                };

                if (NetworkServer.active)
                {
                    healthComponent.TakeDamage(info);
                }

                AkSoundEngine.PostEvent(Events.Play_item_proc_igniteOnKill, base.gameObject);
            }

            if (!base.isAuthority)
            {
                return;
            }

            BulletAttack attack = new()
            {
                aimVector = base.GetAimRay().direction,
                falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                damage = base.damageStat * DamageCoeff,
                isCrit = base.RollCrit(),
                damageType = DamageType.Generic,
                owner = base.gameObject,
                muzzleName = "Muzzle",
                origin = base.GetAimRay().origin,
                tracerEffectPrefab = TracerEffect,
                procCoefficient = ProcCoeff
            };

            attack.Fire();
        }
    }
}