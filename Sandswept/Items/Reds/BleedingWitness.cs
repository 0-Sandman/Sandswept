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

        public override string ItemLore => "<style=cMono>> Automated report 3947b729237d17//?v=7YXoXter6H0 is now available from site record 62a73bd8473f8914239c9a837f.\r\n> Please refer to record 62a73bd8473f8914239c9a837f for additional details during your review.\r\n> Report Type: Machine-generated Transcription> base Vernacular Profile \"HotR\"\r\n> Source: 62a73bd8473f8914239c9a837f (Unknown Material Artifact #3715)\r\n> Priority: High\r\n> Report Content:\r\n\r\n-- Beginning of Excerpt Flagged for Review --\r\n\r\n</style>[your eminence]. The scout has returned from [the white plane].\r\n\r\nWHERE IS THE SCOUT?\r\n\r\nThis is their remains, [your eminence].\r\n\r\nWHAT COULD HAVE DONE THIS? THIS IS A PLANET INHABITED BY LOWLY BEASTS.\r\n\r\nThe scout was sent back through [the space between], by an unknown entity, as if it were some message to us. There is no trace of the remaining party.\r\n\r\nTHE CREATOR OF THE ARTIFACT, NO DOUBT. A POWERFUL GUARDIAN. HE YET LINGERS THERE...\r\n\r\nIt would seem that way, [your eminence]. Whatever lies there in [the white plane] is of great danger to us. Maybe it would be best to leave it be.\r\n\r\nIF ITS CREATOR LIES THERE, SO MUST THE ARTIFACT. YOUR COWARDICE MAKES NO DIFFERENCE; WE MUST RETRIEVE IT. HAIL TO [the red plane].\r\n\r\n...it will be done, [your eminence]. Hail to [the red plane].<style=cMono>\r\n\r\n-- End of Recording --\r\n\r\n-- End of Excerpt Flagged for Review --\r\n\r\n> TRANSLATION ERRORS: 4\r\n> 1> [your eminence] could not be fully translated.\r\n> 2> [the space between] could not be fully translated.\r\n> 3> [the white plane] could not be fully translated.\r\n> 4> [the red plane] could not be fully translated.\r\n\r\n> Please refer to report b438ff73774c9801238d48130d048cd757a for full audio excerpt.\r\n===================================================\r\n</style>";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("BleedingWitnessHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texBleedingWitness.png");

        [ConfigField("Hemorrhage Chance", "", 8f)]
        public static float hemorrhageChance;

        [ConfigField("Hemorrhage Damage", "Decimal.", 5f)]
        public static float hemorrhageDamage;

        [ConfigField("Hemorrhage Duration", "", 3f)]
        public static float hemorrhageDuration;

        [ConfigField("Base DoT Healing", "Decimal.", 0.0033f)]
        public static float baseDoTHealing;

        [ConfigField("Stack DoT Healing", "Decimal.", 0.0033f)]
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
            // On.RoR2.DotController.FixedUpdate += DotController_FixedUpdate;
            // CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
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

                var healAmount = baseDoTHealing + stackDoTHealing * (stacks - 1);
                if ((report.damageInfo.damageType & DamageType.DoT) > 0)
                {
                    Main.ModLogger.LogError("dealt damage from dot, tryna heal");

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

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}