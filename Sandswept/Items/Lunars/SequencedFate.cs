using MonoMod.Cil;
using System.Linq;
using Mono.Cecil.Cil;
using UnityEngine;
using static Rewired.Utils.Classes.Utility.ObjectInstanceTracker;
using RoR2.Orbs;
using R2API;

namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Sequenced Fate")]
    public class SequencedFate : ItemBase<SequencedFate>
    {
        public override string ItemName => "Sequenced Fate";

        public override string ItemLangTokenName => "SEQUENCED_FATE";

        public override string ItemPickupDesc => "Shrines of order appear more often and grant extra items.";

        public override string ItemFullDescription => $"Using a shrine of order grants $su{baseExtraItemsCount}$se $ss(+{stackExtraItemsCount} per stack)$se extra items. $suShrines of order appear more frequently$se.".AutoFormat();

        public override string ItemLore => "it's blueprints for shrine of order";

        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => Main.Assets.LoadAsset<GameObject>("PickupTheirProminence.prefab");

        public override Sprite ItemIcon => null;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.InteractableRelated, ItemTag.AIBlacklist };

        public static List<InteractableSpawnCard> shrinesOfOrder = new() { Paths.InteractableSpawnCard.iscShrineRestack, Paths.InteractableSpawnCard.iscShrineRestackSandy, Paths.InteractableSpawnCard.iscShrineRestackSnowy };

        public static GameObject sequencedFateTracker;

        public static int itemCount = 0;

        public override void Init(ConfigFile config)
        {
            sequencedFateTracker = new GameObject("Sequenced Fate Tracker", typeof(SetDontDestroyOnLoad), typeof(SequencedFateController));

            CreateLang();
            CreateItem();
            Hooks();
        }

        [ConfigField("Base Extra Items Count", "", 6)]
        public static int baseExtraItemsCount;

        [ConfigField("Stack Extra Items Count", "", 3)]
        public static int stackExtraItemsCount;

        [ConfigField("Base Shrine Of Order Category Average Selection Weight Percentage", "Decimal.", 0.3f)]
        public static float baseShrineOfOrderCategoryAverageSelectionWeightPercentage;

        [ConfigField("Stack Shrine Of Order Category Average Selection Weight Percentage", "", 0.3f)]
        public static float stackShrineOfOrderCategoryAverageSelectionWeightPercentage;

        public override void Hooks()
        {
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.ShrineRestackBehavior.AddShrineStack += ShrineRestackBehavior_AddShrineStack;
            IL.RoR2.Inventory.ShrineRestackInventory += Inventory_ShrineRestackInventory;
            On.RoR2.ClassicStageInfo.RebuildCards += ClassicStageInfo_RebuildCards;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            itemCount = Util.GetItemCountGlobal(instance.ItemDef.itemIndex, true);
            if (itemCount <= 0)
            {
                return;
            }

            sequencedFateTracker.GetComponent<SequencedFateController>().lastItemCount = itemCount;
        }

        private void ClassicStageInfo_RebuildCards(On.RoR2.ClassicStageInfo.orig_RebuildCards orig, ClassicStageInfo self, DirectorCardCategorySelection forcedMonsterCategory, DirectorCardCategorySelection forcedInteractableCategory)
        {
            orig(self, forcedMonsterCategory, forcedInteractableCategory);

            var lastItemCount = sequencedFateTracker.GetComponent<SequencedFateController>().lastItemCount;

            Main.ModLogger.LogError("stack is " + itemCount);
            Main.ModLogger.LogError("last stack is " + lastItemCount);
            if (itemCount > 0 || lastItemCount > 0)
            {
                Main.ModLogger.LogFatal("trying to add shrine of order category and card");

                float totalWeight = 0f;

                for (int i = 0; i < self.interactableCategories.categories.Length; i++)
                {
                    var category = self.interactableCategories.categories[i];
                    totalWeight += category.selectionWeight;
                }

                Main.ModLogger.LogError("total weight is " + totalWeight);

                var averageWeight = totalWeight / self.interactableCategories.categories.Length;

                Main.ModLogger.LogError("average weight is " + averageWeight);

                var finalWeight = averageWeight * (baseShrineOfOrderCategoryAverageSelectionWeightPercentage + stackShrineOfOrderCategoryAverageSelectionWeightPercentage * (itemCount - 1));

                Main.ModLogger.LogError("final weight is " + finalWeight);

                Array.Resize(ref self.interactableCategories.categories, self.interactableCategories.categories.Length + 1);

                var shrineOfOrderCard = new DirectorCard
                {
                    spawnCard = Paths.InteractableSpawnCard.iscShrineRestack,
                    selectionWeight = 1,
                    minimumStageCompletions = -1
                };

                var newShrineOfOrderCategory = new DirectorCardCategorySelection.Category { name = "Sequenced Fate Shrine of Order", selectionWeight = finalWeight, cards = new DirectorCard[1] { shrineOfOrderCard } };

                self.interactableCategories.categories[self.interactableCategories.categories.Length - 1] = newShrineOfOrderCategory;
            }
        }

        private void Inventory_ShrineRestackInventory(ILContext il)
        {
            ILCursor c = new(il);

            ILLabel sigma = null;

            c.TryGotoNext(MoveType.After,
                x => x.MatchBneUn(out _)
            );

            c.TryGotoNext(MoveType.Before, x => x.MatchLdloc(6));
            int index = c.Index;
            c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0), x => x.MatchLdloc(7));
            c.Index++;
            sigma = c.MarkLabel();
            c.Index = index;
            c.Emit(OpCodes.Ldloc, 8);
            c.EmitDelegate<Func<ItemDef, bool>>((x) => { return x == ItemDef; });
            c.Emit(OpCodes.Brtrue, sigma);
        }

        private void ShrineRestackBehavior_AddShrineStack(On.RoR2.ShrineRestackBehavior.orig_AddShrineStack orig, ShrineRestackBehavior self, Interactor interactor)
        {
            orig(self, interactor);
            var interactorBody = interactor.GetComponent<CharacterBody>();
            var stack = GetCount(interactorBody);
            if (stack > 0 && interactorBody)
            {
                var inventory = interactorBody.inventory;
                if (inventory)
                {
                    var itemCount = baseExtraItemsCount + stackExtraItemsCount * (stack - 1);

                    Dictionary<ItemIndex, int> dict = GenerateRandomItems(itemCount);

                    foreach (var kvp in dict)
                    {
                        ItemTransferOrb.DispatchItemTransferOrb(self.transform.position, inventory, kvp.Key, kvp.Value);
                    }

                    AkSoundEngine.PostEvent(Events.Play_UI_3D_printer_selectItem, self.gameObject);
                }
            }
        }

        private static Dictionary<ItemIndex, int> GenerateRandomItems(int count)
        {
            Dictionary<ItemIndex, int> dict = new();

            WeightedSelection<List<PickupIndex>> weightedSelection = new();
            weightedSelection.AddChoice(Run.instance.availableTier1DropList, 100f);
            weightedSelection.AddChoice(Run.instance.availableTier2DropList, 60f);
            weightedSelection.AddChoice(Run.instance.availableTier3DropList, 4f);
            weightedSelection.AddChoice(Run.instance.availableLunarItemDropList, 4f);
            weightedSelection.AddChoice(Run.instance.availableVoidTier1DropList, 4f);
            weightedSelection.AddChoice(Run.instance.availableVoidTier2DropList, 2.3999999f);
            weightedSelection.AddChoice(Run.instance.availableVoidTier3DropList, 0.16f);

            for (int i = 0; i < count; i++)
            {
                List<PickupIndex> tier = weightedSelection.Evaluate(Random.value);
                PickupIndex pickup = tier[Random.Range(0, tier.Count)];

                ItemDef def = ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(pickup)?.itemIndex ?? ItemIndex.None);

                if (def)
                {
                    if (dict.ContainsKey(def.itemIndex))
                    {
                        dict[def.itemIndex]++;
                    }
                    else
                    {
                        dict.Add(def.itemIndex, 1);
                    }
                }
            }

            return dict;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }

    public class SequencedFateController : MonoBehaviour
    {
        public int lastItemCount = 0;
    }
}