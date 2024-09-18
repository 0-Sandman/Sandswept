namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Sacrificial Band")]
    public class SacrificialBand : ItemBase<SacrificialBand>
    {
        public override string ItemName => "Sacrificial Band";

        public override string ItemLangTokenName => "SACRIFICIAL_BAND";

        public override string ItemPickupDesc => "High damage hits also make enemies bleed. Recharges over time.";

        public override string ItemFullDescription => ("Hits that deal $sdmore than 400% damage$se also inflict $sd" + baseBleedCount + "$se $ss(+" + stackBleedCount + " per stack)$se $sdbleeds$se on enemies for each $sd" + d(damageScalar) + "$se of $sdskill damage$se. Recharges every $su10$se seconds.").AutoFormat();

        public override string ItemLore => "\"When we draw our final breaths,\r\nWhen N'kuhana's grasp entwines us,\r\nMay our patience and our solace\r\nClear the clouds of deathly silence.\r\nWill you live with me?\"\r\n\r\n- The Syzygy of Io and Europa";

        [ConfigField("Base Bleed Count", "", 1)]
        public static float baseBleedCount;

        [ConfigField("Stack Bleed Count", "", 1)]
        public static float stackBleedCount;

        [ConfigField("Per Skill Damage Scalar", "Decimal.", 1.1f)]
        public static float damageScalar;

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("SacrificialBandHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texSacrificialBand.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };

        public static BuffDef ready;
        public static BuffDef cooldown;

        public static GameObject vfx;

        public override void Init(ConfigFile config)
        {
            ready = ScriptableObject.CreateInstance<BuffDef>();
            ready.isDebuff = false;
            ready.canStack = false;
            ready.isHidden = false;
            ready.isCooldown = false;
            ready.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffSacrificialBandReady.png");
            ready.buffColor = new Color32(160, 0, 5, 255);
            ready.name = "Sacrificial Band Ready";

            ContentAddition.AddBuffDef(ready);

            cooldown = ScriptableObject.CreateInstance<BuffDef>();
            cooldown.canStack = true;
            cooldown.isDebuff = false;
            cooldown.isHidden = false;
            cooldown.isCooldown = true;
            cooldown.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffSacrificialBandCooldown.png");
            cooldown.buffColor = new Color(0.4151f, 0.4014f, 0.4014f, 1f);
            cooldown.name = "Sacrificial Band Cooldown";

            ContentAddition.AddBuffDef(cooldown);

            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            var inventory = body.inventory;
            if (!inventory)
            {
                return;
            }
            body.AddItemBehavior<SacrificialBandController>(inventory.GetItemCount(instance.ItemDef));
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            bool triggered = false;
            CharacterBody attackerBody = null;
            var attacker = damageInfo.attacker;
            if (attacker && victim)
            {
                attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    var stack = GetCount(attackerBody);
                    if (stack > 0)
                    {
                        var skillDamage = damageInfo.damage / attackerBody.damage;

                        if (attackerBody.HasBuff(ready) && skillDamage >= 4f)
                        {
                            triggered = true;

                            var realerDamageScalar = 1f / damageScalar;
                            var scaledSkillDamage = skillDamage * realerDamageScalar;
                            var roundedSkillDamage = Mathf.RoundToInt(scaledSkillDamage) * stack;

                            for (int i = 0; i < roundedSkillDamage; i++)
                            {
                                DotController.InflictDot(victim, attacker, DotController.DotIndex.Bleed, 4f * damageInfo.procCoefficient, 1f, uint.MaxValue);
                            }

                            Util.PlaySound("Play_bleedOnCritAndExplode_explode", victim);
                            Util.PlaySound("Play_bleedOnCritAndExplode_explode", victim);
                            Object.Instantiate(GlobalEventManager.CommonAssets.bleedOnHitAndExplodeBlastEffect, victim.transform.position, Quaternion.identity);

                            // damageInfo.procChainMask.AddProc(ProcType.PlasmaCore);
                        }
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

    public class SacrificialBandController : CharacterBody.ItemBehavior
    {
        public CharacterBody body;
        public bool shouldRun = false;

        public void Start()
        {
            body = GetComponent<CharacterBody>();
            if (!body.HasBuff(SacrificialBand.ready) && stack > 0)
            {
                body.AddBuff(SacrificialBand.ready);
                shouldRun = true;
            }
        }

        public void FixedUpdate()
        {
            if (!body || !shouldRun)
            {
                return;
            }

            if (!body.HasBuff(SacrificialBand.cooldown) && !body.HasBuff(SacrificialBand.ready))
            {
                body.AddBuff(SacrificialBand.ready);
            }
        }

        public void OnDestroy()
        {
            if (body.HasBuff(SacrificialBand.ready))
            {
                body.RemoveBuff(SacrificialBand.ready);
            }
            if (body.HasBuff(SacrificialBand.cooldown))
            {
                body.RemoveBuff(SacrificialBand.cooldown);
            }
        }
    }
}