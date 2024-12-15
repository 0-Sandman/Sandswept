using Sandswept.Survivors.Ranger.VFX;
using System;

namespace Sandswept.Survivors.Ranger.States.Primary
{
    public class Enflame : BaseState
    {
        public static int ShotsPerSecond = 6;
        public static float ProcCoeff = 0.8f;
        public static float DamageCoeff = 0.9f;
        private GameObject TracerEffect;
        private GameObject TracerEffectHeated;
        private float shots;
        private float shotDelay => 1f / shots;
        private float stopwatch = 0f;
        private RangerHeatController heat;
        private Transform modelTransform;

        public override void OnEnter()
        {
            base.OnEnter();

            heat = GetComponent<RangerHeatController>();
            heat.isFiring = false;
            shots = ShotsPerSecond * attackSpeedStat;

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                TracerEffect = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => EnflameVFX.tracerPrefabMajor,
                    "SKINDEF_RENEGADE" => EnflameVFX.tracerPrefabRenegade,
                    "SKINDEF_MILEZERO" => EnflameVFX.tracerPrefabMileZero,
                    "SKINDEF_SANDSWEPT" => EnflameVFX.tracerPrefabSandswept,
                    _ => EnflameVFX.tracerPrefabDefault
                };

                TracerEffectHeated = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => EnflameVFX.tracerHeatedPrefabMajor,
                    "SKINDEF_RENEGADE" => EnflameVFX.tracerHeatedPrefabRenegade,
                    "SKINDEF_MILEZERO" => EnflameVFX.tracerHeatedPrefabMileZero,
                    "SKINDEF_SANDSWEPT" => EnflameVFX.tracerHeatedPrefabSandswept,
                    _ => EnflameVFX.tracerHeatedPrefabDefault
                };
            }

            if (characterBody)
            {
                characterBody.isSprinting = true;
            }
            if (characterBody)
                characterBody.SetAimTimer(1f);
        }

        public void Exit()
        {
            heat.isFiring = false;
            outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.SetAimTimer(0.4f);

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
            Util.PlayAttackSpeedSound("Play_drone_attack", gameObject, attackSpeedStat);
            Util.PlayAttackSpeedSound("Play_drone_attack", gameObject, attackSpeedStat);
            Util.PlayAttackSpeedSound("Play_lunar_wisp_attack1_shoot_bullet", gameObject, attackSpeedStat);
            Util.PlayAttackSpeedSound("Play_lunar_wisp_attack1_shoot_bullet", gameObject, attackSpeedStat);

            PlayAnimation("Gesture, Override", "OverdriveFire");

            if (!isAuthority)
            {
                return;
            }

            var aimDiretion = GetAimRay().direction;

            var isHeatedShot = Util.CheckRoll(heat.currentHeat);

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
                minSpread = -1f - heat.currentHeat * 0.015f,
                maxSpread = 1f + heat.currentHeat * 0.015f,
                damageColorIndex = isHeatedShot ? DamageColorIndex.Fragile : DamageColorIndex.Default,
                radius = 0.4f,
                smartCollision = true,
            };

            attack.damageType.damageSource = DamageSource.Primary;

            AddRecoil(0.3f + heat.currentHeat * 0.006f, -0.3f - heat.currentHeat * 0.006f, 0.1f + heat.currentHeat * 0.006f, -0.1f - heat.currentHeat * 0.006f);

            attack.Fire();

            heat.isFiring = true;
        }
    }
}