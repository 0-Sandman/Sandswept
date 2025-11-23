using LookingGlass.ItemStatsNameSpace;
using Rebindables;
using Rewired;
using RoR2.CharacterAI;
using UnityEngine.TextCore;

namespace Sandswept.Items.Reds
{
    [ConfigSection("Items :: Torn Feather")]
    public class TornFeather : ItemBase<TornFeather>
    {
        public override string ItemName => "Torn Feather";

        public override string ItemLangTokenName => "TORN_FEATHER";

        public override string ItemPickupDesc => $"Tap $su%FEATHERKEY%$se to perform an omni-directional dash. Refreshes upon landing.".AutoFormat();

        public override string ItemFullDescription => $"Tap $su%FEATHERKEY%$se to perform an $suomni-directional dash$se, up to $su2 times$se before hitting the ground. Gain $su{baseMovementSpeedGain * 100f}%$se $ss(+{stackMovementSpeedGain * 100f}% per stack)$se movement speed.".AutoFormat();

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

        public override ItemTag[] ItemTags => [ItemTag.Utility, ItemTag.CanBeTemporary, ItemTag.MobilityRelated];

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
        public static ModKeybind FeatherDash = RebindAPI.RegisterModKeybind(new ModKeybind("SANDSWEPT_INPUT_FEATHER".Add("Torn Feather Dash"), KeyCode.F, 10, "Jump"));

        public static GameObject particles;
        public override Dictionary<string, Func<string>> LangReplacements => new Dictionary<string, Func<string>>()
        {
            {"%FEATHERKEY%", () => {
                return Main.input ? Glyphs.GetGlyphString(Main.input.eventSystem, RebindAPI.KeybindActions[FeatherDash].name, FeatherDash.AxisRange) : FeatherDash.DefaultKeyboardInput.ToString();
            }}
        };

        public override void Init()
        {
            base.Init();
            SetUpVFX();

            Main.onInputAvailable += Refresh;
            On.RoR2.InputMapperHelper.InputMapperOnInputMappedEvent += OnInputMapped;
        }

        private void OnInputMapped(On.RoR2.InputMapperHelper.orig_InputMapperOnInputMappedEvent orig, InputMapperHelper self, InputMapper.InputMappedEventData inputMappedEventData)
        {
            orig(self, inputMappedEventData);
            Refresh();
        }

        private void Refresh()
        {
            // Debug.Log("torn feather applying lang replacement to: " + Glyphs.GetGlyphString(Main.input.eventSystem, RebindAPI.KeybindActions[FeatherDash].name, FeatherDash.AxisRange));
            ApplyLanguage();
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
            pinkOverlay.SetColor("_TintColor", new Color32(255, 97, 128, 150));
            pinkOverlay.SetInt("_Cull", 1); // 0 = no cull = whole body, 1 = front = outline, 2 = back = whole body

            blueOverlay = new Material(Paths.Material.matHuntressFlashExpanded);
            blueOverlay.SetColor("_TintColor", new Color32(0, 136, 255, 150));
            blueOverlay.SetInt("_Cull", 1); // 0 = no cull = whole body, 1 = front = outline, 2 = back = whole body

            whiteOverlay = new Material(Paths.Material.matHuntressFlashExpanded);
            whiteOverlay.SetColor("_TintColor", new Color32(255, 255, 255, 150));
            whiteOverlay.SetInt("_Cull", 1); // 0 = no cull = whole body, 1 = front = outline, 2 = back = whole body

            particles = PrefabAPI.InstantiateClone(Paths.GameObject.Bandit2SmokeBomb, "Torn Feather VFX", false);

            particles.GetComponent<EffectComponent>().applyScale = true;
            VFXUtils.OdpizdzijPierdoloneGownoKurwaCoZaJebanyKurwaSmiecToKurwaDodalPizdaKurwaJebanaKurwa(particles);

            VFXUtils.RecolorMaterialsAndLights(particles, Color.white, Color.white, true, true);

            var transform = particles.transform.Find("Core");
            transform.localScale = Vector3.one / 12f;// base radius at 1 scale is 12m according to bandit's util value
            transform.localPosition = Vector3.zero;

            var sparks = transform.Find("Sparks");
            var sparksPS = sparks.GetComponent<ParticleSystem>();
            var sparksMain = sparksPS.main;
            sparksMain.maxParticles = 100;
            var sparksEmission = sparksPS.emission;
            var burst = new ParticleSystem.Burst(0f, 100, 100, 1, 0.01f);
            burst.probability = 1f;
            sparksEmission.SetBurst(0, burst);

            var sparksPSR = sparks.GetComponent<ParticleSystemRenderer>();
            sparksPSR.material.SetTexture("_MainTex", Paths.Texture2D.texGlowPaintMask);

            transform.Find("Smoke, Edge Circle").gameObject.SetActive(false);
            transform.Find("Dust, CenterSphere").gameObject.SetActive(false);
            transform.Find("Dust, CenterTube").gameObject.SetActive(false);

            var pointLight = transform.Find("Point Light");

            var light = pointLight.GetComponent<Light>();
            light.intensity = 12.5f;
            light.range = 13f;

            ContentAddition.AddEffect(particles);

            VFXUtils.MultiplyDuration(particles, 2f);

            pointLight.GetComponent<LightIntensityCurve>().timeMax = 0.4f;
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

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var itemDisplay = SetUpFollowerIDRS(0.72f, 132f, false, 0f, true, 5f, true, 15f);

            return new ItemDisplayRuleDict(new ItemDisplayRule()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(1.5f, 0.5f, 1.5f),
                localScale = new Vector3(0.0125f, 0.0125f, 0.0125f),

                followerPrefab = itemDisplay,
                limbMask = LimbFlags.None,
                followerPrefabAddress = new AssetReferenceGameObject("")
            });
        }
    }

    public class FeatherBehaviour : CharacterBody.ItemBehavior
    {
        public int DashesRemaining = 2;
        public const float dashCooldown = 1.4f;
        public const float minAirborneTimer = 0.5f;
        public const float dashTravelDistance = 15f;
        public const float dashDuration = 0.2f;
        public float airborneTimer = 0f;
        public float dashCooldownTimer = 0f;
        public float dashTimer = 0f;
        public ParticleSystem dashTrail;
        public Vector3 dashVector;
        public int localHurtboxIntangibleCount;
        public InteractionDriver driver;
        public bool startedAboveGround = false;
        public bool canWavedash = false;
        public float wavedashTimer = 0.45f;
        public bool wavedashNextFrame = false;
        public int vfxCycle = 3;
        public bool isAiControlled = false;
        public BaseAI baseAI = null;

        public void OnEnable()
        {
            GameObject trail = GameObject.Instantiate(TornFeather.DashTrailEffect, base.transform);
            dashTrail = trail.GetComponent<ParticleSystem>();

            driver = GetComponent<InteractionDriver>();

            if (body)
            {
                isAiControlled = !body.isPlayerControlled;
                var master = body.master;
                baseAI = master.GetComponent<BaseAI>();
            }
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

            if (wavedashTimer > 0f && startedAboveGround && body.inputBank.jump.justPressed && canWavedash)
            {
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
                    Transform modelTransform = null;
                    if (body.modelLocator)
                    {
                        modelTransform = body.modelLocator.modelTransform;
                    }

                    var temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                    temporaryOverlay.duration = 0.3f;
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = TornFeather.whiteOverlay;
                    temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();

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

            if (dashTimer <= dashDuration * 0.5f && canWavedash)
            {
                wavedashTimer -= Time.fixedDeltaTime;

                if (body.inputBank.jump.down && body.characterMotor.isGrounded)
                {
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

            if (!wavedash)
            {
                body.characterMotor.velocity *= 0.5f;
            }
            else
            {
                body.characterMotor.Motor.ForceUnground();
                float speed = dashTravelDistance / dashDuration;
                body.characterMotor.velocity = Vector3.up * (body.jumpPower * 0.35f) * 2f + (speed * body.characterDirection.forward * 0.5f);
                DashesRemaining++;
                canWavedash = true;
            }

            dashTrail.Stop();
        }

        public void PerformDash()
        {
            if (DashesRemaining <= 0 || (dashTimer <= (dashDuration / 4f) && dashTimer > 0f)) return;

            canWavedash = true;

            wavedashTimer = 0.45f;

            localHurtboxIntangibleCount++;
            body.hurtBoxGroup.hurtBoxesDeactivatorCounter++;

            Transform modelTransform = null;
            if (body.modelLocator)
            {
                modelTransform = body.modelLocator.modelTransform;
            }

            if (modelTransform)
            {
                var overlayMat = vfxCycle switch
                {
                    3 => TornFeather.blueOverlay,
                    2 => TornFeather.pinkOverlay,
                    _ => TornFeather.whiteOverlay
                };

                var temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                temporaryOverlay.duration = 0.5f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = overlayMat;
                temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
            }

            DashesRemaining -= 1;
            vfxCycle--;
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

            /* makes A/D inputs while W is held cause a perfect 90* horizontal dash, which imo feels like shit so its commented out
            else {
                if (body.inputBank.rawMoveLeft.down && !body.inputBank.rawMoveRight.down) {
                    dashVector = Quaternion.AngleAxis(-90f, Vector3.up) * body.inputBank.aimDirection.Nullify(y: true);
                }

                if (!body.inputBank.rawMoveLeft.down && body.inputBank.rawMoveRight.down) {
                    dashVector = Quaternion.AngleAxis(90f, Vector3.up) * body.inputBank.aimDirection.Nullify(y: true);
                }
            }
            */

            dashTrail.transform.forward = -dashVector;

            dashTrail.Play();

            // Util.PlaySound("Play_arenaCrab_swim_stroke", base.gameObject);
            Util.PlaySound("Play_huntress_shift_mini_blink", base.gameObject);

            if (vfxCycle <= 1)
            {
                vfxCycle = 3;
            }

            EffectManager.SpawnEffect(TornFeather.particles, new EffectData() { scale = 16f, origin = body.corePosition }, true);
        }
    }
}