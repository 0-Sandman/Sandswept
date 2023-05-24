using System;

namespace Sandswept.Skills.Ranger {
    public class PewPew : SkillBase<PewPew>
    {
        public override string Name => "Gun Go Shoot";

        public override string Description => "Fire your rifle in a $suburst$se for $sd2x300% damage$se. Landing both hits will give a stack of $suCharge$se".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.PewPew);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => null;
        public override int StockToConsume => 0;
    }
}