/*
using IL.RoR2.EntitlementManagement;
using RoR2.Items;

namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Dissonant Veil")]
    internal class DissonantVeil : ItemBase<DissonantVeil>
    {
        public override string ItemName => "Dissonant Veil";

        public override string ItemLangTokenName => "DISSONANT_VEIL";

        public override string ItemPickupDesc => "Cloak upon using your Equipment. Being cloaked immediately exits combat and danger. Recharges over time.";

        public override string ItemFullDescription => $"Upon using your $suEquipment$se, become $sucloaked$se for $su{cloakBuffDuration}s$se. While $sucloaked$se, $suexit$se combat and danger. Recharges every $su{baseRechargeTime} seconds$se $ss(-{stackRechargeTime * 100f}% per stack)$se.".AutoFormat();

        public override string ItemLore =>
        """
        [Sounds of metal tearing on metal, ripping, rending. Gunshots being fired, screams of the crew, screams of unknown entities, complete and utter pandemonium. Everything comes to a screeching halt. Claws stop in their place, guns drop on the ground, crew-members dropping dead. Everything stopping in a reverent fear]

        "Children, surrender. They must all be dead. I sense no excess soul here. Except for you."

        [Footsteps, golden boots clinking against a steel floor]

        "You, your decorations suggest you're supreme among your vessel? Answer me, now."

        [The footsteps get louder]

        "Answer me" [the figure mutters something under its breath about brother being right] "Now"

        <size=50%>"C-captain of the UES C-"</style>

        "Louder. Thief"

        "Captain of the UES Contact Light"

        "Captain...Tell me, why is one of my constructs aboard your vessel?"

        "C-corporate kind of handles that stuff..."

        "Corporate? A thief and a liar. Despicable, not even the one controlling the ship. Hand over the construct now or there won't be a story to tell. No one left to form a message in the sand, begging for saving. No castaway's bottle, no veil to shroud you from danger. Nothing. You'll die like the rest"

        [Distinct sound of a sword being unsheathed]

        "I- UES Policy wont let m-"

        [A sword is swung, and a head rolls, finally, the figure stops]
        """;

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.assets.LoadAsset<GameObject>("PickupTheirProminence.prefab");

        public override Sprite ItemIcon => null;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

        [ConfigField("Cloak Buff Duration", "", 5f)]
        public static float cloakBuffDuration;

        [ConfigField("Base Recharge Time", "Formula for Recharge Time: Base Recharge Time / (1 + Stack Recharge Time * (Stack - 1))", 45f)]
        public static float baseRechargeTime;

        [ConfigField("Stack Recharge Time", "Formula for Recharge Time: Base Recharge Time / (1 + Stack Recharge Time * (Stack - 1))", 0.25f)]
        public static float stackRechargeTime;

        [ConfigField("Works with Gesture?", "", false)]
        public static bool worksWithGesture;

        public static BuffDef cooldown;

        public override void Init()
        {
            base.Init();
            SetUpBuff();
        }

        public void SetUpBuff()
        {
            cooldown = ScriptableObject.CreateInstance<BuffDef>();
            cooldown.isHidden = false;
            cooldown.isDebuff = false;
            cooldown.canStack = false;
            cooldown.isCooldown = false;
            cooldown.buffColor = new Color(0.4151f, 0.4014f, 0.4014f, 1f);
            cooldown.iconSprite = Paths.BuffDef.bdEnergized.iconSprite;
            cooldown.name = "Dissonant Veil - Cooldown";
            ContentAddition.AddBuffDef(cooldown);
        }

        public override void Hooks()
        {
            EquipmentSlot.onServerEquipmentActivated += EquipmentSlot_onServerEquipmentActivated;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
        }

        private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            var stack = GetCount(self);
            if (stack > 0 && (self.hasCloakBuff || buffDef == RoR2Content.Buffs.Cloak))
            {
                self.outOfCombatStopwatch = 9999f;
                self.outOfDangerStopwatch = 9999f;

                if (NetworkServer.active)
                {
                    self.outOfCombat = true;
                    self.outOfDanger = true;
                }

                if (self.healthComponent)
                {
                    self.healthComponent.ForceShieldRegen();
                }
            }
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

            if (body.HasBuff(cooldown) || body.hasCloakBuff)
            {
                return;
            }

            body.AddTimedBuff(RoR2Content.Buffs.Cloak, cloakBuffDuration);

            if (!body.HasBuff(RoR2Content.Buffs.CloakSpeed))
            {
                body.AddTimedBuff(RoR2Content.Buffs.CloakSpeed, cloakBuffDuration);
            }

            var cooldownBuffDuration = baseRechargeTime / (1f + stackRechargeTime * (stack - 1));
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