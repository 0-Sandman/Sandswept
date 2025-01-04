using System;

namespace Sandswept.Survivors.Renegade.States {
    public class Slam : BaseSkillState {
        public float damageCoeff = 19f;
        public float duration = 10f;
        public float radius = 14f;
        public float force = 300f;
        public float upForce = 40f;
        public float velocity = 150f;
        private bool impacted = false;
        private Vector3 dir;
        public static LazyAddressable<GameObject> BlastEffect = new(() => Paths.GameObject.LoaderGroundSlam);

        public override void OnEnter()
        {
            base.OnEnter();
            base.characterMotor.onMovementHit += OnMovementHit;
            dir = Vector3.down * velocity;
            base.characterMotor.velocity = Vector3.up * 150f;
            base.characterMotor.Motor.ForceUnground();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= 0.1f && base.fixedAge <= 0.35f) {
                base.characterMotor.velocity = dir;
            }
            else {
                if (base.characterMotor.velocity.y < 0f && base.fixedAge <= 0.35f) {
                    base.characterMotor.velocity.y = 0f;
                }
            }


            if ((impacted || base.characterMotor.Motor.GroundingStatus.IsStableOnGround) && base.fixedAge >= 0.14f) {
                OnImpact();
                outer.SetNextStateToMain();
            }

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
        }

        public void OnMovementHit(ref CharacterMotor.MovementHitInfo info) {
            if (base.fixedAge <= 0.1f) return;

            impacted = true;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public void OnImpact() {
            BlastAttack attack = new();
            attack.radius = radius;
            attack.baseDamage = base.damageStat * damageCoeff;
            attack.crit = base.RollCrit();
            attack.baseForce = force;
            attack.bonusForce = upForce * Vector3.up;
            attack.position = base.transform.position;
            attack.teamIndex = base.GetTeam();
            attack.procCoefficient = 1f;
            attack.attacker = base.gameObject;
            attack.damageType = DamageType.Stun1s;
            attack.falloffModel = BlastAttack.FalloffModel.None;
            
            BlastAttack.Result result = attack.Fire();
            
            List<HurtBox> boxes = new();

            foreach (var res in result.hitPoints) {
                if (res.hurtBox) boxes.Add(res.hurtBox);
            }

            RenegadeComboController combo = GetComponent<RenegadeComboController>();
            
            if (combo.HitTargets(boxes.ToArray())) {
                combo.UpdateCombo(DamageSource.Hazard);
            }

            EffectManager.SpawnEffect(BlastEffect, new EffectData {
                origin = attack.position,
                scale = attack.radius * 2f,
            }, true);

            base.characterMotor.velocity = Vector3.zero;
        }

        public override void OnExit()
        {
            base.OnExit();
            base.characterMotor.onMovementHit -= OnMovementHit;
        }
    }
}