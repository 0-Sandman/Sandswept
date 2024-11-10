/* 
 * the bruh
namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Mayas Veil")]
    internal class MayasVeil : ItemBase<MayasVeil>
    {
        public override string ItemName => "Maya's Veil";

        public override string ItemLangTokenName => "MAYAS_VEIL";

        public override string ItemPickupDesc => "Cloak upon using your Equipment. Being cloaked immediately exits combat and danger. Recharges over time.";

        public override string ItemFullDescription => ("Upon using your Equipment, become $sucloaked$se for $su" + cloakBuffDuration + "s$se. While $sucloaked$se, $suexit$se combat and danger. Recharges every $su" + baseRechargeTime + " seconds$se $ss(-" + d(stackRechargeTime) + " per stack)$se.").AutoFormat();

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("RedSpringWaterHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texRedSpringWater.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

        [ConfigField("Cloak Buff Duration", "", 5f)]
        public static float cloakBuffDuration;

        [ConfigField("Base Recharge Time", "Formula for Recharge Time: Base Recharge Time / (1 + Stack Recharge Time * (Stack - 1))", 30f)]
        public static float baseRechargeTime;

        [ConfigField("Stack Recharge Time", "Formula for Recharge Time: Base Recharge Time / (1 + Stack Recharge Time * (Stack - 1))", 0.2f)]
        public static float stackRechargeTime;

        [ConfigField("Works with Gesture?", "", false)]
        public static bool worksWithGesture;

        public static BuffDef cooldown;

        public override void Init(ConfigFile config)
        {
            cooldown = ScriptableObject.CreateInstance<BuffDef>();
            cooldown.isHidden = false;
            cooldown.isDebuff = false;
            cooldown.canStack = false;
            cooldown.isCooldown = false;
            cooldown.buffColor = new Color(0.4151f, 0.4014f, 0.4014f, 1f);
            cooldown.iconSprite = Paths.BuffDef.bdEnergized.iconSprite;
            ContentAddition.AddBuffDef(cooldown);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            EquipmentSlot.onServerEquipmentActivated += EquipmentSlot_onServerEquipmentActivated;
        }

        private void EquipmentSlot_onServerEquipmentActivated(EquipmentSlot equipmentSlot, EquipmentIndex equipmentIndex)
        {
            var inventory = equipmentSlot.inventory;
            if (!inventory)
            {
                return;
            }

            var hasGesture = inventory.GetItemCount(RoR2Content.Items.AutoCastEquipment) > 0;
            if (hasGesture && !worksWithGesture)
            {
                return;
            }

            var body = equipmentSlot.characterBody;
            if (!body)
            {
                return;
            }

            var stack = GetCount(body);
            if (stack <= 0)
            {
                return;
            }

            var cooldownBuffDuration = baseRechargeTime / (1f + stackRechargeTime * (stack - 1));

            if (body.HasBuff(cooldown) || body.HasBuff(RoR2Content.Buffs.Cloak) || body.HasBuff(RoR2Content.Buffs.CloakSpeed))
            {
                return;
            }

            body.AddTimedBuff(RoR2Content.Buffs.Cloak, cloakBuffDuration);
            body.AddTimedBuff(RoR2Content.Buffs.CloakSpeed, cloakBuffDuration);
            body.AddTimedBuff(cooldown, cooldownBuffDuration);

            Util.PlaySound("Play_merc_m2_uppercut", equipmentSlot.gameObject);
            EffectManager.SimpleEffect(Paths.GameObject.SniperTargetHitEffect, body.corePosition, Quaternion.identity, true);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
*/