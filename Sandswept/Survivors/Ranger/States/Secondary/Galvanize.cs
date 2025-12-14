using RoR2BepInExPack.GameAssetPaths;
using Sandswept.Survivors.Ranger.Projectiles;
using Sandswept.Survivors.Ranger.VFX;
using System;
using System.Collections;

namespace Sandswept.Survivors.Ranger.States.Secondary
{
    public class Galvanize : BaseState
    {
        public static float damageCoefficient = 0.8f;
        public static int minProjectiles = 3;
        public static int maxProjectiles = 10;
        public static float baseDuration = 0.25f;
        public float duration;
        public bool hasFired = false;
        private GameObject tracerEffect;
        private GameObject impactEffect;
        private GameObject muzzleFlash;
        private Transform modelTransform;
        private int projectilesFired = 0;

        public override void OnEnter()
        {
            base.OnEnter();

            projectilesFired = 0;

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
                        // tracerEffect = ReleaseVFX.tracerPrefabDefault;
                        // impactEffect = ReleaseVFX.impactPrefabDefault;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabDefault;
                        break;

                    case "RANGER_SKIN_MAJOR_NAME":
                        // tracerEffect = ReleaseVFX.tracerPrefabMajor;
                        // impactEffect = ReleaseVFX.impactPrefabMajor;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabMajor;
                        break;

                    case "RANGER_SKIN_RENEGADE_NAME":
                        // tracerEffect = ReleaseVFX.tracerPrefabRenegade;
                        // impactEffect = ReleaseVFX.impactPrefabRenegade;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabRenegade;
                        break;

                    case "RANGER_SKIN_MILEZERO_NAME":
                        // tracerEffect = ReleaseVFX.tracerPrefabMileZero;
                        // impactEffect = ReleaseVFX.impactPrefabMileZero;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabMileZero;
                        break;

                    case "RANGER_SKIN_SANDSWEPT_NAME":
                        // tracerEffect = ReleaseVFX.tracerPrefabSandswept;
                        // impactEffect = ReleaseVFX.impactPrefabSandswept;
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

            // could potentially make it charge up and increase projectile speed with charge so that you can hit more distant targets

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
            var aimDirection = GetAimRay().direction;

            AddRecoil(3f + 0.3f * buffCount, 3f + 0.3f * buffCount, 0f, 0f);

            characterMotor?.ApplyForce((-2900f - 290f * buffCount) * aimDirection, false, false);

            outer.StartCoroutine(FireProjectiles());
        }

        public IEnumerator FireProjectiles()
        {
            var buffCount = characterBody.GetBuffCount(Buffs.Charge.instance.BuffDef);
            var projectileCount = (int)Util.Remap(buffCount, 0, DirectCurrent.maxCharge, minProjectiles, maxProjectiles);
            var aimDirection = GetAimRay().direction;
            for (int i = 0; i < projectileCount; i++)
            {
                var angleSpread = (float)Mathf.FloorToInt(projectilesFired - (projectileCount - 1) / 2f) / (projectileCount - 1) * 60f;
                if (isAuthority)
                {
                    var fpi = new FireProjectileInfo()
                    {
                        damage = damageStat * damageCoefficient,
                        crit = RollCrit(),
                        damageColorIndex = DamageColorIndex.Default,
                        owner = gameObject,
                        rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(aimDirection, 0f, 0f, 1f, 1f, angleSpread, 0f)),
                        //position = GetModelChildLocator().FindChild("Muzzle").position,
                        position = transform.position,
                        force = 500f,
                        procChainMask = default,
                        projectilePrefab = AltSecondaries.galvanizeProjectile,
                        damageTypeOverride = new DamageTypeCombo?(DamageTypeCombo.GenericPrimary)
                    };

                    ProjectileManager.instance.FireProjectile(fpi);
                }

                AddRecoil(1f + 0.05f * buffCount, 1f + 0.05f * buffCount, 0f, 0f);

                EffectManager.SimpleMuzzleFlash(muzzleFlash, gameObject, "Muzzle", transmit: true);

                characterMotor?.ApplyForce((-150f - 7.5f * buffCount) * aimDirection, false, false);
                projectilesFired++;
                yield return new WaitForSeconds(duration * 2f / projectileCount);
            }

            characterBody.SetBuffCountSynced(Buffs.Charge.instance.BuffDef.buffIndex, Mathf.Max(0, buffCount - DirectCurrent.maxCharge));
        }
    }
}