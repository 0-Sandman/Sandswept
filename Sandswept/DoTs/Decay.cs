using Sandswept.Items.VoidGreens;
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

        [ConfigField("Base Damage", "Decimal.", 3f)]
        public static float baseDamage;

        [ConfigField("Scale Damage with Enemy Missing Health?", "Scales decay's base damage up to 200% of its damage value linearly with the enemy's missing health.", true)]
        public static bool scaleDamage;

        public static DamageColorIndex decayColor = DamageColourHelper.RegisterDamageColor(new Color32(96, 56, 177, 255));

        public static Material decayMat;

        public static BurnEffectController.EffectParams decayEffect;

        public static void Init()
        {
            decayMat = new Material(Paths.Material.matBlighted);
            decayMat.SetColor("_TintColor", new Color(0.49888185f, 0.20220098f, 1.0991436f, 1f)); // hdr color (with intensity), hence why a value is above 1
            decayMat.SetTexture("_RemapTex", Paths.Texture2D.texRampVoidSurvivorBase1);
            decayEffect = new BurnEffectController.EffectParams()
            {
                overlayMaterial = decayMat,
                startSound = "Play_voidBarnacle_death"
            };
            decayBuff = ScriptableObject.CreateInstance<BuffDef>();
            decayBuff.canStack = true;
            decayBuff.isCooldown = false;
            decayBuff.isDebuff = true;
            decayBuff.isHidden = false;
            decayBuff.buffColor = new Color32(96, 56, 177, 255);
            decayBuff.name = "Decay";
            decayBuff.iconSprite = Utils.Assets.BuffDef.bdBlight.iconSprite;
            ContentAddition.AddBuffDef(decayBuff);

            decayDef = new()
            {
                associatedBuff = decayBuff,
                resetTimerOnAdd = false,
                interval = 0.2f,
                damageColorIndex = decayColor,
                damageCoefficient = 1f
            };

            CustomDotVisual visual = delegate (DotController self)
            {
                var victim = self.victimObject;
                var modelLocator = victim.GetComponent<ModelLocator>();
                if (modelLocator && modelLocator.modelTransform)
                {
                    var decayEffectController = victim.AddComponent<BurnEffectController>();
                    decayEffectController.effectType = decayEffect;
                    decayEffectController.target = modelLocator.modelTransform.gameObject;
                }
            };

            CustomDotBehaviour behavior = delegate (DotController self, DotStack dotStack)
            {
                var victimBody = self.victimBody;
                var attackerBody = dotStack.attackerObject?.GetComponent<CharacterBody>();
                if (victimBody && attackerBody)
                {
                    dotStack.damage = attackerBody.damage * baseDamage * 0.2f;
                    if (scaleDamage)
                    {
                        var victimHc = victimBody.healthComponent;
                        if (victimHc)
                        {
                            var scalar = 1f + (1f - victimHc.combinedHealthFraction);
                            dotStack.damage = attackerBody.damage * baseDamage * 0.2f * scalar;
                        }
                    }
                }
            };

            decayIndex = RegisterDotDef(decayDef, behavior, visual);
        }
    }
}