using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Crowns Diamond")]
    public class CrownsDiamond : ItemBase<CrownsDiamond>
    {
        public override string ItemName => "Crown's Diamond";

        public override string ItemLangTokenName => "CROWNS_DIAMOND";

        public override string ItemPickupDesc => "Being out of danger grants barrier. Barrier decays slower while out of combat or danger.";

        public override string ItemFullDescription => ("Being out of danger grants $sh" + d(barrierGain) + "$se of $shmaximum health$se as $shbarrier$se. $shBarrier$se decays $su" + d(baseBarrierDecayReduction) + "$se $ss(+" + d(stackBarrierDecayReduction) + " per stack)$se $suslower$se while out of combat or danger.").AutoFormat();

        public override string ItemLore => "";

        [ConfigField("Barrier Gain", "Decimal.", 0.15f)]
        public static float barrierGain;

        [ConfigField("Base Barrier Decay Reduction", "Decimal.", 0.15f)]
        public static float baseBarrierDecayReduction;

        [ConfigField("Stack Barrier Decay Reduction", "Decimal.", 0.15f)]
        public static float stackBarrierDecayReduction;

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("CrownsDiamondHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texCrownsDiamond.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.BrotherBlacklist };

        public override void Init(ConfigFile config)
        {
            CreateLang();
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
            body.AddItemBehavior<CrownsDiamondController>(GetCount(body));
        }

        private void HealthComponent_ServerFixedUpdate(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt(typeof(CharacterBody).GetProperty("barrierDecayRate").GetGetMethod())))
            {
                c.Index++;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, HealthComponent, float>>((orig, self) =>
                {
                    var body = self.body;
                    var stack = GetCount(body);

                    if (stack > 0 && (body.outOfDanger || body.outOfCombat))
                    {
                        var reduction = MathHelpers.InverseHyperbolicScaling(1f - baseBarrierDecayReduction, stackBarrierDecayReduction, 0f, stack);
                        // Main.ModLogger.LogError("reduction multiplier is " + reduction);
                        var final = Util.ConvertAmplificationPercentageIntoReductionPercentage(orig * reduction);
                        // Main.ModLogger.LogError("barrier decay BEFORE changes is " + orig);
                        //Main.ModLogger.LogError("final barrier DECAY is " + final);
                        // all works fine
                        return final;
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
        public HealthComponent hc;
        public float timer = 0f;
        public float interval = 7f;

        public void Start()
        {
            hc = body.healthComponent;
        }

        public void FixedUpdate()
        {
            if (body.outOfDanger)
            {
                timer += Time.fixedDeltaTime;
            }
            else
            {
                timer = 0f;
            }

            if (timer >= interval && stack > 0)
            {
                hc.AddBarrierAuthority(hc.fullCombinedHealth * CrownsDiamond.barrierGain);
                timer = 0f;
            }
        }
    }
}