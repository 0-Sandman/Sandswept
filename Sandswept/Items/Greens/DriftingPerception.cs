using IL.RoR2.Achievements.Engi;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Drifting Perception")]
    public class DriftingPerception : ItemBase<DriftingPerception>
    {
        public override string ItemName => "Drifting Perception";

        public override string ItemLangTokenName => "DRIFTING_PERCEPTION";

        public override string ItemPickupDesc => "Cloak upon entering combat. Being cloaked increases your 'Critical Strike' chance and 'Critical Damage'. Recharges over time.";

        public override string ItemFullDescription => ("Upon entering combat, become $sucloaked$se for $su" + cloakBuffDuration + "s$se. While $sucloaked$se, increase '$sdCritical Strike$se' chance by $sd" + baseCritChanceGain + "%$se and '$sdCritical Strike$se' damage by $sd" + d(baseCritDamageGain) + "$se $ss(+" + d(stackCritDamageGain) + " per stack)$se. Recharges every $su" + rechargeTime + " seconds$se.").AutoFormat();

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

        public static BuffDef cooldown;
        public static BuffDef ready;

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("DriftingPerceptionHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texDriftingPerception.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Utility };

        public override void Init(ConfigFile config)
        {
            cooldown = ScriptableObject.CreateInstance<BuffDef>();
            cooldown.isHidden = false;
            cooldown.isDebuff = false;
            cooldown.canStack = false;
            cooldown.isCooldown = false;
            cooldown.buffColor = new Color(0.4151f, 0.4014f, 0.4014f, 1f);
            cooldown.iconSprite = Paths.BuffDef.bdCloak.iconSprite;
            ContentAddition.AddBuffDef(cooldown);

            ready = ScriptableObject.CreateInstance<BuffDef>();
            ready.isHidden = true;
            ready.isDebuff = false;
            ready.canStack = false;
            ready.isCooldown = false;
            ready.buffColor = Color.white;
            ready.iconSprite = Paths.BuffDef.bdCloak.iconSprite;
            ContentAddition.AddBuffDef(ready);

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

        public void Start()
        {
            hc = body.healthComponent;
        }

        public void FixedUpdate()
        {
            if (!body.HasBuff(DriftingPerception.cooldown) && stack > 0)
            {
                if (!body.outOfCombat && !body.HasBuff(DriftingPerception.ready))
                {
                    body.AddBuff(DriftingPerception.ready);
                }

                if (body.HasBuff(DriftingPerception.ready))
                {
                    if (!body.HasBuff(RoR2Content.Buffs.Cloak) || !body.HasBuff(RoR2Content.Buffs.CloakSpeed))
                    {
                        body.AddTimedBuffAuthority(RoR2Content.Buffs.Cloak.buffIndex, DriftingPerception.cloakBuffDuration);
                        body.AddTimedBuffAuthority(RoR2Content.Buffs.CloakSpeed.buffIndex, DriftingPerception.cloakBuffDuration);

                        Util.PlaySound("Play_roboBall_attack2_mini_spawn", gameObject);
                        Util.PlaySound("Play_roboBall_attack2_mini_spawn", gameObject);
                        EffectManager.SimpleEffect(Paths.GameObject.SmokescreenEffect, body.corePosition, Quaternion.identity, true);
                    }

                    body.RemoveBuff(DriftingPerception.ready);
                }
                else
                {
                    body.AddTimedBuffAuthority(DriftingPerception.cooldown.buffIndex, DriftingPerception.rechargeTime);

                    Util.PlaySound("Play_roboBall_attack2_mini_laser_stop", gameObject);
                    Util.PlaySound("Play_roboBall_attack2_mini_laser_stop", gameObject);
                }
            }
        }
    }
}