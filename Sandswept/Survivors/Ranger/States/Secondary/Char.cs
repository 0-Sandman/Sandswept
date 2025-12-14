using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Survivors.Ranger.States.Secondary
{
    public class Char : BaseState
    {
        public static float damageCoefficient = 2f;
        public static float baseDuration = 0.5f;
        public static float heatReduction = 0.5f;
        private float duration;
        public float finalDamageCoefficient;

        private GameObject charProjectile;
        private GameObject muzzleFlash;
        public RangerHeatController rangerHeatController;

        public override void OnEnter()
        {
            base.OnEnter();

            var modelTransform = GetModelTransform();

            rangerHeatController = GetComponent<RangerHeatController>();
            float heatRemoved = rangerHeatController.currentHeat * heatReduction;
            rangerHeatController.currentHeat -= Mathf.Max(0, heatRemoved);

            finalDamageCoefficient = damageCoefficient + (heatRemoved / 25f);

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                charProjectile = skinNameToken switch
                {
                    /*
                    "RANGER_SKIN_MAJOR_NAME" => Projectiles.DirectCurrent.prefabMajor,
                    "RANGER_SKIN_RENEGADE_NAME" => Projectiles.DirectCurrent.prefabRenegade,
                    "RANGER_SKIN_MILEZERO_NAME" => Projectiles.DirectCurrent.prefabMileZero,
                    "RANGER_SKIN_SANDSWEPT_NAME" => Projectiles.DirectCurrent.prefabSandswept,
                    */
                    _ => Projectiles.AltSecondaries.charBallProjectile
                };

                muzzleFlash = skinNameToken switch
                {
                    "RANGER_SKIN_MAJOR_NAME" => DirectCurrentVFX.muzzleFlashPrefabMajorOverdrive,
                    "RANGER_SKIN_RENEGADE_NAME" => DirectCurrentVFX.muzzleFlashPrefabRenegadeOverdrive,
                    "RANGER_SKIN_MILEZERO_NAME" => DirectCurrentVFX.muzzleFlashPrefabMileZeroOverdrive,
                    "RANGER_SKIN_SANDSWEPT_NAME" => DirectCurrentVFX.muzzleFlashPrefabSandsweptOverdrive,
                    _ => DirectCurrentVFX.muzzleFlashPrefabDefaultOverdrive
                };

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
                    damage = damageStat * finalDamageCoefficient,
                    crit = RollCrit(),
                    damageColorIndex = DamageColorIndex.Default,
                    owner = gameObject,
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    //position = GetModelChildLocator().FindChild("Muzzle").position,
                    position = transform.position,
                    force = 500f,
                    procChainMask = default,
                    projectilePrefab = charProjectile,
                    damageTypeOverride = new DamageTypeCombo?(DamageTypeCombo.GenericSecondary)
                };

                ProjectileManager.instance.FireProjectile(fpi);
            }

            AddRecoil(1.2f, 1.5f, 0.3f, 0.5f);

            EffectManager.SimpleMuzzleFlash(muzzleFlash, gameObject, "Muzzle", transmit: true);
        }
    }
}