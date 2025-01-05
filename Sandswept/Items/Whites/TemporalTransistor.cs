namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Temporal Transistor")]
    internal class TemporalTransistor : ItemBase<TemporalTransistor>
    {
        public override string ItemName => "Temporal Transistor";

        public override string ItemLangTokenName => "TEMPORAL_TRANSISTOR";

        public override string ItemPickupDesc => "Chance on kill to store an extra jump.";

        public override string ItemFullDescription => ("Gain a $su" + chance + "%$se chance on kill to store an $suextra jump$se. Can store up to $su" + baseMaxJumps + "$se $ss(+" + stackMaxJumps + " per stack)$se $suextra jumps$se.").AutoFormat();

        public override string ItemLore => "trans lore";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("RedSpringWaterHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texRedSpringWater.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.OnKillEffect, ItemTag.Utility };

        [ConfigField("Chance", "", 15f)]
        public static float chance;

        [ConfigField("Base Max Jumps", "", 2)]
        public static int baseMaxJumps;

        [ConfigField("Stack Max Jumps", "", 2)]
        public static int stackMaxJumps;

        public static BuffDef extraJump;

        public override void Init(ConfigFile config)
        {
            extraJump = ScriptableObject.CreateInstance<BuffDef>();
            extraJump.isHidden = false;
            extraJump.isDebuff = false;
            extraJump.canStack = true;
            extraJump.isCooldown = false;
            extraJump.buffColor = Color.white;
            extraJump.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffTemporalTransistor.png");
            extraJump.name = "Temporal Transistor - Extra Jump";
            ContentAddition.AddBuffDef(extraJump);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            On.EntityStates.GenericCharacterMain.ProcessJump += GenericCharacterMain_ProcessJump;
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var masterrrrPleadingMasterrr = attackerBody.master;
            if (!masterrrrPleadingMasterrr)
            {
                return;
            }

            var stack = GetCount(attackerBody);
            if (stack > 0 && Util.CheckRoll(chance, masterrrrPleadingMasterrr))
            {
                var storedJumps = attackerBody.GetBuffCount(extraJump);
                var maxStoredJumps = baseMaxJumps + stackMaxJumps * (stack - 1);

                attackerBody.SetBuffCount(extraJump.buffIndex, Mathf.Min(storedJumps + 1, maxStoredJumps));
            }
        }

        private void GenericCharacterMain_ProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump orig, GenericCharacterMain self)
        {
            if (!self.hasCharacterMotor || !self.jumpInputReceived)
            {
                orig(self);
                return;
            }

            var storedJumps = self.characterBody.GetBuffCount(extraJump);

            if (self.characterMotor.jumpCount == self.characterBody.maxJumpCount && storedJumps >= 1)
            {
                var jumpCount = self.characterBody.maxJumpCount;

                self.characterBody.SetBuffCount(extraJump.buffIndex, self.characterBody.GetBuffCount(extraJump) - 1);
                self.characterBody.maxJumpCount += 1;

                orig(self);

                self.characterBody.maxJumpCount = jumpCount;
            }
            else
            {
                orig(self);
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}