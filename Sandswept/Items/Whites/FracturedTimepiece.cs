using LookingGlass.ItemStatsNameSpace;
using System.Linq;
using UnityEngine;

namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Fractured Timepiece")]
    public class FracturedTimepiece : ItemBase<FracturedTimepiece>
    {
        public override string ItemName => "Fractured Timepiece";

        public override string ItemLangTokenName => "FRACTURED_TIMEPIECE";

        public override string ItemPickupDesc => "Using your Utility skill heals you and reduces Special skill cooldown.";

        public override string ItemFullDescription => $"Upon using your $suUtility skill$se, $shheal$se for $sh{basePercentHealing * 100f}%$se $ss(+{stackPercentHealing * 100f}% per stack)$se of your $shmaximum health$se and $sureduce Special skill cooldown$se by $su{specialCooldownReduction * 100f}%$se.".AutoFormat();

        public override string ItemLore =>
        """
        Order: Timepiece
        Tracking Number: 864*******
        Estimated Delivery: 02/23/2054
        Shipping Method: Priority
        Shipping Address: Hall of the Revered, Mars
        Shipping Details:

        Our team uncovered this in the desert where the old Hall was, before it burned down. I guess someone really wanted to protect it from the Purge, since it was carefully wrapped and boxed where we found it. You can probably glean more knowledge from it than we can, and it was the Hall's to begin with in any case.

        The box had a note in it from the Time Keeper of the era, too, which I've included in the package. Nobody here can read the old language, so hopefully you can make some sense of it.
        """;
        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("FracturedTimepieceHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texFracturedTimepiece.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Healing, ItemTag.Utility, ItemTag.AIBlacklist };

        public override float modelPanelParametersMinDistance => 4f;
        public override float modelPanelParametersMaxDistance => 12f;

        public static List<string> blacklistedSkills = new() { "MAGE_UTILITY_ICE_NAME", "ENGI_SKILL_HARPOON_NAME" };

        public static GameObject healVFX;
        public static GameObject cdrVFX;

        [ConfigField("Base Percent Healing", "Decimal.", 0.05f)]
        public static float basePercentHealing;

        [ConfigField("Stack Percent Healing", "Decimal.", 0.05f)]
        public static float stackPercentHealing;

        [ConfigField("Special Cooldown Reduction", "Decimal.", 0.15f)]
        public static float specialCooldownReduction;

        public override void Init()
        {
            base.Init();
            SetUpVFX();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Healing: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Healing);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.PercentHealth);
            itemStatsDef.descriptions.Add("Special Skill Cooldown Reduction: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Utility);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Seconds);
            itemStatsDef.calculateValues = (master, stack) =>
            {
                float healValue = 0f;
                float cooldownReductionValue = 0f;
                var body = master.GetBody();
                if (body)
                {
                    var skillLocator = body.skillLocator;
                    if (skillLocator)
                    {
                        var utility = skillLocator.utility;
                        var special = skillLocator.special;

                        if (utility && special)
                        {
                            var utilitySkillDef = utility.skillDef;
                            var specialSkillDef = special.skillDef;
                            if (utilitySkillDef && specialSkillDef && !blacklistedSkills.Contains(utilitySkillDef.skillNameToken))
                            {
                                healValue = basePercentHealing + stackPercentHealing * (stack - 1) / utilitySkillDef.baseMaxStock;
                                cooldownReductionValue = special.baseRechargeInterval * specialCooldownReduction / utilitySkillDef.baseMaxStock;
                            }
                        }
                    }
                }

                List<float> values = new()
                {
                    healValue,
                    cooldownReductionValue
                };

                return values;
            };

            return itemStatsDef;
        }

        public void SetUpVFX()
        {
            healVFX = PrefabAPI.InstantiateClone(Paths.GameObject.MedkitHealEffect, "Fractured Timepiece Heal VFX", false);
            var effectComponent = healVFX.GetComponent<EffectComponent>();
            effectComponent.applyScale = true;

            var cdrRamp = Paths.Texture2D.texRampHuntressSoft2;

            var trans = healVFX.transform;

            for (int i = 0; i < trans.childCount; i++)
            {
                var child = trans.GetChild(i);
                child.transform.localScale = Vector3.one * 1.5f;
            }

            var healSpinner = trans.GetChild(0).GetComponent<ParticleSystemRenderer>();

            var newHealSpinnerMat = new Material(Paths.Material.matHealTrail);
            newHealSpinnerMat.SetTexture("_RemapTex", Paths.Texture2D.texRampGold);
            newHealSpinnerMat.SetFloat("_Boost", 9.9f);

            healSpinner.trailMaterial = newHealSpinnerMat;

            var healCrosses = trans.GetChild(1).GetComponent<ParticleSystemRenderer>();

            var newHealCrossesMat = new Material(Paths.Material.matHealingCross);
            newHealCrossesMat.SetTexture("_RemapTex", Paths.Texture2D.texRampMinorConstructElectric);
            newHealCrossesMat.SetFloat("_Boost", 5f);

            healCrosses.material = newHealCrossesMat;

            ContentAddition.AddEffect(healVFX);

            cdrVFX = PrefabAPI.InstantiateClone(Paths.GameObject.MedkitHealEffect, "Fractured Timepiece CDR VFX", false);
            var effectComponent2 = cdrVFX.GetComponent<EffectComponent>();
            effectComponent2.applyScale = true;
            effectComponent2.soundName = "";

            var trans2 = cdrVFX.transform;

            var cdrSpinner = trans2.GetChild(0).GetComponent<ParticleSystemRenderer>();
            cdrSpinner.transform.eulerAngles = new Vector3(90f, 0f, 0f);
            cdrSpinner.transform.localScale = Vector3.one * 1.5f;
            var cdrSpinnerMain = cdrSpinner.GetComponent<ParticleSystem>().main;
            cdrSpinnerMain.startDelay = 0.1f;
            cdrSpinnerMain.startLifetime = 0.8f;

            var newCdrSpinnerMat = new Material(Paths.Material.matHealTrail);
            newCdrSpinnerMat.SetTexture("_RemapTex", cdrRamp);
            newCdrSpinnerMat.SetFloat("_Boost", 4.8f);

            cdrSpinner.trailMaterial = newCdrSpinnerMat;

            var cdrCrosses = trans2.GetChild(1).GetComponent<ParticleSystemRenderer>();
            cdrCrosses.transform.localScale = Vector3.one * 2f;
            cdrCrosses.transform.eulerAngles = new Vector3(90f, 0f, 0f);

            var mask = Paths.Texture2D.texVFXExplosionMask;

            var newCdrCrossesMat = new Material(Paths.Material.matHealingCross);
            newCdrCrossesMat.SetTexture("_RemapTex", cdrRamp);
            newCdrCrossesMat.SetTexture("_MainTex", mask);
            newCdrCrossesMat.SetTexture("_Cloud1Tex", mask);
            newCdrCrossesMat.SetFloat("_Boost", 5f);
            newCdrCrossesMat.SetFloat("_AlphaBoost", 2.079557f);
            newCdrCrossesMat.SetFloat("_AlphaBias", 0.2769386f);

            cdrCrosses.material = newCdrCrossesMat;

            ContentAddition.AddEffect(cdrVFX);
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            On.EntityStates.Mage.Weapon.PrepWall.OnExit += PrepWall_OnExit;
            On.EntityStates.Engi.EngiMissilePainter.Fire.FireMissile += Fire_FireMissile;
        }

        private void Fire_FireMissile(On.EntityStates.Engi.EngiMissilePainter.Fire.orig_FireMissile orig, EntityStates.Engi.EngiMissilePainter.Fire self, HurtBox target, Vector3 position)
        {
            orig(self, target, position);
            var skillLocator = self.skillLocator;
            if (skillLocator)
            {
                var skill = skillLocator.utility;
                TryHeal(self.characterBody, skill);
            }
        }

        public static void TryHeal(CharacterBody characterBody, GenericSkill skill, bool checkUtilityAndBlacklist = false)
        {
            if (!characterBody)
            {
                return;
            }

            if (skill == null)
            {
                return;
            }

            var inventory = characterBody.inventory;
            if (!inventory)
            {
                return;
            }

            var stack = inventory.GetItemCount(instance.ItemDef);
            var skillLocator = characterBody.GetComponent<SkillLocator>();

            var passesCondition = stack > 0 && (!checkUtilityAndBlacklist || skillLocator && skill == skillLocator.utility && skill.cooldownRemaining > 0 && !blacklistedSkills.Contains(skill.skillDef.skillNameToken));

            if (passesCondition)
            {
                var special = skillLocator.special;
                if (special && special.stock < special.maxStock)
                {
                    special.rechargeStopwatch += special.baseRechargeInterval * specialCooldownReduction / skill.skillDef.baseMaxStock;
                }
                characterBody.healthComponent?.HealFraction((basePercentHealing + stackPercentHealing * (stack - 1)) / skill.skillDef.baseMaxStock, default);

                var effectData = new EffectData()
                {
                    origin = characterBody.gameObject.transform.position,
                    scale = 4f
                };
                effectData.SetNetworkedObjectReference(characterBody.gameObject);

                EffectManager.SpawnEffect(healVFX, effectData, true);
                EffectManager.SpawnEffect(cdrVFX, effectData, true);
            }
        }

        private void PrepWall_OnExit(On.EntityStates.Mage.Weapon.PrepWall.orig_OnExit orig, EntityStates.Mage.Weapon.PrepWall self)
        {
            if (!self.outer.destroying)
            {
                if (self.goodPlacement)
                {
                    var skillLocator = self.skillLocator;
                    if (skillLocator)
                    {
                        var skill = skillLocator.utility;
                        TryHeal(self.characterBody, skill);
                    }
                }
            }
            orig(self);
        }

        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            orig(self, skill);
            TryHeal(self, skill, true);
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
}