using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Crowns Diamond")]
    public class CrownsDiamond : ItemBase<CrownsDiamond>
    {
        public override string ItemName => "Crown's Diamond";

        public override string ItemLangTokenName => "CROWNS_DIAMOND";

        public override string ItemPickupDesc => "Leaving combat grants barrier. Barrier decays slower while out of combat or danger.";

        public override string ItemFullDescription => ("Leaving combat grants $sh" + d(barrierGain) + "$se of $shmaximum health$se as $shbarrier$se. $shBarrier$se decays $su" + d(baseBarrierDecayReduction) + "$se $ss(+" + d(StackBarrierDecayReduction) + " per stack)$se slower while out of combat or danger.").AutoFormat();

        public override string ItemLore => "";

        [ConfigField("Barrier Gain", "Decimal.", 0.25f)]
        public static float barrierGain;

        [ConfigField("Base Barrier Decay Reduction", "Decimal.", 0.2f)]
        public static float baseBarrierDecayReduction;

        [ConfigField("Stack Barrier Decay Reduction", "Decimal.", 0.2f)]
        public static float StackBarrierDecayReduction;

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("UniVIPPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("UniVIPIcon.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateUnlockLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            IL.RoR2.HealthComponent.ServerFixedUpdate += HealthComponent_ServerFixedUpdate;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            body.AddItemBehavior<CrownsDiamondController>(GetCount(body));
        }

        private void HealthComponent_ServerFixedUpdate(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt(typeof(CharacterBody).GetPropertyGetter("get_barrierDecayRate"))))
            {
                c.Index++;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, CharacterBody, float>>((orig, self) =>
                {
                    var stack = GetCount(self);
                    if (stack > 0 && (self.outOfDanger || self.outOfCombat))
                    {
                        var reduction = baseBarrierDecayReduction + StackBarrierDecayReduction * (stack - 1);
                        return Util.ConvertAmplificationPercentageIntoReductionPercentage(orig * reduction);
                    }

                    return orig;
                });
            }
            else
            {
                Main.ModLogger.LogError("Failed to apply Barrier Decay Rate hook");
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }

    public class CrownsDiamondController : CharacterBody.ItemBehavior
    {
        public float timer;
        public float interval = 7f;
        public bool hasLeftCombat = false;
        public bool canGetBarrier = false;
        public HealthComponent hc;

        public void Start()
        {
            hc = body.healthComponent;
        }

        public void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            hasLeftCombat = body.outOfDanger || body.outOfCombat;
            canGetBarrier = false;
            if (timer >= interval && hasLeftCombat)
            {
                hc.AddBarrierAuthority(hc.fullCombinedHealth * CrownsDiamond.barrierGain);
                canGetBarrier = true;
                timer = 0f;
            }
        }
    }
}