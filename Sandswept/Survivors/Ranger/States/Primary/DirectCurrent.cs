using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Survivors.Ranger.States.Primary
{
    public class DirectCurrent : BaseState
    {
        public static float damageCoefficient = 2.5f;
        public static float baseDuration = 0.5f;
        private float duration;

        private GameObject directCurrentProjectile;
        private GameObject muzzleFlash;

        public override void OnEnter()
        {
            base.OnEnter();

            var modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                switch (skinNameToken)
                {
                    default:
                        directCurrentProjectile = Projectiles.DirectCurrent.prefabDefault;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabDefault;
                        break;

                    case "RANGER_SKIN_MAJOR_NAME":
                        directCurrentProjectile = Projectiles.DirectCurrent.prefabMajor;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabMajor;
                        break;

                    case "RANGER_SKIN_RENEGADE_NAME":
                        directCurrentProjectile = Projectiles.DirectCurrent.prefabRenegade;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabRenegade;
                        break;

                    case "RANGER_SKIN_MILEZERO_NAME":
                        directCurrentProjectile = Projectiles.DirectCurrent.prefabMileZero;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabMileZero;
                        break;

                    case "RANGER_SKIN_SANDSWEPT_NAME":
                        directCurrentProjectile = Projectiles.DirectCurrent.prefabSandswept;
                        muzzleFlash = DirectCurrentVFX.muzzleFlashPrefabSandswept;
                        break;
                }
            }

            FireShot();

            duration = baseDuration / attackSpeedStat;

            PlayAnimation("Gesture, Override", "Fire", "Fire.playbackRate", duration);
        }

        public override void Update()
        {
            base.Update();
            // characterDirection.forward = GetAimRay().direction;

            characterBody.SetAimTimer(0.05f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration)
            {
                GetModelAnimator().SetBool("isFiring", false);
                outer.SetNextStateToMain();
                return;
            }

            GetModelAnimator().SetBool("isFiring", true);
        }

        public void FireShot()
        {
            Util.PlayAttackSpeedSound("Play_commando_M2", gameObject, 0.85f);
            Util.PlayAttackSpeedSound("Play_drone_attack", gameObject, attackSpeedStat);
            Util.PlayAttackSpeedSound("Play_drone_attack", gameObject, attackSpeedStat);

            characterBody.SetSpreadBloom(12f, true);

            var aimRay = GetAimRay();
            if (isAuthority)
            {
                var fpi = new FireProjectileInfo()
                {
                    damage = damageStat * damageCoefficient,
                    crit = RollCrit(),
                    damageColorIndex = DamageColorIndex.Default,
                    owner = gameObject,
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    //position = GetModelChildLocator().FindChild("Muzzle").position,
                    position = transform.position,
                    force = 500f,
                    procChainMask = default,
                    projectilePrefab = directCurrentProjectile,
                    damageTypeOverride = new DamageTypeCombo?(DamageTypeCombo.GenericPrimary)
                };

                ProjectileManager.instance.FireProjectile(fpi);
            }

            AddRecoil(1.2f, 1.5f, 0.3f, 0.5f);
            EffectManager.SimpleMuzzleFlash(muzzleFlash, gameObject, "Muzzle", transmit: true);
        }
    }
}