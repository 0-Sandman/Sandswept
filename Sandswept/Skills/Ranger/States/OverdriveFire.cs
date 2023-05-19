using System;

namespace Sandswept.States.Ranger {
    public class OverdriveFire : BaseState {
        public static int ShotsPerSecond = 5;
        public static float DamageCoeff = 2f;
        public static float SelfDamageCoeff = 0.1f;
        public static float ProcCoeff = 1f;
        public static int HeatPerSecond = 20;
        public static GameObject TracerEffect => Utils.Assets.GameObject.TracerCommandoShotgun;

        private float shots;
        private float shotDelay => 1f / shots;
        private float stopwatch = 0f;

        private GenericSkill heat;

        public override void OnEnter()
        {
            base.OnEnter();
            heat = base.skillLocator.secondary;
            shots = ShotsPerSecond * base.attackSpeedStat;
        }

        public void Exit() {
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

            if (base.inputBank.skill1.down && stopwatch >= shotDelay) {
                stopwatch = 0f;
                FireShot();
            }

            base.fixedAge += Time.fixedDeltaTime;
            if (base.fixedAge >= (1f / HeatPerSecond)) {
                base.fixedAge = 0f;
                if (base.inputBank.skill1.down) {
                    heat.stock += 1;
                    if (heat.stock > 200) {
                        heat.stock = 200;
                    }
                }
                else {
                    heat.stock -= 1;
                    if (heat.stock < 0) {
                        heat.stock = 0;
                    }
                }
            }
        }

        public void FireShot() {
            AkSoundEngine.PostEvent(Events.Play_commando_M2, base.gameObject);

            if (heat.stock > 100) {
                DamageInfo info = new();
                info.attacker = base.gameObject;
                info.procCoefficient = 0;
                info.damage = base.damageStat * SelfDamageCoeff;
                info.crit = false;
                info.position = base.transform.position;
                
                if (NetworkServer.active) {
                    healthComponent.TakeDamage(info);
                }

                AkSoundEngine.PostEvent(Events.Play_item_proc_igniteOnKill, base.gameObject);
            }

            if (!base.isAuthority) {
                return;
            }

            BulletAttack attack = new();
            attack.aimVector = base.GetAimRay().direction;
            attack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
            attack.damage = base.damageStat * DamageCoeff;
            attack.isCrit = base.RollCrit();
            attack.damageType = DamageType.Generic;
            attack.owner = base.gameObject;
            attack.muzzleName = "MuzzleR";
            attack.origin = base.GetAimRay().origin;
            attack.tracerEffectPrefab = TracerEffect;
            attack.procCoefficient = ProcCoeff;

            attack.Fire();
        }
    }
}