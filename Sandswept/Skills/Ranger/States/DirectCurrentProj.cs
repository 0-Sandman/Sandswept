using Sandswept.Skills.Ranger.VFX;

namespace Sandswept.States.Ranger
{
    public class DirectCurrentProj : BaseState
    {
        public static float damageCoefficient = 3.5f;
        public static float baseDuration = 0.5f;
        public static GameObject GhostEffect => DirectCurrentVFX.ghostPrefab;
        private float duration;
        private float stopwatch = 0f;

        public static GameObject directCurrentProjectile = Skills.Ranger.Projectiles.DirectCurrent.prefab;

        public override void OnEnter()
        {
            base.OnEnter();
            FireShot();

            duration = baseDuration / attackSpeedStat;

            PlayAnimation("Gesture, Override", "Fire", "Fire.playbackRate", duration);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            characterDirection.forward = GetAimRay().direction;

            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }

            GetModelAnimator().SetBool("isFiring", true);
        }

        public void FireShot()
        {
            Util.PlayAttackSpeedSound("Play_commando_M2", gameObject, attackSpeedStat);
            Util.PlayAttackSpeedSound("Play_drone_attack", gameObject, attackSpeedStat);
            Util.PlayAttackSpeedSound("Play_drone_attack", gameObject, attackSpeedStat);

            var aimRay = GetAimRay();

            var fpi = new FireProjectileInfo()
            {
                damage = damageStat * damageCoefficient,
                crit = RollCrit(),
                damageColorIndex = DamageColorIndex.Default,
                owner = gameObject,
                rotation = Quaternion.LookRotation(aimRay.direction),
                position = aimRay.origin,
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