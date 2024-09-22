using Sandswept.Survivors.Ranger.Projectiles;
using Sandswept.Survivors.Ranger.VFX;
using System;
using System.Collections;

namespace Sandswept.Survivors.Ranger.States.Secondary
{
    public class Galvanize : BaseState
    {
        public static float DamageCoefficient = 0.5f;
        public static int Projectiles = 3;
        public static int MaxProjectiles = 10;
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

            /*
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

                    case "SKINDEF_MAJOR":
                        tracerEffect = ReleaseVFX.tracerPrefabMajor;
                        impactEffect = ReleaseVFX.impactPrefabMajor;
                        break;

                    case "SKINDEF_RENEGADE":
                        tracerEffect = ReleaseVFX.tracerPrefabRenegade;
                        impactEffect = ReleaseVFX.impactPrefabRenegade;
                        break;

                    case "SKINDEF_MILEZERO":
                        tracerEffect = ReleaseVFX.tracerPrefabMileZero;
                        impactEffect = ReleaseVFX.impactPrefabMileZero;
                        break;

                    case "SKINDEF_SANDSWEPT":
                        tracerEffect = ReleaseVFX.tracerPrefabSandswept;
                        impactEffect = ReleaseVFX.impactPrefabSandswept;
                        break;
                }
            }
            */
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
            AkSoundEngine.PostEvent(Events.Play_lunar_wisp_attack2_launch, gameObject);
            AkSoundEngine.PostEvent(Events.Play_bleedOnCritAndExplode_impact, gameObject);
            AkSoundEngine.PostEvent(Events.Play_item_use_meteor_impact, gameObject);
            AkSoundEngine.PostEvent(Events.Play_commando_M2_impact, gameObject);

            var buffCount = characterBody.GetBuffCount(Buffs.Charge.instance.BuffDef);
            var aimDirection = GetAimRay().direction;

            AddRecoil(3f + 0.3f * buffCount, 3f + 0.3f * buffCount, 0f, 0f);

            characterMotor?.ApplyForce((-2500f - 250f * buffCount) * aimDirection, false, false);

            outer.StartCoroutine(FireProjectiles());
        }

        public IEnumerator FireProjectiles()
        {
            var buffCount = characterBody.GetBuffCount(Buffs.Charge.instance.BuffDef);
            var projectileCount = (int)Util.Remap(buffCount, 0, DirectCurrent.maxCharge, Projectiles, MaxProjectiles);
            var aimDirection = GetAimRay().direction;
            for (int i = 0; i < projectileCount; i++)
            {
                if (isAuthority)
                {
                    var fpi = new FireProjectileInfo()
                    {
                        damage = damageStat * DamageCoefficient,
                        crit = RollCrit(),
                        damageColorIndex = DamageColorIndex.Default,
                        owner = gameObject,
                        rotation = Quaternion.LookRotation(aimDirection), // make this fire in some kinda pattern most likely, I suck at rotation
                        //position = GetModelChildLocator().FindChild("Muzzle").position,
                        position = transform.position,
                        force = 500f,
                        procChainMask = default,
                        projectilePrefab = TheFuckingBFG.prefabDefault
                    };

                    ProjectileManager.instance.FireProjectile(fpi);
                }

                AddRecoil(1f + 0.1f * buffCount, 1f + 0.1f * buffCount, 0f, 0f);

                characterMotor?.ApplyForce((-350f - 35f * buffCount) * aimDirection, false, false);

                yield return new WaitForSeconds(duration * 2f / projectileCount);
            }

            characterBody.SetBuffCount(Buffs.Charge.instance.BuffDef.buffIndex, Mathf.Max(0, buffCount - DirectCurrent.maxCharge));
        }
    }
}