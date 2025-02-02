using System;

namespace Sandswept.Survivors.Megalomaniac.Skills
{
    public class Shatter : SkillBase<Shatter>
    {
        public override string Name => "Shatter";
        public override string Description => "Fire alternating bomblets that stick in before exploding for <style=cIsDamage>200% damage</style> after a short delay.";
        public override Type ActivationStateType => typeof(States.ShatterWind);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 0f;
        public override Sprite Icon => null;
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;
        public override string[] Keywords => null;
    }
}