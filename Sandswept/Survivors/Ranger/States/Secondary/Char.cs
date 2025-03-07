﻿using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Survivors.Ranger.States.Secondary
{
    public class Char : BaseState
    {
        public static float damageCoefficient = 6f;
        public static float baseDuration = 0.5f;
        private float duration;

        private GameObject charProjectile;

        public override void OnEnter()
        {
            base.OnEnter();

            var modelTransform = GetModelTransform();

            GetComponent<RangerHeatController>().currentHeat -= 50f;

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                charProjectile = skinNameToken switch
                {
                    /*
                    "SKINDEF_MAJOR" => Projectiles.DirectCurrent.prefabMajor,
                    "SKINDEF_RENEGADE" => Projectiles.DirectCurrent.prefabRenegade,
                    "SKINDEF_MILEZERO" => Projectiles.DirectCurrent.prefabMileZero,
                    "SKINDEF_SANDSWEPT" => Projectiles.DirectCurrent.prefabSandswept,
                    */
                    _ => Projectiles.TheFuckingBFG.SigmaProjectile
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
                    projectilePrefab = charProjectile,
                    damageTypeOverride = new DamageTypeCombo?(DamageTypeCombo.GenericSecondary)
                };

                ProjectileManager.instance.FireProjectile(fpi);
            }

            AddRecoil(1.2f, 1.5f, 0.3f, 0.5f);
        }
    }
}