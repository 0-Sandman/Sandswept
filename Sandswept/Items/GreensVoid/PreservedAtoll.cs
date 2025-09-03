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

        public override string ItemPickupDesc => $"Retaliate with decay on taking damage. $svCorrupts all Sacrificial Bands$se.".AutoFormat();

        public override string ItemFullDescription => $"Getting hit causes the void to inflict $sdpermanent decay$se for $sd{DoTs.Decay.baseDamage * 100f}%$se base damage to all enemies in a $sd{radius}m$se radius $sdonce$se for each $sh{Mathf.Round(100f / baseHealthDivisor)}%$se $ss(-{Mathf.Round((1f - 100f / (baseHealthDivisor + stackHealthDivisor) / (100f / baseHealthDivisor)) * 100f)}% per stack)$se of $shmaximum health$se lost. Simulates every $su{cooldown}$se seconds. $svCorrupts all Sacrificial Bands$se.".AutoFormat();

        public override string ItemLore =>
        """
        No lore yet [...]
        """;

        [ConfigField("Radius", "", 20f)]
        public static float radius;

        [ConfigField("Base Health Divisor", "Divides the Maximum Health (standard health + shield) by this value to get the damage taken thresholds for each decay stack to inflict, for example, this value being equal to 4 makes the item inflict up to 4 decay at 100% maximum health lost (100 / 4 = 25) and 1 decay at 25% maximum health lost.", 4f)]
        public static float baseHealthDivisor;

        [ConfigField("Stack Health Divisor", "Adds to the Base Health Divisor based on stack count. Total Health Divisor = Base Health Divisor + this value * (Preserved Atoll - 1). When Base Health Divisor = 4 and this value = 2, with 2 stacks of the item, it makes each 16.66% of maximum health lost inflict one stack of decay, since 100 / 6 = 16.66, and up to 6 stacks at 100% maximum health lost,", 2f)]
        public static float stackHealthDivisor;

        [ConfigField("Cooldown", "", 10f)]
        public static float cooldown;

        public static BuffDef movementSpeedBuff;

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override GameObject ItemModel => Main.sandsweptHIFU.LoadAsset<GameObject>("PreservedAtollHolder.prefab");

        public override Sprite ItemIcon => Main.assets.LoadAsset<Sprite>("texFesteringHunger.png");

        public override ItemTag[] ItemTags => [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];

        public override ItemDef ItemToCorrupt => SacrificialBand.instance.ItemDef;

        public static BuffDef readyBuff;
        public static BuffDef cooldownBuff;
        public static GameObject lineVFX;

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
            readyBuff.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffSacrificialBandReady.png");
            readyBuff.buffColor = new Color32(96, 56, 177, 255);
            readyBuff.name = "Preserved Atoll Ready";

            ContentAddition.AddBuffDef(readyBuff);

            cooldownBuff = ScriptableObject.CreateInstance<BuffDef>();
            cooldownBuff.canStack = true;
            cooldownBuff.isDebuff = false;
            cooldownBuff.isHidden = false;
            cooldownBuff.isCooldown = true;
            cooldownBuff.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffSacrificialBandCooldown.png");
            cooldownBuff.buffColor = new Color(0.4151f, 0.4014f, 0.4014f, 1f);
            cooldownBuff.name = "Preserved Atoll Cooldown";

            ContentAddition.AddBuffDef(cooldownBuff);
        }

        public void SetUpVFX()
        {
            lineVFX = PrefabAPI.InstantiateClone(Paths.GameObject.VoidSurvivorBeamTracer, "Preserved Atoll Line VFX", false);

            VFXUtils.RecolorMaterialsAndLights(lineVFX, new Color32(96, 56, 177, 255), new Color32(96, 56, 177, 255), true);
            VFXUtils.MultiplyDuration(lineVFX, 3.5f);

            ContentAddition.AddEffect(lineVFX);
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
            return new ItemDisplayRuleDict();
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
                attackerFiltering = AttackerFiltering.NeverHitSelf,
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

                var mainHurtBox = victimBody.mainHurtBox;

                EffectData effectData = new()
                {
                    origin = body.corePosition,
                    start = victimBody.corePosition,
                    scale = 2f
                };
                // effectData.SetHurtBoxReference(mainHurtBox);
                EffectManager.SpawnEffect(PreservedAtoll.lineVFX, effectData, transmit: true);

                // maybe just have inflictdot in the loop, but not sure
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
                for (int i = 0; i < 20; i++)
                {
                    Util.PlaySound("Play_voidDevastator_step", gameObject);
                }

                if (body.HasBuff(PreservedAtoll.readyBuff))
                {
                    body.RemoveBuff(PreservedAtoll.readyBuff);
                    for (int j = 1; j <= PreservedAtoll.cooldown; j++)
                    {
                        body.AddTimedBuff(PreservedAtoll.cooldownBuff, j);
                    }
                }
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