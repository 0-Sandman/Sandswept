using System;

namespace Sandswept.States.Ranger
{
    public class PewPew : BaseState
    {
        public static float DamageCoefficient = 3f;
        public static float ShotDelay = 0.3f;
        public static float TotalDuration => ShotDelay * 2;
        public static float ProcCoefficient = 1f;
        public static GameObject TracerEffect => Utils.Assets.GameObject.TracerCommandoShotgun;
        private int totalHit = 0;
        private float stopwatch = 0f;
        private bool fired = false;
        private float shotDelay;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            FireShot();

            shotDelay = ShotDelay / base.attackSpeedStat;
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

            if (base.fixedAge >= duration)
            {
                if (totalHit >= 2 && NetworkServer.active && characterBody.GetBuffCount(Buffs.Charged.instance.BuffDef) <= 10)
                {
                    characterBody.AddBuff(Buffs.Charged.instance.BuffDef);
                }

                if (totalHit >= 2)
                {
                    GenericSkill util = base.skillLocator.utility;
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
            AkSoundEngine.PostEvent(Events.Play_commando_M2, base.gameObject);

            if (!NetworkServer.active)
            {
                return;
            }

            BulletAttack attack = new();
            attack.aimVector = base.GetAimRay().direction;
            attack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
            attack.damage = base.damageStat * DamageCoefficient;
            attack.isCrit = base.RollCrit();
            attack.damageType = DamageType.Generic;
            attack.owner = base.gameObject;
            attack.muzzleName = "Muzzle";
            attack.origin = base.GetAimRay().origin;
            attack.tracerEffectPrefab = TracerEffect;
            attack.procCoefficient = ProcCoefficient;

            attack.hitCallback = (BulletAttack attack, ref BulletAttack.BulletHit hit) =>
            {
                if (hit.hitHurtBox)
                {
                    totalHit++;
                }
                return BulletAttack.defaultHitCallback(attack, ref hit);
            };

            attack.Fire();
        }
    }
}