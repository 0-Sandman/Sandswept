using static RoR2.DotController;

namespace Sandswept.Items.Reds
{
    internal class BleedingWitness : ItemBase<BleedingWitness>
    {
        public override string ItemName => "Bleeding Witness";

        public override string ItemLangTokenName => "BLEEDING_WITNESS";

        public override string ItemPickupDesc => "Your damage over time effects heal all allies.";

        public override string ItemFullDescription => "$sd" + hemorrhageChance.Value + "%$se chance to $sdhemorrhage$se enemies for $sd" + d(hemorrhageDamage.Value) + "$se base damage. Your $sddamage over time effects$se $shheal$se all allies for $sh" + d(baseDoTHealing.Value) + "$se $ss(+" + d(stackDoTHealing.Value) + " per stack)$se of their $sdmaximum health$se.".AutoFormat();

        public override string ItemLore => "no";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("Assets/Sandswept/BleedingWitnessHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texBleedingWitness.png");

        public static ConfigEntry<float> hemorrhageChance { get; private set; }
        public static ConfigEntry<float> hemorrhageDamage { get; private set; }
        public static ConfigEntry<float> hemorrhageDuration { get; private set; }
        public static ConfigEntry<float> baseDoTHealing { get; private set; }
        public static ConfigEntry<float> stackDoTHealing { get; private set; }

        public override bool AIBlacklisted => true;

        public override void Init(ConfigFile config)
        {
            hemorrhageChance = config.Bind("Items :: Reds :: Bleeding Witness", "Hemorrhage Chance", 5f);
            hemorrhageDamage = config.Bind("Items :: Reds :: Bleeding Witness", "Hemorrhage Damage", 4.8f, "Decimal.");
            hemorrhageDuration = config.Bind("Items :: Reds :: Bleeding Witness", "Hemorrhage Duration", 3f, "");
            baseDoTHealing = config.Bind("Items :: Reds :: Bleeding Witness", "Base DoT Healing", 0.005f, "Decimal.");
            stackDoTHealing = config.Bind("Items :: Reds :: Bleeding Witness", "Stack DoT Healing", 0.0025f, "Decimal.");
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
                var healAmount = baseDoTHealing.Value + stackDoTHealing.Value * (stack - 1);
                for (var dotIndex = DotController.DotIndex.Bleed; dotIndex < DotController.DotIndex.Count; dotIndex++)
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
                if (Util.CheckRoll(hemorrhageChance.Value * damageInfo.procCoefficient, attackerBody.master))
                {
                    InflictDotInfo baseDamage = new()
                    {
                        victimObject = victimBody.gameObject,
                        attackerObject = attackerBody.gameObject,
                        totalDamage = attackerBody.damage * hemorrhageDamage.Value,
                        dotIndex = DotIndex.SuperBleed,
                        duration = hemorrhageDuration.Value,
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