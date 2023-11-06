using static Sandswept.Utils.Components.MaterialControllerComponents;

namespace Sandswept.Items.Greens
{
    [ConfigSection("Items :: Smouldering Document")]
    internal class SmoulderingDocument : ItemBase<SmoulderingDocument>
    {
        public static BuffDef SmoulderingDocumentDebuff;

        public override string ItemName => "Smouldering Document";

        public override string ItemLangTokenName => "SMOULDERING_DOCUMENT";

        public override string ItemPickupDesc => "Damage over time effects reduce enemy damage and attack speed.";

        public override string ItemFullDescription => ("$sd" + chance + "%$se chance to $sdignite$se enemies on hit for $sd" + d(totalDamage) + "$se TOTAL damage. $sdDamage over time$se effects $sdburden$se enemies, reducing their $sddamage$se by $sd" + d(burdenBaseDamageReduction) + "$se $ss(+" + d(burdenStackDamageReduction) + " per stack)$se and $sdattack speed$se by $sd" + d(burdenBaseAttackSpeedReduction) + "$se $ss(+" + d(burdenStackAttackSpeedReduction) + " per stack)$se.").AutoFormat();

        public override string ItemLore => "<style=cStack>[insert sad story about corporate exploitation here]</style>";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("DocumentPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("DocumentIcon.png");

        [ConfigField("Chance", "", 5f)]
        public static float chance;

        [ConfigField("TOTAL Damage", "Decimal.", 2.5f)]
        public static float totalDamage;

        [ConfigField("Burden Base Damage Reduction", "Decimal.", 0.15f)]
        public static float burdenBaseDamageReduction;

        [ConfigField("Burden Stack Damage Reduction", "Decimal.", 0.1f)]
        public static float burdenStackDamageReduction;

        [ConfigField("Burden Base Attack Speed Reduction", "Decimal.", 0.15f)]
        public static float burdenBaseAttackSpeedReduction;

        [ConfigField("Burden Stack Attack Speed Reduction", "Decimal.", 0.1f)]
        public static float burdenStackAttackSpeedReduction;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Utility };

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            CreateBuff();
            Hooks();
        }

        public void CreateBuff()
        {
            SmoulderingDocumentDebuff = ScriptableObject.CreateInstance<BuffDef>();
            SmoulderingDocumentDebuff.name = "Burdened";
            SmoulderingDocumentDebuff.buffColor = new Color32(245, 153, 80, 255);
            SmoulderingDocumentDebuff.canStack = false;
            SmoulderingDocumentDebuff.isDebuff = true;
            SmoulderingDocumentDebuff.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texBuffBurdened.png");
            ContentAddition.AddBuffDef(SmoulderingDocumentDebuff);
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            GetStatCoefficients += ApplyArmourChange;
            DotController.onDotInflictedServerGlobal += DotController_onDotInflictedServerGlobal;
        }

        private void DotController_onDotInflictedServerGlobal(DotController dotController, ref InflictDotInfo inflictDotInfo)
        {
            var attackerBody = inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
            if (!attackerBody)
            {
                return;
            }

            var victimBody = inflictDotInfo.victimObject.GetComponent<CharacterBody>();
            if (!victimBody)
            {
                return;
            }

            var stacks = GetCount(attackerBody);

            if (stacks > 0)
            {
                var infoStorage = victimBody.GetComponent<SmoulderingDocumentController>() ? victimBody.GetComponent<SmoulderingDocumentController>() : victimBody.AddComponent<SmoulderingDocumentController>();

                infoStorage.body = victimBody;
                infoStorage.stacks = GetCount(attackerBody);

                if (inflictDotInfo.dotIndex == DotController.DotIndex.Burn)
                {
                    victimBody.AddTimedBuff(SmoulderingDocumentDebuff, (float)inflictDotInfo.totalDamage / attackerBody.damage);
                    return;
                }

                victimBody.AddTimedBuff(SmoulderingDocumentDebuff, inflictDotInfo.duration);
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
                    var totlaMad = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, totalDamage);

                    InflictDotInfo inflictDotInfo = new()
                    {
                        attackerObject = attackerBody.gameObject,
                        victimObject = victim.gameObject,
                        totalDamage = attackerBody.damage * totlaMad,
                        damageMultiplier = 4f,
                        dotIndex = DotController.DotIndex.Burn
                    };

                    if (attackerBody?.inventory)
                    {
                        StrengthenBurnUtils.CheckDotForUpgrade(attackerBody.inventory, ref inflictDotInfo);
                    }

                    DotController.InflictDot(ref inflictDotInfo);
                }
            }
        }

        public void ApplyArmourChange(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(SmoulderingDocumentDebuff))
            {
                var token = sender.gameObject.GetComponent<SmoulderingDocumentController>();
                var damageReduction = Util.ConvertAmplificationPercentageIntoReductionPercentage((burdenBaseDamageReduction * 100f) + (burdenStackDamageReduction * 100f) * (token.stacks - 1)) * -0.01f;
                args.damageMultAdd += damageReduction;
                args.attackSpeedMultAdd -= burdenBaseAttackSpeedReduction + burdenStackAttackSpeedReduction * (token.stacks - 1);
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            MeshRenderer component = ItemModel.transform.Find("Document").GetComponent<MeshRenderer>();
            var rendererAssign = ItemModel.gameObject.AddComponent<HGControllerFinder>();
            rendererAssign.Renderer = component;
            return new ItemDisplayRuleDict();
        }
    }

    public class SmoulderingDocumentController : MonoBehaviour
    {
        public CharacterBody body;
        public int stacks;
    }
}