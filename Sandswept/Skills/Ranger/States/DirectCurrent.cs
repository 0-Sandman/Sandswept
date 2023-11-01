using Sandswept2.Skills.Ranger.VFX;
using System;

namespace Sandswept2.States.Ranger
{
    public class DirectCurrent : BaseState
    {
        public static float DamageCoefficient = 2f;
        public static float ShotDelay = 0.3f;
        public static float TotalDuration => ShotDelay * 2;
        public static float ProcCoefficient = 1f;
        public static GameObject TracerEffect => DirectCurrentVFX.ghostPrefab;
        private int totalHit = 0;
        private float stopwatch = 0f;
        private bool fired = false;
        private float shotDelay;
        private float duration;

        // change to fast projectile later, maybe make it slightly arc

        public override void OnEnter()
        {
            base.OnEnter();
            FireShot();

            shotDelay = ShotDelay / attackSpeedStat;
            duration = shotDelay * 2;

            PlayAnimation("Gesture, Override", "Fire", "Fire.playbackRate", shotDelay);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            base.characterDirection.forward = base.GetAimRay().direction;
            if (!fired && stopwatch >= shotDelay)
            {
                fired = true;
                FireShot();
            }

            if (fixedAge >= duration)
            {
                if (totalHit >= 2 && NetworkServer.active && characterBody.GetBuffCount(Buffs.Charged.instance.BuffDef) <= 9)
                {
                    characterBody.AddBuff(Buffs.Charged.instance.BuffDef);
                }
                /*
                if (totalHit >= 2)
                {
                    GenericSkill util = skillLocator.utility;
                    if (util && util.skillDef == Skills.Ranger.Skilldefs.Sidestep.instance.skillDef)
                    {
                        util.rechargeStopwatch += 1f;
                    }
                }
                */

                // commented this out because it incentivizes holding charge and never using m2 for more mobility

                outer.SetNextStateToMain();
            }

            base.GetModelAnimator().SetBool("isFiring", true);
        }

        public void FireShot()
        {
            // AkSoundEngine.PostEvent(Events.Play_commando_M2, base.gameObject);
            Util.PlayAttackSpeedSound("Play_commando_M2", gameObject, attackSpeedStat);
            Util.PlayAttackSpeedSound("Play_drone_attack", gameObject, attackSpeedStat);
            Util.PlayAttackSpeedSound("Play_drone_attack", gameObject, attackSpeedStat);

            if (!NetworkServer.active)
            {
                return;
            }

            BulletAttack attack = new()
            {
                aimVector = GetAimRay().direction,
                falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                damage = damageStat * DamageCoefficient,
                isCrit = RollCrit(),
                damageType = DamageType.Generic,
                owner = gameObject,
                muzzleName = "Muzzle",
                origin = GetAimRay().origin,
                tracerEffectPrefab = TracerEffect,
                procCoefficient = ProcCoefficient
            };

            attack.hitCallback = (BulletAttack attack, ref BulletAttack.BulletHit hit) =>
            {
                if (hit.hitHurtBox)
                {
                    Util.PlaySound("Play_lunar_wisp_attack1_shoot_impact", hit.hitHurtBox.gameObject);
                    totalHit++;
                }
                return BulletAttack.defaultHitCallback(attack, ref hit);
            };

            AddRecoil(0.6f, 0.8f, 0.05f, 0.1f);

            attack.Fire();
        }
    }
}