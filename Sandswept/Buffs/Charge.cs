using Sandswept.Skills.Ranger.Projectiles;
using Sandswept.Skills.Ranger.VFX;

namespace Sandswept.Buffs
{
    public class Charge : BuffBase<Charge>
    {
        public override string BuffName => "Charge";

        public override Color Color => new Color32(45, 187, 188, 255);

        public override Sprite BuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texBuffCharged.png");
        public override bool CanStack => true;
        public static Material overlayMat1 = SidestepVFX.dashMat1;
        public static Material overlayMat2 = SidestepVFX.dashMat2;

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
                var levelScale = 0.12f * 0.2f * (body.level - 1);
                args.baseRegenAdd += (0.12f + levelScale) * body.GetBuffCount(BuffDef);
            }
        }

        private static void CharacterBody_AddBuff_BuffDef(On.RoR2.CharacterBody.orig_AddBuff_BuffDef orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (self.GetBuffCount(instance.BuffDef) > (DirectCurrent.maxCharge - 1) && buffDef == instance.BuffDef)
            {
                var modelTransform = self.modelLocator?.modelTransform;
                // Main.ModLogger.LogError("model transform is " + modelTransform); works
                if (modelTransform)
                {
                    var temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay.duration = 0.5f;
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = overlayMat1;
                    temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());

                    var temporaryOverlay2 = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay2.duration = 0.6f;
                    temporaryOverlay2.animateShaderAlpha = true;
                    temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay2.destroyComponentOnEnd = true;
                    temporaryOverlay2.originalMaterial = overlayMat2;
                    temporaryOverlay2.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
                }
                AkSoundEngine.PostEvent(Events.Play_vagrant_attack1_pop, self.gameObject);
                AkSoundEngine.PostEvent(Events.Play_vagrant_attack1_pop, self.gameObject);
            }
        }
    }
}