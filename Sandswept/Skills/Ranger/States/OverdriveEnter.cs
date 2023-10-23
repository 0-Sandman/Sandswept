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

            SkillLocator locator = skillLocator;
            locator.primary.SetSkillOverride(gameObject, PrimarySkill, GenericSkill.SkillOverridePriority.Contextual);
            // locator.utility.SetSkillOverride(base.gameObject, NullSkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.SetSkillOverride(gameObject, CancelSkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.DeductStock(1);

            PlayAnimation("Gesture, Override", "EnterOverdrive");
            Util.PlaySound("Play_item_use_BFG_charge", gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();

            SkillLocator locator = skillLocator;
            locator.primary.UnsetSkillOverride(gameObject, PrimarySkill, GenericSkill.SkillOverridePriority.Contextual);
            // locator.utility.UnsetSkillOverride(base.gameObject, NullSkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.UnsetSkillOverride(gameObject, CancelSkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.DeductStock(1);

            PlayAnimation("Gesture, Override", "ExitOverdrive");
            Util.PlaySound("Play_lunar_wisp_attack2_windDown", gameObject);
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
            EntityStateMachine machine = EntityStateMachine.FindByCustomName(gameObject, "Overdrive");
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