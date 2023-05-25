namespace Sandswept.Items.Whites
{
    public class AmberKnife : ItemBase<AmberKnife>
    {
        public override string ItemName => "Amber Knife";

        public override string ItemLangTokenName => "AMBER_KNIFE";

        public override string ItemPickupDesc => "'Critical Strikes' give temporary barrier.";

        public override string ItemFullDescription => "Gain $sd5%$se $ss(+5% per stack)$se $sdcritical chance$se. $sdCritical Strikes$se give a $shtemporary barrier$se for $sh10$se $ss(+5 per stack)$se $shhealth$se.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("AmberKnifePrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("AmberKnifeIcon.png");

        public override void Init(ConfigFile config)
        {
        }

        public override void Hooks()
        {
            RoR2.GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            GetStatCoefficients += GiveCrit;
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;
            if (!damageInfo.crit)
            {
                return;
            }

            if (damageInfo.procCoefficient <= 0)
            {
                return;
            }

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            int stacks = GetCount(attackerBody);

            if (stacks > 0)
            {
                attackerBody.healthComponent.AddBarrier(5f + (5f * stacks));
            }
        }

        public void GiveCrit(CharacterBody sender, StatHookEventArgs args)
        {
            int stacks = GetCount(sender);
            if (stacks > 0)
            {
                args.critAdd += 5f * stacks;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}