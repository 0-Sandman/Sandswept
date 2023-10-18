using Sandswept.Skills.Ranger.VFX;
using System;

namespace Sandswept.States.Ranger
{
    public class GunGoShoot : BaseState
    {
        public static float DamageCoefficient = 3f;
        public static float ShotDelay = 0.3f;
        public static float TotalDuration => ShotDelay * 2;
        public static float ProcCoefficient = 1f;
        public static GameObject TracerEffect => GunGoShootVFX.tracerPrefab;
        private int totalHit = 0;
        private float stopwatch = 0f;
        private bool fired = false;
        private float shotDelay;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            FireShot();

            shotDelay = ShotDelay / attackSpeedStat;
            duration = shotDelay * 2;

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
            if (!fired && stopwatch >= shotDelay)
            {
                fired = true;
                FireShot();
            }

            if (fixedAge >= duration)
            {
                if (totalHit >= 2 && NetworkServer.active && characterBody.GetBuffCount(Buffs.Charged.instance.BuffDef) <= 10)
                {
                    characterBody.AddBuff(Buffs.Charged.instance.BuffDef);
                }

                if (totalHit >= 2)
                {
                    GenericSkill util = skillLocator.utility;
                    if (util && util.skillDef == Skills.Ranger.Sidestep.instance.skillDef)
                    {
                        util.rechargeStopwatch += 1f;
                    }
                }

                outer.SetNextStateToMain();
            }
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

            BulletAttack attack = new();
            attack.aimVector = GetAimRay().direction;
            attack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
            attack.damage = damageStat * DamageCoefficient;
            attack.isCrit = RollCrit();
            attack.damageType = DamageType.Generic;
            attack.owner = gameObject;
            attack.muzzleName = "Muzzle";
            attack.origin = GetAimRay().origin;
            attack.tracerEffectPrefab = TracerEffect;
            attack.procCoefficient = ProcCoefficient;

            attack.hitCallback = (BulletAttack attack, ref BulletAttack.BulletHit hit) =>
            {
                if (hit.hitHurtBox)
                {
                    Util.PlaySound("Play_lunar_wisp_attack1_shoot_impact", hit.hitHurtBox.gameObject);
                    totalHit++;
                }
                return BulletAttack.defaultHitCallback(attack, ref hit);
            };

            attack.Fire();
        }
    }
}