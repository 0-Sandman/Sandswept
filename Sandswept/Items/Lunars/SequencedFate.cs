
using MonoMod.Cil;
using System.Linq;
using Mono.Cecil.Cil;
using UnityEngine;
using static Rewired.Utils.Classes.Utility.ObjectInstanceTracker;
using RoR2.Orbs;
using R2API;
using LookingGlass.ItemStatsNameSpace;

namespace Sandswept.Items.Lunars
{
    [ConfigSection("Items :: Sequenced Fate")]
    public class SequencedFate : ItemBase<SequencedFate>
    {
        public override string ItemName => "Sequenced Fate";

        public override string ItemLangTokenName => "SEQUENCED_FATE";

        public override string ItemPickupDesc => "Shrines of Order appear more often and grant additional random items.";

        public override string ItemFullDescription => $"Using a Shrine of Order grants $su{baseExtraItemsCount}$se $ss(+{stackExtraItemsCount} per stack)$se additional random items. $suShrines of order appear more frequently$se.".AutoFormat();

        public override string ItemLore =>
        """
        What is this? Some kind of interface?

        I see. This is not one of my designs, but it is nevertheless a competent one. Though it was not required of you, you have yet again made something of your own. Very good. You learn quickly.

        What you propose is...interesting. Such a construction would create perfect order, as you say, but there is no use for that on this ivory rock. There is nothing of value to sequence besides my own trinkets. As you note, the effects are only useful when greater variety is present. Nevertheless, you may create one here as a test, if you wish.

        What is most curious, however, is the structure; you know the methods of my designs, for you have worked with them -- but this is arranged differently. The capacity to design this proves you are not so foolish as to create a sculpted defiance of my teachings and place it into my hands. Explain the reason for your deviation.

        Fascinating. In my hatred of them, I had not considered using the vermin of the planet for my purposes. I suppose you are proof of their competence. The idea suits me; it would be the ultimate revenge. This shrine is a simple thing, but I -- we -- could create far greater gifts for them. Bind them to my will, and turn them against him. Creating more copies will be trivial. It is nothing but stone, after all, yet within it lies the seeds of this ploy, and my brother's beloved pets will sow them.

        Already, you have shown yourself to be a worthy investment. I invite you to oversee the next construction completely -- with the tincture, you proved your capacity to create, and with this, your capacity to design is proven in turn.
        """;

        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => Main.sandsweptHIFU.LoadAsset<GameObject>("SequencedFateHolder.prefab");

        public override Sprite ItemIcon => Main.sandsweptHIFU.LoadAsset<Sprite>("texSequencedFate.png");

        public override ItemTag[] ItemTags => [ItemTag.Utility, ItemTag.InteractableRelated, ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.BrotherBlacklist, ItemTag.ObjectiveRelated];

        public override string AchievementName => "A Fool Moon [...]";

        public override string AchievementDesc => "Interact with the Shrine of Order on the moon.";

        public static List<InteractableSpawnCard> shrinesOfOrder = new() { Paths.InteractableSpawnCard.iscShrineRestack, Paths.InteractableSpawnCard.iscShrineRestackSandy, Paths.InteractableSpawnCard.iscShrineRestackSnowy };

        public static GameObject sequencedFateTracker;

        public static int itemCount = 0;

        public override void Init()
        {
            base.Init();
            sequencedFateTracker = new GameObject("Sequenced Fate Tracker", typeof(SetDontDestroyOnLoad), typeof(SequencedFateController));
        }

        [ConfigField("Base Extra Items Count", "", 6)]
        public static int baseExtraItemsCount;

        [ConfigField("Stack Extra Items Count", "", 1)]
        public static int stackExtraItemsCount;

        [ConfigField("Base Shrine Of Order Category Average Selection Weight Percentage", "Decimal.", 0.3f)]
        public static float baseShrineOfOrderCategoryAverageSelectionWeightPercentage;

        [ConfigField("Stack Shrine Of Order Category Average Selection Weight Percentage", "", 0.3f)]
        public static float stackShrineOfOrderCategoryAverageSelectionWeightPercentage;

        public override void Hooks()
        {
            Run.onRunDestroyGlobal += OnRunEnd;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.ShrineRestackBehavior.AddShrineStack += ShrineRestackBehavior_AddShrineStack;
            // IL.RoR2.Inventory.ShrineRestackInventory += Inventory_ShrineRestackInventory;
            On.RoR2.ClassicStageInfo.RebuildCards += ClassicStageInfo_RebuildCards;
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Extra Item Count: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Utility);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);

            itemStatsDef.calculateValues = (master, stack) =>
            {
                List<float> values = new()
                {
                    baseExtraItemsCount + stackExtraItemsCount * (itemCount - 1)
                };

                return values;
            };

            return itemStatsDef;
        }

        private void OnRunEnd(Run run)
        {
            if (sequencedFateTracker)
            {
                sequencedFateTracker.GetComponent<SequencedFateController>().lastItemCount = 0;
            }
            itemCount = 0;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            itemCount = GetPlayerItemCountGlobal(instance.ItemDef.itemIndex, true);
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

            // Main.ModLogger.LogError("stack is " + itemCount);
            // Main.ModLogger.LogError("last stack is " + lastItemCount);

            var shrineOfOrderCountBeforeApplying = GameObject.FindObjectsOfType<ShrineRestackBehavior>().Length;

            if (itemCount > 0 || lastItemCount > 0)
            {
                // Main.ModLogger.LogFatal("trying to add shrine of order category and card");

                float totalWeight = 0f;

                for (int i = 0; i < self.interactableCategories.categories.Length; i++)
                {
                    var category = self.interactableCategories.categories[i];
                    totalWeight += category.selectionWeight;
                }

                // Main.ModLogger.LogError("total weight is " + totalWeight);

                var averageWeight = totalWeight / self.interactableCategories.categories.Length;

                // Main.ModLogger.LogError("average weight is " + averageWeight);

                var finalWeight = averageWeight * (baseShrineOfOrderCategoryAverageSelectionWeightPercentage + stackShrineOfOrderCategoryAverageSelectionWeightPercentage * (itemCount - 1));

                // Main.ModLogger.LogError("final weight is " + finalWeight);

                Array.Resize(ref self.interactableCategories.categories, self.interactableCategories.categories.Length + 1);

                var shrineOfOrderCard = new DirectorCard
                {
                    spawnCard = Paths.InteractableSpawnCard.iscShrineRestack,
                    selectionWeight = 1,
                    minimumStageCompletions = -1
                };

                var newShrineOfOrderCategory = new DirectorCardCategorySelection.Category { name = "Sequenced Fate Shrine of Order", selectionWeight = finalWeight, cards = new DirectorCard[1] { shrineOfOrderCard } };

                self.interactableCategories.categories[self.interactableCategories.categories.Length - 1] = newShrineOfOrderCategory;

                // var shrineOfOrderCountAfterApplying = GameObject.FindObjectsOfType<ShrineRestackBehavior>().Length;

                // Main.ModLogger.LogError($"item count: {itemCount}, last item count: {lastItemCount}, stage: {SceneCatalog.mostRecentSceneDef.cachedName}, shrine of order count: {shrineOfOrderCountAfterApplying - shrineOfOrderCountBeforeApplying}");
            }
        }

        private void ShrineRestackBehavior_AddShrineStack(On.RoR2.ShrineRestackBehavior.orig_AddShrineStack orig, ShrineRestackBehavior self, Interactor interactor)
        {
            orig(self, interactor);
            var interactorBody = interactor.GetComponent<CharacterBody>();
            var lastItemCount = sequencedFateTracker.GetComponent<SequencedFateController>().lastItemCount;
            var stack = itemCount > lastItemCount ? itemCount : lastItemCount;
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
                    Util.PlaySound("Play_moonBrother_phase4_step", self.gameObject);
                    Util.PlaySound("Play_huntress_R_rain_start", self.gameObject);
                    Util.PlaySound("Play_engi_seekerMissile_shoot", self.gameObject);
                    Util.PlaySound("Play_engi_seekerMissile_shoot", self.gameObject);
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
            weightedSelection.AddChoice(Run.instance.availableVoidTier2DropList, 2.4f);
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

            var itemDisplay = SetUpFollowerIDRS(1f, 60f);

            return new ItemDisplayRuleDict(new ItemDisplayRule()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(0.4f, 1.25f, 1.4f),
                localScale = new Vector3(0.5f, 0.5f, 0.5f),

                followerPrefab = itemDisplay,
                limbMask = LimbFlags.None,
                followerPrefabAddress = new AssetReferenceGameObject("")
            });
        }
    }

    public class SequencedFateController : MonoBehaviour
    {
        public int lastItemCount = 0;
    }
}
