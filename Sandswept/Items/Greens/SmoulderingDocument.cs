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

        public override string ItemFullDescription => $"$sd{chance}%$se chance to $sdignite$se enemies on hit for $sd{totalDamage * 100f}%$se TOTAL damage. $sdDamage over time$se effects $sdburden$se enemies, reducing their $sddamage$se by $sd{burdenBaseDamageReduction * 100f}%$se $ss(+{burdenStackDamageReduction * 100f}% per stack)$se and $sdattack speed$se by $sd{burdenBaseAttackSpeedReduction * 100f}%$se $ss(+{burdenStackAttackSpeedReduction * 100f}% per stack)$se.".AutoFormat();

        public override string ItemLore => "Order: UES Classified Document\r\nTracking Number: 244*****\r\nEstimated Delivery: 09/14/2056\r\nShipping Method: Priority\r\nShipping Address: |||||||, Druid Hills, Earth\r\nShipping Details:\r\n\r\nTo Joel:\r\nUESC didn't take kindly to the accusations. I should have figured; there's a reason they're still operational despite all the things they've done. We were close, so very close, but we got our times wrong, and someone was in the room when Elijah got there. He says they threw the documents into the fireplace, but he grabbed them straight out of the flames as he fled. Most of it is still legible — maybe, just maybe enough to condemn them.\r\n\r\nNeedless to say, he can't set foot in any UES building again. For now, he's hiding out in a location I dare not disclose here. I don't trust UES not to get him arrested, or even killed, if they get the chance — we have some damning evidence here. Since I was part of the lawsuit, I'm sure they're looking for me, too, which means I'll be joining him. They don't know about you or Margaret, though. Keep this safe until it's safe for us to return.\r\n\r\nThis is not over. We will be back.\r\n\r\n\r\nAnd, to the UES employee reading this:\r\nHaving read the above, and seeing what I'm shipping, I'm sure I don't need to tell you that the suits at the top of UES wouldn't be fond of what we're doing here. I have no doubt that someone at your level resents UES nearly as much as we do, with all they put you through. Just don't report us, and you could help change things for the better.\r\n\r\nIt might seem idiotic to ship evidence convicting UESC through UES, but there's really nobody else out there at this point. We didn't see this coming, and there's not enough time for me to think of an alternative. My life's work, perhaps my life itself, not to mention the lives of my accomplices, all rest in your hands now. Please do the right thing.\r\n";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("DocumentPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("DocumentIcon.png");

        [ConfigField("Chance", "", 5f)]
        public static float chance;

        [ConfigField("TOTAL Damage", "Decimal.", 2.5f)]
        public static float totalDamage;

        [ConfigField("Burden Base Damage Reduction", "Decimal.", 0.125f)]
        public static float burdenBaseDamageReduction;

        [ConfigField("Burden Stack Damage Reduction", "Decimal.", 0.125f)]
        public static float burdenStackDamageReduction;

        [ConfigField("Burden Base Attack Speed Reduction", "Decimal.", 0.125f)]
        public static float burdenBaseAttackSpeedReduction;

        [ConfigField("Burden Stack Attack Speed Reduction", "Decimal.", 0.125f)]
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
            SmoulderingDocumentDebuff.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffBurdened.png");
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
            var attackerObject = inflictDotInfo.attackerObject;
            if (!attackerObject)
            {
                return;
            }

            var attackerBody = attackerObject.GetComponent<CharacterBody>();
            if (!attackerBody)
            {
                return;
            }

            var victimObject = inflictDotInfo.victimObject;
            if (!victimObject)
            {
                return;
            }

            var victimBody = victimObject.GetComponent<CharacterBody>();
            if (!victimBody)
            {
                return;
            }

            var stacks = GetCount(attackerBody);

            if (stacks > 0)
            {
                var smoulderingDocumentController = victimBody.GetComponent<SmoulderingDocumentController>() ? victimBody.GetComponent<SmoulderingDocumentController>() : victimBody.AddComponent<SmoulderingDocumentController>();

                smoulderingDocumentController.body = victimBody;
                smoulderingDocumentController.stacks = GetCount(attackerBody);

                if (inflictDotInfo.dotIndex == DotController.DotIndex.Burn && inflictDotInfo.totalDamage != null)
                {
                    victimBody.AddTimedBuff(SmoulderingDocumentDebuff, 3f);
                    return;
                }

                if (!victimBody.HasBuff(SmoulderingDocumentDebuff))
                {
                    victimBody.AddTimedBuff(SmoulderingDocumentDebuff, inflictDotInfo.duration);
                }
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
                        // totalDamage = attackerBody.damage * totlaMad,
                        totalDamage = totlaMad,
                        damageMultiplier = totlaMad / 25f,
                        dotIndex = DotController.DotIndex.Burn,
                        maxStacksFromAttacker = uint.MaxValue
                    };

                    if (attackerBody.inventory)
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
                if (sender.TryGetComponent<SmoulderingDocumentController>(out var smoulderingDocumentController))
                {
                    var damageReduction = Util.ConvertAmplificationPercentageIntoReductionPercentage((burdenBaseDamageReduction * 100f) + (burdenStackDamageReduction * 100f) * (smoulderingDocumentController.stacks - 1)) * -0.01f;
                    args.damageMultAdd += damageReduction;
                    args.attackSpeedMultAdd -= burdenBaseAttackSpeedReduction + burdenStackAttackSpeedReduction * (smoulderingDocumentController.stacks - 1);
                }
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