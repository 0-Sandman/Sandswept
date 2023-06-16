using System.Text.RegularExpressions;
using System;

namespace Sandswept.States.Ranger {
    public class OverdriveEnter : BaseState {
        public static SkillDef NullSkill => Utils.Assets.SkillDef.CaptainSkillDisconnected;
        public static SkillDef HeatSkill => Skills.Ranger.Heat.instance.skillDef;
        public static SkillDef PrimarySkill => Skills.Ranger.OverdriveFire.instance.skillDef;
        public static SkillDef CancelSkill => Skills.Ranger.OverdriveExit.instance.skillDef;
        public override void OnEnter()
        {
            base.OnEnter();

            SkillLocator locator = base.skillLocator;
            locator.primary.SetSkillOverride(base.gameObject, PrimarySkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.secondary.SetSkillOverride(base.gameObject, HeatSkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.secondary.stock = 0;
            // locator.utility.SetSkillOverride(base.gameObject, NullSkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.SetSkillOverride(base.gameObject, CancelSkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.DeductStock(1);
        }

        public override void OnExit()
        {
            base.OnExit();

            SkillLocator locator = base.skillLocator;
            locator.primary.UnsetSkillOverride(base.gameObject, PrimarySkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.secondary.UnsetSkillOverride(base.gameObject, HeatSkill, GenericSkill.SkillOverridePriority.Contextual);
            // locator.utility.UnsetSkillOverride(base.gameObject, NullSkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.UnsetSkillOverride(base.gameObject, CancelSkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.DeductStock(1);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }

    public class OverdriveExit : BaseState { 
        public override void OnEnter()
        {
            base.OnEnter();
            EntityStateMachine machine = EntityStateMachine.FindByCustomName(base.gameObject, "Weapon");
            if (machine.state is OverdriveFire) {
                (machine.state as OverdriveFire).Exit();
            }
            outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}