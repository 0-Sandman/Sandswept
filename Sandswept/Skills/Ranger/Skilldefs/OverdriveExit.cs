using System;

namespace Sandswept.Skills.Ranger.Skilldefs
{
    public class OverdriveExit : SkillBase<OverdriveExit>
    {
        public override string Name => "Cancel";

        public override string Description => "$suAgile$se. Return your rifle to normal.";

        public override Type ActivationStateType => typeof(States.Ranger.OverdriveExit);

        public override string ActivationMachineName => "Overdrive";

        public override float Cooldown => 2f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("OverheatPrimary.png");
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;
        public override string[] Keywords => new string[] { Utils.Keywords.Agile };
        public override bool Agile => true;

        public override void Init()
        {
            base.Init();
        }
    }
}