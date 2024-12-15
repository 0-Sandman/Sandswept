using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Survivors.Ranger.States.Primary
{
    public class DirectCurrent : BaseState
    {
        public static float damageCoefficient = 2.5f;
        public static float baseDuration = 0.5f;
        private float duration;

        private GameObject directCurrentProjectile;

        public override void OnEnter()
        {
            base.OnEnter();

            var modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                directCurrentProjectile = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => Projectiles.DirectCurrent.prefabMajor,
                    "SKINDEF_RENEGADE" => Projectiles.DirectCurrent.prefabRenegade,
                    "SKINDEF_MILEZERO" => Projectiles.DirectCurrent.prefabMileZero,
                    "SKINDEF_SANDSWEPT" => Projectiles.DirectCurrent.prefabSandswept,
                    _ => Projectiles.DirectCurrent.prefabDefault,
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
        }
    }
}