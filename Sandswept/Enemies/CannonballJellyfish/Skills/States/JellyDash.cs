using System;
using R2API.Utils;
using RoR2.CharacterAI;
using System.Linq;

namespace Sandswept.Enemies.CannonballJellyfish.States
{
    [ConfigSection("Enemies :: Cannonball Jellyfish")]
    public class JellyDash : BaseState
    {
        [ConfigField("Primary - Damage Coefficient", "Decimal.", 4f)]
        public static float DamageCoefficient;

        [ConfigField("Primary - Dash Force", "The amount of force to use when dashing", 4000f)]
        public static float DashForce;

        //
        private float maxStallDur = 1f;

        private OverlapAttack attack;
        private float duration = 0.5f;
        private bool dashedAlready = false;
        private BaseAI ai;

        public override void OnEnter()
        {
            base.OnEnter();
            duration += maxStallDur;

            if (base.characterBody.master)
            {
                ai = base.characterBody.master.GetComponent<BaseAI>();
            }

            attack = new()
            {
                attacker = base.gameObject,
                damage = base.damageStat * DamageCoefficient,
                isCrit = base.RollCrit(),
                hitBoxGroup = base.FindHitBoxGroup("HBCharge"),
                procCoefficient = 1f,
                teamIndex = base.GetTeam(),
                attackerFiltering = AttackerFiltering.NeverHitSelf
            };

            if (NetworkServer.active)
            {
                characterBody.AddBuff(RoR2Content.Buffs.Immune);
            }

            FlipComponents();
        }

        public void SetDir()
        {
            base.characterBody.SetAimTimer(0.02f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            SetDir();

            if (duration >= maxStallDur && !dashedAlready && base.isAuthority)
            {
                dashedAlready = true;

                PhysForceInfo info = new();
                info.force = base.inputBank.aimDirection * DashForce;
                info.disableAirControlUntilCollision = false;

                base.rigidbodyMotor.ApplyForceImpulse(in info);
            }

            if (dashedAlready && base.isAuthority)
            {
                attack.Fire();
            }

            if (base.fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public void FlipComponents()
        {
            VectorPID[] vPids = base.gameObject.GetComponents<VectorPID>();
            QuaternionPID[] qPids = base.gameObject.GetComponents<QuaternionPID>();

            vPids.ToList().ForEach(x => x.enabled = !x.enabled);
            qPids.ToList().ForEach(x => x.enabled = !x.enabled);

            base.rigidbodyMotor.rootMotion = Vector3.zero;
        }

        public override void OnExit()
        {
            base.OnExit();
            FlipComponents();
            base.rigidbodyMotor.rootMotion = Vector3.zero;
            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(RoR2Content.Buffs.Immune);
            }
        }
    }
}