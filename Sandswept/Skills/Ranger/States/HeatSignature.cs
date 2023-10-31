using RoR2.Projectile;
using Sandswept.Components;
using Sandswept.Skills.Ranger.VFX;

namespace Sandswept.States.Ranger
{
    public class HeatSignature : BaseState
    {
        public static float damageCoefficient = 3f;
        private RangerHeatManager heat;
        private Transform modelTransform;
        public static Material overlayMat1 = HeatSignatureVFX.dashMat1;
        public static Material overlayMat2 = HeatSignatureVFX.dashMat2;
        private TemporaryOverlay overlay1;
        private TemporaryOverlay overlay2;

        public override void OnEnter()
        {
            base.OnEnter();

            heat = GetComponent<RangerHeatManager>();
            heat.isUsingHeatSignature = true;

            if (characterBody && NetworkServer.active)
            {
                characterBody.AddBuff(Buffs.HeatSignatureBuff.instance.BuffDef);
            }

            modelTransform = GetModelTransform();

            // spawn fire trail
            // I think it's hardcoded to do 150% damage though, and might not even work if you aren't blazing
            // also hook takedamage somewhere and add to utility cooldown if ranger is in overdrive
            // hopoo games ! !

            if (modelTransform)
            {
                overlay1 = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                overlay1.duration = 99f;
                overlay1.animateShaderAlpha = true;
                overlay1.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                overlay1.destroyComponentOnEnd = true;
                overlay1.originalMaterial = overlayMat1;
                overlay1.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());

                overlay2 = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                overlay2.duration = 99f;
                overlay2.animateShaderAlpha = true;
                overlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                overlay2.destroyComponentOnEnd = true;
                overlay2.originalMaterial = overlayMat2;
                overlay2.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (heat.CurrentHeat <= 0f || inputBank.skill3.justPressed)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            heat.isUsingHeatSignature = false;
            if (characterBody && NetworkServer.active)
            {
                characterBody.RemoveBuff(Buffs.HeatSignatureBuff.instance.BuffDef);
            }
            if (overlay1)
            {
                overlay1.enabled = false;
            }
            if (overlay2)
            {
                overlay2.enabled = false;
            }
        }
    }
}