using Sandswept2.Skills.Ranger.VFX;
using System;

namespace Sandswept2.States.Ranger
{
    public class OverdriveFire : BaseState
    {
        public static int ShotsPerSecond = 6;
        public static float ProcCoeff = 0.7f;
        public static float DamageCoeff = 1f;
        public static GameObject TracerEffect => OverdriveShotVFX.tracerPrefab; // beef this up later
        public static GameObject TracerEffectHeated => OverdriveShotHeatedVFX.tracerPrefab; // beef this up later
        public static float SelfDamageCoeff = 0.2f;
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

        public override void Update()
        {
            base.Update();
            characterDirection.forward = GetAimRay().direction;
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

            if (!inputBank.skill1.down)
            {
                Exit();
            }
        }

        public void FireShot()
        {
            Util.PlayAttackSpeedSound("Play_drone_attack", gameObject, attackSpeedStat);
            Util.PlayAttackSpeedSound("Play_drone_attack", gameObject, attackSpeedStat);

            PlayAnimation("Gesture, Override", "OverdriveFire");

            if (!isAuthority)
            {
                return;
            }

            var aimDiretion = GetAimRay().direction;

            var isHeatedShot = Util.CheckRoll(heat.CurrentHeat * 0.5f);

            if (isHeatedShot)
                Util.PlayAttackSpeedSound("Play_commando_M2", gameObject, 1f);
            else
                Util.PlayAttackSpeedSound("Play_commando_M2", gameObject, 1.1f);

            BulletAttack attack = new()
            {
                aimVector = aimDiretion,
                falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                damage = damageStat * DamageCoeff,
                isCrit = RollCrit(),
                owner = gameObject,
                muzzleName = "Muzzle",
                origin = GetAimRay().origin,
                tracerEffectPrefab = isHeatedShot ? TracerEffectHeated : TracerEffect,
                procCoefficient = ProcCoeff,
                damageType = isHeatedShot ? DamageType.IgniteOnHit : DamageType.Generic,
                minSpread = heat.CurrentHeat * 0.005f,
                maxSpread = heat.CurrentHeat * 0.006f,
                damageColorIndex = isHeatedShot ? DamageColorIndex.Fragile : DamageColorIndex.Default,
                radius = 0.5f,
                smartCollision = true
            };

            AddRecoil(0.3f + heat.CurrentHeat * 0.005f, -0.3f - heat.CurrentHeat * 0.005f, 0.1f + heat.CurrentHeat * 0.005f, -0.1f - heat.CurrentHeat * 0.005f);

            attack.Fire();

            heat.isFiring = true;
        }
    }
}