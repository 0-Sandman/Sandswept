using System;
using R2API.Utils;
using RoR2.CharacterAI;
using System.Linq;

namespace Sandswept.Enemies.CannonballJellyfish.States
{
    [ConfigSection("Enemies :: Cannonball Jellyfish")]
    public class JellyDash : BaseState
    {
        [ConfigField("Primary Damage Coefficient", "Decimal.", 4f)]
        public static float DamageCoefficient;

        public static float DashForce = 50f;

        //
        private float maxStallDur = 1f;

        private OverlapAttack attack;
        private float duration = 0.5f;
        private bool dashedAlready = false;
        private BaseAI ai;
        private Vector3 lockVector;

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
                hitBoxGroup = base.FindHitBoxGroup("Impact"),
                procCoefficient = 1f,
                teamIndex = base.GetTeam(),
                attackerFiltering = AttackerFiltering.NeverHitSelf
            };

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
                Util.PlaySound("Play_AntlerShield_Pickup", gameObject);
                Util.PlaySound("Play_AntlerShield_Pickup", gameObject);

                dashedAlready = true;

                BaseAI ai = base.characterBody.master.GetComponent<BaseAI>();

                if (ai && ai.currentEnemy.gameObject)
                {
                    float dist = Vector3.Distance(base.transform.position, ai.currentEnemy.gameObject.transform.position);
                    DashForce = dist * 2f;
                }

                PhysForceInfo info = new();
                info.force = base.inputBank.aimDirection * DashForce;
                info.massIsOne = true;
                info.disableAirControlUntilCollision = false;

                base.rigidbodyMotor.ApplyForceImpulse(in info);

                base.FindModelChild("Spray").GetComponent<ParticleSystem>().Play();

                lockVector = base.transform.forward;
                rigidbodyDirection.freezeXRotation = true;
                rigidbodyDirection.freezeYRotation = true;
                rigidbodyDirection.freezeZRotation = true;
            }

            if (dashedAlready && base.isAuthority)
            {
                if (attack.Fire())
                {
                    Util.PlaySound("Play_bison_headbutt_attack_hit", gameObject);
                }

                base.transform.forward = lockVector;
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
            rigidbodyDirection.freezeXRotation = false;
            rigidbodyDirection.freezeYRotation = false;
            rigidbodyDirection.freezeZRotation = false;
            base.rigidbodyMotor.rootMotion = Vector3.zero;
            base.rigidbody.velocity = Vector3.zero;
        }
    }
}