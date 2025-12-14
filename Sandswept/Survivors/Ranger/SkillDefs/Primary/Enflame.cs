using System;

namespace Sandswept.Survivors.Ranger.SkillDefs.Primary
{
    public class Enflame : SkillBase<Enflame>
    {
        public override string Name => "Enflame";

        public override string Description => "$suAgile$se. Fire a rapid stream of bullets for $sd90% damage$se. $suFire rate and ignite chance increase with heat$se.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Primary.Enflame);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texRangerSkillIcon_hm1.png");
        public override string[] Keywords => [Utils.Keywords.Agile];
        public override bool Agile => false;
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;
        public override float GetProcCoefficientData() => 1f;
    }
}