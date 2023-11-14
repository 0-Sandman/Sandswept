using Sandswept.Survivors.Ranger;
using Sandswept.Survivors.Ranger.Projectiles;
using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Buffs
{
    public class Charge : BuffBase<Charge>
    {
        public override string BuffName => "Charge";

        public override Color Color => new Color32(45, 187, 188, 255);

        public override Sprite BuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texBuffCharged.png");
        public override bool CanStack => true;

        public override void Init()
        {
            base.Init();
            GetStatCoefficients += Charged_GetStatCoefficients;
            On.RoR2.CharacterBody.AddBuff_BuffDef += CharacterBody_AddBuff_BuffDef;
        }

        private void Charged_GetStatCoefficients(CharacterBody body, StatHookEventArgs args)
        {
            if (NetworkServer.active && body)
            {
                var levelScale = 0.25f * 0.2f * (body.level - 1);
                args.baseRegenAdd += (0.25f + levelScale) * body.GetBuffCount(BuffDef);
            }
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

                    switch (skinNameToken)
                    {
                        default:
                            overlayMat1 = SidestepVFX.dashMat1Default;
                            overlayMat2 = SidestepVFX.dashMat2Default;
                            break;

                        case "SKINDEF_MAJOR":
                            overlayMat1 = SidestepVFX.dashMat1Major;
                            overlayMat2 = SidestepVFX.dashMat2Major;
                            break;

                        case "SKINDEF_RENEGADE":
                            overlayMat1 = SidestepVFX.dashMat1Renegade;
                            overlayMat2 = SidestepVFX.dashMat2Renegade;
                            break;

                        case "SKINDEF_MILEZERO":
                            overlayMat1 = SidestepVFX.dashMat1MileZero;
                            overlayMat2 = SidestepVFX.dashMat2MileZero;
                            break;
                    }

                    var temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay.duration = duration - 0.1f;
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = overlayMat1;
                    temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());

                    var temporaryOverlay2 = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay2.duration = duration;
                    temporaryOverlay2.animateShaderAlpha = true;
                    temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay2.destroyComponentOnEnd = true;
                    temporaryOverlay2.originalMaterial = overlayMat2;
                    temporaryOverlay2.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
                }

                AkSoundEngine.PostEvent(Events.Play_loader_R_shock, self.gameObject);

                if (buffCount > (DirectCurrent.maxCharge - 1))
                {
                    AkSoundEngine.PostEvent(Events.Play_vagrant_attack1_pop, self.gameObject);
                    AkSoundEngine.PostEvent(Events.Play_vagrant_attack1_pop, self.gameObject);
                }
            }
        }
    }
}