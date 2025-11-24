using Rewired.Demos;
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
        private GameObject muzzleFlash;
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
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabDefault;
                        break;

                    case "RANGER_SKIN_MAJOR_NAME":
                        tracerEffect = ReleaseVFX.tracerPrefabMajor;
                        impactEffect = ReleaseVFX.impactPrefabMajor;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabMajor;
                        break;

                    case "RANGER_SKIN_RENEGADE_NAME":
                        tracerEffect = ReleaseVFX.tracerPrefabRenegade;
                        impactEffect = ReleaseVFX.impactPrefabRenegade;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabRenegade;
                        break;

                    case "RANGER_SKIN_MILEZERO_NAME":
                        tracerEffect = ReleaseVFX.tracerPrefabMileZero;
                        impactEffect = ReleaseVFX.impactPrefabMileZero;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabMileZero;
                        break;

                    case "RANGER_SKIN_SANDSWEPT_NAME":
                        tracerEffect = ReleaseVFX.tracerPrefabSandswept;
                        impactEffect = ReleaseVFX.impactPrefabSandswept;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabSandswept;
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
                    origin = GetAimRay().origin,
                    // tracerEffectPrefab = tracerEffect,
                    hitEffectPrefab = impactEffect,
                    procCoefficient = procCoefficient,
                    weapon = gameObject,
                    radius = 2f,
                    smartCollision = true,
                    stopperMask = LayerIndex.world.mask,
                    force = 2500f + 125f * buffCount,
                    damageType = DamageType.Generic,
                    maxDistance = 99999f
                };

                // ja pierdole kurwa mac ile trzeba zjebanego opizdzialego gowna kurwa copy paste'owac tylko zeby zeskalowac jebane vfx bo hopoo games ! ! sobie pomyslalo ze effectdata w bulletattack bedzie hardcoded kurwa
                attack.hitCallback = delegate (BulletAttack bulletAttack, ref BulletAttack.BulletHit bulletHit)
                {
                    int muzzleIndex = -1;
                    if (!attack.weapon)
                    {
                        attack.weapon = attack.owner;
                    }
                    if (attack.weapon)
                    {
                        var modelLocator = attack.weapon.GetComponent<ModelLocator>();
                        if (modelLocator && modelLocator.modelTransform)
                        {
                            var childLocator = modelLocator.modelTransform.GetComponent<ChildLocator>();
                            if (childLocator)
                            {
                                muzzleIndex = childLocator.FindChildIndex(attack.muzzleName);
                            }
                        }
                    }
                    var defaultHitCallback = BulletAttack.defaultHitCallback(bulletAttack, ref bulletHit);
                    var aimVector = TrajectoryAimAssist.ApplyTrajectoryAimAssist(attack.aimVector, attack.origin, 99999f, attack.owner, attack.weapon, attack.trajectoryAimAssistMultiplier).normalized;

                    var stupidFuckingHardcodedIllConsideredBullshit = new EffectData();

                    stupidFuckingHardcodedIllConsideredBullshit.origin = attack.origin + aimVector * 99999f; // the end position, supposedly
                    stupidFuckingHardcodedIllConsideredBullshit.start = attack.origin;
                    stupidFuckingHardcodedIllConsideredBullshit.genericUInt = (uint)buffCount;
                    stupidFuckingHardcodedIllConsideredBullshit.SetChildLocatorTransformReference(attack.weapon, muzzleIndex);
                    EffectManager.SpawnEffect(tracerEffect, stupidFuckingHardcodedIllConsideredBullshit, true);

                    return defaultHitCallback;
                };

                // KURWA MAC ale zjebane gowno kurwa ja pierkurwadole kurwa

                attack.damageType.damageSource = DamageSource.Secondary;
                attack.damageType.AddModdedDamageType(Electrician.Electrician.LIGHTNING);

                characterMotor?.ApplyForce((-4500f - 175f * buffCount) * aimDirection, false, false);

                attack.Fire();
            }

            EffectManager.SimpleMuzzleFlash(muzzleFlash, gameObject, "Muzzle", transmit: true);

            AddRecoil(3f + 0.15f * buffCount, 3f + 0.15f * buffCount, 0f, 0f);

            characterBody.SetBuffCountSynced(Buffs.Charge.instance.BuffDef.buffIndex, Mathf.Max(0, buffCount - DirectCurrent.maxCharge));
        }
    }
}