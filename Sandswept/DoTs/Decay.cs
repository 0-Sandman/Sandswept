using Sandswept.Items.VoidGreens;
using System.Linq;
using static R2API.DotAPI;
using static RoR2.DotController;

namespace Sandswept.DoTs
{
    [ConfigSection("DoTs :: Decay")]
    public class Decay
    {
        public static BuffDef decayBuff;
        public static DotDef decayDef;
        public static DotIndex decayIndex;

        [ConfigField("Base Damage", "Decimal.", 2.5f)]
        public static float baseDamage;

        [ConfigField("Scale Damage with Player Missing Health and Enemy Missing Health?", "Scales decay's base damage up to 300% of its damage value linearly with the player's and enemy's missing health.", true)]
        public static bool scaleDamage;
        public static Color32 decayColor = new Color32(96, 56, 177, 255);

        public static DamageColorIndex decayColorIndex = DamageColourHelper.RegisterDamageColor(decayColor);

        public static BurnEffectController.EffectParams decayEffect;

        public static void Init()
        {
            var decayRamp = Main.sandsweptHIFU.LoadAsset<Texture2D>("texRampGrandparent.png");

            var decayMat = new Material(Paths.Material.matBlighted);
            decayMat.SetColor("_TintColor", new Color32(52, 0, 138, 255));
            decayMat.SetTexture("_RemapTex", decayRamp);
            decayMat.SetFloat("_Boost", 1f);
            decayMat.SetFloat("_AlphaBoost", 20f);
            decayMat.SetFloat("_AlphaBias", 1f);
            decayMat.name = "matDecaying";

            var decayVFX = PrefabAPI.InstantiateClone(Paths.GameObject.BlightEffect, "DecayEffect", false);
            var particleSystemRenderer = decayVFX.GetComponent<ParticleSystemRenderer>();
            var decayVFXMat = new Material(Paths.Material.matCrocoBlightBillboard);
            // decayVFXMat.SetTexture("_RemapTex", Paths.Texture2D.texRampVoidSurvivorBase1);
            decayVFXMat.SetTexture("_RemapTex", decayRamp);
            decayVFXMat.SetTexture("_MainTex", Paths.Texture2D.texBandit2BackstabMask);
            decayVFXMat.name = "matDecayingVFX";

            particleSystemRenderer.material = decayVFXMat;

            var decayVFXBurst = decayVFX.transform.Find("Burst");
            var decayVFXBurstMain = decayVFXBurst.GetComponent<ParticleSystem>().main;
            var decayVFXBurstStartColor = decayVFXBurstMain.startColor;
            decayVFXBurstStartColor.color = new Color32(96, 56, 177, 255);

            var decayVFXBurstPSR = decayVFXBurst.GetComponent<ParticleSystemRenderer>();
            var decayVFXBurstMat = new Material(Paths.Material.matCrocoGooLarge);
            decayVFXBurstMat.SetColor("_TintColor", new Color32(78, 21, 176, 255));
            decayVFXBurstMat.SetColor("_EmColor", new Color32(50, 10, 120, 255));
            // decayVFXBurstMat.SetTexture("_RemapTex", Paths.Texture2D.texRampVoidSurvivorBase1);
            decayVFXBurstMat.SetTexture("_RemapTex", decayRamp);

            decayVFXBurstPSR.material = decayVFXBurstMat;

            decayEffect = new BurnEffectController.EffectParams()
            {
                overlayMaterial = decayMat,
                fireEffectPrefab = decayVFX,
            };
            decayBuff = ScriptableObject.CreateInstance<BuffDef>();
            decayBuff.canStack = true;
            decayBuff.isCooldown = false;
            decayBuff.isDebuff = true;
            decayBuff.isHidden = false;
            decayBuff.buffColor = new Color32(96, 56, 177, 255);
            decayBuff.name = "Decay";
            decayBuff.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffDecay.png");
            ContentAddition.AddBuffDef(decayBuff);

            decayDef = new()
            {
                associatedBuff = decayBuff,
                resetTimerOnAdd = false,
                interval = 0.2f,
                damageColorIndex = decayColorIndex,
                damageCoefficient = 1f / baseDamage
            };

            On.RoR2.DotController.UpdateDotVisuals += DotController_UpdateDotVisuals;

            CustomDotBehaviour behavior = delegate (DotController self, DotStack dotStack)
            {
                var victimBody = self.victimBody;
                var attackerBody = dotStack.attackerObject?.GetComponent<CharacterBody>();
                if (victimBody)
                {
                    if (attackerBody)
                    {
                        dotStack.damage = attackerBody.damage * 0.2f;
                        if (scaleDamage)
                        {
                            var victimHc = victimBody.healthComponent;
                            var attackerBodyHc = attackerBody.healthComponent;
                            if (victimHc && attackerBodyHc)
                            {
                                var scalar = 1f + (1f - victimHc.combinedHealthFraction) + (1f - attackerBodyHc.combinedHealthFraction);
                                dotStack.damage = attackerBody.damage * 0.2f * scalar;
                            }
                        }
                    }

                    // this throws at BurnEffectController.OnDestroy IL_0065 (it's `i` I think)
                    /*
                    var modelLocator = victimBody.modelLocator;
                    if (modelLocator)
                    {
                        if (!victimBody.GetComponent<WhatTheFuck>())
                        {
                            var decayEffectController = victimBody.AddComponent<BurnEffectController>();
                            decayEffectController.effectType = decayEffect;
                            decayEffectController.target = modelLocator.modelTransform.gameObject;
                            decayEffectController.fireParticleSize = 5f + (victimBody.radius * 2f);

                            for (int i = 0; i < 2; i++)
                            {
                                Util.PlaySound("Play_voidDevastator_impact", victimBody.gameObject);
                                Util.PlaySound("Play_voidRaid_step", victimBody.gameObject);
                                Util.PlaySound("Play_voidRaid_step", victimBody.gameObject);
                                Util.PlaySound("Play_item_proc_scrapGoop_consume", victimBody.gameObject);
                            }

                            victimBody.AddComponent<WhatTheFuck>();
                        }

                        // NEVER RUNS!?!?!?
                        // victim loses the buff in game
                        // tried getbuffcount <= 0
                        if (!victimBody.HasBuff(decayBuff))
                        {
                            Main.ModLogger.LogError("doesnt have decay buff anymore");
                            foreach (BurnEffectController controller in victimBody.GetComponents<BurnEffectController>())
                            {
                                Main.ModLogger.LogError("iterating through every burneffectcontroller");
                                if (controller.effectType == decayEffect)
                                {
                                    Main.ModLogger.LogError("found burneffectcontroller with an effecttype of decayeffect");
                                    GameObject.Destroy(controller);
                                    break;
                                }
                            }

                            if (victimBody.GetComponent<WhatTheFuck>())
                            {
                                Main.ModLogger.LogError("found lock, removing");
                                victimBody.RemoveComponent<WhatTheFuck>();
                            }
                        }
                    }
                    */
                }
            };

            decayIndex = RegisterDotDef(decayDef, behavior, null);
        }

        private static void DotController_UpdateDotVisuals(On.RoR2.DotController.orig_UpdateDotVisuals orig, DotController self)
        {
            orig(self);
            var victim = self.victimBody;
            if (!victim) return;
            var modelLocator = victim.GetComponent<ModelLocator>();
            var hasDecay = victim.HasBuff(decayBuff);
            var decayController = victim.GetComponents<BurnEffectController>().FirstOrDefault(x => x.effectType == decayEffect);
            if (hasDecay)
            {
                if (decayController == default) // what the fuck is default :sob:
                {
                    var decayEffectController = victim.AddComponent<BurnEffectController>();
                    decayEffectController.effectType = decayEffect;
                    decayEffectController.target = modelLocator.modelTransform.gameObject;
                    Util.PlaySound("Play_voidDevastator_impact", victim.gameObject);
                    Util.PlaySound("Play_voidRaid_step", victim.gameObject);
                    Util.PlaySound("Play_voidRaid_step", victim.gameObject);
                    Util.PlaySound("Play_item_proc_scrapGoop_consume", victim.gameObject);

                    decayEffectController.fireParticleSize = 7f + (victim.radius * 3f);
                }
            }
            else if (decayController != default)
            {
                Object.Destroy(decayController);
            }
        }
    }
}