using LookingGlass.ItemStatsNameSpace;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Crowns Diamond")]
    public class CrownsDiamond : ItemBase<CrownsDiamond>
    {
        public override string ItemName => "Crown's Diamond";

        public override string ItemLangTokenName => "CROWNS_DIAMOND";

        public override string ItemPickupDesc => "Being out of danger periodically grants barrier. Barrier decays slower while out of combat or danger.";

        public override string ItemFullDescription => $"Being out of danger grants $sh{barrierGain * 100f}% barrier$se every $sh7$se seconds. $shBarrier$se decays $su{baseBarrierDecayReduction * 100f}%$se $ss(+{stackBarrierDecayReduction * 100f}% per stack)$se $suslower$se while out of combat or danger.".AutoFormat();

        public override string ItemLore =>
        """
        Order: Faux Cut Opal
        Tracking Number: 443*****
        Estimated Delivery: 7/01/2026
        Shipping Method: Top Priority
        Shipping Address: Aganta Palace, Mercury
        Shipping Details:

        The Venusians know that Mercury is responsible for the assassination, and are planning a counterattack. They have tried to keep it under wraps, of course, but Your Majesty's intelligence proves its excellence once again, and we remain a step ahead. Intelligence has heard whispers that Venus has now acquired one of the Neptunian ritual knives, as well, and are now developing ultra-phasic shielding of their own -- though I cannot yet confirm this.

        Your Majesty's appointed Master of Strategy worries that striking first to stop their inevitable counterattack may lead to outright war, and suggests Your Majesty increase defensive measures instead. The Venusians are planning to send a solitary assassin, as we did, in order to infiltrate Mercury easily and avoid blame. Of course, it is Your Majesty's decision -- but if you should accept the Master of Strategy's proposal, she suggests you make use of this trinket.

        As Your Majesty will see, it is a near-perfect replica of the frontmost Agantan Crown diamond, meant to temporarily replace it. Within it is an ultra-phasic shield generator, which will provide protection in the event the assassin is able to reach the Palace. This new technology is unknown to Venus, and beyond them, no one knows of ultra-phasic shielding; it will be not only unexpected, but invisible, requiring no change in Your Majesty's routine or appearance to be effective.

        If Your Majesty would like to proceed with this plan, we have similar diamonds for every other in the Agantan Crown, providing Your Majesty with unparalleled protection. Should I or the Master of Strategy receive word, we will send them to the Palace post-haste.

        For the glory of Immortal Mercury.
        """;
        [ConfigField("Barrier Gain", "Decimal.", 0.2f)]
        public static float barrierGain;

        [ConfigField("Base Barrier Decay Reduction", "Decimal.", 0.15f)]
        public static float baseBarrierDecayReduction;

        [ConfigField("Stack Barrier Decay Reduction", "Decimal.", 0.15f)]
        public static float stackBarrierDecayReduction;

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("CrownsDiamondHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texCrownsDiamond.png");

        public override ItemTag[] ItemTags => [ItemTag.Utility, ItemTag.Healing, ItemTag.CanBeTemporary];

        public override float modelPanelParametersMinDistance => 4f;
        public override float modelPanelParametersMaxDistance => 11f;

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            // IL.RoR2.HealthComponent.ServerFixedUpdate += HealthComponent_ServerFixedUpdate;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            // MoreStats.StatHooks.GetMoreStatCoefficients += StatHooks_GetMoreStatCoefficients;
            RecalculateStatsAPI.GetStatCoefficients += OnGetStatCoefficients;
        }

        private void OnGetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender)
            {
                var stack = GetCount(sender);
                if (stack > 0 && (sender.outOfCombat || sender.outOfDanger))
                {
                    var reduction = MathHelpers.GetHyperbolic(baseBarrierDecayReduction, 100f, baseBarrierDecayReduction + stackBarrierDecayReduction * (stack - 1));

                    args.barrierDecayMult -= reduction;
                }
            }
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Barrier Decay Reduction: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Healing);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.calculateValues = (master, stack) =>
            {
                List<float> values = new()
                {
                    MathHelpers.GetHyperbolic(baseBarrierDecayReduction, 100f, baseBarrierDecayReduction + stackBarrierDecayReduction * (stack - 1))
                };

                return values;
            };

            return itemStatsDef;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (!NetworkServer.active) return;
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

                    if (stack > 0 && body && (body.outOfDanger || body.outOfCombat))
                    {
                        var reduction = MathHelpers.InverseHyperbolicScaling(1f - baseBarrierDecayReduction, stackBarrierDecayReduction, 0f, stack);
                        // Main.ModLogger.LogError("reduction multiplier is " + reduction);
                        // var final = Util.ConvertAmplificationPercentageIntoReductionPercentage(orig * reduction);
                        var final = orig * reduction;
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

            var itemDisplay = SetUpIDRS();

            ItemDisplayRuleDict i = new();

            #region Sandswept Survivors
            /*
            i.Add("RangerBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00387F, 0.11857F, 0.01629F),
                    localAngles = new Vector3(84.61184F, 220.3867F, 47.41245F),
                    localScale = new Vector3(0.14531F, 0.14659F, 0.14531F),

                    followerPrefab = itemDisplay,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );
            */

            i.Add("ElectricianBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.01041F, 0.08162F, -0.00924F),
                    localAngles = new Vector3(85.0407F, 197.8464F, 22.78797F),
                    localScale = new Vector3(0.12683F, 0.11843F, 0.11843F),

                    followerPrefab = itemDisplay,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            #endregion

            return i;

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