using System;

namespace Sandswept.Skills.Ranger.Skilldefs
{
    public class DirectCurrent : SkillBase<DirectCurrent>
    {
        public override string Name => "Direct Current";

        public override string Description => "Fire an arcing electric current for $sd250% damage$se. Hitting enemies increases $rcCharge$ec by $rc1$ec.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.DirectCurrentProj);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("Pew.png");
        public override int StockToConsume => 0;
    }
}