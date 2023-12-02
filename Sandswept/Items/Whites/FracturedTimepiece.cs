namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Fractured Timepiece")]
    public class FracturedTimepiece : ItemBase<FracturedTimepiece>
    {
        public override string ItemName => "Fractured Timepiece";

        public override string ItemLangTokenName => "FRACTURED_TIMEPIECE";

        public override string ItemPickupDesc => "Using your utility skill heals you and reduces special cooldown.";

        public override string ItemFullDescription => ("Upon using your $suutility skill$se, $shheal$se for $sh" + d(basePercentHealing) + "$se $ss(+" + d(stackPercentHealing) + " per stack)$se of your $shmaximum health$se and $sureduce special skill cooldown$se by $su" + d(specialCooldownReduction) + "$se.").AutoFormat();

        public override string ItemLore => "Order: Timepiece\r\nTracking Number: 864*******\r\nEstimated Delivery: 02/23/2054\r\nShipping Method: Priority\r\nShipping Address: Hall of the Revered, Mars\r\nShipping Details:\r\n\r\nOur team uncovered this in the desert where the old Hall was, before it burned down. I guess someone really wanted to protect it from the Purge, since it was carefully wrapped and boxed where we found it. You can probably glean more knowledge from it than we can, and it was the Hall's to begin with in any case.\r\n\r\nThe box had a note in it from the Time Keeper of the era, too, which I've included in the package. Nobody hear can read the old language, though -- hopefully you can make some sense of it.";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("Assets/Sandswept/FracturedTimepieceHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texFracturedTimepiece.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Healing, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        [ConfigField("Base Percent Healing", "Decimal.", 0.05f)]
        public static float basePercentHealing;

        [ConfigField("Stack Percent Healing", "Decimal.", 0.05f)]
        public static float stackPercentHealing;

        [ConfigField("Special Cooldown Reduction", "Decimal.", 0.15f)]
        public static float specialCooldownReduction;

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            On.EntityStates.Mage.Weapon.PrepWall.OnExit += PrepWall_OnExit;
        }

        private void PrepWall_OnExit(On.EntityStates.Mage.Weapon.PrepWall.orig_OnExit orig, EntityStates.Mage.Weapon.PrepWall self)
        {
            if (!self.outer.destroying)
            {
                if (self.goodPlacement)
                {
                    var stack = GetCount(self.characterBody);
                    var skillLocator = self.GetComponent<SkillLocator>();
                    if (stack > 0)
                    {
                        var special = skillLocator.special;
                        var reduction = Util.ConvertAmplificationPercentageIntoReductionPercentage(specialCooldownReduction);
                        if (special && special.stock < special.maxStock)
                        {
                            special.rechargeStopwatch += special.baseRechargeInterval * reduction;
                        }
                        self.healthComponent?.HealFraction(basePercentHealing + stackPercentHealing * (stack - 1), default);
                    }
                }
            }
            orig(self);
        }

        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            orig(self, skill);
            var stack = GetCount(self);
            var skillLocator = self.GetComponent<SkillLocator>();
            if (stack > 0 && skillLocator && skill == skillLocator.utility && skill.cooldownRemaining > 0 && skill.skillDef.skillNameToken != "MAGE_UTILITY_ICE_NAME")
            {
                var special = skillLocator.special;
                var reduction = Util.ConvertAmplificationPercentageIntoReductionPercentage(specialCooldownReduction);
                if (special && special.stock < special.maxStock)
                {
                    special.rechargeStopwatch += special.baseRechargeInterval * reduction;
                }
                self.healthComponent?.HealFraction(basePercentHealing + stackPercentHealing * (stack - 1), default);
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}