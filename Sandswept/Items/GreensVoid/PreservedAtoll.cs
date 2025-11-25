using HarmonyLib;
using LookingGlass.ItemStatsNameSpace;
using Sandswept.Items.Greens;
using System.Collections;
using UnityEngine.UIElements;
using static R2API.DotAPI;
using static RoR2.DotController;

namespace Sandswept.Items.VoidGreens
{
    [ConfigSection("Items :: Preserved Atoll")]
    public class PreservedAtoll : ItemBase<PreservedAtoll>
    {
        public override string ItemName => "Preserved Atoll";

        public override string ItemLangTokenName => "PRESERVED_ATOLL";

        public override string ItemPickupDesc => $"The void retaliates on taking damage, inflicting permanent decay. Recharges over time. $svCorrupts all Sacrificial Bands$se.".AutoFormat();

        public override string ItemFullDescription => $"Getting hit causes you to implode, inflicting up to $sd{baseHealthDivisor}$se $ss(+{stackHealthDivisor} per stack)$se $sdpermanent decay$se for $sd{DoTs.Decay.baseDamage * 100f}%$se base damage to all targets in a $sd{radius}m$se radius based on $shhealth lost$se. Recharges every $su{cooldown}$se seconds. $svCorrupts all Sacrificial Bands$se.".AutoFormat();

        public override string ItemLore =>
        """
        A memory...Lost to time, records of it wiped from the archives. Pictures of it went missing. Paintings, gone. Stories, removed. A famed atoll, after the incident...Not a soul could remember its name. Nothing, only this one memento remains...
        """;

        [ConfigField("Radius", "", 24f)]
        public static float radius;

        [ConfigField("Base Health Divisor", "Divides the Maximum Health (standard health + shield) by this value to get the damage taken thresholds for each decay stack to inflict, for example, this value being equal to 5 makes the item inflict up to 5 decay at 100% maximum health lost (100 / 5 = 20) and 1 decay at 20% maximum health lost. Uses banker's rounding.", 5f)]
        public static float baseHealthDivisor;

        [ConfigField("Stack Health Divisor", "Adds to the Base Health Divisor based on stack count. Total Health Divisor = Base Health Divisor + this value * (Preserved Atoll - 1). When Base Health Divisor = 5 and this value = 3, with 2 stacks of the item, it makes each 12.5% of maximum health lost inflict one stack of decay, since 100 / 8 = 12.5, and up to 8 stacks at 100% maximum health lost. Uses banker's rounding.", 3f)]
        public static float stackHealthDivisor;

        [ConfigField("Cooldown", "", 10f)]
        public static float cooldown;

        public static BuffDef movementSpeedBuff;

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override GameObject ItemModel => Main.sandsweptHIFU.LoadAsset<GameObject>("PreservedAtollHolder.prefab");

        public override Sprite ItemIcon => Main.sandsweptHIFU.LoadAsset<Sprite>("texPreservedAtoll.png");

        public override ItemTag[] ItemTags => [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.CanBeTemporary];

        public override ItemDef ItemToCorrupt => SacrificialBand.instance.ItemDef;

        public static BuffDef readyBuff;
        public static BuffDef cooldownBuff;
        public static GameObject lineVFX;
        public static GameObject missVFX;

        public override void Init()
        {
            base.Init();

            SetUpBuffs();
            SetUpVFX();
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Health Loss Per Decay Stack: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Health);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.FlatHealth);
            itemStatsDef.descriptions.Add("Maximum Decay Amount: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
            itemStatsDef.calculateValues = (master, stackCount) =>
            {
                List<float> values = new()
                {
                    Mathf.Round((master.GetBody() ? master.GetBody().healthComponent.fullCombinedHealth : 110f) / (baseHealthDivisor + stackHealthDivisor * (stackCount - 1))),
                    baseHealthDivisor + stackHealthDivisor * (stackCount - 1)
                };

                return values;
            };

            return itemStatsDef;
        }

        public override void Hooks()
        {
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.HealthComponent.TakeDamageProcess += OnTakeDamage;
        }

        public void SetUpBuffs()
        {
            readyBuff = ScriptableObject.CreateInstance<BuffDef>();
            readyBuff.isDebuff = false;
            readyBuff.canStack = false;
            readyBuff.isHidden = false;
            readyBuff.isCooldown = false;
            readyBuff.iconSprite = Main.sandsweptHIFU.LoadAsset<Sprite>("texBuffPreservedAtollReady.png");
            readyBuff.buffColor = new Color32(59, 148, 70, 255);
            readyBuff.name = "Preserved Atoll Ready";

            ContentAddition.AddBuffDef(readyBuff);

            cooldownBuff = ScriptableObject.CreateInstance<BuffDef>();
            cooldownBuff.canStack = true;
            cooldownBuff.isDebuff = false;
            cooldownBuff.isHidden = false;
            cooldownBuff.isCooldown = true;
            cooldownBuff.iconSprite = Main.sandsweptHIFU.LoadAsset<Sprite>("texBuffPreservedAtollCooldown.png");
            cooldownBuff.buffColor = new Color(0.4151f, 0.4014f, 0.4014f, 1f); // wolfo consistency
            cooldownBuff.name = "Preserved Atoll Cooldown";

            ContentAddition.AddBuffDef(cooldownBuff);
        }

        public void SetUpVFX()
        {
            lineVFX = PrefabAPI.InstantiateClone(Paths.GameObject.VoidSurvivorBeamTracer, "Preserved Atoll Line VFX", false);
            VFXUtils.AddLight(lineVFX, DoTs.Decay.decayColor, 200f, radius, 1.5f);
            VFXUtils.RecolorMaterialsAndLights(lineVFX, DoTs.Decay.decayColor, DoTs.Decay.decayColor, true);

            lineVFX.AddComponent<TracerComponentSucks>();

            // lineVFX.transform.GetChild(1).gameObject.SetActive(false);

            var lineRenderer = lineVFX.GetComponent<LineRenderer>();
            lineRenderer.widthMultiplier = 1.25f;
            lineRenderer.numCapVertices = 10;

            lineRenderer.material.SetFloat("_InvFade", 2f);
            lineRenderer.material.SetFloat("_Boost", 1f);
            lineRenderer.material.SetFloat("_AlphaBoost", 10f);
            lineRenderer.material.SetFloat("_AlphaBias", 0f);

            lineRenderer.endColor = new Color32(84, 0, 255, 255);
            lineRenderer.startColor = new Color32(255, 0, 131, 255);
            lineRenderer.textureMode = LineTextureMode.Tile;

            var animateShaderAlpha = lineVFX.GetComponent<AnimateShaderAlpha>();
            animateShaderAlpha.timeMax = 1.5f;

            ContentAddition.AddEffect(lineVFX);

            missVFX = PrefabAPI.InstantiateClone(Paths.GameObject.IgniteExplosionVFX, "Preserved Atoll Miss VFX", false);

            var effectComponent = missVFX.GetComponent<EffectComponent>();
            effectComponent.soundName = "";

            VFXUtils.RecolorMaterialsAndLights(missVFX, DoTs.Decay.decayColor, DoTs.Decay.decayColor, true);
            VFXUtils.MultiplyDuration(missVFX, 1.5f);
            VFXUtils.AddLight(missVFX, DoTs.Decay.decayColor, 30f, radius, 1f);

            ContentAddition.AddEffect(missVFX);
        }

        private void OnTakeDamage(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);
            var attacker = damageInfo.attacker;
            if (!attacker)
            {
                return;
            }
            var victimBody = self.body;
            if (!victimBody)
            {
                return;
            }

            if (!victimBody.HasBuff(readyBuff))
            {
                return;
            }

            var stack = GetCount(victimBody);
            if (stack <= 0)
            {
                return;
            }

            var preservedAtollController = victimBody.GetComponent<PreservedAtollController>();
            if (!preservedAtollController)
            {
                return;
            }

            var victimMaxCombinedHealth = self.fullCombinedHealth; // e.g. 500
            var damageTaken = damageInfo.damage; // e.g. 250
            var percentDamageTaken = damageTaken / victimMaxCombinedHealth * 100f; // 250 / 500 * 100 = 0.5 * 100 = 50%

            var finalDivisor = 100f / (baseHealthDivisor + stackHealthDivisor * (stack - 1)); // 100 / (4+2 * (1-1)) = 100 / 4 at 1 stack = step every 25%

            var decayStacks = Mathf.RoundToInt(Mathf.Min(percentDamageTaken / finalDivisor, 100f / finalDivisor)); // pick lowest value between supposed current step count, 50 / 25 = 2 and the step count from losing all health to cap it just in case

            if (decayStacks <= 0)
            {
                return;
            }

            preservedAtollController.ApplyDecay(decayStacks); // pass step count to method that checks for nearby enemies and if it finds one, inflict decay stacks equal to step count to each nearby enemy and give cooldown, otherwise keep this item ready if no enemy is in range, else it would be frustrating to use
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            body.AddItemBehavior<PreservedAtollController>(GetCount(body));
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var itemDisplay = SetUpFollowerIDRS(0.3f, 25f, true, -33f, true, 20f, false, 0f);

            return new ItemDisplayRuleDict(new ItemDisplayRule()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(-1f, 1.8f, 0.4f),
                localScale = new Vector3(0.1f, 0.1f, 0.1f),

                followerPrefab = itemDisplay,
                limbMask = LimbFlags.None,
                followerPrefabAddress = new AssetReferenceGameObject("")
            });
        }
    }

    public class PreservedAtollController : CharacterBody.ItemBehavior
    {
        public float radiusSquared = PreservedAtoll.radius * PreservedAtoll.radius;
        public TeamIndex ownerIndex;
        public bool shouldRun = false;

        public void Start()
        {
            ownerIndex = body.teamComponent.teamIndex;
            if (!body.HasBuff(PreservedAtoll.readyBuff) && stack > 0)
            {
                body.AddBuff(PreservedAtoll.readyBuff);
                shouldRun = true;
            }
        }

        public void FixedUpdate()
        {
            if (!body || !shouldRun)
            {
                return;
            }

            if (!body.HasBuff(PreservedAtoll.cooldownBuff) && !body.HasBuff(PreservedAtoll.readyBuff))
            {
                body.AddBuff(PreservedAtoll.readyBuff);
            }
        }

        public void ApplyDecay(int decayStacks)
        {
            BlastAttack blastAttack = new()
            {
                radius = PreservedAtoll.radius,
                baseDamage = 0, // dont ask
                procCoefficient = 0,
                crit = body.RollCrit(),
                attackerFiltering = AttackerFiltering.NeverHitSelf, // I tried to make it work with self damage and wasn't able to WHAT ETH UFSADJUJDFGIUJD GF
                falloffModel = BlastAttack.FalloffModel.None,
                attacker = body.gameObject,
                teamIndex = body.teamComponent.teamIndex,
                position = body.corePosition,
                damageType = DamageType.Silent,
            };

            var result = blastAttack.Fire();

            for (int i = 0; i < result.hitPoints.Length; i++)
            {
                var hitPoint = result.hitPoints[i];
                var hurtBox = hitPoint.hurtBox;
                if (!hurtBox)
                {
                    continue;
                }
                var hc = hurtBox.healthComponent;
                if (!hc)
                {
                    continue;
                }

                var victimBody = hc.body;
                if (!victimBody)
                {
                    continue;
                }

                EffectData effectData = new()
                {
                    origin = body.corePosition,
                    start = victimBody.corePosition
                };

                EffectManager.SpawnEffect(PreservedAtoll.lineVFX, effectData, transmit: true);

                // OF COURSE I CANT MAKE THE LINERENRDENRSD MOVE WITH EVERYONE

                for (int j = 0; j < decayStacks; j++)
                {
                    InflictDotInfo inflictDotInfo = new()
                    {
                        attackerObject = body.gameObject,
                        victimObject = victimBody.gameObject,
                        totalDamage = null,
                        damageMultiplier = 1f,
                        dotIndex = DoTs.Decay.decayIndex,
                        maxStacksFromAttacker = uint.MaxValue,
                        duration = float.MaxValue
                    };

                    DotController.InflictDot(ref inflictDotInfo);
                }
            }

            if (result.hitPoints.Length > 0)
            {
                // this is dumb as shit
                for (int i = 0; i < 20; i++)
                {
                    Util.PlaySound("Play_voidDevastator_step", gameObject);
                }

                Util.PlaySound("Play_voidRaid_gauntlet_platform_move_start", gameObject);
                Util.PlaySound("Play_voidDevastator_m2_secondary_explo", gameObject);
                Util.PlaySound("Play_voidJailer_m1_shoot", gameObject);

                EffectData effectData = new()
                {
                    origin = body.corePosition,
                    scale = PreservedAtoll.radius
                };

                EffectData effectData2 = new()
                {
                    origin = body.corePosition,
                    scale = PreservedAtoll.radius / 1.35f
                };

                EffectData effectData3 = new()
                {
                    origin = body.corePosition,
                    scale = PreservedAtoll.radius / 2f
                };

                EffectManager.SpawnEffect(PreservedAtoll.missVFX, effectData, transmit: true);
                EffectManager.SpawnEffect(PreservedAtoll.missVFX, effectData2, transmit: true);
                EffectManager.SpawnEffect(PreservedAtoll.missVFX, effectData3, transmit: true);

                if (body.HasBuff(PreservedAtoll.readyBuff))
                {
                    body.RemoveBuff(PreservedAtoll.readyBuff);
                    for (int j = 1; j <= PreservedAtoll.cooldown; j++)
                    {
                        body.AddTimedBuff(PreservedAtoll.cooldownBuff, j);
                    }
                }
            }
            else
            {
                // show vfx on miss to help with distance/range/depth
                EffectData effectData = new()
                {
                    origin = body.corePosition,
                    scale = PreservedAtoll.radius
                };
                EffectManager.SpawnEffect(PreservedAtoll.missVFX, effectData, transmit: true);

                Util.PlaySound("Play_voidRaid_snipe_pool_damage", gameObject);
            }
        }

        public void OnDestroy()
        {
            if (body.HasBuff(PreservedAtoll.readyBuff))
            {
                body.RemoveBuff(PreservedAtoll.readyBuff);
            }
            if (body.HasBuff(PreservedAtoll.cooldownBuff))
            {
                body.RemoveBuff(PreservedAtoll.cooldownBuff);
            }
        }
    }
}