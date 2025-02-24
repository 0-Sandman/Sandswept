using System;
using Sandswept.Survivors;

namespace Sandswept.Enemies.StonePillar.Skills {
    public class Slam : SkillBase<Slam>
    {
        public override string Name => "";

        public override string Description => "";

        public override Type ActivationStateType => typeof(States.Slam);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 1f;

        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override bool BeginCooldownOnSkillEnd => true;
        public override bool CanceledFromSprinting => false;
    }
}