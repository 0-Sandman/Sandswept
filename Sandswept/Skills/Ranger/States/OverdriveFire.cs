using System;

namespace Sandswept.States.Ranger
{
    public class OverdriveFire : BaseState
    {
        public static int ShotsPerSecond = 5;
        public static float DamageCoeff = 2f;
        public static float SelfDamageCoeff = 0.1f;
        public static float ProcCoeff = 1f;
        public static GameObject TracerEffect => Utils.Assets.GameObject.TracerCommandoShotgun;
        private float shots;
        private float shotDelay => 1f / shots;
        private float stopwatch = 0f;
        private Components.RangerHeatManager heat;

        public override void OnEnter()
        {
            base.OnEnter();
            shots = ShotsPerSecond * base.attackSpeedStat;
            heat = GetComponent<Components.RangerHeatManager>();
            heat.isFiring = true;
        }

        public void Exit()
        {
            heat.isFiring = false;
            outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;

            if (base.inputBank.skill1.down && stopwatch >= shotDelay)
            {
                stopwatch = 0f;
                FireShot();
            }

            if (!base.inputBank.skill1.down)
            {
                Exit();
            }
        }

        public void FireShot()
        {
            AkSoundEngine.PostEvent(Events.Play_commando_M2, base.gameObject);

            if (heat.IsOverheating)
            {
                DamageInfo info = new();
                info.attacker = base.gameObject;
                info.procCoefficient = 0;
                info.damage = base.damageStat * SelfDamageCoeff;
                info.crit = false;
                info.position = base.transform.position;

                if (NetworkServer.active)
                {
                    healthComponent.TakeDamage(info);
                }

                AkSoundEngine.PostEvent(Events.Play_item_proc_igniteOnKill, base.gameObject);
            }

            PlayAnimation("Gesture, Override", "OverdriveFire");

            if (!base.isAuthority)
            {
                return;
            }

            BulletAttack attack = new();
            attack.aimVector = base.GetAimRay().direction;
            attack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
            attack.damage = base.damageStat * DamageCoeff;
            attack.isCrit = base.RollCrit();
            attack.damageType = DamageType.Generic;
            attack.owner = base.gameObject;
            attack.muzzleName = "Muzzle";
            attack.origin = base.GetAimRay().origin;
            attack.tracerEffectPrefab = TracerEffect;
            attack.procCoefficient = ProcCoeff;

            attack.Fire();
        }
    }
}