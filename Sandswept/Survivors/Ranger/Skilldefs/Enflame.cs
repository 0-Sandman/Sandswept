using System;
using Sandswept.Survivors;

namespace Sandswept.Survivors.Ranger.Skilldefs
{
    public class Enflame : SkillBase<Enflame>
    {
        public override string Name => "Enflame";

        public override string Description => "$suAgile$se. Fire a rapid stream of bullets for $sd90% damage$se. $srHeat$se increases $sdignite chance$se and $srspread$se.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Enflame);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Main.hifuSandswept.LoadAsset<Sprite>("texOverdriveFire.png");
        public override string[] Keywords => new string[] { Utils.Keywords.Agile };
        public override bool Agile => true;
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;
    }
}