using Sandswept.Skills.Ranger.VFX;
using System;

namespace Sandswept.States.Ranger
{
    public class Sidestep : BaseState
    {
        public static float Duration = 0.1f;
        public static float BuffDuration = 1f;
        public static float SpeedCoefficient = 17f;
        private Vector3 stepVector;
        private Transform modelTransform;
        public static Material overlayMat1 = SidestepVFX.dashMat1;
        public static Material overlayMat2 = SidestepVFX.dashMat2;

        public override void OnEnter()
        {
            base.OnEnter();
            if (base.modelLocator)
            {
            }
            if (base.characterBody)
            {
                base.characterBody.isSprinting = true;
            }

            modelTransform = GetModelTransform();

            Util.PlaySound("Play_huntress_shift_mini_blink", gameObject);
            Util.PlaySound("Play_engi_M2_spider_dash", gameObject);
            Util.PlaySound("Play_lunar_wisp_attack1_shoot_impact", gameObject);

            stepVector = (base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector;

            PlayAnimation("FullBody, Override", "Twirl");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.characterMotor && base.characterDirection)
            {
                base.characterMotor.velocity = Vector3.zero;
                base.characterMotor.rootMotion += stepVector * (base.moveSpeedStat * SpeedCoefficient * Time.fixedDeltaTime);
            }

            if (base.characterBody)
            {
                base.characterBody.isSprinting = true;
            }

            if (modelTransform)
            {
                var temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 0.9f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = overlayMat1;
                temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());

                var temporaryOverlay2 = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay2.duration = 1f;
                temporaryOverlay2.animateShaderAlpha = true;
                temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay2.destroyComponentOnEnd = true;
                temporaryOverlay2.originalMaterial = overlayMat2;
                temporaryOverlay2.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
            }

            if (base.fixedAge >= Duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (base.characterBody)
            {
                base.characterBody.isSprinting = true;
            }

            if (base.isAuthority)
            {
                base.characterBody.AddTimedBuffAuthority(Buffs.SidestepBuff.instance.BuffDef.buffIndex, BuffDuration);
            }
        }
    }
}