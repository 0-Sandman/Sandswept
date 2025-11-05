using Sandswept.Survivors.Ranger.Projectiles;
using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Survivors.Ranger.States.Secondary
{
    public class Release : BaseState
    {
        public static float minDamageCoefficient = 6f;
        public static float maxDamageCoefficient = 18f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.25f;
        public float duration;
        public bool hasFired = false;
        private GameObject tracerEffect;
        private GameObject impactEffect;
        private Transform modelTransform;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;

            Util.PlaySound("Play_mage_m2_charge", gameObject);
            Util.PlaySound("Play_mage_m2_charge", gameObject);
            Util.PlaySound("Play_voidBarnacle_m1_chargeUp", gameObject);
            Util.PlaySound("Play_voidBarnacle_m1_chargeUp", gameObject);
            Util.PlaySound("Play_voidBarnacle_m1_chargeUp", gameObject);
            // Util.PlaySound("Play_railgunner_R_gun_chargeUp", gameObject);

            PlayAnimation("Gesture, Override", "Fire", "Fire.playbackRate", duration);

            if (characterBody)
                characterBody.SetAimTimer(1.5f);

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                switch (skinNameToken)
                {
                    default:
                        tracerEffect = ReleaseVFX.tracerPrefabDefault;
                        impactEffect = ReleaseVFX.impactPrefabDefault;
                        break;

                    case "RANGER_SKIN_MAJOR_NAME":
                        tracerEffect = ReleaseVFX.tracerPrefabMajor;
                        impactEffect = ReleaseVFX.impactPrefabMajor;
                        break;

                    case "RANGER_SKIN_RENEGADE_NAME":
                        tracerEffect = ReleaseVFX.tracerPrefabRenegade;
                        impactEffect = ReleaseVFX.impactPrefabRenegade;
                        break;

                    case "RANGER_SKIN_MILEZERO_NAME":
                        tracerEffect = ReleaseVFX.tracerPrefabMileZero;
                        impactEffect = ReleaseVFX.impactPrefabMileZero;
                        break;

                    case "RANGER_SKIN_SANDSWEPT_NAME":
                        tracerEffect = ReleaseVFX.tracerPrefabSandswept;
                        impactEffect = ReleaseVFX.impactPrefabSandswept;
                        break;
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (characterBody)
            {
                characterBody.isSprinting = false;
            }

            if (fixedAge < duration || !isAuthority)
            {
                return;
            }

            if (!hasFired)
            {
                FireShot();
                hasFired = true;
            }

            outer.SetNextStateToMain();
        }

        public void FireShot()
        {
            Util.PlaySound("Play_lunar_wisp_attack2_launch", gameObject);
            Util.PlaySound("Play_bleedOnCritAndExplode_impact", gameObject);
            Util.PlaySound("Play_item_use_meteor_impact", gameObject);
            Util.PlaySound("Play_commando_M2_impact", gameObject);

            var buffCount = characterBody.GetBuffCount(Buffs.Charge.instance.BuffDef);

            if (isAuthority)
            {
                var aimDirection = GetAimRay().direction;

                BulletAttack attack = new()
                {
                    aimVector = aimDirection,
                    falloffModel = BulletAttack.FalloffModel.None,
                    damage = damageStat * Util.Remap(buffCount, 0, DirectCurrent.maxCharge, minDamageCoefficient, maxDamageCoefficient),
                    isCrit = RollCrit(),
                    minSpread = 0,
                    maxSpread = 0,
                    owner = gameObject,
                    muzzleName = "MuzzleR",
                    origin = transform.position,
                    tracerEffectPrefab = tracerEffect,
                    hitEffectPrefab = impactEffect,
                    procCoefficient = procCoefficient,
                    weapon = gameObject,
                    radius = 2f,
                    smartCollision = true,
                    stopperMask = LayerIndex.world.mask,
                    force = 2500f + 125f * buffCount,
                    damageType = DamageType.Generic,
                };

                attack.damageType.damageSource = DamageSource.Secondary;
                attack.damageType.AddModdedDamageType(Electrician.Electrician.LIGHTNING);

                AddRecoil(3f + 0.15f * buffCount, 3f + 0.15f * buffCount, 0f, 0f);

                characterMotor?.ApplyForce((-4500f - 175f * buffCount) * aimDirection, false, false);

                attack.Fire();
            }

            characterBody.SetBuffCountSynced(Buffs.Charge.instance.BuffDef.buffIndex, Mathf.Max(0, buffCount - DirectCurrent.maxCharge));
        }
    }
}