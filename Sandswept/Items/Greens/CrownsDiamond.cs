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

        public override string ItemLore => "<style=cMono>Order: Faux Cut Opal\r\nTracking Number: 443*****\r\nEstimated Delivery: 7/03/2026\r\nShipping Method: Top Priority\r\nShipping Address: Aganta Palace, Mercury\r\nShipping Details:\r\n\r\n</style>The Venusians know that Mercury is responsible for the assassination, and are planning a counterattack. They have tried to keep it under wraps, of course, but Your Majesty's intelligence proves its excellence once again, and we remain a step ahead. Intelligence has heard whispers that Venus has now acquired one of the Neptunian ritual knives, as well, and are now developing ultra-phasic shielding of their own — though I cannot yet confirm this.\r\n\r\nYour Majesty's appointed Master of Strategy worries that striking first to stop their inevitable counterattack may lead to outright war, and suggests Your Majesty increase defensive measures instead. The Venusians are planning to send a solitary assassin, as we did, in order to infiltrate Mercury easily and avoid blame. Of course, it is Your Majesty's decision — but if you should accept the Master of Strategy's proposal, she suggests you make use of this trinket.\r\n\r\nAs Your Majesty will see, it is a near-perfect replica of the frontmost Agantan Crown diamond, meant to temporarily replace it. Within it is an ultra-phasic shield generator, which will provide protection in the event the assassin is able to reach the Palace. This new technology is unknown to Venus, and beyond them, no one knows of ultra-phasic shielding; it will be not only unexpected, but invisible, requiring no change in Your Majesty's routine or appearance to be effective.\r\n\r\nIf Your Majesty would like to proceed with this plan, we have similar diamonds for every other in the Agantan Crown, providing Your Majesty with unparalleled protection. Should I or the Master of Strategy receive word, we will send them to the Palace post-haste.\r\n\r\nFor the glory of Immortal Mercury.";

        [ConfigField("Barrier Gain", "Decimal.", 0.2f)]
        public static float barrierGain;

        [ConfigField("Base Barrier Decay Reduction", "Decimal.", 0.15f)]
        public static float baseBarrierDecayReduction;

        [ConfigField("Stack Barrier Decay Reduction", "Decimal.", 0.15f)]
        public static float stackBarrierDecayReduction;

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("CrownsDiamondHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texCrownsDiamond.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

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