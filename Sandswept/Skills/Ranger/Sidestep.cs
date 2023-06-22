using System;

namespace Sandswept.Skills.Ranger {
    public class Sidestep : SkillBase<Sidestep>
    {
        public override string Name => "Sidestep";

        public override string Description => "$suCharged$se. Do a quick sidestep in a direction. $sdShots taken immediately after deal bonus damage$se.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.Sidestep);

        public override string ActivationMachineName => "Dash";

        public override float Cooldown => 5;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("Dash.png");
        public override string[] Keywords => new string[] { Utils.Keywords.Charged };
        public override bool Agile => true;
    }
}