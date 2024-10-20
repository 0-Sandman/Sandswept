using System;
using Sandswept.Survivors;
using Sandswept.Survivors.Electrician;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class SignalOverload : SkillBase<SignalOverload>
    {
        public override string Name => (UnityEngine.Random.Range(0f, 100f) >= 99f) ? "Sigma Overload" : "Signal Overload";
        public override string Description => "winds up over ~0.8s, before spinning for 3s to zap everything in a big radius, pulling things towards you and grounding aerial targets (grounding an aerial target does a bonus 50%)";
        public override Type ActivationStateType => typeof(States.SignalOverloadCharge);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 12f;
        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;
        public override string[] Keywords => null;
    }
}