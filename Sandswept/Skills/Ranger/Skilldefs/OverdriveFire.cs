using System;

namespace Sandswept.Skills.Ranger.Skilldefs
{
    public class OverdriveFire : SkillBase<OverdriveFire>
    {
        public override string Name => "Overdrive";

        public override string Description => "$suAgile$se. Fire a rapid stream of $sdbullets$se for $sd200% damage$se. $srContinous fire will cause overheating, increasing spread, ignite chance and damaging you.$se".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.OverdriveFire);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("Pew.png");
        public override string[] Keywords => new string[] { Utils.Keywords.Agile };
        public override bool Agile => true;
        public override int StockToConsume => 0;
    }
}