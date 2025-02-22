using System;

namespace Sandswept.Survivors.Ranger.SkillDefs.Primary
{
    public class Enflame : SkillBase<Enflame>
    {
        public override string Name => "Enflame";

        public override string Description => "Fire a rapid stream of bullets for $sd75% damage$se. $srHeat$se increases $sdfire rate$se and $sdignite chance$se but $srreduces range$se.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Primary.Enflame);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texRangerSkillIcon_hm1.png");
        public override string[] Keywords => new string[] { Utils.Keywords.Agile };
        public override bool Agile => true;
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;
    }
}