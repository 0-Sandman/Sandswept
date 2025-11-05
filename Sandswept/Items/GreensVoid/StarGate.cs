
using HarmonyLib;
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
                    baseVoidSeedCreditMultiplier + stackVoidSeedCreditMultiplier * (stack - 1)
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
}
