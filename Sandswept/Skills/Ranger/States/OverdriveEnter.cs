using System.Text.RegularExpressions;
using System;

namespace Sandswept.States.Ranger
{
    public class OverdriveEnter : BaseState
    {
        public static SkillDef PrimarySkill => Skills.Ranger.OverdriveFire.instance.skillDef;
        public static SkillDef CancelSkill => Skills.Ranger.OverdriveExit.instance.skillDef;

        public override void OnEnter()
        {
            base.OnEnter();

            SkillLocator locator = base.skillLocator;
            locator.primary.SetSkillOverride(base.gameObject, PrimarySkill, GenericSkill.SkillOverridePriority.Contextual);
            // locator.utility.SetSkillOverride(base.gameObject, NullSkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.SetSkillOverride(base.gameObject, CancelSkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.DeductStock(1);
        }

        public override void OnExit()
        {
            base.OnExit();

            SkillLocator locator = base.skillLocator;
            locator.primary.UnsetSkillOverride(base.gameObject, PrimarySkill, GenericSkill.SkillOverridePriority.Contextual);
            // locator.utility.UnsetSkillOverride(base.gameObject, NullSkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.UnsetSkillOverride(base.gameObject, CancelSkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.DeductStock(1);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public void Exit()
        {
            outer.SetNextStateToMain();
        }
    }

    public class OverdriveExit : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            EntityStateMachine machine = EntityStateMachine.FindByCustomName(base.gameObject, "Overdrive");
            if (machine.state is OverdriveEnter)
            {
                (machine.state as OverdriveEnter).Exit();
            }
            outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}