using System;

namespace Sandswept.Skills.Ranger {
    public class OverdriveFire : SkillBase<OverdriveFire>
    {
        public override string Name => "Gun Go Brrrrr";

        public override string Description => "Fire a rapid stream of $sulasers$se for $sd200% damage$se. $srContinous fire will cause overheating, damaging you.$se".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.OverdriveFire);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("Pew.png");
        public override int StockToConsume => 0;
    }
}