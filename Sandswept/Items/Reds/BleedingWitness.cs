using LookingGlass.ItemStatsNameSpace;
using static RoR2.DotController;

namespace Sandswept.Items.Reds
{
    [ConfigSection("Items :: Bleeding Witness")]
    internal class BleedingWitness : ItemBase<BleedingWitness>
    {
        public override string ItemName => "Bleeding Witness";

        public override string ItemLangTokenName => "BLEEDING_WITNESS";

        public override string ItemPickupDesc => "Your damage over time ticks heal all allies.";

        public override string ItemFullDescription => $"$sd{hemorrhageChance}%$se chance on hit to $sdhemorrhage$se enemies for $sd{hemorrhageDamage * 100f}%$se base damage. Your $sddamage over time ticks$se $shheal$se all allies for $sh{baseDoTHealing * 100f}%$se $ss(+{stackDoTHealing * 100f}% per stack)$se of their $shmaximum health$se.".AutoFormat();

        public override string ItemLore =>
        """
        > Automated report 3947b729237d17//?v=7YXoXter6H0 is now available from site record 62a73bd8473f8914239c9a837f.
        > Please refer to record 62a73bd8473f8914239c9a837f for additional details during your review.
        > Report Type: Machine-generated Transcription > base Vernacular Profile "HotR"
        > Source: 62a73bd8473f8914239c9a837f (Unknown Material Artifact #3715)
        > Priority: High
        > Report Content:

        -- Beginning of Excerpt Flagged for Review --

        [your eminence]. The scout has returned from [the white plane].

        WHERE IS THE SCOUT?

        This is their remains, [your eminence].

        WHAT COULD HAVE DONE THIS? THIS IS A PLANET INHABITED BY LOWLY BEASTS.

        The scout was sent back through [the space between], by an unknown entity, as if it were some message to us. There is no trace of the remaining party.

        THE CREATOR OF THE ARTIFACT, NO DOUBT. A POWERFUL GUARDIAN. HE YET LINGERS THERE...

        It would seem that way, [your eminence]. Whatever lies there in [the white plane] is of great danger to us. Maybe it would be best to leave it be.

        IF ITS CREATOR LIES THERE, SO MUST THE ARTIFACT. YOUR COWARDICE MAKES NO DIFFERENCE; WE MUST RETRIEVE IT. HAIL TO [the red plane].

        ...it will be done, [your eminence]. Hail to [the red plane].

        -- End of Recording --

        -- End of Excerpt Flagged for Review --

        > TRANSLATION ERRORS: 4
        > 1> [your eminence] could not be fully translated.
        > 2> [the space between] could not be fully translated.
        > 3> [the white plane] could not be fully translated.
        > 4> [the red plane] could not be fully translated.

        > Please refer to report b438ff73774c9801238d48130d048cd757a for full audio excerpt.
        ===================================================
        """;
        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("BleedingWitnessHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texBleedingWitness.png");

        [ConfigField("Hemorrhage Chance", "", 8f)]
        public static float hemorrhageChance;

        [ConfigField("Hemorrhage Damage", "Decimal.", 3.6f)]
        public static float hemorrhageDamage;

        [ConfigField("Hemorrhage Duration", "", 3f)]
        public static float hemorrhageDuration;

        [ConfigField("Base DoT Healing", "Decimal.", 0.0033f)]
        public static float baseDoTHealing;

        [ConfigField("Stack DoT Healing", "Decimal.", 0.0033f)]
        public static float stackDoTHealing;

        public override ItemTag[] ItemTags => [ItemTag.Damage, ItemTag.Healing, ItemTag.CanBeTemporary];

        public override float modelPanelParametersMinDistance => 7f;
        public override float modelPanelParametersMaxDistance => 15f;

        public override void Init()
        {
            base.Init();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Hemorrhage Chance: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.descriptions.Add("Healing: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Healing);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.PercentHealth);
            itemStatsDef.hasChance = true;
            itemStatsDef.chanceScaling = ItemStatsDef.ChanceScaling.DoesNotScale;
            itemStatsDef.calculateValuesNew = (luck, stack, procChance) =>
            {
                List<float> values = new()
                {
                    LookingGlass.Utils.CalculateChanceWithLuck(hemorrhageChance * procChance * 0.01f, luck),
                    baseDoTHealing + stackDoTHealing * (stack - 1)
                };

                return values;
            };

            return itemStatsDef;
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        public static int stack = 0;

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
                    // Main.ModLogger.LogError("dealt damage from dot, tryna heal");

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

            var itemDisplay = SetUpFollowerIDRS(0.3f, 90f, true, 10f, false, 0f, true, 5f);

            return new ItemDisplayRuleDict(new ItemDisplayRule()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(-1f, 2.5f, -1.5f),
                localScale = new Vector3(0.05f, 0.05f, 0.05f),

                followerPrefab = itemDisplay,
                limbMask = LimbFlags.None,
                followerPrefabAddress = new AssetReferenceGameObject("")
            });
        }
    }
}