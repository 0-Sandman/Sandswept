using System;

namespace Sandswept.Skills.Ranger.Skilldefs
{
    public class DirectCurrent : SkillBase<DirectCurrent>
    {
        public override string Name => "Direct Current";

        public override string Description => "Fire your rifle in a $suburst$se for $sd2x200% damage$se. Landing both hits will give $rc1 Charge$ec.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.DirectCurrent);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("Pew.png");
        public override int StockToConsume => 0;
    }
}