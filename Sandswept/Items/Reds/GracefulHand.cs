namespace Sandswept.Items.Reds
{
    public class GracefulHand : ItemBase<GracefulHand>
    {
        public override string ItemName => "Graceful Hand";

        public override string ItemLangTokenName => "GRACEFUL_HAND";

        public override string ItemPickupDesc => "Chance to trigger On-Kill effects on hit.";

        public override string ItemFullDescription => StringExtensions.AutoFormat("Gain a $sd4%$se $ss(+1% per stack)$se $sdchance$se on hit to trigger $sdon kill$se effects. Enemies effected by this become immune to it for $sd10$se seconds.");

        public override string ItemLore => "Literally witches ring, L komrade specter.";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("Funny.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("Funny.png");

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;

            var victimBody = report.victimBody;
            if (!victimBody)
            {
                return;
            }

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var stack = GetCount(attackerBody);

            if (stack > 0)
            {
                if (Util.CheckRoll(4f + 1f * (stack - 1) * damageInfo.procCoefficient))
                {
                    DamageReport damageReport = new(damageInfo, victimBody.healthComponent, damageInfo.damage, victimBody.healthComponent.combinedHealth);
                    GlobalEventManager.instance.OnCharacterDeath(damageReport);
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}