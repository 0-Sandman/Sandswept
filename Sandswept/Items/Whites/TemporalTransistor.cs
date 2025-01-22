using RoR2.Orbs;
using System.Reflection;

namespace Sandswept.Items.Whites
{
    [ConfigSection("Items :: Temporal Transistor")]
    internal class TemporalTransistor : ItemBase<TemporalTransistor>
    {
        public override string ItemName => "Temporal Transistor";

        public override string ItemLangTokenName => "TEMPORAL_TRANSISTOR";

        public override string ItemPickupDesc => "Chance on kill to store an extra jump.";

        public override string ItemFullDescription => $"Gain a $su{chance}%$se chance on kill to store an $suextra jump$se. Can store up to $su{baseMaxJumps}$se $ss(+{stackMaxJumps} per stack)$se $suextra jumps$se.".AutoFormat();

        public override string ItemLore => "wanted to do a white jump item and make it a trans reference somehow cause I heavily support lol, you can make it some funny trans/brazilian/celeste reference or just do whatever ig";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("FracturedTimepieceHolder.prefab");
        public override Sprite ItemIcon => null;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.OnKillEffect, ItemTag.Utility };

        [ConfigField("Chance", "", 25f)]
        public static float chance;

        [ConfigField("Base Max Jumps", "", 2)]
        public static int baseMaxJumps;

        [ConfigField("Stack Max Jumps", "", 2)]
        public static int stackMaxJumps;

        public static BuffDef extraJump;

        public static GameObject orbEffect;

        public override void Init(ConfigFile config)
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

            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            On.EntityStates.GenericCharacterMain.ProcessJump += GenericCharacterMain_ProcessJump;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            var storedJumps = self.GetBuffCount(extraJump);
            self.maxJumpCount += storedJumps;
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

        private void GenericCharacterMain_ProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump orig, GenericCharacterMain self)
        {
            if (!self.hasCharacterMotor || !self.jumpInputReceived)
            {
                orig(self);
                return;
            }

            var storedJumps = self.characterBody.GetBuffCount(extraJump);

            if (self.characterMotor.jumpCount > self.characterBody.maxJumpCount - storedJumps)
            {
                self.characterBody.SetBuffCount(extraJump.buffIndex, self.characterBody.GetBuffCount(extraJump) - 1);

                orig(self);
            }
            else
            {
                orig(self);
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
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