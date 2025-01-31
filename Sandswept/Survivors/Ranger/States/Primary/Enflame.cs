using Sandswept.Survivors.Ranger.VFX;
using System;

namespace Sandswept.Survivors.Ranger.States.Primary
{
    public class Enflame : BaseState
    {
        public static float procCoefficient = 0.9f;
        public static float damageCoefficient = 0.75f;

        public static float baseShotDistance = 200f;
        public static float shotDistanceScaling = -15f;
        public static float minimumShotDistance = 50f;
        public float finalShotDistance;

        public static float baseShotSoundPitch = 1f;
        public static float shotSoundPitchScaling = 0.014f;
        public float finalShotSoundPitch;

        public static int baseShotsPerSecond = 4;
        public static int extraShotsScaling = 1;
        public static int extraShotsCap = 8;
        public static float extraShotsTimer = 4f;
        public float finalExtraShots;
        public float finalShotsPerSecond;
        public float finalDurationPerShot;
        public float shotTimer = 0f;

        public GameObject tracerEffect;
        public GameObject tracerEffectHeated;

        public RangerHeatController rangerHeatController;
        public Transform modelTransform;

        public override void OnEnter()
        {
            base.OnEnter();

            rangerHeatController = GetComponent<RangerHeatController>();

            finalExtraShots = Mathf.Min(extraShotsCap, extraShotsScaling * (1f / extraShotsTimer) * rangerHeatController.currentHeat / 5);
            finalShotsPerSecond = (baseShotsPerSecond + finalExtraShots) * attackSpeedStat;
            finalDurationPerShot = 1f / finalShotsPerSecond;

            finalShotSoundPitch = baseShotSoundPitch + (shotSoundPitchScaling * finalShotsPerSecond);

            finalShotDistance = Mathf.Max(minimumShotDistance, baseShotDistance + shotDistanceScaling * rangerHeatController.currentHeat / 5);

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                tracerEffect = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => EnflameVFX.tracerPrefabMajor,
                    "SKINDEF_RENEGADE" => EnflameVFX.tracerPrefabRenegade,
                    "SKINDEF_MILEZERO" => EnflameVFX.tracerPrefabMileZero,
                    "SKINDEF_SANDSWEPT" => EnflameVFX.tracerPrefabSandswept,
                    _ => EnflameVFX.tracerPrefabDefault
                };

                tracerEffectHeated = skinNameToken switch
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
                characterBody.SetAimTimer(1f);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            shotTimer += Time.fixedDeltaTime;

            if (shotTimer >= finalDurationPerShot)
            {
                shotTimer = 0f;
                FireShot();
                outer.SetNextStateToMain();
            }
        }

        public void FireShot()
        {
            Util.PlayAttackSpeedSound("Play_drone_attack", gameObject, attackSpeedStat);
            Util.PlayAttackSpeedSound("Play_drone_attack", gameObject, attackSpeedStat);
            Util.PlayAttackSpeedSound("Play_lunar_wisp_attack1_shoot_bullet", gameObject, attackSpeedStat);
            Util.PlayAttackSpeedSound("Play_lunar_wisp_attack1_shoot_bullet", gameObject, attackSpeedStat);

            Util.PlayAttackSpeedSound("Play_commando_M2", gameObject, finalShotSoundPitch);

            PlayAnimation("Gesture, Override", "OverdriveFire");

            var aimDirection = GetAimRay().direction;

            var isHeatedShot = Util.CheckRoll(rangerHeatController.currentHeat);

            if (isHeatedShot)
            {
                Util.PlayAttackSpeedSound("Play_drone_attack", gameObject, attackSpeedStat);
                Util.PlayAttackSpeedSound("Play_lunar_wisp_attack1_shoot_bullet", gameObject, attackSpeedStat);
            }

            BulletAttack attack = new()
            {
                aimVector = aimDirection,
                falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                damage = damageStat * damageCoefficient,
                isCrit = RollCrit(),
                owner = gameObject,
                muzzleName = "Muzzle",
                origin = GetAimRay().origin,
                tracerEffectPrefab = isHeatedShot ? tracerEffectHeated : tracerEffect,
                procCoefficient = procCoefficient,
                damageType = isHeatedShot ? DamageType.IgniteOnHit : DamageType.Generic,
                minSpread = -1.5f,
                maxSpread = 1.5f,
                damageColorIndex = isHeatedShot ? DamageColorIndex.Fragile : DamageColorIndex.Default,
                radius = 0.3f,
                smartCollision = true,
                maxDistance = finalShotDistance
            };

            attack.damageType.damageSource = DamageSource.Primary;

            AddRecoil(0.3f + rangerHeatController.currentHeat * 0.006f, -0.3f - rangerHeatController.currentHeat * 0.006f, 0.1f + rangerHeatController.currentHeat * 0.006f, -0.1f - rangerHeatController.currentHeat * 0.006f);

            if (!isAuthority)
            {
                return;
            }
            attack.Fire();
        }
    }
}