using System;
using RoR2.Orbs;

namespace Sandswept.Survivors.Electrician.States
{
    public class SignalOverloadCharge : BaseSkillState {
        public float baseDuration = 1.2f;

        public override void OnEnter()
        {
            base.OnEnter();

            baseDuration /= base.attackSpeedStat;

            PlayAnimation("Fullbody, Override", "WindDischarge", "Generic.playbackRate", baseDuration * 2f);

            base.characterMotor.walkSpeedPenaltyCoefficient = 0.1f;
        }

        public override void OnExit()
        {
            base.OnExit();
            base.characterMotor.walkSpeedPenaltyCoefficient = 1f;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= baseDuration) {
                outer.SetNextState(new SignalOverloadDischarge());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }

    public class SignalOverloadDischarge : BaseSkillState {
        public float duration = 3f;
        public float totalDamageCoef = 30f;
        public int totalHits = 10;
        public float delay => duration / totalHits;
        public float coeff => totalDamageCoef / totalHits;
        public float radius = 60f;
        public float stopwatch = 0f;
        public Animator animator;
        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Fullbody, Override", "EnterDischarge", "Generic.playbackRate", 0.5f);
            animator = GetModelAnimator();
            animator.SetBool("discharging", true);

            base.characterMotor.walkSpeedPenaltyCoefficient = 0.1f;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= delay) {
                stopwatch = 0f;

                HandleBlastAuthority();
            }

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            base.characterMotor.walkSpeedPenaltyCoefficient = 1f;
            animator.SetBool("discharging", false);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        public void HandleBlastAuthority()
        {
            SphereSearch search = new();
            search.radius = radius;
            search.mask = LayerIndex.entityPrecise.mask;
            search.origin = base.transform.position;
            search.RefreshCandidates();
            search.FilterCandidatesByDistinctHurtBoxEntities();
            search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(base.GetTeam()));

            foreach (HurtBox box in search.GetHurtBoxes())
            {
                LightningOrb orb = new();
                orb.attacker = base.gameObject;
                orb.damageValue = base.damageStat;
                orb.bouncesRemaining = 0;
                orb.isCrit = base.RollCrit();
                orb.lightningType = LightningOrb.LightningType.Loader;
                orb.origin = base.transform.position;
                orb.procCoefficient = 1f;
                orb.target = box;
                orb.teamIndex = base.GetTeam();
                orb.AddModdedDamageType(Electrician.Grounding);

                if (box.healthComponent) {
                    CharacterMotor motor = box.healthComponent.GetComponent<CharacterMotor>();
                    RigidbodyMotor motor2 = box.healthComponent.GetComponent<RigidbodyMotor>();
                    
                    if (motor) {
                        motor.Motor.ForceUnground();
                        motor.velocity += (base.transform.position - motor.transform.position).normalized * ((20.5f) * delay);
                    }

                    if (motor2) {
                        PhysForceInfo info = new();
                        info.massIsOne = true;
                        info.force = (base.transform.position - motor2.transform.position).normalized * (4.5f * delay);
                        motor2.ApplyForceImpulse(in info);
                    }
                }

                OrbManager.instance.AddOrb(orb);
            }
        }
    }


    public class SignalOverloadFire : BaseSkillState
    {
        public float recoilDuration = 0.8f;
        public float effectMultiplier = 1f;
        private float damageCoeff = 8f;
        private float radius = 70f;
        public SignalOverloadFire(float modifier) {
            effectMultiplier = modifier;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            recoilDuration *= effectMultiplier;
            damageCoeff *= effectMultiplier;
            radius *= effectMultiplier;

            PlayAnimation("Fullbody, Override", "Discharge", "Generic.playbackRate", recoilDuration);

            if (base.isAuthority)
            {
                HandleBlastAuthority();
            }

            EffectManager.SpawnEffect(Paths.GameObject.LoaderGroundSlam, new EffectData
            {
                origin = base.transform.position,
                scale = radius * 2f
            }, true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= recoilDuration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            characterMotor.walkSpeedPenaltyCoefficient = 1f;
        }

        public void HandleBlastAuthority()
        {
            SphereSearch search = new();
            search.radius = radius;
            search.mask = LayerIndex.entityPrecise.mask;
            search.origin = base.transform.position;
            search.RefreshCandidates();
            search.FilterCandidatesByDistinctHurtBoxEntities();
            search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(base.GetTeam()));

            foreach (HurtBox box in search.GetHurtBoxes())
            {
                LightningOrb orb = new();
                orb.attacker = base.gameObject;
                orb.damageValue = base.damageStat;
                orb.bouncesRemaining = 0;
                orb.isCrit = base.RollCrit();
                orb.lightningType = LightningOrb.LightningType.Loader;
                orb.origin = base.transform.position;
                orb.procCoefficient = 1f;
                orb.target = box;
                orb.teamIndex = base.GetTeam();
                orb.AddModdedDamageType(Electrician.Grounding);

                OrbManager.instance.AddOrb(orb);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}