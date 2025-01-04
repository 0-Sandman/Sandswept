using System;

namespace Sandswept.Survivors.Renegade.Skills {
    public class Charge : SkillBase<Charge>
    {
        public override string Name => "charge";

        public override string Description => "oh my gyatt".AutoFormat();

        public override Type ActivationStateType => typeof(States.Charge);

        public override string ActivationMachineName => "Body";

        public override float Cooldown => 6f;

        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override int MaxStock => 1;
        public override bool FullRestockOnAssign => false;
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;

        public override string[] Keywords => new string[] { Utils.Keywords.OverdriveFormPrimary };
    }
}