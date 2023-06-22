using System;

namespace Sandswept.Skills.Ranger {
    public class OverdriveEnter : SkillBase<OverdriveEnter>
    {
        public override string Name => "Overdrive";

        public override string Description => "Transform your rifle into a rapid-fire machine gun. $srProne to overheating...$se".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.OverdriveEnter);

        public override string ActivationMachineName => "Overdrive";

        public override float Cooldown => 12;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("OverheatPrimary.png");
    }
}