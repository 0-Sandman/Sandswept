using System;

namespace Sandswept.States.Ranger
{
    public class Sidestep : BaseState
    {
        public static float Duration = 0.1f;
        public static float BuffDuration = 1f;
        public static float SpeedCoefficient = 5f;
        private Vector3 stepVector;

        public override void OnEnter()
        {
            base.OnEnter();
            stepVector = (base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.characterMotor && base.characterDirection)
            {
                base.characterMotor.velocity = Vector3.zero;
                base.characterMotor.rootMotion += stepVector * (base.moveSpeedStat * SpeedCoefficient * Time.fixedDeltaTime);
            }

            if (base.fixedAge >= Duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (base.isAuthority)
            {
                base.characterBody.AddTimedBuffAuthority(Buffs.SidestepBuff.instance.BuffDef.buffIndex, BuffDuration);
            }
        }
    }
}