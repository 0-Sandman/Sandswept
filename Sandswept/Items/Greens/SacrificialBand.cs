namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Sacrificial Band")]
    public class SacrificialBand : ItemBase<SacrificialBand>
    {
        public override string ItemName => "Sacrificial Band";

        public override string ItemLangTokenName => "SACRIFICIAL_BAND";

        public override string ItemPickupDesc => "High damage hits also make enemies bleed. Recharges over time.";

        public override string ItemFullDescription => ("Hits that deal $sdmore than 400% damage$se also inflict $sd" + baseBleedCount + "$se $ss(+" + stackBleedCount + " per stack)$se $sdbleeds$se on enemies for each $sd" + d(damageScalar) + "%$se of $sdskill damage$se. Recharges every $su10$se seconds.").AutoFormat();

        public override string ItemLore => "Some say a guy called HIFU wanted to name this item Band of Sacrifice as a funny reference but other devs disagreed with it because of naming convention.. :joker:";

        [ConfigField("Base Bleed Count", "", 1)]
        public static float baseBleedCount;

        [ConfigField("Stack Bleed Count", "", 1)]
        public static float stackBleedCount;

        [ConfigField("Per Skill Damage Scalar", "Decimal.", 1.2f)]
        public static float damageScalar;

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("CrownsDiamondHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texCrownsDiamond.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };

        public static BuffDef ready;
        public static BuffDef cooldown;

        public override void Init(ConfigFile config)
        {
            ready = ScriptableObject.CreateInstance<BuffDef>();
            ready.isDebuff = false;
            ready.canStack = false;
            ready.isHidden = false;
            ready.isCooldown = false;
            ready.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texCrownsDiamond.png");
            ready.buffColor = Color.white;

            ContentAddition.AddBuffDef(ready);

            cooldown = ScriptableObject.CreateInstance<BuffDef>();
            cooldown.canStack = true;
            cooldown.isDebuff = false;
            cooldown.isHidden = false;
            cooldown.isCooldown = true;
            cooldown.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texCrownsDiamond.png");
            cooldown.buffColor = Color.gray;

            ContentAddition.AddBuffDef(cooldown);

            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
            var inventory = body.inventory;
            if (!inventory)
            {
                return;
            }
            if (body.GetComponent<SacrificialBandController>() == null)
            {
                body.gameObject.AddComponent<SacrificialBandController>();
            }
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            bool triggered = false;
            CharacterBody attackerBody = null;
            var attacker = damageInfo.attacker;
            if (attacker && victim)
            {
                attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                var skillDamage = damageInfo.damage / attackerBody.damage;
                if (attackerBody && skillDamage >= 4f)
                {
                    if (attackerBody.HasBuff(ready))
                    {
                        triggered = true;

                        var stack = GetCount(attackerBody);
                        if (stack > 0)
                        {
                            var realerDamageScalar = 1f / damageScalar;
                            var scaledSkillDamage = skillDamage * realerDamageScalar;
                            var roundedSkillDamage = Mathf.RoundToInt(scaledSkillDamage);

                            for (int i = 0; i < roundedSkillDamage; i++)
                            {
                                DotController.InflictDot(victim, attacker, DotController.DotIndex.Bleed, 4f * damageInfo.procCoefficient, 1f, uint.MaxValue);
                            }
                        }

                        damageInfo.procChainMask.AddProc(ProcType.PlasmaCore);
                    }
                }
            }

            orig(self, damageInfo, victim);

            if (triggered && attackerBody.HasBuff(ready))
            {
                attackerBody.RemoveBuff(ready);
                for (int j = 1; j <= 10f; j++)
                {
                    attackerBody.AddTimedBuff(cooldown, j);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }

    public class SacrificialBandController : MonoBehaviour
    {
        public CharacterBody body;

        public void Start()
        {
            body = GetComponent<CharacterBody>();
            if (!body)
            {
                Main.ModLogger.LogError("no body found");
                return;
            }

            if (!body.HasBuff(SacrificialBand.ready))
            {
                Main.ModLogger.LogError("gave ready buff to " + body.name);
                body.AddBuff(SacrificialBand.ready);
                Main.ModLogger.LogError("ready buff is " + SacrificialBand.ready);
                Main.ModLogger.LogError("cooldown buff is " + SacrificialBand.cooldown);
                Main.ModLogger.LogError("does body have ready buff? " + body.HasBuff(SacrificialBand.ready));
            }
        }
    }
}