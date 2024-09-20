using Sandswept.Survivors.Ranger;
using Sandswept.Survivors.Ranger.Projectiles;
using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Buffs
{
    public class Charge : BuffBase<Charge>
    {
        public override string BuffName => "Charge - 0.25 hp/s Regen and 1.5 Armor Per, 10% Base Damage Amp in Full Heat Per";

        public override Color Color => new Color32(45, 188, 148, 255);

        public override Sprite BuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("texBuffCharged.png");
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
                args.armorAdd += 1.5f * body.GetBuffCount(BuffDef);
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

                    overlayMat1 = skinNameToken switch
                    {
                        "SKINDEF_MAJOR" => SidestepVFX.dashMat1Major,
                        "SKINDEF_RENEGADE" => SidestepVFX.dashMat1Renegade,
                        "SKINDEF_MILEZERO" => SidestepVFX.dashMat1MileZero,
                        _ => SidestepVFX.dashMat1Default
                    };

                    overlayMat2 = skinNameToken switch
                    {
                        "SKINDEF_MAJOR" => SidestepVFX.dashMat2Major,
                        "SKINDEF_RENEGADE" => SidestepVFX.dashMat2Renegade,
                        "SKINDEF_MILEZERO" => SidestepVFX.dashMat2MileZero,
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