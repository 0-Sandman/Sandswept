using LookingGlass.ItemStatsNameSpace;
using UnityEngine.SceneManagement;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Universal VIP Pass")]
    public class UniversalVIPPass : ItemBase<UniversalVIPPass>
    {
        public override string ItemName => "Universal VIP Pass";

        public override string ItemLangTokenName => "UNIVERSAL_VIP_PASS";

        public override string ItemPickupDesc => "Category chests have a chance to drop an extra item.";

        public override string ItemFullDescription => $"$suCategory chests$se have a $su{baseChance * 100f}%$se $ss(+{stackChance * 100f}% per stack)$se chance of dropping $su{extraItems}$se $suextra item$se.".AutoFormat();

        public override string ItemLore =>
        """
        "I'm sorry, sir, this is a restricted area. We can't allow you in."

        "Special orders from UES. It would be in your best interests to make an exception."

        "I'm afraid I can't do that. We received specific instructions from two of our guests not to let in anyone from UES. It'd be against policy to betray their trust."

        "Those two 'guests' have stolen from the UESC. You're harboring criminals. If you do not let us in, you will be obstructing justice in violation of interplanetary law."

        "The UESC does not have legal jurisdiction over Pluto, sir. We're under no obligation to let you in. If you don't vacate the premises, I'll be forced to call security, and make no mistake, our security is the best of the best."

        ...

        "...is that...?"

        "Yes. Universal. This is serious business. I'll ask one more time: let us through."

        "...right away, sir."
        """;
        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.mainAssets.LoadAsset<GameObject>("UniVIPPrefab.prefab");

        public override Sprite ItemIcon => Main.mainAssets.LoadAsset<Sprite>("UniVIPIcon.png");

        [ConfigField("Base Chance", "Decimal.", 0.4f)]
        public static float baseChance;

        [ConfigField("Stack Chance", "Decimal.", 0.4f)]
        public static float stackChance;

        [ConfigField("Extra Items", "", 1)]
        public static int extraItems;

        public static GameObject vfx;

        public static Color32 pink = new(229, 0, 218, 255);

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.InteractableRelated, ItemTag.AIBlacklist, ItemTag.CannotDuplicate };

        public override void Init()
        {
            base.Init();
            SetUpVFX();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Extra Item Chance: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Utility);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            if (Main.cursedConfig.Value)
            {
                itemStatsDef.descriptions.Add("UwU~ Love is Love! ");
                itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Healing);
                itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            }
            itemStatsDef.calculateValues = (master, stack) =>
            {
                List<float> values = new()
                {
                    MathHelpers.InverseHyperbolicScaling(baseChance, stackChance, 1f, stack)
                };

                if (Main.cursedConfig.Value)
                {
                    values.Add(1f);
                }

                return values;
            };

            return itemStatsDef;
        }

        public void SetUpVFX()
        {
            var uniVip = Main.mainAssets.LoadAsset<GameObject>("UniVIPPrefab.prefab");
            var uniVipMat = uniVip.transform.GetChild(0).GetComponent<MeshRenderer>().material;
            uniVipMat.SetColor("_Color", new Color32(205, 205, 205, 249));

            vfx = PrefabAPI.InstantiateClone(Paths.GameObject.ShrineChanceDollUseEffect, "Universal VIP Paws VFX", false);

            var transform = vfx.transform;

            var coloredLightShafts = transform.Find("ColoredLightShafts").GetComponent<ParticleSystemRenderer>();
            //
            var newColoredLightShaftMat = new Material(Paths.Material.matClayBossLightshaft);
            newColoredLightShaftMat.SetFloat("_AlphaBoost", 4f);
            newColoredLightShaftMat.SetFloat("_AlphaBias", 0.1f);
            newColoredLightShaftMat.SetColor("_TintColor", new Color32(255, 84, 0, 255));

            coloredLightShafts.material = newColoredLightShaftMat;

            transform.Find("ColoredLightShaftsBalance").GetComponent<ParticleSystemRenderer>().material = newColoredLightShaftMat;

            var coloredDustBalance = transform.Find("ColoredDustBalance").GetComponent<ParticleSystemRenderer>();
            var tex = Main.cursedConfig.Value ? Main.hifuSandswept.LoadAsset<Texture2D>("texPawMask.png") : Paths.Texture2D.texSpark1Mask;
            var newColoredDustBalanceMat = new Material(Paths.Material.matChanceShrineDollEffect);
            newColoredDustBalanceMat.SetColor("_TintColor", pink);
            newColoredDustBalanceMat.SetTexture("_RemapTex", Paths.Texture2D.texRampAreaIndicator);
            newColoredDustBalanceMat.SetFloat("_Boost", 12f);
            newColoredDustBalanceMat.SetTexture("_MainTex", tex);

            VFXUtils.MultiplyScale(vfx, 3f);
            VFXUtils.MultiplyDuration(vfx, 1.5f);
            VFXUtils.AddLight(vfx, pink, 100f, 20f, 2f);

            coloredDustBalance.material = newColoredDustBalanceMat;

            ContentAddition.AddEffect(vfx);
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnInteractionBegin += GlobalEventManager_OnInteractionBegin;
        }

        private void GlobalEventManager_OnInteractionBegin(On.RoR2.GlobalEventManager.orig_OnInteractionBegin orig, GlobalEventManager self, Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            bool isCategoryChest = false;
            var chestBehavior = interactableObject.GetComponent<ChestBehavior>();
            if (chestBehavior)
            {
                var purchaseInteraction = interactableObject.GetComponent<PurchaseInteraction>();
                if (purchaseInteraction)
                {
                    isCategoryChest = isCategoryChest || purchaseInteraction.displayNameToken.ToLower().Contains("category") || purchaseInteraction.contextToken.ToLower().Contains("category");
                }
                var genericDisplayNameProvider = interactableObject.GetComponent<GenericDisplayNameProvider>();
                if (genericDisplayNameProvider)
                {
                    isCategoryChest = isCategoryChest || genericDisplayNameProvider.displayToken.ToLower().Contains("category");
                }

                if (isCategoryChest)
                {
                    if (interactor)
                    {
                        var interactorBody = interactor.GetComponent<CharacterBody>();
                        if (interactorBody)
                        {
                            var stack = GetCount(interactorBody);
                            var scale = 0.5f + Run.instance.participatingPlayerCount * 0.5f;

                            var chance = MathHelpers.InverseHyperbolicScaling(baseChance, stackChance, 1f, stack) * 100f;

                            if (stack > 0 && Util.CheckRoll(chance / scale))
                            {
                                var isCategoryChestFinal = interactableObject.name.ToLower().Contains("category") || isCategoryChest;
                                if (isCategoryChestFinal)
                                {
                                    chestBehavior.dropCount = 1 + extraItems;

                                    Util.PlaySound("Play_UI_commandHUD_select", chestBehavior.gameObject);
                                    Util.PlaySound("Play_UI_commandHUD_select", chestBehavior.gameObject);

                                    EffectManager.SpawnEffect(vfx, new EffectData
                                    {
                                        origin = interactableObject.transform.position,
                                        rotation = Quaternion.identity,
                                        scale = 3f,
                                        color = pink
                                    }, true);

                                    Util.PlaySound("Play_UI_obj_casinoChest_open", interactableObject);
                                    Util.PlaySound("Play_ui_obj_lunarPool_activate", interactableObject);

                                    if (Main.cursedConfig.Value)
                                    {
                                        Chat.AddMessage("<color=#DC4C7B>Universal VIP Paws :3 x3 OwO UwU :3 <3</color>");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            orig(self, interactor, interactable, interactableObject);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}