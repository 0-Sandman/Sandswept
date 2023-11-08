using System.Text.RegularExpressions;
using System;
using Sandswept.Components;
using UnityEngine.TextCore;

namespace Sandswept.States.Ranger
{
    public class OverdriveEnter : BaseState
    {
        public static SkillDef PrimarySkill => Skills.Ranger.Skilldefs.OverdriveFire.instance.skillDef;

        public static SkillDef SecondarySkill => Skills.Ranger.Skilldefs.Exhaust.instance.skillDef;
        public static SkillDef UtilitySkill => Skills.Ranger.Skilldefs.HeatSignature.instance.skillDef;

        public static SkillDef SpecialSkill => Skills.Ranger.Skilldefs.HeatSink.instance.skillDef;
        public RoR2.UI.CrosshairUtils.OverrideRequest crosshairRequest;

        public override void OnEnter()
        {
            base.OnEnter();

            SkillLocator locator = skillLocator;
            locator.primary.SetSkillOverride(gameObject, PrimarySkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.secondary.SetSkillOverride(gameObject, SecondarySkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.utility.SetSkillOverride(gameObject, UtilitySkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.SetSkillOverride(gameObject, SpecialSkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.DeductStock(1);

            if (characterBody)
            {
                var crosshairOverrideBehavior = characterBody.GetComponent<RoR2.UI.CrosshairUtils.CrosshairOverrideBehavior>();
                crosshairRequest = crosshairOverrideBehavior.AddRequest(Crosshairs.Ranger.hitscanCrosshairPrefab, RoR2.UI.CrosshairUtils.OverridePriority.Skill);
            }

            PlayAnimation("Gesture, Override", "EnterOverdrive");
            Util.PlaySound("Play_item_use_BFG_charge", gameObject);

            GetComponent<RangerHeatManager>().EnterOverdrive();
        }

        public override void OnExit()
        {
            base.OnExit();

            SkillLocator locator = skillLocator;
            locator.primary.UnsetSkillOverride(gameObject, PrimarySkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.secondary.UnsetSkillOverride(gameObject, SecondarySkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.utility.UnsetSkillOverride(gameObject, UtilitySkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.UnsetSkillOverride(gameObject, SpecialSkill, GenericSkill.SkillOverridePriority.Contextual);
            locator.special.DeductStock(1);

            PlayAnimation("Gesture, Override", "ExitOverdrive");
            Util.PlaySound("Play_lunar_wisp_attack2_windDown", gameObject);

            if (characterBody)
            {
                var crosshairOverrideBehavior = characterBody.GetComponent<RoR2.UI.CrosshairUtils.CrosshairOverrideBehavior>();
                crosshairOverrideBehavior.RemoveRequest(crosshairRequest);
            }

            GetComponent<RangerHeatManager>().isUsingHeatSignature = false;

            GetComponent<RangerHeatManager>().ExitOverdrive();

            if (characterBody)
            {
                characterBody.isSprinting = true;
            }
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
        public RangerHeatManager heat;

        public override void OnEnter()
        {
            base.OnEnter();
            EntityStateMachine machine = EntityStateMachine.FindByCustomName(gameObject, "Overdrive");
            if (machine.state is OverdriveEnter)
            {
                (machine.state as OverdriveEnter).Exit();
            }

            heat = GetComponent<RangerHeatManager>();
            heat.CurrentHeat = Mathf.Max(0f, heat.CurrentHeat - 50f);

            outer.SetNextStateToMain();

            if (characterBody)
            {
                characterBody.isSprinting = true;
                characterBody.SetBuffCount(Buffs.Charged.instance.BuffDef.buffIndex, Mathf.Min(characterBody.GetBuffCount(Buffs.Charged.instance.BuffDef) + 3, 10));
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }

    public class OverdriveExitHeatSink : BaseState
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

            if (characterBody)
            {
                characterBody.isSprinting = true;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}