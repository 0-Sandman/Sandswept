namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Fractured Timepiece")]
    public class FracturedTimepiece : ItemBase<FracturedTimepiece>
    {
        public override string ItemName => "Fractured Timepiece";

        public override string ItemLangTokenName => "FRACTURED_TIMEPIECE";

        public override string ItemPickupDesc => "Using your utility skill heals you and reduces special cooldown.";

        public override string ItemFullDescription => ("Upon using your $suutility skill$se, $shheal$se for $sh" + d(basePercentHealing) + "$se $ss(+" + d(stackPercentHealing) + " per stack)$se of your $shmaximum health$se and $sureduce special skill cooldown$se by $su" + d(baseSpecialCooldownReduction) + "$se $ss(+" + d(stackSpecialCooldownReduction) + " per stack)$se.").AutoFormat();

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("Assets/Sandswept/FracturedTimepieceHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texFracturedTimepiece.png");

        public override bool AIBlacklisted => true;

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

        [ConfigField("Base Special Cooldown Reduction", "Decimal.", 0.15f)]
        public static float baseSpecialCooldownReduction;

        [ConfigField("Stack Special Cooldown Reduction", "Decimal.", 0.15f)]
        public static float stackSpecialCooldownReduction;

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
        }

        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            var stack = GetCount(self);
            var skillLocator = self.GetComponent<SkillLocator>();
            if (stack > 0 && skillLocator && skill == skillLocator.utility && skill.cooldownRemaining > 0 && skill.skillDef.skillNameToken != "MAGE_UTILITY_ICE_NAME")
            {
                var special = skillLocator.special;
                var reduction = Util.ConvertAmplificationPercentageIntoReductionPercentage(baseSpecialCooldownReduction + stackSpecialCooldownReduction * (stack - 1)) * 0.01f;
                if (special && special.stock < special.maxStock)
                {
                    special.rechargeStopwatch += special.baseRechargeInterval * reduction;
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