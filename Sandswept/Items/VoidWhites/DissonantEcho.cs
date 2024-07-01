using HarmonyLib;

namespace Sandswept.Items.VoidWhites
{
    [ConfigSection("Items :: Dissonant Echo")]
    public class DissonantEcho : ItemBase<DissonantEcho>
    {
        public override string ItemName => "Dissonant Echo";

        public override string ItemLangTokenName => "DISSONANT_ECHO";

        public override string ItemPickupDesc => "Upon taking damage, inflict the attacker with Dissonance. $svCorrupts all Oddly-shaped Opals$se.";

        public override string ItemFullDescription => ("Upon taking damage, inflict the attacker with $suDissonance$se for $su" + baseDuration + "s$se $ss(+" + stackDuration + "s per stack)$se, which reduces $sharmor$ by $sh" + armorReduction + "$se and $sddamage$se by $sd" + d(damageReduction) + "$se. $svCorrupts all Oddly-shaped Opals$se.").AutoFormat();

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

        public static GameObject vfx;

        public override void Init(ConfigFile config)
        {
            dissonance = ScriptableObject.CreateInstance<BuffDef>();
            dissonance.isHidden = false;
            dissonance.isDebuff = true;
            dissonance.buffColor = Color.red;
            dissonance.isCooldown = false;
            dissonance.canStack = false;

            ContentAddition.AddBuffDef(dissonance);

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
                            attackerBody.AddTimedBuff(dissonance, baseDuration + stackDuration * (stack - 1));
                        }
                    }
                }
            }
        }

        private void ContagiousItemManager_Init(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            ItemDef.Pair transformation = new()
            {
                itemDef1 = instance.ItemDef,
                itemDef2 = DLC1Content.Items.OutOfCombatArmor
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