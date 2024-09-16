using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Survivors.Ranger.States
{
    public class Sidestep : BaseState
    {
        public static float Duration = 0.15f;
        public static float SpeedCoefficient = 11.3333333f;
        private Vector3 stepVector;
        private Transform modelTransform;
        private Material overlayMat1;
        private Material overlayMat2;

        public override void OnEnter()
        {
            base.OnEnter();
            if (characterBody)
            {
                characterBody.isSprinting = true;
                if (NetworkServer.active)
                    characterBody.AddBuff(Buffs.SidestepCharge.instance.BuffDef);
            }

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                overlayMat1 = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => SidestepVFX.dashMat1Major,
                    "SKINDEF_RENEGADE" => SidestepVFX.dashMat1Renegade,
                    "SKINDEF_MILEZERO" => SidestepVFX.dashMat1MileZero,
                    "SKINDEF_SANDSWEPT" => SidestepVFX.dashMat1Sandswept,
                    _ => SidestepVFX.dashMat1Default
                };

                overlayMat2 = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => SidestepVFX.dashMat2Major,
                    "SKINDEF_RENEGADE" => SidestepVFX.dashMat2Renegade,
                    "SKINDEF_MILEZERO" => SidestepVFX.dashMat2MileZero,
                    "SKINDEF_SANDSWEPT" => SidestepVFX.dashMat2Sandswept,
                    _ => SidestepVFX.dashMat2Default
                };
                var temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                temporaryOverlay.duration = 0.9f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = overlayMat1;
                temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();

                var temporaryOverlay2 = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                temporaryOverlay2.duration = 1f;
                temporaryOverlay2.animateShaderAlpha = true;
                temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay2.destroyComponentOnEnd = true;
                temporaryOverlay2.originalMaterial = overlayMat2;
                temporaryOverlay2.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
            }

            Util.PlayAttackSpeedSound("Play_huntress_shift_mini_blink", gameObject, 0.5f);
            Util.PlayAttackSpeedSound("Play_commando_shift", gameObject, 1.2f);

            stepVector = inputBank.moveVector == Vector3.zero ? characterDirection.forward : inputBank.moveVector;

            PlayAnimation("FullBody, Override", "Twirl");
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = Vector3.zero;
                characterMotor.rootMotion += stepVector * (moveSpeedStat * SpeedCoefficient * Time.fixedDeltaTime);
            }

            if (characterBody)
            {
                characterBody.isSprinting = true;
            }

            if (fixedAge >= Duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (characterBody)
            {
                characterBody.isSprinting = true;
                if (NetworkServer.active)
                    characterBody.RemoveBuff(Buffs.SidestepCharge.instance.BuffDef);
            }

            if (characterMotor)
            {
                SmallHop(characterMotor, 12f);
            }
        }
    }
}