namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Fractured Timepiece")]
    public class FracturedTimepiece : ItemBase<FracturedTimepiece>
    {
        public override string ItemName => "Fractured Timepiece";

        public override string ItemLangTokenName => "FRACTURED_TIMEPIECE";

        public override string ItemPickupDesc => "Using your utility skill heals you and reduces special cooldown.";

        public override string ItemFullDescription => ("Upon using your $suutility skill$se, $shheal$se for $sh" + d(basePercentHealing) + "$se $ss(+" + d(stackPercentHealing) + " per stack)$se of your $shmaximum health$se and $sureduce special skill cooldown$se by $su" + d(baseSpecialCooldownReduction) + "$se $ss(+" + d(stackSpecialCooldownReduction) + " per stack)$se.").AutoFormat();

        public override string ItemLore => "Order: Timepiece\r\nTracking Number: 864*******\r\nEstimated Delivery: 02/23/2054\r\nShipping Method: Priority\r\nShipping Address: Hall of the Revered, Mars\r\nShipping Details:\r\n\r\nThis was uncovered by some archeologists in the desert where the old Hall was, before it burned down. I guess someone really wanted to protect it from the purge, since it was carefully wrapped and boxed where we found it. We're sending it to you, free of charge, since it was owned by the Hall to begin with, and you can probably glean more knowledge from it than we can.\r\n\r\nThe box had a note in it from the Time Keeper of the era, too, which I've included in the package. It's in the old language, so we couldn't make it out -- hopefully you can make some sense of it.";

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

        [ConfigField("Base Percent Healing", "Decimal.", 0.04f)]
        public static float basePercentHealing;

        [ConfigField("Stack Percent Healing", "Decimal.", 0.04f)]
        public static float stackPercentHealing;

        [ConfigField("Base Special Cooldown Reduction", "Decimal.", 0.15f)]
        public static float baseSpecialCooldownReduction;

        [ConfigField("Stack Special Cooldown Reduction", "Decimal.", 0.15f)]
        public static float stackSpecialCooldownReduction;

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
                }
            }
            orig(self);
        }

        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            var stack = GetCount(self);
            var skillLocator = self.GetComponent<SkillLocator>();
            if (stack > 0 && skillLocator && skill == skillLocator.utility && skill.cooldownRemaining > 0 && skill.skillDef.skillNameToken != "MAGE_UTILITY_ICE_NAME")
            {
                Main.ModLogger.LogError("has timepiece and skill locator and skill is utility and skill has a cooldown and isnt ice wall");
                var special = skillLocator.special;
                var reduction = Util.ConvertAmplificationPercentageIntoReductionPercentage(baseSpecialCooldownReduction + stackSpecialCooldownReduction * (stack - 1)) * 0.01f;
                Main.ModLogger.LogError("reduction is " + reduction);
                if (special && special.stock < special.maxStock)
                {
                    Main.ModLogger.LogError("has special and special stock is less than max stock");
                    Main.ModLogger.LogError("special recharge stopwatch pre change: " + special.rechargeStopwatch);
                    special.rechargeStopwatch += special.baseRechargeInterval * reduction;
                    Main.ModLogger.LogError("special recharge stopwatch AFTERRR change: " + special.rechargeStopwatch);
                }
                self.healthComponent?.HealFraction(basePercentHealing + stackPercentHealing * (stack - 1), default);
            }
            orig(self, skill);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}