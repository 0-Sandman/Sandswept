using System;

namespace Sandswept.Skills.Ranger {
    public class OverdriveExit : SkillBase<OverdriveExit>
    {
        public override string Name => "Cancel";

        public override string Description => "Return your rifle to normal.";

        public override Type ActivationStateType => typeof(States.Ranger.OverdriveExit);

        public override string ActivationMachineName => "Overdrive";

        public override float Cooldown => 2;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("OverheatPrimary.png");
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;

        public override void Init()
        {
            base.Init();
        }
    }
}