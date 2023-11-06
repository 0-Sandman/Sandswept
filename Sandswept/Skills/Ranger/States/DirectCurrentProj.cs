namespace Sandswept.States.Ranger
{
    public class DirectCurrentProj : BaseState
    {
        public static float damageCoefficient = 2.5f;
        public static float baseDuration = 0.5f;
        private float duration;

        public static GameObject directCurrentProjectile = Skills.Ranger.Projectiles.DirectCurrent.prefab;

        public override void OnEnter()
        {
            base.OnEnter();
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
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration)
            {
                base.GetModelAnimator().SetBool("isFiring", false);
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

            var fpi = new FireProjectileInfo()
            {
                damage = damageStat * damageCoefficient,
                crit = RollCrit(),
                damageColorIndex = DamageColorIndex.Default,
                owner = gameObject,
                rotation = Quaternion.LookRotation(aimRay.direction),
                //position = GetModelChildLocator().FindChild("Muzzle").position,
                position = base.transform.position,
                force = 500f,
                procChainMask = default,
                projectilePrefab = directCurrentProjectile
            };

            if (isAuthority)
                ProjectileManager.instance.FireProjectile(fpi);

            AddRecoil(0.8f, 1f, 0.1f, 0.2f);
        }
    }
}