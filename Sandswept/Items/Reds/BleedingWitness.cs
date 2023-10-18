using static RoR2.DotController;

namespace Sandswept.Items.Reds
{
    internal class BleedingWitness : ItemBase<BleedingWitness>
    {
        public override string ItemName => "Bleeding Witness";

        public override string ItemLangTokenName => "BLEEDING_WITNESS";

        public override string ItemPickupDesc => "Your bleed effects deal a percentage of the enemy's maximum health.";

        public override string ItemFullDescription => "$sd5%$se chance to $sdbleed$se enemies for $sd240%$se base damage. Your $sdbleed$se effects additionally deal $sd1%$se $ss(+1% per stack)$se of the enemy's $sdmaximum health$se as damage.".AutoFormat();

        public override string ItemLore => "no";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("WitnessPrefab.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texBleedingWitness.png");

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            onDotInflictedServerGlobal += DotController_onDotInflictedServerGlobal;
        }

        private void DotController_onDotInflictedServerGlobal(DotController dotController, ref InflictDotInfo inflictDotInfo)
        {
            if (inflictDotInfo.dotIndex != DotIndex.Bleed)
            {
                return;
            }

            var victim = inflictDotInfo.victimObject;
            if (!victim)
            {
                return;
            }

            var victimBody = victim.GetComponent<CharacterBody>();
            if (!victimBody)
            {
                return;
            }

            var victimHc = victimBody.healthComponent;
            if (!victimHc)
            {
                return;
            }

            var attacker = inflictDotInfo.attackerObject;
            if (!attacker)
            {
                return;
            }

            var attackerBody = attacker.GetComponent<CharacterBody>();
            if (!attackerBody)
            {
                return;
            }

            var stack = GetCount(attackerBody);
            if (stack > 0)
            {
                var increase = 0.01f * stack;

                InflictDotInfo maxHpDamage = new()
                {
                    victimObject = victim,
                    attackerObject = attacker,
                    totalDamage = increase * victimHc.fullCombinedHealth,
                    dotIndex = DotIndex.Poison,
                    duration = 3f,
                    damageMultiplier = 1f,
                    maxStacksFromAttacker = 1
                };
                InflictDot(ref maxHpDamage);
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
                if (Util.CheckRoll(5f * damageInfo.procCoefficient, attackerBody.master))
                {
                    InflictDotInfo baseDamage = new()
                    {
                        victimObject = victimBody.gameObject,
                        attackerObject = attackerBody.gameObject,
                        totalDamage = attackerBody.damage * 2.4f,
                        dotIndex = DotIndex.Bleed,
                        duration = 3f,
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