using System;
using Sandswept.Survivors;

namespace Sandswept.Enemies.OmicronConstruct.Skills {
    public class CommandSlam : SkillBase<CommandSlam>
    {
        public override string Name => "";

        public override string Description => "";

        public override Type ActivationStateType => typeof(States.CommandSlam);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 6f;

        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override bool BeginCooldownOnSkillEnd => true;
        public override bool CanceledFromSprinting => false;
    }
}