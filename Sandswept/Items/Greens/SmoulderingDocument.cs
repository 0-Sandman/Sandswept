using static Sandswept.Items.Greens.BleedingWitness;
using static Sandswept.Utils.Components.MaterialControllerComponents;
using R2API;

namespace Sandswept.Items.Greens
{
    internal class SmoulderingDocument : ItemBase<SmoulderingDocument>
    {
        public class InfoStorage : MonoBehaviour
        {
            public CharacterBody body;
            public int stacks;
        }

        public static BuffDef SmoulderingDocumentDebuff;

        public override string ItemName => "Smouldering Document";

        public override string ItemLangTokenName => "SMOULDERING_DOCUMENT";

        public override string ItemPickupDesc => "Damage over time effects reduce enemy armor and attack speed.";

        public override string ItemFullDescription => StringExtensions.AutoFormat("$sd8%$se chance to $sdignite$se enemies on hit for $sd100%$se TOTAL damage. $sdDamage over time$se effects $sdburden$se enemies, reducing their $sdarmor$se by $sd10$se $ss(+5 per stack)$se and $sdattack speed$se by $sd10%$se $ss(+5% per stack)$se.");

        public override string ItemLore => "<style=cStack>[insert sad story about corporate exploitation here]</style>";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("DocumentPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("DocumentIcon.png");

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
            SmoulderingDocumentDebuff.buffColor = new Color(245f, 153f, 80f, 255f);
            SmoulderingDocumentDebuff.canStack = false;
            SmoulderingDocumentDebuff.isDebuff = true;
            SmoulderingDocumentDebuff.iconSprite = Main.MainAssets.LoadAsset<Sprite>("BurdenDebuffIcon.png");
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
                var infoStorage = victimBody.GetComponent<InfoStorage>() ? victimBody.GetComponent<InfoStorage>() : victimBody.AddComponent<InfoStorage>();

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
                if (Util.CheckRoll(8f * damageInfo.procCoefficient, attackerBody.master))
                {
                    InflictDotInfo inflictDotInfo = new()
                    {
                        attackerObject = attackerBody.gameObject,
                        victimObject = victim.gameObject,
                        totalDamage = attackerBody.damage,
                        damageMultiplier = 1f,
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
                var token = sender.gameObject.GetComponent<InfoStorage>();
                args.armorAdd += -10f + (token.stacks - 1) * -5;
                args.attackSpeedMultAdd -= 0.1f + 0.05f * (token.stacks - 1);
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
}