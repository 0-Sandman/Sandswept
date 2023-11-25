using static RoR2.DotController;

namespace Sandswept.Items.Reds
{
    [ConfigSection("Items :: Bleeding Witness")]
    internal class BleedingWitness : ItemBase<BleedingWitness>
    {
        public override string ItemName => "Bleeding Witness";

        public override string ItemLangTokenName => "BLEEDING_WITNESS";

        public override string ItemPickupDesc => "Your damage over time effects heal all allies.";

        public override string ItemFullDescription => ("$sd" + hemorrhageChance + "%$se chance to $sdhemorrhage$se enemies for $sd" + d(hemorrhageDamage) + "$se base damage. Your $sddamage over time effects$se $shheal$se all allies for $sh" + d(baseDoTHealing) + "$se $ss(+" + d(stackDoTHealing) + " per stack)$se of their $shmaximum health$se.").AutoFormat();

        public override string ItemLore => "no";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("Assets/Sandswept/BleedingWitnessHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texBleedingWitness.png");

        [ConfigField("Hemorrhage Chance", "", 5f)]
        public static float hemorrhageChance;

        [ConfigField("Hemorrhage Damage", "Decimal.", 4.8f)]
        public static float hemorrhageDamage;

        [ConfigField("Hemorrhage Duration", "", 3f)]
        public static float hemorrhageDuration;

        [ConfigField("Base DoT Healing", "Decimal.", 0.005f)]
        public static float baseDoTHealing;

        [ConfigField("Stack DoT Healing", "Decimal.", 0.0025f)]
        public static float stackDoTHealing;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Healing };

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            // RoR2.DotController.onDotInflictedServerGlobal += DotController_onDotInflictedServerGlobal;
            On.RoR2.DotController.FixedUpdate += DotController_FixedUpdate;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            stack = Util.GetItemCountGlobal(instance.ItemDef.itemIndex, true);
        }

        public static int stack = 0;

        private void DotController_FixedUpdate(On.RoR2.DotController.orig_FixedUpdate orig, DotController self)
        {
            orig(self);
            if (NetworkServer.active && stack > 0)
            {
                var healAmount = baseDoTHealing + stackDoTHealing * (stack - 1);
                for (var dotIndex = DotIndex.Bleed; dotIndex < DotIndex.Count; dotIndex++)
                {
                    uint num = 1U << (int)dotIndex;
                    if ((self.activeDotFlags & num) > 0U)
                    {
                        var lastDotTimer = self.dotTimers[(int)dotIndex] - Time.fixedDeltaTime;
                        if (lastDotTimer <= 0f)
                        {
                            for (int i = 0; i < CharacterBody.instancesList.Count; i++)
                            {
                                var body = CharacterBody.instancesList[i];
                                if (body.teamComponent.teamIndex == TeamIndex.Player)
                                {
                                    body.healthComponent?.HealFraction(healAmount, default);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var victimBody = report.victimBody;
            if (!victimBody)
            {
                return;
            }

            if (!victimBody.healthComponent)
            {
                return;
            }

            int stacks = GetCount(attackerBody);

            if (stacks > 0)
            {
                if (Util.CheckRoll(hemorrhageChance * damageInfo.procCoefficient, attackerBody.master))
                {
                    InflictDotInfo baseDamage = new()
                    {
                        victimObject = victimBody.gameObject,
                        attackerObject = attackerBody.gameObject,
                        totalDamage = attackerBody.damage * hemorrhageDamage,
                        dotIndex = DotIndex.SuperBleed,
                        duration = hemorrhageDuration,
                        damageMultiplier = 1f
                    };

                    InflictDot(ref baseDamage);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}