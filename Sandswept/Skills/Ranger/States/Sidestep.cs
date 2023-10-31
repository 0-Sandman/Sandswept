using Sandswept.Components;
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
            if (characterBody)
            {
                characterBody.isSprinting = true;
                if (NetworkServer.active)
                    characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            modelTransform = GetModelTransform();

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

            Util.PlaySound("Play_huntress_shift_mini_blink", gameObject);
            Util.PlaySound("Play_engi_M2_spider_dash", gameObject);
            Util.PlaySound("Play_lunar_wisp_attack1_shoot_impact", gameObject);

            stepVector = (inputBank.moveVector == Vector3.zero) ? characterDirection.forward : inputBank.moveVector;

            PlayAnimation("FullBody, Override", "Twirl");
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
                    characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            if (characterMotor)
            {
                SmallHop(characterMotor, 12f);
            }
        }
    }
}