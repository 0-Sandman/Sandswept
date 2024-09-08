using Sandswept.Buffs;
using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Survivors.Ranger.States
{
    public class OverdriveEnter : BaseState
    {
        public static SkillDef PrimarySkill => Skilldefs.Enflame.instance.skillDef;

        public static SkillDef SecondarySkill => Skilldefs.Exhaust.instance.skillDef;
        public static SkillDef UtilitySkill => Skilldefs.HeatSignature.instance.skillDef;

        public static SkillDef SpecialSkill => Skilldefs.HeatSink.instance.skillDef;
        public RoR2.UI.CrosshairUtils.OverrideRequest crosshairRequest;
        public RangerHeatController heat;
        public Material heatMat;
        public Transform modelTransform;

        public override void OnEnter()
        {
            base.OnEnter();

            heat = GetComponent<RangerHeatController>();

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

                var modelTransform = GetModelTransform();

                if (modelTransform)
                {
                    var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                    heatMat = skinNameToken switch
                    {
                        "SKINDEF_MAJOR" => HeatVFX.heatMatMajor,
                        "SKINDEF_RENEGADE" => HeatVFX.heatMatRenegade,
                        "SKINDEF_MILEZERO" => HeatVFX.heatMatMileZero,
                        _ => HeatVFX.heatMatDefault
                    };

                    var temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                    temporaryOverlay.duration = 9999f;
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = heatMat;
                    temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
                }
            }

            GetModelAnimator().SetBool("gunOpen", true);
            Util.PlaySound("Play_item_use_BFG_charge", gameObject);

            heat.EnterOverdrive();
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

            GetModelAnimator().SetBool("gunOpen", false);
            Util.PlaySound("Play_lunar_wisp_attack2_windDown", gameObject);

            if (characterBody)
            {
                var crosshairOverrideBehavior = characterBody.GetComponent<RoR2.UI.CrosshairUtils.CrosshairOverrideBehavior>();
                crosshairOverrideBehavior.RemoveRequest(crosshairRequest);

                if (modelTransform)
                {
                    foreach (TemporaryOverlay overlay in modelTransform.GetComponents<TemporaryOverlay>())
                    {
                        Destroy(overlay);
                    }
                }
            }

            heat.ExitOverdrive();

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

    /*
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
    */
}