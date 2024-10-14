using HarmonyLib;
using System.Collections;
using UnityEngine.UIElements;
using static R2API.DotAPI;
using static RoR2.DotController;

namespace Sandswept.Items.VoidGreens
{
    [ConfigSection("Items :: Festering Hunger")]
    public class FesteringHunger : ItemBase<FesteringHunger>
    {
        public override string ItemName => "Festering Hunger";

        public override string ItemLangTokenName => "FESTERING_HUNGER";

        public override string ItemPickupDesc => "Chance to decay enemies on hit. Moving near decaying enemies increases attack speed. $svCorrupts all Smouldering Documents$se.".AutoFormat();

        public override string ItemFullDescription => ("$sd" + chance + "%$se chance on hit to inflict $sddecay$se for $sd" + d(baseDamage) + "$se base damage. Moving near $sddecaying$se enemies increases $sdattack speed$se by $sd" + d(baseAttackSpeedGain) + "$se $ss(+" + d(stackAttackSpeedGain) + " per stack)$se for $sd" + attackSpeedBuffDuration + "$se seconds. $svCorrupts all Smouldering Documents$se.").AutoFormat();

        public override string ItemLore => "This hunger..\r\nIt grows inside me.\r\nSevers mortality.\r\n\r\nIts showing its teeth.\r\n\r\n\r\nBlood like wine!";

        [ConfigField("Chance", "", 7f)]
        public static float chance;

        [ConfigField("Base Damage", "Decimal.", 3f)]
        public static float baseDamage;

        [ConfigField("Scale Damage with Enemy Missing Health?", "Scales decay's base damage up to 200% of its damage value linearly with the enemy's missing health.", true)]
        public static bool scaleDamage;

        [ConfigField("Base Attack Speed Gain", "Decimal.", 0.33f)]
        public static float baseAttackSpeedGain;

        [ConfigField("Stack Attack Speed Gain", "Decimal.", 0.33f)]
        public static float stackAttackSpeedGain;

        [ConfigField("Attack Speed Buff Duration", "", 2f)]
        public static float attackSpeedBuffDuration;

        [ConfigField("Attack Speed Buff Range", "", 13f)]
        public static float attackSpeedBuffRange;

        public static BuffDef attackSpeedBuff;

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("MilleniumHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texMillenium.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public static DamageColorIndex milleniumColor = DamageColourHelper.RegisterDamageColor(new Color32(75, 27, 174, 255));

        public static GameObject vfx;

        public static BuffDef decay;
        public static DotDef decayDef;
        public static DotIndex decayIndex;

        public override void Init(ConfigFile config)
        {
            attackSpeedBuff = ScriptableObject.CreateInstance<BuffDef>();
            attackSpeedBuff.canStack = false;
            attackSpeedBuff.isCooldown = false;
            attackSpeedBuff.buffColor = new Color32(96, 56, 177, 255);
            attackSpeedBuff.iconSprite = Utils.Assets.BuffDef.bdAttackSpeedOnCrit.iconSprite;
            attackSpeedBuff.isHidden = false;
            attackSpeedBuff.isDebuff = false;
            ContentAddition.AddBuffDef(attackSpeedBuff);

            decay = ScriptableObject.CreateInstance<BuffDef>();
            decay.canStack = true;
            decay.isCooldown = false;
            decay.isDebuff = true;
            decay.isHidden = false;
            decay.buffColor = new Color32(96, 56, 177, 255);
            decay.name = "Decay";
            decay.iconSprite = Utils.Assets.BuffDef.bdBlight.iconSprite;
            ContentAddition.AddBuffDef(decay);

            decayDef = new()
            {
                associatedBuff = decay,
                resetTimerOnAdd = false,
                interval = 0.2f,
                damageColorIndex = DamageColorIndex.DeathMark,
                damageCoefficient = 1f
            };

            CustomDotBehaviour behavior = delegate (DotController self, DotStack dotStack)
            {
                var victimBody = self.victimBody;
                var attackerBody = dotStack.attackerObject?.GetComponent<CharacterBody>();
                if (victimBody && attackerBody)
                {
                    dotStack.damage = attackerBody.damage * baseDamage * 0.2f;
                    if (scaleDamage)
                    {
                        var victimHc = victimBody.healthComponent;
                        if (victimHc)
                        {
                            var scalar = 1f + (1f - victimHc.combinedHealthFraction);
                            dotStack.damage = attackerBody.damage * baseDamage * 0.2f * scalar;
                        }
                    }
                }
            };

            decayIndex = RegisterDotDef(decayDef, behavior);

            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.Items.ContagiousItemManager.Init += ContagiousItemManager_Init;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            body.AddItemBehavior<FesteringHungerController>(GetCount(body));
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(attackSpeedBuff))
            {
                var stack = GetCount(sender);
                args.baseAttackSpeedAdd += baseAttackSpeedGain + stackAttackSpeedGain * (stack - 1);
            }
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var victim = report.victim;
            if (!victim)
            {
                return;
            }

            var stack = GetCount(attackerBody);
            if (stack > 0)
            {
                if (Util.CheckRoll(chance * damageInfo.procCoefficient, attackerBody.master))
                {
                    InflictDotInfo inflictDotInfo = new()
                    {
                        attackerObject = attackerBody.gameObject,
                        victimObject = victim.gameObject,
                        totalDamage = null,
                        damageMultiplier = 1f,
                        dotIndex = decayIndex,
                        maxStacksFromAttacker = uint.MaxValue,
                        duration = 3f
                    };

                    DotController.InflictDot(ref inflictDotInfo);
                }
            }
        }

        private void ContagiousItemManager_Init(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            ItemDef.Pair transformation = new()
            {
                itemDef2 = instance.ItemDef,
                itemDef1 = Greens.SmoulderingDocument.instance.ItemDef
            };
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);
            orig();
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }

    public class FesteringHungerController : CharacterBody.ItemBehavior
    {
        public float checkInterval = 0.05f;
        public float timer;
        public float radiusSquared = FesteringHunger.attackSpeedBuffRange * FesteringHunger.attackSpeedBuffRange;
        public float distance = FesteringHunger.attackSpeedBuffRange;
        public TeamIndex ownerIndex;
        // public GameObject radiusIndicator;

        private void Start()
        {
            ownerIndex = body.teamComponent.teamIndex;
            // enableRadiusIndicator = false;
            // var radiusTrans = radiusIndicator.transform.GetChild(1);
            // radiusTrans.localScale = Vector3.one * FesteringHunger.attackSpeedBuffRange;
        }

        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;

            if (timer < checkInterval)
            {
                return;
            }

            // Main.WRBLogger.LogError("enabling razorwire indicator");

            // enableRadiusIndicator = true;

            for (TeamIndex firstIndex = TeamIndex.Neutral; firstIndex < TeamIndex.Count; firstIndex++)
            {
                if (firstIndex == ownerIndex || firstIndex <= TeamIndex.Neutral)
                {
                    continue;
                }

                foreach (TeamComponent teamComponent in TeamComponent.GetTeamMembers(firstIndex))
                {
                    var enemyPosition = teamComponent.transform.position;
                    var corePosition = body.corePosition;
                    if ((enemyPosition - corePosition).sqrMagnitude <= radiusSquared)
                    {
                        TryGiveBuff(teamComponent);
                    }
                }
            }

            timer = 0f;
        }

        private void TryGiveBuff(TeamComponent teamComponent)
        {
            var victimBody = teamComponent.body;
            if (!victimBody)
            {
                return;
            }

            if (!victimBody.HasBuff(FesteringHunger.decay))
            {
                return;
            }

            if (body.HasBuff(FesteringHunger.attackSpeedBuff))
            {
                return;
            }

            body.AddTimedBuffAuthority(FesteringHunger.attackSpeedBuff.buffIndex, FesteringHunger.attackSpeedBuffDuration);
        }

        /*
        private bool enableRadiusIndicator
        {
            get
            {
                return radiusIndicator;
            }
            set
            {
                if (enableRadiusIndicator != value)
                {
                    if (value)
                    {
                        radiusIndicator = Instantiate(FesteringHunger.indicator, body.corePosition, Quaternion.identity);
                        radiusIndicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject, null);
                    }
                    else
                    {
                        Object.Destroy(radiusIndicator);
                        radiusIndicator = null;
                    }
                }
            }
        }
        */

        private void OnDisable()
        {
            // enableRadiusIndicator = false;
        }
    }
}