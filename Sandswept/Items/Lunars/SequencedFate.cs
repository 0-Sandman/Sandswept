using MonoMod.Cil;
using System.Linq;
using Mono.Cecil.Cil;
using UnityEngine;
using static Rewired.Utils.Classes.Utility.ObjectInstanceTracker;
using RoR2.Orbs;

namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Sequenced Fate")]
    public class SequencedFate : ItemBase<SequencedFate>
    {
        public override string ItemName => "Sequenced Fate";

        public override string ItemLangTokenName => "SEQUENCED_FATE";

        public override string ItemPickupDesc => "Shrines of order appear more often and yield additional items.";

        public override string ItemFullDescription => ("Using a shrine of order yields $su" + baseExtraItemsCount + "$se $ss(+" + stackExtraItemsCount + " per stack)$se additional items, but has a $su" + baseSelfOrderChance + "%$se $ss(+" + stackSelfOrderChance + "% per stack)$se chance to reroll this item. $suShrines of order appear more frequently$se.").AutoFormat();

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => null;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.InteractableRelated, ItemTag.AIBlacklist };

        public static List<InteractableSpawnCard> shrinesOfOrder = new() { Paths.InteractableSpawnCard.iscShrineRestack, Paths.InteractableSpawnCard.iscShrineRestackSandy, Paths.InteractableSpawnCard.iscShrineRestackSnowy };

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        [ConfigField("Base Extra Items Count", "", 6)]
        public static int baseExtraItemsCount;

        [ConfigField("Stack Extra Items Count", "", 3)]
        public static int stackExtraItemsCount;

        [ConfigField("Base Self Order Chance", "", 0f)]
        public static float baseSelfOrderChance;

        [ConfigField("Stack Self Order Chance", "", 20f)]
        public static float stackSelfOrderChance;

        [ConfigField("Shrine Category Weight Multiplier", "", 2f)]
        public static float shrineCategoryWeightMultiplier;

        [ConfigField("Shrine of Order Weight Multiplier", "", 3f)]
        public static float shrineOfOrderWeightMultiplier;

        public override void Hooks()
        {
            On.RoR2.ShrineRestackBehavior.AddShrineStack += ShrineRestackBehavior_AddShrineStack;
            IL.RoR2.Inventory.ShrineRestackInventory += Inventory_ShrineRestackInventory;
            On.RoR2.ClassicStageInfo.Start += ClassicStageInfo_Start;
        }

        private void ClassicStageInfo_Start(On.RoR2.ClassicStageInfo.orig_Start orig, ClassicStageInfo self)
        {
            orig(self);
            var stack = Util.GetItemCountGlobal(instance.ItemDef.itemIndex, true);
            if (stack > 0)
            {
                var categories = self.interactableCategories.categories;
                bool hasShrineOfOrder = false;
                for (int i = 0; i < categories.Length; i++)
                {
                    var categoryIndex = categories[i];
                    if (categoryIndex.name.ToLower().Contains("shrine")) // wharever im pupy
                    {
                        Main.ModLogger.LogError("found shrine category");

                        for (int j = 0; j < categoryIndex.cards.Length; j++)
                        {
                            var cardIndex = categoryIndex.cards[j];
                            if (shrinesOfOrder.Contains(cardIndex.spawnCard))
                            {
                                hasShrineOfOrder = true;
                                Main.ModLogger.LogError("found shrine of order spawn");

                                Main.ModLogger.LogError("shrine of order weight beforehand: " + cardIndex.selectionWeight);
                                cardIndex.selectionWeight = Mathf.RoundToInt(cardIndex.selectionWeight * shrineOfOrderWeightMultiplier);
                                Main.ModLogger.LogError("shrine of order weight AFTERRRRR: " + cardIndex.selectionWeight);
                                break;
                            }
                        }

                        if (hasShrineOfOrder)
                        {
                            Main.ModLogger.LogError("shrine category weight beforehand: " + categoryIndex.selectionWeight);
                            categoryIndex.selectionWeight = Mathf.RoundToInt(categoryIndex.selectionWeight * shrineCategoryWeightMultiplier);
                            Main.ModLogger.LogError("shrine category weight AFTERRRRR: " + categoryIndex.selectionWeight);
                        }
                    }
                }
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
                stack = (int)Mathf.Min(stack, 100f / stackSelfOrderChance);
                var inventory = interactorBody.inventory;
                if (inventory)
                {
                    var itemCount = baseExtraItemsCount + stackExtraItemsCount * (stack - 1);

                    Dictionary<ItemIndex, int> dict = GenerateRandomItems(itemCount);

                    foreach (var kvp in dict)
                    {
                        ItemTransferOrb.DispatchItemTransferOrb(self.transform.position, inventory, kvp.Key, kvp.Value);

                        AkSoundEngine.PostEvent(Events.Play_UI_3D_printer_selectItem, self.gameObject);
                    }
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
}