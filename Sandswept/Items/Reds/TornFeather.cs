using LookingGlass.ItemStatsNameSpace;
using Rebindables;

namespace Sandswept.Items.Reds
{
    [ConfigSection("Items :: Torn Feather")]
    public class TornFeather : ItemBase<TornFeather>
    {
        public override string ItemName => "Torn Feather";

        public override string ItemLangTokenName => "TORN_FEATHER";

        public override string ItemPickupDesc => "Tap Interact to perform an omni-directional dash. Refreshes upon landing.";

        public override string ItemFullDescription => $"Tap $suInteract$se to perform an $suomni-directional dash$se, up to $su2 times$se before hitting the ground. Gain $su{baseMovementSpeedGain * 100f}%$se $ss(+{stackMovementSpeedGain * 100f}% per stack)$se movement speed.".AutoFormat();

        public override string ItemLore =>
        """
        <style=cMono>
        //--AUTO-TRANSCRIPTION FROM LOADING BAY 6 OF THE UES [Redacted] --//
        </style>
        "Whoa..."

        "Quite something, eh?"

        "Yeah, it's...it's like holding a dream."

        "I know the feeling. Once in a while, these steel tubes of hell are blessed with something so special, it's hard to resist the temptation to just take it for yourself and fly away."

        "...mhm."

        "But-"

        "Hey!"

        "-we must not fall prey to it. The steel-toed boot of the UESC is swift and agonizing."

        "...yeah, you're right."

        "Indeed I am! As with many things, you'll soon learn. I can't blame you, though. Honestly, this might just be the most magical thing I've seen in these docks. It probably ought to be in the Hall of the Revered, but I guess some lucky sap nabbed it first. Time will tell how long it'll be before those Martians get their divine mitts on it. They always do, in the end."

        ...

        "Well, now you're just ogling at it, too."

        "Ah, so I am! See, not even I, in my ultimate wisdom, am fully immune to such things. Oh, well; into the abyssal maw of the shipping crate it goes!"

        ...you'll get over it, with time. Just try not to think about it, especially while it's still in this dock. For us, it's back to work."
        """;
        public override ItemTier Tier => ItemTier.Tier3;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("TornFeatherHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texTornFeather.png");

        public override float modelPanelParametersMinDistance => 20f;
        public override float modelPanelParametersMaxDistance => 60f;

        [ConfigField("Base Movement Speed Gain", "Decimal.", 0.2f)]
        public static float baseMovementSpeedGain;

        [ConfigField("Stack Movement Speed Gain", "Decimal.", 0.35f)]
        public static float stackMovementSpeedGain;

        //
        public static GameObject PassiveParticleEffect;

        public static GameObject DashTrailEffect;
        public static Material BlueParticles;
        public static Material PinkParticles;

        public static Material pinkOverlay;
        public static Material blueOverlay;
        public static Material whiteOverlay;
        public static ModKeybind FeatherDash = RebindAPI.RegisterModKeybind(new ModKeybind("SANDSWEPT_INPUT_FEATHER".Add("Torn Feather Dash"), Rewired.KeyboardKeyCode.F, 10, "Jump"));

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Init()
        {
            base.Init();
            SetUpVFX();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Movement Speed: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Utility);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);

            itemStatsDef.calculateValuesNew = (luck, stack, procChance) =>
            {
                List<float> values = new()
                {
                    baseMovementSpeedGain + stackMovementSpeedGain * (stack - 1)
                };

                return values;
            };

            return itemStatsDef;
        }

        public void SetUpVFX()
        {
            PassiveParticleEffect = Main.assets.LoadAsset<GameObject>("ActiveIndicatorParticles.prefab");
            DashTrailEffect = Main.assets.LoadAsset<GameObject>("DashTrailEffect.prefab");
            BlueParticles = Main.assets.LoadAsset<Material>("matFeatherBlue.mat");
            PinkParticles = Main.assets.LoadAsset<Material>("matFeatherPink.mat");

            pinkOverlay = new Material(Paths.Material.matHuntressFlashExpanded);
            pinkOverlay.SetColor("_TintColor", new Color32(245, 169, 184, 150));
            pinkOverlay.SetInt("_Cull", 1); // 0 = no cull = whole body, 1 = front = outline, 2 = back = whole body

            blueOverlay = new Material(Paths.Material.matHuntressFlashExpanded);
            blueOverlay.SetColor("_TintColor", new Color32(91, 206, 250, 150));
            blueOverlay.SetInt("_Cull", 1); // 0 = no cull = whole body, 1 = front = outline, 2 = back = whole body

            whiteOverlay = new Material(Paths.Material.matHuntressFlashExpanded);
            whiteOverlay.SetColor("_TintColor", Color.white);
            whiteOverlay.SetInt("_Cull", 1); // 0 = no cull = whole body, 1 = front = outline, 2 = back = whole body
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += HandleStats;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            var inventory = body.inventory;
            if (!inventory)
            {
                return;
            }

            var stack = GetCount(body);

            body.AddItemBehavior<FeatherBehaviour>(stack);
        }

        private void HandleStats(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.inventory)
            {
                var stack = GetCount(sender);
                if (stack > 0)
                {
                    args.moveSpeedMultAdd += baseMovementSpeedGain + stackMovementSpeedGain * (stack - 1);
                }
            }
        }
    }

    public class FeatherBehaviour : CharacterBody.ItemBehavior
    {
        private int DashesRemaining = 2;
        private const float dashCooldown = 1.4f;
        private const float minAirborneTimer = 0.5f;
        private const float dashTravelDistance = 15f;
        private const float dashDuration = 0.2f;
        private float airborneTimer = 0f;
        private float dashCooldownTimer = 0f;
        private float dashTimer = 0f;
        private ParticleSystem dashTrail;
        private Vector3 dashVector;
        private int localHurtboxIntangibleCount;
        private InteractionDriver driver;
        private bool startedAboveGround = false;
        private bool canWavedash = false;
        private float wavedashTimer = 0.25f;
        private bool wavedashNextFrame = false;

        public void OnEnable()
        {
            GameObject trail = GameObject.Instantiate(TornFeather.DashTrailEffect, base.transform);
            dashTrail = trail.GetComponent<ParticleSystem>();

            driver = GetComponent<InteractionDriver>();
        }

        public void OnDisable()
        {
            if (dashTrail)
            {
                GameObject.Destroy(dashTrail.gameObject);
            }
        }

        public void Update()
        {
            if (body.inputBank.GetButtonState(TornFeather.FeatherDash).justPressed && body.inputBank.moveVector != Vector3.zero)
            {
                PerformDash();
            }

            if (dashTrail)
            {
                dashTrail.gameObject.transform.position = body.corePosition;
            }

            if (wavedashTimer >= 0f && startedAboveGround && body.inputBank.jump.justPressed && canWavedash) {
                wavedashNextFrame = true;
            }
        }

        public void FixedUpdate()
        {
            if (DashesRemaining < 2 && !body.characterMotor.isGrounded)
            {
                airborneTimer += Time.fixedDeltaTime;
            }

            if (airborneTimer >= minAirborneTimer && body.characterMotor.isGrounded)
            {
                DashesRemaining = 2;
                airborneTimer = 0f;
                dashCooldownTimer = 0f;
            }

            if (DashesRemaining < 2)
            {
                dashCooldownTimer += Time.fixedDeltaTime;

                if (dashCooldownTimer >= dashCooldown && body.characterMotor.isGrounded)
                {
                    DashesRemaining = 2;
                    airborneTimer = 0f;
                    dashCooldownTimer = 0f;
                }
            }

            if (dashTimer > 0f)
            {
                dashTimer -= Time.fixedDeltaTime;

                float speed = dashTravelDistance / dashDuration;
                base.body.characterMotor.velocity = speed * dashVector;

                if (dashTimer <= 0f)
                {
                    dashTimer = 0f;
                    EndDash();
                    return;
                }
            }

            if (dashTimer <= dashDuration * 0.5f && canWavedash) {
                wavedashTimer -= Time.fixedDeltaTime;

                if (wavedashNextFrame && body.characterMotor.isGrounded) {
                    EndDash(true);
                }
            }
        }

        public void EndDash(bool wavedash = false)
        {
            dashTimer = 0f;
            body.hurtBoxGroup.hurtBoxesDeactivatorCounter -= localHurtboxIntangibleCount;
            localHurtboxIntangibleCount = 0;
            body.gameObject.layer = LayerIndex.defaultLayer.intVal;
            body.characterMotor.Motor.RebuildCollidableLayers();

            wavedashNextFrame = false;

            canWavedash = false;

            if (!wavedash) {
                body.characterMotor.velocity *= 0.5f;
            }
            else {
                body.characterMotor.Motor.ForceUnground();
                float speed = dashTravelDistance / dashDuration;
                body.characterMotor.velocity = Vector3.up * body.jumpPower * 1.5f + (speed * body.characterDirection.forward * 0.5f);
                DashesRemaining++;
                canWavedash = true;
            }

            dashTrail.Stop();
        }

        public void PerformDash()
        {
            if (DashesRemaining <= 0 || (dashTimer <= (dashDuration / 4f) && dashTimer > 0f)) return;

            canWavedash = true;

            wavedashTimer = 0.25f;

            localHurtboxIntangibleCount++;
            body.hurtBoxGroup.hurtBoxesDeactivatorCounter++;

            Transform modelTransform = null;
            if (body.modelLocator)
            {
                modelTransform = body.modelLocator.modelTransform;
            }

            if (modelTransform)
            {
                var overlayMat = DashesRemaining switch
                {
                    2 => TornFeather.blueOverlay,
                    1 => TornFeather.pinkOverlay,
                    _ => TornFeather.whiteOverlay
                };

                var temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                temporaryOverlay.duration = 0.2f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = overlayMat;
                temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
            }

            DashesRemaining -= 1;
            dashTimer = dashDuration;

            airborneTimer = 0f;
            dashCooldownTimer = 0f;

            body.gameObject.layer = LayerIndex.fakeActor.intVal;
            body.characterMotor.Motor.RebuildCollidableLayers();
            body.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, dashDuration + 0.2f);

            dashVector = body.inputBank.moveVector;

            startedAboveGround = !body.characterMotor.isGrounded;

            if (!body.inputBank.rawMoveLeft.down && !body.inputBank.rawMoveRight.down && !body.inputBank.rawMoveDown.down)
            {
                dashVector = body.inputBank.aimDirection;
            }

            dashTrail.transform.forward = -dashVector;

            dashTrail.Play();

            Util.PlaySound("Play_huntress_shift_mini_blink", base.gameObject);
        }
    }
}