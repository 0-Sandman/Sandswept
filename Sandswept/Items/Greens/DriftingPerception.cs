namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Drifting Perception")]
    public class DriftingPerception : ItemBase<DriftingPerception>
    {
        public override string ItemName => "Drifting Perception";

        public override string ItemLangTokenName => "DRIFTING_PERCEPTION";

        public override string ItemPickupDesc => "Cloak upon entering combat. Being cloaked increases your 'Critical Strike' chance and 'Critical Damage'. Recharges over time.";

        public override string ItemFullDescription => ("Upon entering combat, become $sucloaked$se for $su" + cloakBuffDuration + "s$se. While $sucloaked$se, increase '$sdCritical Strike$se' chance by " + baseCritChanceGain + "% and '$sdCritical Strike$se' damage by $sd" + d(baseCritDamageGain) + "$se $ss(+" + d(stackCritDamageGain) + " per stack)$se. Recharges every $su" + rechargeTime + " seconds$se.").AutoFormat();

        public override string ItemLore => "";

        [ConfigField("Base Crit Chance Gain", "", 20f)]
        public static float baseCritChanceGain;

        [ConfigField("Base Crit Damage Gain", "Decimal.", 0.6f)]
        public static float baseCritDamageGain;

        [ConfigField("Stack Crit Damage Gain", "Decimal.", 0.6f)]
        public static float stackCritDamageGain;

        [ConfigField("Cloak Buff Duration", "", 6f)]
        public static float cloakBuffDuration;

        [ConfigField("Recharge Time", "", 25f)]
        public static float rechargeTime;

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("CrownsDiamondHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texCrownsDiamond.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            var stack = GetCount(sender);
            if (stack > 0 && sender.HasBuff(RoR2Content.Buffs.Cloak))
            {
                args.critAdd += baseCritChanceGain;
                args.critDamageMultAdd += baseCritDamageGain + stackCritDamageGain * (stack - 1);
            }
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            body.AddItemBehavior<DriftingPerceptionController>(GetCount(body));
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }

    public class DriftingPerceptionController : CharacterBody.ItemBehavior
    {
        public HealthComponent hc;
        public float timer = 26f;
        public float interval = 25f;
        public bool canProc = true;

        public void Start()
        {
            hc = body.healthComponent;
        }

        public void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;

            if (timer >= interval && stack > 0 && !body.outOfCombat)
            {
                canProc = true;
                timer = 0f;
            }
            else
            {
                canProc = false;
            }

            if (canProc && !body.HasBuff(RoR2Content.Buffs.Cloak) && !body.HasBuff(RoR2Content.Buffs.CloakSpeed))
            {
                body.AddTimedBuffAuthority(RoR2Content.Buffs.Cloak.buffIndex, 5f);
                body.AddTimedBuffAuthority(RoR2Content.Buffs.CloakSpeed.buffIndex, 5f);
                Util.PlaySound("Play_bandit2_shift_enter", gameObject);
                // Play_bandit_shift_jump
            }
        }
    }
}