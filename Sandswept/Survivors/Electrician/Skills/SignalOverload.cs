using System;
using Sandswept.Survivors;
using Sandswept.Survivors.Electrician;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class SignalOverload : SkillBase<SignalOverload>
    {
        public override string Name => "Signal Overload";
        public override string Description => "Overload yo <style=cIsDamage>balls</style>.";
        public override Type ActivationStateType => typeof(States.SignalOverloadCharge);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 12f;
        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;
        public override string[] Keywords => null;
    }
}