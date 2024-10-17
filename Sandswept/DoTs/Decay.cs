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

        public static void Init()
        {
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
                damageColorIndex = DamageColorIndex.DeathMark,
                damageCoefficient = 1f
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

            decayIndex = RegisterDotDef(decayDef, behavior);
        }
    }
}