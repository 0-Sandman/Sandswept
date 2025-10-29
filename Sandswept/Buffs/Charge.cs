using Sandswept.Survivors.Ranger;
using Sandswept.Survivors.Ranger.Projectiles;
using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Buffs
{
    public class Charge : BuffBase<Charge>
    {
        public override string BuffName => "Charge";
        public override Color Color => new Color32(45, 188, 148, 255);
        public override Sprite BuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("texBuffCharged.png");
        public override bool CanStack => true;

        public override void Init()
        {
            base.Init();
            On.RoR2.CharacterBody.AddBuff_BuffDef += CharacterBody_AddBuff_BuffDef;
        }

        private static Transform GetModelTransform(CharacterBody body)
        {
            if (!body.modelLocator)
            {
                return null;
            }
            return body.modelLocator.modelTransform;
        }

        private static void CharacterBody_AddBuff_BuffDef(On.RoR2.CharacterBody.orig_AddBuff_BuffDef orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);

            var duration = 0.15f;

            var buffCount = self.GetBuffCount(instance.BuffDef);

            for (int i = 0; i < buffCount; i++)
            {
                duration += 0.05f;
            }

            Material overlayMat1 = null;
            Material overlayMat2 = null;

            var modelTransform = GetModelTransform(self);

            if (buffDef == instance.BuffDef)
            {
                // Main.ModLogger.LogError("model transform is " + modelTransform); works
                if (modelTransform)
                {
                    var skinNameToken = GetModelTransform(self)?.GetComponentInChildren<ModelSkinController>().skins[self.skinIndex].nameToken;

                    overlayMat1 = skinNameToken switch
                    {
                        "RANGER_SKIN_MAJOR_NAME" => SidestepVFX.dashMat1Major,
                        "RANGER_SKIN_RENEGADE_NAME" => SidestepVFX.dashMat1Renegade,
                        "RANGER_SKIN_MILEZERO_NAME" => SidestepVFX.dashMat1MileZero,
                        "RANGER_SKIN_SANDSWEPT_NAME" => SidestepVFX.dashMat1Sandswept,
                        _ => SidestepVFX.dashMat1Default
                    };

                    overlayMat2 = skinNameToken switch
                    {
                        "RANGER_SKIN_MAJOR_NAME" => SidestepVFX.dashMat2Major,
                        "RANGER_SKIN_RENEGADE_NAME" => SidestepVFX.dashMat2Renegade,
                        "RANGER_SKIN_MILEZERO_NAME" => SidestepVFX.dashMat2MileZero,
                        "RANGER_SKIN_SANDSWEPT_NAME" => SidestepVFX.dashMat2Sandswept,
                        _ => SidestepVFX.dashMat2Default
                    };

                    var temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                    temporaryOverlay.duration = duration - 0.1f;
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = overlayMat1;
                    temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();

                    var temporaryOverlay2 = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                    temporaryOverlay2.duration = duration;
                    temporaryOverlay2.animateShaderAlpha = true;
                    temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay2.destroyComponentOnEnd = true;
                    temporaryOverlay2.originalMaterial = overlayMat2;
                    temporaryOverlay2.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
                }

                Util.PlaySound("Play_loader_R_shock", self.gameObject);

                if (buffCount > (DirectCurrent.maxCharge - 1))
                {
                    Util.PlaySound("Play_vagrant_attack1_pop", self.gameObject);
                    Util.PlaySound("Play_vagrant_attack1_pop", self.gameObject);
                }
            }
        }
    }
}