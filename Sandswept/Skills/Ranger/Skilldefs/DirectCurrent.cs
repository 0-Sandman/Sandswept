using System;

namespace Sandswept2.Skills.Ranger.Skilldefs
{
    public class DirectCurrent : SkillBase<DirectCurrent>
    {
        public override string Name => "Direct Current";

        public override string Description => "Fire an arcing $sdelectric current$se for $sd280% damage$se. Increases $rcCharge$ec by $rc1$ec on hit.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.DirectCurrentProj);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("Pew.png");
        public override int StockToConsume => 0;
    }
}