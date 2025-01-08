using HarmonyLib;
using Sandswept.Items.Greens;
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

        public override string ItemPickupDesc => "Chance to decay enemies on hit. Moving near decaying enemies increases movement speed. $svCorrupts all Smouldering Documents$se.".AutoFormat();

        public override string ItemFullDescription => ("$sd" + chance + "%$se chance on hit to inflict $sddecay$se for $sd" + d(DoTs.Decay.baseDamage) + "$se base damage. Moving near $sddecaying$se enemies increases $sumovement speed$se by $su" + d(baseMovementSpeedGain) + "$se $ss(+" + d(stackMovementSpeedGain) + " per stack)$se for $su" + movementSpeedBuffDuration + "$se seconds. $svCorrupts all Smouldering Documents$se.").AutoFormat();

        public override string ItemLore => "This hunger..\r\nIt grows inside me.\r\nSevers mortality.\r\n\r\nIts showing its teeth.\r\n\r\n\r\nBlood like wine!";

        [ConfigField("Chance", "", 7f)]
        public static float chance;

        [ConfigField("Base Movement Speed Gain", "Decimal.", 0.33f)]
        public static float baseMovementSpeedGain;

        [ConfigField("Stack Movement Speed Gain", "Decimal.", 0.33f)]
        public static float stackMovementSpeedGain;

        [ConfigField("Movement Speed Buff Duration", "", 2f)]
        public static float movementSpeedBuffDuration;

        [ConfigField("Movement Speed Buff Range", "", 13f)]
        public static float movementSpeedBuffRange;

        public static BuffDef movementSpeedBuff;

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override GameObject ItemModel => Main.Assets.LoadAsset<GameObject>("DisplayFesteringHunger.prefab");

        public override Sprite ItemIcon => Main.Assets.LoadAsset<Sprite>("texFesteringHunger.png");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public static DamageColorIndex milleniumColor = DamageColourHelper.RegisterDamageColor(new Color32(75, 27, 174, 255));

        public static GameObject vfx;

        public override void Init(ConfigFile config)
        {
            if (!ItemBase.DefaultEnabledCallback(SmoulderingDocument.instance))
            {
                return;
            }

            movementSpeedBuff = ScriptableObject.CreateInstance<BuffDef>();
            movementSpeedBuff.canStack = false;
            movementSpeedBuff.isCooldown = false;
            movementSpeedBuff.buffColor = new Color32(96, 56, 177, 255);
            movementSpeedBuff.iconSprite = Utils.Assets.BuffDef.bdAttackSpeedOnCrit.iconSprite;
            movementSpeedBuff.isHidden = false;
            movementSpeedBuff.isDebuff = false;
            movementSpeedBuff.name = "Festering Hunger - Movement Speed";
            ContentAddition.AddBuffDef(movementSpeedBuff);

            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.Items.ContagiousItemManager.Init += ContagiousItemManager_Init;
        }

        private static void ContagiousItemManager_Init(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            ItemDef.Pair transformation = new()
            {
                itemDef2 = FesteringHunger.instance.ItemDef,
                itemDef1 = Greens.SmoulderingDocument.instance.ItemDef
            };
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);

            orig();
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            body.AddItemBehavior<FesteringHungerController>(GetCount(body));
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(movementSpeedBuff))
            {
                var stack = GetCount(sender);
                args.moveSpeedMultAdd += baseMovementSpeedGain + stackMovementSpeedGain * (stack - 1);
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
                        dotIndex = DoTs.Decay.decayIndex,
                        maxStacksFromAttacker = uint.MaxValue,
                        duration = 3f
                    };

                    // Util.PlaySound("", attackerBody.gameObject);

                    DotController.InflictDot(ref inflictDotInfo);
                }
            }
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
        public float radiusSquared = FesteringHunger.movementSpeedBuffRange * FesteringHunger.movementSpeedBuffRange;
        public float distance = FesteringHunger.movementSpeedBuffRange;
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

            if (!victimBody.HasBuff(DoTs.Decay.decayBuff))
            {
                return;
            }

            if (body.HasBuff(FesteringHunger.movementSpeedBuff))
            {
                return;
            }

            body.AddTimedBuffAuthority(FesteringHunger.movementSpeedBuff.buffIndex, FesteringHunger.movementSpeedBuffDuration);
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