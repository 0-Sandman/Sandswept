
/*using HarmonyLib;
using LookingGlass.ItemStatsNameSpace;
using MonoMod.Cil;
using RoR2.ExpansionManagement;
using Sandswept.Items.Greens;
using System.Collections;
using System.Linq;
using System.Reflection.Emit;

namespace Sandswept.Items.VoidGreens
{
    [ConfigSection("Items :: Star Gate")]
    public class StarGate : ItemBase<StarGate>
    {
        public override string ItemName => "Star Gate";

        public override string ItemLangTokenName => "STAR_GATE";

        public override string ItemPickupDesc => "The void becomes curious... $svCorrupts all Universal VIP Passes.$se".AutoFormat();

        public override string ItemFullDescription => $"$svVoid seeds$se contain $su{baseVoidSeedCreditMultiplier * 100f}%$se $ss(+{stackVoidSeedCreditMultiplier * 100f}% per stack)$se more enemies and $suinteractables$se. $svCorrupts all Universal VIP Passes.$se".AutoFormat();

        public override string ItemLore =>
        """
        No one knew what would await us in Zone 5.

        We fear the unknown, yet desire its illumination above almost all else. A fascinating conundrum of our limited instincts.

        "Sharks seek the blood of the flesh as we seek the blood of the stones. A mechanism of chemistry and biology beyond our base understanding of the world -- yet the shark requires no explanation to be brought to its prey."

        ...what majestic horror had the rainfall's afterglow led me to?

        One thing is clear -- despite our intentions, the first intelligent being in this Zone of the universe was not the House Beyond. This planet and its neighbors, though utterly alien and now desolate, could not have been formed through the simple logic of the forces of nature. Crumbling and pockmarked with strange holes though they are, the twisted spires of these places were imbued with intent, more obvious in the strange, small spherical cages embedded into the ground throughout the rocky plain.

        Where that intangible varnish is most plain is in the object in front of me, however. A device, crafted by intelligence. Despite its simplicity, I can describe it only as a bringer of dread. Strange flora surround it that I dare not touch, even within my ENV suit.

        After combing through eleven planets, it seems to be the only thing in this ruined place with a semblance of functionality.

        ...what could cause an entire society to simply disappear?

        "When the Promised Land is found, we will have no need for the mortal trappings of our former civilization."

        Perhaps we were off course, and Precedes were right from the start. Perhaps, in fact, Heaven lies not in the cosmos -- but in another plane of reality.

        A message comes through. Two words.

        "Take it."

        I reach out, eagerness overpowering dread, grasping the metal frame.

        As I do, a vision of a time beyond time is thrust upon me. I drop it in shock, but it doesn't matter now. It is a taunt. We have already invited them.

        As the gate falls onto the blueish rock below, its impact draws no ichor.

        How foolish were we, to think ourselves beyond our own dread?

        If the Precedes spoke truth of Heaven, would it not be rational to assume its counterpart fabled within them, too, slumbers somewhere in the folds of existence?

        "What are you waiting for?"

        ...what could cause an entire society to simply disappear?
        """;

        [ConfigField("Base Void Seed Credit Multiplier Add", "Decimal.", 0.5f)]
        public static float baseVoidSeedCreditMultiplier;

        [ConfigField("Stack Void Seed Credit Multiplier Add", "Decimal.", 0.5f)]
        public static float stackVoidSeedCreditMultiplier;

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => null;

        public override ItemTag[] ItemTags => [ItemTag.Utility, ItemTag.InteractableRelated, ItemTag.OnStageBeginEffect, ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.BrotherBlacklist];

        public override float modelPanelParametersMinDistance => 5f;
        public override float modelPanelParametersMaxDistance => 12f;

        public override ItemDef ItemToCorrupt => UniversalVIPPass.instance.ItemDef;

        public static GameObject starGateTracker;

        public static int itemCount = 0;

        public override void Init()
        {
            base.Init();
            starGateTracker = new GameObject("Star Gate Tracker", typeof(SetDontDestroyOnLoad), typeof(StarGateController));
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Interactable and Monster Count Increase: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Utility);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);

            itemStatsDef.calculateValues = (master, stack) =>
            {
                List<float> values = new()
                {
                    baseVoidSeedCreditMultiplier + stackVoidSeedCreditMultiplier * (itemCount - 1)
                };

                return values;
            };

            return itemStatsDef;
        }

        public override void Hooks()
        {
            IL.RoR2.CampDirector.PopulateCamp += AddMoreSpawnAttempts;
            On.RoR2.CampDirector.Start += AddCredits;

            Run.onRunDestroyGlobal += OnRunEnd;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void AddCredits(On.RoR2.CampDirector.orig_Start orig, CampDirector self)
        {
            var lastItemCount = starGateTracker.GetComponent<StarGateController>().lastItemCount;
            var stack = itemCount > lastItemCount ? itemCount : lastItemCount;
            if (stack > 0)
            {
                var modelLocator = self.GetComponent<ModelLocator>();

                var mdlVoidFogEmitter = modelLocator._modelTransform;

                var rangeIndicator = mdlVoidFogEmitter.Find("RangeIndicator");
                rangeIndicator.localScale = Vector3.one * (60f + 3f * stack);

                var decal = self.transform.Find("Decal");
                decal.localScale = Vector3.one * (60f + 3f * stack) * 2.08f;

                self.campMaximumRadius = 60f + 3f * stack;

                // Main.ModLogger.LogError("running addcredits");

                var finalCreditMultiplier = 1f + (baseVoidSeedCreditMultiplier + (stackVoidSeedCreditMultiplier * (stack - 1)));

                // Main.ModLogger.LogError($"interactable credits before: {self.baseInteractableCredit} | monster credits before: {self.baseMonsterCredit}");
                self.baseInteractableCredit = Mathf.CeilToInt(self.baseInteractableCredit * finalCreditMultiplier);
                self.baseMonsterCredit = Mathf.CeilToInt(self.baseMonsterCredit * finalCreditMultiplier);
                // Main.ModLogger.LogError($"interactable credits AFTERRRR: {self.baseInteractableCredit} | monster credits AFTRRRRRRRERR: {self.baseMonsterCredit}");
            }
            orig(self);
        }

        // why the hell does it use a for loop with 10 steps for spawning interactables?
        private void AddMoreSpawnAttempts(ILContext il)
        {
            ILCursor c = new(il);

            bool found = c.TryGotoNext(MoveType.After,

            x => x.MatchLdcI4(10));

            if (!found)
            {
                // Main.ModLogger.LogError("Failed to apply Camp Director Populate Camp hook");
                return;
            }

            c.EmitDelegate<Func<int, int>>((orig) =>
            {
                var lastItemCount = starGateTracker.GetComponent<StarGateController>().lastItemCount;
                var stack = itemCount > lastItemCount ? itemCount : lastItemCount;
                if (stack > 0)
                {
                    // Main.ModLogger.LogError("addmorespawnattempts stack above 0");
                    return orig * (1 + stack);
                }
                return orig;
            });

        }

        private void OnRunEnd(Run run)
        {
            if (starGateTracker)
            {
                starGateTracker.GetComponent<StarGateController>().lastItemCount = 0;
            }
            itemCount = 0;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            itemCount = GetPlayerItemCountGlobal(instance.ItemDef.itemIndex, true);
            if (itemCount <= 0)
            {
                // Main.ModLogger.LogError("item count below or equal to 0, returning");
                return;
            }

            // Main.ModLogger.LogError("setting last item count to item count");

            starGateTracker.GetComponent<StarGateController>().lastItemCount = itemCount;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }

    public class StarGateController : MonoBehaviour
    {
        public int lastItemCount = 0;
    }
}*/
