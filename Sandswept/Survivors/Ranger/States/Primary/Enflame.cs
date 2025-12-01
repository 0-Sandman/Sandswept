using Sandswept.Survivors.Ranger.VFX;
using System;

namespace Sandswept.Survivors.Ranger.States.Primary
{
    public class Enflame : BaseState
    {
        public static float procCoefficient = 1f;
        public static float damageCoefficient = 0.9f;

        public static float baseShotDistance = 200f;
        public static float shotDistanceScaling = -15f;
        public static float minimumShotDistance = 50f;
        public float finalShotDistance;

        public static float baseShotSoundPitch = 1f;
        public static float shotSoundPitchScaling = 0.014f;
        public float finalShotSoundPitch;

        public static int baseShotsPerSecond = 4;
        public static int extraShotsScaling = 1;
        public static float heatPerExtraShot = 25f;
        public float finalExtraShots;
        public float finalShotsPerSecond;
        public float finalDurationPerShot;
        public float shotTimer = 0f;

        public float heatPerSecond = 25f;

        public GameObject tracerEffect;
        public GameObject tracerEffectHeated;
        public GameObject muzzleFlash;

        public RangerHeatController rangerHeatController;
        public Transform modelTransform;

        public override void OnEnter()
        {
            base.OnEnter();

            rangerHeatController = GetComponent<RangerHeatController>();

            finalExtraShots = extraShotsScaling * (rangerHeatController.currentHeat / heatPerExtraShot);
            finalShotsPerSecond = (baseShotsPerSecond + finalExtraShots) * attackSpeedStat;
            finalDurationPerShot = 1f / finalShotsPerSecond;

            finalShotSoundPitch = baseShotSoundPitch + (shotSoundPitchScaling * finalShotsPerSecond);

            finalShotDistance = Mathf.Max(minimumShotDistance, baseShotDistance + shotDistanceScaling * rangerHeatController.currentHeat / 5);

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                switch (skinNameToken)
                {
                    default:
                        tracerEffect = EnflameVFX.tracerPrefabDefault;
                        tracerEffectHeated = EnflameVFX.tracerHeatedPrefabDefault;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabDefaultOverdrive;
                        break;

                    case "RANGER_SKIN_MAJOR_NAME":
                        tracerEffect = EnflameVFX.tracerPrefabMajor;
                        tracerEffectHeated = EnflameVFX.tracerHeatedPrefabMajor;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabMajorOverdrive;
                        break;

                    case "RANGER_SKIN_RENEGADE_NAME":
                        tracerEffect = EnflameVFX.tracerPrefabRenegade;
                        tracerEffectHeated = EnflameVFX.tracerHeatedPrefabRenegade;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabRenegadeOverdrive;
                        break;

                    case "RANGER_SKIN_MILEZERO_NAME":
                        tracerEffect = EnflameVFX.tracerPrefabMileZero;
                        tracerEffectHeated = EnflameVFX.tracerHeatedPrefabMileZero;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabMileZeroOverdrive;
                        break;

                    case "RANGER_SKIN_SANDSWEPT_NAME":
                        tracerEffect = EnflameVFX.tracerPrefabSandswept;
                        tracerEffectHeated = EnflameVFX.tracerHeatedPrefabSandswept;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabSandsweptOverdrive;
                        break;
                }
            }

            if (characterBody)
            {
                characterBody.isSprinting = false;
                // characterBody.bodyFlags |= CharacterBody.BodyFlags.SprintAnyDirection;
                // characterBody.isSprinting = true;
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

            rangerHeatController.currentHeat += heatPerSecond * Time.fixedDeltaTime;

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

            var isHeatedShot = Util.CheckRoll(rangerHeatController.currentHeat * 0.5f);

            EffectManager.SimpleMuzzleFlash(muzzleFlash, gameObject, "Muzzle", transmit: true);

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
                tracerEffectPrefab = tracerEffectHeated,
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