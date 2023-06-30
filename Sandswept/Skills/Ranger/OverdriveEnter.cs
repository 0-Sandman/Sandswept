using System;

namespace Sandswept.Skills.Ranger
{
    public class OverdriveEnter : SkillBase<OverdriveEnter>
    {
        public override string Name => "Overdrive";

        public override string Description => "Transform your rifle into a $serapid-fire machine gun$se. $srProne to overheating...$se".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.OverdriveEnter);

        public override string ActivationMachineName => "Overdrive";

        public override float Cooldown => 12f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("OverheatPrimary.png");
    }
}