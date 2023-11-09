using System;

namespace Sandswept.Skills.Ranger.Skilldefs
{
    public class Enflame : SkillBase<Enflame>
    {
        public override string Name => "Enflame";

        public override string Description => "$suAgile$se. Fire a rapid stream of bullets for $sd120% damage$se. $srContinuous fire increases heat, spread, and ignite chance$se.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.Enflame);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texOverdriveFire.png");
        public override string[] Keywords => new string[] { Utils.Keywords.Agile };
        public override bool Agile => true;
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;
    }
}