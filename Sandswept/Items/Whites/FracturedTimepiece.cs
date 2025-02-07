using System.Linq;

namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Fractured Timepiece")]
    public class FracturedTimepiece : ItemBase<FracturedTimepiece>
    {
        public override string ItemName => "Fractured Timepiece";

        public override string ItemLangTokenName => "FRACTURED_TIMEPIECE";

        public override string ItemPickupDesc => "Using your Utility skill heals you and reduces Special skill cooldown.";

        public override string ItemFullDescription => $"Upon using your $suUtility skill$se, $shheal$se for $sh{basePercentHealing * 100f}%$se $ss(+{stackPercentHealing * 100f}% per stack)$se of your $shmaximum health$se and $sureduce Special skill cooldown$se by $su{specialCooldownReduction * 100f}%$se.".AutoFormat();

        public override string ItemLore => "Order: Timepiece\r\nTracking Number: 864*******\r\nEstimated Delivery: 02/23/2054\r\nShipping Method: Priority\r\nShipping Address: Hall of the Revered, Mars\r\nShipping Details:\r\n\r\nOur team uncovered this in the desert where the old Hall was, before it burned down. I guess someone really wanted to protect it from the Purge, since it was carefully wrapped and boxed where we found it. You can probably glean more knowledge from it than we can, and it was the Hall's to begin with in any case.\r\n\r\nThe box had a note in it from the Time Keeper of the era, too, which I've included in the package. Nobody here can read the old language, so hopefully you can make some sense of it.\r\n";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("FracturedTimepieceHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texFracturedTimepiece.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Healing, ItemTag.Utility, ItemTag.AIBlacklist };

        public override float modelPanelParametersMinDistance => 5f;
        public override float modelPanelParametersMaxDistance => 13f;

        public static List<string> blacklistedSkills = new() { "MAGE_UTILITY_ICE_NAME", "ENGI_SKILL_HARPOON_NAME" };

        public static GameObject healVFX;
        public static GameObject cdrVFX;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        [ConfigField("Base Percent Healing", "Decimal.", 0.05f)]
        public static float basePercentHealing;

        [ConfigField("Stack Percent Healing", "Decimal.", 0.05f)]
        public static float stackPercentHealing;

        [ConfigField("Special Cooldown Reduction", "Decimal.", 0.15f)]
        public static float specialCooldownReduction;

        public override void Hooks()
        {
            healVFX = PrefabAPI.InstantiateClone(Paths.GameObject.MedkitHealEffect, "Fractured Timepiece Heal VFX", false);
            var effectComponent = healVFX.GetComponent<EffectComponent>();
            effectComponent.applyScale = true;

            var healRamp = Paths.Texture2D.texRampArtifactShellSoft;
            var cdrRamp = Paths.Texture2D.texRampLaserTurbine;

            var trans = healVFX.transform;

            for (int i = 0; i < trans.childCount; i++)
            {
                var child = trans.GetChild(i);
                child.transform.localScale = Vector3.one * 1.5f;
            }

            var spinner = trans.GetChild(0).GetComponent<ParticleSystemRenderer>();

            var newMat = Object.Instantiate(Paths.Material.matHealTrail);
            newMat.SetTexture("_RemapTex", healRamp);
            newMat.SetFloat("_Boost", 9.9f);

            spinner.trailMaterial = newMat;

            var crosses = trans.GetChild(1).GetComponent<ParticleSystemRenderer>();

            var newMat2 = Object.Instantiate(Paths.Material.matHealingCross);
            newMat2.SetTexture("_RemapTex", healRamp);

            crosses.material = newMat2;

            ContentAddition.AddEffect(healVFX);

            cdrVFX = PrefabAPI.InstantiateClone(Paths.GameObject.MedkitHealEffect, "Fractured Timepiece CDR VFX", false);
            var effectComponent2 = cdrVFX.GetComponent<EffectComponent>();
            effectComponent2.applyScale = true;
            effectComponent2.soundName = "";

            var trans2 = cdrVFX.transform;

            var spinner2 = trans2.GetChild(0).GetComponent<ParticleSystemRenderer>();
            spinner2.transform.eulerAngles = new Vector3(90f, 0f, 0f);
            var spinner2guh = spinner2.GetComponent<ParticleSystem>().main;
            spinner2guh.startDelay = 0.2f;

            var newMat3 = Object.Instantiate(Paths.Material.matHealTrail);
            newMat3.SetTexture("_RemapTex", cdrRamp);
            newMat3.SetFloat("_Boost", 4.8f);

            spinner2.trailMaterial = newMat3;

            var crosses2 = trans2.GetChild(1).GetComponent<ParticleSystemRenderer>();
            crosses2.transform.eulerAngles = new Vector3(90f, 0f, 0f);

            var mask = Paths.Texture2D.texGalaxy1Mask;

            var newMat4 = Object.Instantiate(Paths.Material.matHealingCross);
            newMat4.SetTexture("_RemapTex", cdrRamp);
            newMat4.SetTexture("_MainTex", mask);
            newMat4.SetTexture("_Cloud1Tex", mask);

            crosses2.material = newMat4;

            ContentAddition.AddEffect(cdrVFX);

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
                var reduction = Util.ConvertAmplificationPercentageIntoReductionPercentage(specialCooldownReduction);
                if (special && special.stock < special.maxStock)
                {
                    special.rechargeStopwatch += special.baseRechargeInterval * reduction / skill.skillDef.baseMaxStock;
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
            return new ItemDisplayRuleDict();
        }
    }
}