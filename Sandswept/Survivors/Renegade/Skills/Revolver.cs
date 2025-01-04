using System;

namespace Sandswept.Survivors.Renegade.Skills
{
    public class Revolver : SkillBase<Revolver>
    {
        public override string Name => "pew pew";

        public override string Description => "shoot a revolver for some damage. the last round shatters into shrapnel that inflicts <some debuff that reduces gravity>".AutoFormat();

        public override Type ActivationStateType => typeof(States.Revolver);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override int MaxStock => 4;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;

        public override string[] Keywords => new string[] { Utils.Keywords.OverdriveFormPrimary };

        public override void CreateSkillDef()
        {
            skillDef = ScriptableObject.CreateInstance<AmmoSkillDef>();
        }
    }
}