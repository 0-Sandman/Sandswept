using System.Linq;

namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Their Prominence")]
    public class TheirProminence : ItemBase<TheirProminence>
    {
        public override string ItemName => "Their Prominence";

        public override string ItemLangTokenName => "THEIR_PROMINENCE";

        public override string ItemPickupDesc => "Using a Shrine has a chance to invite the challenge of the Mountain.";

        public override string ItemFullDescription => ("Using a shrine has a $su" + d(baseChance) + "$se $ss(+" + d(stackChance) + " per stack)$se chance to invite the $suchallenge of the Mountain$se.").AutoFormat();

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("FracturedTimepieceHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texFracturedTimepiece.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        [ConfigField("Base Chance", "Decimal.", 0.35f)]
        public static float baseChance;

        [ConfigField("Stack Chance", "Decimal.", 0.15f)]
        public static float stackChance;

        public override void Hooks()
        {
            GlobalEventManager.OnInteractionsGlobal += GlobalEventManager_OnInteractionsGlobal;
        }

        private void GlobalEventManager_OnInteractionsGlobal(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            if (interactor.TryGetComponent<CharacterBody>(out var interactorBody))
            {
                var stack = GetCount(interactorBody);
                if (stack <= 0)
                {
                    return;
                }

                if (interactableObject.TryGetComponent<PurchaseInteraction>(out var purchaseInteraction))
                {
                    if (!purchaseInteraction.isShrine)
                    {
                        return;
                    }

                    var chance = MathHelpers.InverseHyperbolicScaling(baseChance, stackChance, 1f, stack);

                    if (!Util.CheckRoll(chance))
                    {
                        return;
                    }

                    var teleporterInteraction = TeleporterInteraction.instance;
                    if (!teleporterInteraction)
                    {
                        return;
                    }

                    teleporterInteraction.AddShrineStack();
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}