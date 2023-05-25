namespace Sandswept.Items.Whites
{
    public class Tinkers : ItemBase<Tinkers>
    {
        public override string ItemName => "Tinkers";

        public override string ItemLangTokenName => "TINKERS";

        public override string ItemPickupDesc => "Your drones gain damage and attack speed.";

        public override string ItemFullDescription => "Your drones gain 10% (+10% per stack) damage and 5% (+5% per stack) attack speed.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => null;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            GetStatCoefficients += GiveStats;
        }

        private void GiveStats(CharacterBody body, StatHookEventArgs args)
        {
            var isDrone = body.master && body.master.minionOwnership && body.master.minionOwnership.group != null && body.master.minionOwnership.group.isMinion;
            if (isDrone)
            {
                var master = body.master.minionOwnership.ownerMaster;

                var masterBody = master.GetBody();

                int stacks = GetCount(masterBody);

                if (stacks > 0)
                {
                    // Debug.Log(body.name);
                    args.damageMultAdd += 0.10f * stacks;
                    /*
                    if (sender.name.Contains("Drone1Body") || sender.name.Contains("Drone2Body")) // :despair:
                    {
                        args.cooldownMultAdd += 1f - 0.05f * stacks;
                        return;
                    }
                    */
                    // :despair:
                    args.attackSpeedMultAdd += 0.05f * stacks;
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}