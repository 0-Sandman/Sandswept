using System;

namespace Sandswept.Survivors.Renegade.Skills {
    public class Slam : SkillBase<Slam>
    {
        public override string Name => "slam";

        public override string Description => "oh my gyatt".AutoFormat();

        public override Type ActivationStateType => typeof(States.Slam);

        public override string ActivationMachineName => "Body";

        public override float Cooldown => 10f;

        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override int MaxStock => 1;
        public override bool FullRestockOnAssign => false;
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;

        public override string[] Keywords => new string[] { Utils.Keywords.OverdriveFormPrimary };
    }
}