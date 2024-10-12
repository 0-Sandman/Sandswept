using System;
using RoR2.Orbs;

namespace Sandswept.Survivors.Electrician.States
{
    public class SignalOverloadCharge : BaseSkillState
    {
        public float chargeTime = 2f;
        public float movePenalty = 0.5f;
        public float modifier = 1f;

        public SignalOverloadCharge(float effectMultiplier)
        {
            modifier = effectMultiplier;
        }

        public SignalOverloadCharge()
        { }

        public override void OnEnter()
        {
            base.OnEnter();
            characterMotor.walkSpeedPenaltyCoefficient = (1f + movePenalty) - modifier;
            chargeTime *= modifier;

            PlayAnimation("Fullbody, Override", "ChargeOverload", "Generic.playbackRate", chargeTime * 2.2f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= chargeTime)
            {
                outer.SetNextState(new SignalOverloadFire(modifier));
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
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