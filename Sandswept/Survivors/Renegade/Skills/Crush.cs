using System;
using Sandswept.Survivors.Renegade.States;

namespace Sandswept.Survivors.Renegade.Skills
{
    public class Crush : SkillBase<Crush>
    {
        public override string Name => "crush";

        public override string Description => "combo an enemy in melee. does a slam if airborne.".AutoFormat();

        public override Type ActivationStateType => typeof(States.CrushCombo);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 4f;

        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override int MaxStock => 1;
        public override bool FullRestockOnAssign => false;
        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;

        public override string[] Keywords => new string[] { Utils.Keywords.OverdriveFormPrimary };

        public override void CreateSkillDef()
        {
            var steppedSkillDef = ScriptableObject.CreateInstance<DualStateSkillDef>();
            steppedSkillDef.stepGraceDuration = 0.4f;
            steppedSkillDef.stepCount = 3;
            steppedSkillDef.primaryState = new(ActivationStateType);
            steppedSkillDef.secondaryState = new(typeof(CrushAerial));
            skillDef = steppedSkillDef;
        }
    }
}