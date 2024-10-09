/*
using System;
using Sandswept.Survivors;

namespace Sandswept.Survivors.Ranger.Skilldefs
{
    public class OverdriveExit : SkillBase<OverdriveExit>
    {
        public override string Name => "Cancel";

        public override string Description => "$suAgile$se. Return your rifle to normal, $suconsume 25% heat$se and gain $rc3 Charge$ec.".AutoFormat();

        public override Type ActivationStateType => typeof(States.OverdriveExit);

        public override string ActivationMachineName => "Overdrive";

        public override float Cooldown => 2f;

        public override Sprite Icon => Main.hifuSandswept.LoadAsset<Sprite>("texOverdriveExit.png");
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;
        public override string[] Keywords => new string[] { Utils.Keywords.Agile };
        public override bool Agile => true;
        public override bool IsCombat => false;

        public override bool FullRestockOnAssign => false;

        public override void Init()
        {
            base.Init();
        }
    }
}
*/