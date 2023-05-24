using System;

namespace Sandswept.Skills.Ranger {
    public class Heat : SkillBase<Heat>
    {
        public override string Name => "Heat";

        public override string Description => "The current heat of your rifle.";

        public override Type ActivationStateType => typeof(Idle);

        public override string ActivationMachineName => "Sidestep";

        public override float Cooldown => 20000;

        public override Sprite Icon => null;
        public override int StockToConsume => 10000;
        public override int MaxStock => 200;

        public override void CreateSkillDef()
        {
            skillDef = ScriptableObject.CreateInstance<HeatSkillDef>();
        }
    }
}