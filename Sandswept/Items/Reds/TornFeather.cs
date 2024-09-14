using System;
using static Rewired.Controller;

namespace Sandswept.Items.Reds
{
    public class TornFeather : ItemBase<TornFeather>
    {
        public override string ItemName => "Torn Feather";

        public override string ItemLangTokenName => "TORN_FEATHER";

        public override string ItemPickupDesc => "Gain an omni-directional dash when tapping Interact. Refreshes when grounded.";

        public override string ItemFullDescription => "Tap $sdInteract$se to perform an $suomni-directional dash$se. Can dash twice until hitting the ground. Gain $su20%$se $ss(+20% per stack)$se movement speed.".AutoFormat();

        public override string ItemLore => "TBD";

        public override ItemTier Tier => ItemTier.Tier3;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("TornFeatherHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texTornFeather.png");

        //
        public static GameObject PassiveParticleEffect;

        public static GameObject DashTrailEffect;
        public static Material BlueParticles;
        public static Material PinkParticles;

        public static Material pinkOverlay;
        public static Material blueOverlay;
        public static Material whiteOverlay;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Init(ConfigFile config)
        {
            base.Init(config);

            PassiveParticleEffect = Main.Assets.LoadAsset<GameObject>("ActiveIndicatorParticles.prefab");
            DashTrailEffect = Main.Assets.LoadAsset<GameObject>("DashTrailEffect.prefab");
            BlueParticles = Main.Assets.LoadAsset<Material>("matFeatherBlue.mat");
            PinkParticles = Main.Assets.LoadAsset<Material>("matFeatherPink.mat");

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
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += HandleStats;
        }

        private void HandleStats(CharacterBody sender, StatHookEventArgs args)
        {
            int stack = sender.inventory.GetItemCount(ItemDef);
            sender.AddItemBehavior<FeatherBehaviour>(stack);

            args.moveSpeedMultAdd += 0.2f * stack;
        }

        public class FeatherBehaviour : CharacterBody.ItemBehavior
        {
            private int DashesRemaining = 2;
            private const float dashCooldown = 1.4f;
            private const float minAirborneTimer = 0.3f;
            private const float dashTravelDistance = 15f;
            private const float dashDuration = 0.2f;
            private float airborneTimer = 0f;
            private float dashCooldownTimer = 0f;
            private float dashTimer = 0f;
            private ParticleSystem dashTrail;
            private ParticleSystem indicator;
            private ParticleSystemRenderer renderer;
            private Vector3 dashVector;
            private int localHurtboxIntangibleCount;

            public void OnEnable()
            {
                GameObject trail = GameObject.Instantiate(DashTrailEffect, base.transform);
                dashTrail = trail.GetComponent<ParticleSystem>();
                GameObject indicatorPrefab = GameObject.Instantiate(PassiveParticleEffect, base.transform);
                indicator = indicatorPrefab.GetComponent<ParticleSystem>();
                renderer = indicator.GetComponent<ParticleSystemRenderer>();
            }

            public void OnDisable()
            {
                if (dashTrail)
                {
                    GameObject.Destroy(dashTrail.gameObject);
                }

                if (indicator)
                {
                    GameObject.Destroy(indicator.gameObject);
                }
            }

            public void Update()
            {
                if (body.inputBank.interact.justPressed)
                {
                    PerformDash();
                }

                if (indicator)
                {
                    indicator.gameObject.SetActive(DashesRemaining > 0);
                    renderer.material = DashesRemaining == 2 ? PinkParticles : BlueParticles;
                    indicator.gameObject.transform.position = body.corePosition;
                }

                if (dashTrail)
                {
                    dashTrail.gameObject.transform.position = body.corePosition;
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
                    }
                }
            }

            public void EndDash()
            {
                body.hurtBoxGroup.hurtBoxesDeactivatorCounter -= localHurtboxIntangibleCount;
                localHurtboxIntangibleCount = 0;
                body.gameObject.layer = LayerIndex.defaultLayer.intVal;
                body.characterMotor.Motor.RebuildCollidableLayers();
                dashTrail.Stop();
                body.characterMotor.velocity = body.characterMotor.velocity *= 0.2f;
            }

            public void PerformDash()
            {
                if (DashesRemaining <= 0 || (dashTimer <= (dashDuration / 4f) && dashTimer > 0f)) return;

                localHurtboxIntangibleCount++;
                body.hurtBoxGroup.hurtBoxesDeactivatorCounter++;

                DashesRemaining -= 1;
                dashTimer = dashDuration;

                airborneTimer = 0f;
                dashCooldownTimer = 0f;

                body.gameObject.layer = LayerIndex.fakeActor.intVal;
                body.characterMotor.Motor.RebuildCollidableLayers();

                dashVector = body.inputBank.moveVector;
                if (dashVector == Vector3.zero)
                {
                    if (!body.inputBank.jump.down)
                    {
                        dashVector = body.inputBank.aimDirection;
                    }
                    else
                    {
                        dashVector = Vector3.up;
                    }
                }
                else
                {
                    if (body.inputBank.jump.down)
                    {
                        dashVector = Quaternion.AngleAxis(45f, base.transform.right) * dashVector;
                    }
                }

                dashVector.y = Mathf.Abs(dashVector.y);

                indicator.TriggerSubEmitter(0);

                dashTrail.transform.forward = -dashVector;

                dashTrail.Play();

                AkSoundEngine.PostEvent(Events.Play_huntress_shift_mini_blink, base.gameObject);
            }
        }
    }
}