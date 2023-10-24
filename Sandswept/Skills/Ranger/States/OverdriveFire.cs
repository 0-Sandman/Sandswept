using Sandswept.Skills.Ranger.VFX;
using System;

namespace Sandswept.States.Ranger
{
    public class OverdriveFire : BaseState
    {
        public static int ShotsPerSecond = 8;
        public static float ProcCoeff = 1f;
        public static float DamageCoeff = 1f;
        public static GameObject TracerEffect => DirectCurrentVFX.tracerPrefab; // beef this up later
        private float selfDamageCoeff = 0.08f;
        private float shots;
        private float shotDelay => 1f / shots;
        private float stopwatch = 0f;
        private Components.RangerHeatManager heat;

        public override void OnEnter()
        {
            base.OnEnter();

            heat = GetComponent<Components.RangerHeatManager>();
            heat.isFiring = false;
            shots = ShotsPerSecond * attackSpeedStat;

            if (characterBody)
            {
                characterBody.isSprinting = true;
            }
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

            base.characterDirection.forward = base.GetAimRay().direction;

            if (base.inputBank.skill1.down && stopwatch >= shotDelay)
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
            Util.PlayAttackSpeedSound("Play_commando_M2", gameObject, attackSpeedStat);
            Util.PlayAttackSpeedSound("Play_drone_attack", gameObject, attackSpeedStat);
            Util.PlayAttackSpeedSound("Play_drone_attack", gameObject, attackSpeedStat);

            if (heat.IsOverheating)
            {
                DamageInfo info = new()
                {
                    attacker = gameObject,
                    procCoefficient = 0,
                    damage = damageStat * (selfDamageCoeff + 0.0008f * heat.CurrentHeat),
                    crit = false,
                    position = transform.position,
                    damageColorIndex = DamageColorIndex.Bleed,
                    damageType = DamageType.BypassArmor, // makes rap fake and cheesing guh
                    rejected = false
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

            var aimDiretion = GetAimRay().direction;

            BulletAttack attack = new()
            {
                aimVector = aimDiretion,
                falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                damage = damageStat * DamageCoeff,
                isCrit = RollCrit(),
                owner = gameObject,
                muzzleName = "Muzzle",
                origin = GetAimRay().origin,
                tracerEffectPrefab = TracerEffect,
                procCoefficient = ProcCoeff,
                damageType = Util.CheckRoll(heat.CurrentHeat * 0.5f) ? DamageType.IgniteOnHit : DamageType.Generic,
                minSpread = heat.CurrentHeat * 0.009f,
                maxSpread = heat.CurrentHeat * 0.01f
            };

            attack.Fire();

            heat.isFiring = true;
        }
    }
}