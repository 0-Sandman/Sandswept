using LookingGlass.ItemStatsNameSpace;
using RoR2.Orbs;

namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Temporal Transistor")]
    internal class TemporalTransistor : ItemBase<TemporalTransistor>
    {
        public override string ItemName => "Temporal Transistor";

        public override string ItemLangTokenName => "TEMPORAL_TRANSISTOR";

        public override string ItemPickupDesc => "Chance on kill to store an extra jump.";

        public override string ItemFullDescription => $"Gain a $su{chance}%$se chance on kill to store an $suextra jump$se. Can store up to $su{baseMaxJumps}$se $ss(+{stackMaxJumps} per stack)$se $suextra jumps$se.".AutoFormat();

        public override string ItemLore =>
        """
        <style=cMono>//--AUTO-TRANSCRIPTION FROM LOADING BAY 3 OF THE UES [Redacted] --//</style>

        "I've got a pretty weird shipment here. Come check it out, I know you love this stuff."

        "One sec, I gotta finish packing this. Just describe it to me."

        "Oh, okay. Well, when I picked it up, I got this weird feeling. The loading bay seemed a little different, but I didn't really pay it any heed until I looked down at my own limbs. It took me a second, but I realized I was looking through my body from like, five years ago. Crazy stuff. I dropped it and things went back to normal."

        "That's wild. I'll have to try it when I'm done. Do you know what it is?"

        "Input log just says 'transistor.' I guess I can see it, with the shape and everything, but it's an awfully big one. Figures, with what it does, I suppose. I wonder what it'll be used for."

        "What do you mean, it's a 'transistor'?"

        "Oh. Y'know, the things that make computers work."

        "Well, my sister does work in tech, but that seems like a bit of a generalization."

        "What?"
        """;

        public override string AchievementName => "An Epilogue of Life";

        public override string AchievementDesc => "Complete a Shrine of The Future without jumping.";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.assets.LoadAsset<GameObject>("TransistorPickup.prefab");
        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texTemporalTransistor.png");

        public override ItemTag[] ItemTags => [ItemTag.OnKillEffect, ItemTag.Utility, ItemTag.MobilityRelated, ItemTag.Technology]; // no CanBeTemporary because I can't be bothered to clamp buffs to item count for now

        [ConfigField("Chance", "", 25f)]
        public static float chance;

        [ConfigField("Base Max Jumps", "", 2)]
        public static int baseMaxJumps;

        [ConfigField("Stack Max Jumps", "", 2)]
        public static int stackMaxJumps;

        public static BuffDef extraJump;

        public static GameObject orbEffect;

        public override void Init()
        {
            base.Init();
            SetUpBuff();
            SetUpVFX();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Extra Jump Chance: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.descriptions.Add("Max Extra Jumps: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Utility);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
            itemStatsDef.hasChance = true;
            itemStatsDef.chanceScaling = ItemStatsDef.ChanceScaling.DoesNotScale;
            itemStatsDef.calculateValuesNew = (luck, stack, procChance) =>
            {
                List<float> values = new()
                {
                    LookingGlass.Utils.CalculateChanceWithLuck(chance * procChance * 0.01f, luck),
                    baseMaxJumps + stackMaxJumps * (stack - 1)
                };

                return values;
            };

            return itemStatsDef;
        }

        public void SetUpBuff()
        {
            extraJump = ScriptableObject.CreateInstance<BuffDef>();
            extraJump.isHidden = false;
            extraJump.isDebuff = false;
            extraJump.canStack = true;
            extraJump.isCooldown = false;
            extraJump.buffColor = Color.white;
            extraJump.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffTemporalTransistor.png");
            extraJump.name = "Temporal Transistor - Extra Jump";
            ContentAddition.AddBuffDef(extraJump);
        }

        public void SetUpVFX()
        {
            var blue = new Color32(102, 195, 255, 255);
            var darkerBlue = new Color32(102, 148, 255, 255);
            var aqua = new Color32(169, 249, 255, 127);

            TemporalTransistor.orbEffect = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Infusion/InfusionOrbEffect.prefab").WaitForCompletion(), "Temporal Transistor Orb", false);

            var orbEffect = TemporalTransistor.orbEffect.GetComponent<OrbEffect>();
            orbEffect.movementCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.33f, 0f), new Keyframe(1f, 1f));
            GameObject.DestroyImmediate(orbEffect.GetComponent<AkEvent>());
            GameObject.DestroyImmediate(orbEffect.GetComponent<AkTriggerDisable>());

            var trail = TemporalTransistor.orbEffect.transform.Find("TrailParent/Trail").GetComponent<TrailRenderer>();

            var newTrailMaterial = new Material(Paths.Material.matInfusionTrail);
            newTrailMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newTrailMaterial.SetColor("_TintColor", blue);
            newTrailMaterial.SetColor("_CutoffScroll", new Color32(75, 0, 12, 12));

            trail.material = newTrailMaterial;
            trail.time = 1.5f;

            var vfx = TemporalTransistor.orbEffect.transform.Find("VFX");
            var vfxRotateItem = vfx.GetComponent<RotateItem>();
            vfxRotateItem.spinSpeed = 60f;
            vfxRotateItem.bobHeight = 1.5f;

            var core = vfx.Find("Core");
            var coreColorOverLifetime = core.GetComponent<ParticleSystem>().colorOverLifetime;

            var coreColors = new GradientColorKey[3];
            coreColors[0] = new GradientColorKey(blue, 0f);
            coreColors[1] = new GradientColorKey(blue, 0.75f);
            coreColors[2] = new GradientColorKey(darkerBlue, 0f);

            var coreAlphas = new GradientAlphaKey[3];
            coreAlphas[0] = new GradientAlphaKey(1f, 0f);
            coreAlphas[1] = new GradientAlphaKey(1f, 0.75f);
            coreAlphas[2] = new GradientAlphaKey(0f, 0f);

            var newCoreColorGradient = new Gradient();
            newCoreColorGradient.SetKeys(coreColors, coreAlphas);

            coreColorOverLifetime.color = newCoreColorGradient;

            var coreParticleSystemRenderer = core.GetComponent<ParticleSystemRenderer>();

            var newCoreMaterial = Object.Instantiate(Paths.Material.matExpTrail);
            newCoreMaterial.SetTexture("_BaseTex", Paths.Texture2D.texArtifactCompoundDiamondMask);

            coreParticleSystemRenderer.material = newCoreMaterial;

            var pulseGlowMain = vfx.Find("PulseGlow").GetComponent<ParticleSystem>().main;
            var pulseGlowStartColor = pulseGlowMain.startColor;
            pulseGlowStartColor.mode = ParticleSystemGradientMode.Color;
            pulseGlowStartColor.color = aqua;

            ContentAddition.AddEffect(TemporalTransistor.orbEffect);
        }

        public override void Hooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            On.EntityStates.GenericCharacterMain.ProcessJump_bool += ProcessJump;
        }

        private void ProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump_bool orig, EntityStates.GenericCharacterMain self, bool ignoreRequirements)
        {
            var extraJumps = self.characterBody.GetBuffCount(extraJump);

            if (self.jumpInputReceived && extraJumps > 0 && self.characterMotor && self.characterMotor.jumpCount >= self.characterBody.maxJumpCount && !ignoreRequirements)
            {
                if (self.isAuthority)
                {
                    self.characterBody.SetBuffCountSynced(extraJump.buffIndex, extraJumps - 1);
                }

                Util.PlaySound("Play_transistor_jump", self.gameObject);
                ignoreRequirements = true;
            }

            orig(self, ignoreRequirements);
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var victimBody = report.victimBody;
            if (!victimBody)
            {
                return;
            }

            var masterrrrPleadingMasterrr = attackerBody.master;
            if (!masterrrrPleadingMasterrr)
            {
                return;
            }

            var stack = GetCount(attackerBody);
            if (stack > 0 && Util.CheckRoll(chance, masterrrrPleadingMasterrr))
            {
                var temporalTransistorOrb = new TemporalTransistorOrb
                {
                    origin = victimBody.corePosition,
                    target = Util.FindBodyMainHurtBox(attackerBody)
                };

                OrbManager.instance.AddOrb(temporalTransistorOrb);
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();

            var itemDisplay = SetUpFollowerIDRS(0.72f, 132f, true, 30f, false, 0f, true, 20f);

            return new ItemDisplayRuleDict(new ItemDisplayRule()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(-1.5f, 0.5f, 1.5f),
                localScale = new Vector3(1.25f, 1.25f, 1.25f),

                followerPrefab = itemDisplay,
                limbMask = LimbFlags.None,
                followerPrefabAddress = new AssetReferenceGameObject("")
            });
        }
    }

    public class TemporalTransistorOrb : Orb
    {
        public CharacterBody body;
        public Inventory inventory;

        public override void Begin()
        {
            duration = distanceToTarget / 50f;
            var effectData = new EffectData()
            {
                origin = origin,
                genericFloat = duration
            };
            effectData.SetHurtBoxReference(target);
            EffectManager.SpawnEffect(TemporalTransistor.orbEffect, effectData, true);
            var hurtBox = target.GetComponent<HurtBox>();
            var hc = hurtBox.healthComponent;
            if (hc)
            {
                body = hc.GetComponent<CharacterBody>();
                inventory = body.inventory;
            }
        }

        public override void OnArrival()
        {
            if (body && inventory)
            {
                var stack = inventory.GetItemCount(TemporalTransistor.instance.ItemDef);
                var storedJumps = body.GetBuffCount(TemporalTransistor.extraJump);
                var maxStoredJumps = TemporalTransistor.baseMaxJumps + TemporalTransistor.stackMaxJumps * (stack - 1);

                body.SetBuffCount(TemporalTransistor.extraJump.buffIndex, Mathf.Min(storedJumps + 1, maxStoredJumps));

                for (int i = 0; i < 2; i++)
                {
                    Util.PlayAttackSpeedSound("Play_mage_m1_cast_lightning", body.gameObject, 2f);
                    Util.PlayAttackSpeedSound("Play_mage_m1_cast_lightning", body.gameObject, 1.5f);
                }
            }
        }
    }
}