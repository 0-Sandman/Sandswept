using System;
using Sandswept.Survivors;
using Sandswept.Survivors.Electrician;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class SignalOverload : SkillBase<SignalOverload>
    {
        public override string Name => "Signal Overload";
        public override string Description => "Overload yo <style=cIsDamage>balls</style>.";
        public override Type ActivationStateType => typeof(EntityStates.Idle);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 0f;
        public override Sprite Icon => null;
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;
        public override string[] Keywords => null;
    }
}