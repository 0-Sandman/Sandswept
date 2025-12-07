using Sandswept.Buffs;
using Sandswept.Survivors.Ranger.SkillDefs.Primary;
using Sandswept.Survivors.Ranger.SkillDefs.Secondary;
using Sandswept.Survivors.Ranger.SkillDefs.Utility;
using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Survivors.Ranger.States.Special
{
    public class OverdriveEnter : BaseState
    {
        public static SkillDef PrimarySkill = Enflame.instance.skillDef;

        public static SkillDef SecondarySkill = Exhaust.instance.skillDef;
        public static SkillDef UtilitySkill = HeatSignature.instance.skillDef;

        public static SkillDef SpecialSkill = SkillDefs.Special.HeatSink.instance.skillDef;

        public RoR2.UI.CrosshairUtils.OverrideRequest crosshairRequest;
        public RangerHeatController heat;
        public Material heatMat;
        public Transform modelTransform;
        public TemporaryOverlayInstance tempOverlayInstance;

        public Dictionary<SkillDef, SkillDef> originalToOverheatPrimarySkillDefMap = new();
        public Dictionary<SkillDef, SkillDef> originalToOverheatSecondarySkillDefMap = new();
        public Dictionary<SkillDef, SkillDef> originalToOverheatUtilitySkillDefMap = new();
        public Dictionary<SkillDef, SkillDef> originalToOverheatSpecialSkillDefMap = new();

        public TemporaryOverlay temporaryOverlay;

        public override void OnEnter()
        {
            base.OnEnter();

            heat = GetComponent<RangerHeatController>();

            SkillLocator locator = skillLocator;

            SecondarySkill = locator.secondary.skillDef == Release.instance.skillDef ? Exhaust.instance.skillDef : Survivors.Ranger.SkillDefs.Secondary.Char.instance.skillDef;

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
                        "RANGER_SKIN_MAJOR_NAME" => HeatVFX.heatMatMajor,
                        "RANGER_SKIN_RENEGADE_NAME" => HeatVFX.heatMatRenegade,
                        "RANGER_SKIN_MILEZERO_NAME" => HeatVFX.heatMatMileZero,
                        "RANGER_SKIN_SANDSWEPT_NAME" => HeatVFX.heatMatSandswept,
                        _ => HeatVFX.heatMatDefault
                    };
                    /*
                    tempOverlayInstance = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                    tempOverlayInstance.duration = 9999f;
                    tempOverlayInstance.animateShaderAlpha = true;
                    tempOverlayInstance.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    tempOverlayInstance.destroyComponentOnEnd = true;
                    tempOverlayInstance.originalMaterial = heatMat;
                    tempOverlayInstance.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
                    */

                    var characterModel = modelTransform.GetComponent<CharacterModel>();

                    temporaryOverlay = gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.duration = 99999f;
                    temporaryOverlay.originalMaterial = heatMat;
                    temporaryOverlay.inspectorCharacterModel = characterModel;
                    temporaryOverlay.AddToCharacerModel(characterModel);
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
                    // TemporaryOverlayManager.RemoveOverlay(tempOverlayInstance.managerIndex);
                    temporaryOverlay.RemoveFromCharacterModel();
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