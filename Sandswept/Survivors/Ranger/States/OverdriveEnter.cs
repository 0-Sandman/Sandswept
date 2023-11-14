using System.Text.RegularExpressions;
using System;
using UnityEngine.TextCore;
using Sandswept.Survivors.Ranger.Skilldefs;

namespace Sandswept.Survivors.Ranger.States
{
    public class OverdriveEnter : BaseState
    {
        public static SkillDef PrimarySkill => Skilldefs.Enflame.instance.skillDef;

        public static SkillDef SecondarySkill => Skilldefs.Exhaust.instance.skillDef;
        public static SkillDef UtilitySkill => Skilldefs.HeatSignature.instance.skillDef;

        public static SkillDef SpecialSkill => Skilldefs.HeatSink.instance.skillDef;
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

            GetComponent<RangerHeatController>().EnterOverdrive();
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

            PlayAnimation("Gesture, Override", "OverdriveFire", "Fire.playbackRate", 0.1f); // this is jank but it works!

            if (characterBody)
            {
                var crosshairOverrideBehavior = characterBody.GetComponent<RoR2.UI.CrosshairUtils.CrosshairOverrideBehavior>();
                crosshairOverrideBehavior.RemoveRequest(crosshairRequest);
            }

            GetComponent<RangerHeatController>().ExitOverdrive();

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
        public RangerHeatController heat;

        public override void OnEnter()
        {
            base.OnEnter();
            EntityStateMachine machine = EntityStateMachine.FindByCustomName(gameObject, "Overdrive");
            if (machine.state is OverdriveEnter)
            {
                (machine.state as OverdriveEnter).Exit();
            }

            heat = GetComponent<RangerHeatController>();
            heat.currentHeat = Mathf.Max(0f, heat.currentHeat - 50f);

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