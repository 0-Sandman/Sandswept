using HarmonyLib;

namespace Sandswept.Items.VoidWhites
{
    [ConfigSection("Items :: Dissonant Echo")]
    public class DissonantEcho : ItemBase<DissonantEcho>
    {
        public override string ItemName => "Dissonant Echo";

        public override string ItemLangTokenName => "DISSONANT_ECHO";

        public override string ItemPickupDesc => "Taking damage inflicts the attacker with Dissonance. $svCorrupts all Oddly-shaped Opals$se.";

        public override string ItemFullDescription => ("Upon taking damage, inflict the attacker with $suDissonance$se for $su" + baseDuration + "s$se $ss(+" + stackDuration + "s per stack)$se, which reduces $sharmor$se by $sh" + armorReduction + "$se and $sddamage$se by $sd" + d(damageReduction) + "$se. $svCorrupts all Oddly-shaped Opals$se.").AutoFormat();

        public override string ItemLore => "";

        [ConfigField("Base Duration", "", 3f)]
        public static float baseDuration;

        [ConfigField("Stack Duration", "", 3f)]
        public static float stackDuration;

        [ConfigField("Armor Reduction", "", 15f)]
        public static float armorReduction;

        [ConfigField("Damage Reduction", "Decimal.", 0.3f)]
        public static float damageReduction;

        public static BuffDef dissonance;

        public override ItemTier Tier => ItemTier.VoidTier1;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("SunFragmentPrefab.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texSunFragment.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };

        public static GameObject dissonanceTracer;

        public override void Init(ConfigFile config)
        {
            dissonance = ScriptableObject.CreateInstance<BuffDef>();
            dissonance.isHidden = false;
            dissonance.isDebuff = true;
            dissonance.buffColor = Color.red;
            dissonance.isCooldown = false;
            dissonance.canStack = false;
            dissonance.name = "Dissonance -15 Armor and -30% Damage";

            ContentAddition.AddBuffDef(dissonance);

            dissonanceTracer = PrefabAPI.InstantiateClone(Paths.GameObject.VoidSurvivorBeamTracer, "Dissonant Echo Debuff Tracer", false);
            dissonanceTracer.transform.GetChild(0).gameObject.SetActive(false);
            dissonanceTracer.transform.GetChild(1).gameObject.SetActive(false);

            var lineRenderer = dissonanceTracer.GetComponent<LineRenderer>();
            lineRenderer.widthMultiplier = 0.25f;
            lineRenderer.numCapVertices = 10;

            var newMat = GameObject.Instantiate(Paths.Material.matVoidSurvivorBeamTrail);
            newMat.SetTexture("_RemapTex", Paths.Texture2D.texRampDeathBomb);

            lineRenderer.material = newMat;

            var animateShaderAlpha = dissonanceTracer.GetComponent<AnimateShaderAlpha>();
            animateShaderAlpha.timeMax = 0.33f;

            ContentAddition.AddEffect(dissonanceTracer);

            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.Items.ContagiousItemManager.Init += ContagiousItemManager_Init;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(dissonance))
            {
                args.armorAdd -= armorReduction;
                args.damageMultAdd -= damageReduction;
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);
            var attacker = damageInfo.attacker;
            if (attacker)
            {
                var attackerBody = attacker.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    var victimBody = self.body;
                    if (victimBody)
                    {
                        var stack = GetCount(victimBody);
                        if (stack > 0)
                        {
                            if (!attackerBody.HasBuff(dissonance))
                            {
                                attackerBody.AddTimedBuff(dissonance, baseDuration + stackDuration * (stack - 1));
                                EffectManager.SpawnEffect(dissonanceTracer, new EffectData
                                {
                                    start = victimBody.corePosition,
                                    origin = attackerBody.corePosition
                                }, true);
                            }
                        }
                    }
                }
            }
        }

        private void ContagiousItemManager_Init(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            ItemDef.Pair transformation = new()
            {
                itemDef2 = instance.ItemDef,
                itemDef1 = DLC1Content.Items.OutOfCombatArmor
            };
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);
            orig();
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}