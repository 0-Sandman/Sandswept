using System;

namespace Sandswept.Skills.Ranger
{
    public class GunGoShoot : SkillBase<GunGoShoot>
    {
        public override string Name => "Gun Go Shoot";

        public override string Description => "Fire your rifle in a $suburst$se for $sd2x300% damage$se. Landing both hits will give $su1 Charge$se.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.GunGoShoot);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("Pew.png");
        public override int StockToConsume => 0;
    }
}