/*
namespace Sandswept.Items.Yellows
{
    public class Betrayal : ItemBase<Betrayal>
    {
        public class SunderBehaviour : MonoBehaviour
        {
            public int HitCount = 0;
            public int ProcCount = 0;
            public NetworkInstanceId LastID;
        }

        public static DamageColorIndex betrayalDamageColour = DamageColourHelper.RegisterDamageColor(new Color32(175, 255, 30, 255));

        public override string ItemName => "Betrayal";

        public override string ItemLangTokenName => "BETRAYAL";

        public override string ItemPickupDesc => "Repeated strikes on enemies sunder them.";

        public override string ItemFullDescription => StringExtensions.AutoFormat("Hitting an enemy $sd3$se times $sdsunders$se them for $sd300%$se damage, increasing by $sd300%$se each time they are $sdsundered$se.");

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Boss;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("BetrayalPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("BetrayalIcon.png");

        public override void Init(ConfigFile config)
        {
        }

        public override void Hooks()
        {
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            // On.RoR2.EquipmentSlot.FireBossHunter += EquipmentSlot_FireBossHunter;
            // prevent mithrix cheese, uncomment if truer
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (GetCount(body) > 0 && !body.gameObject.GetComponent<SunderBehaviour>())
            {
                body.gameObject.AddComponent<SunderBehaviour>();
            }
        }

        private bool EquipmentSlot_FireBossHunter(On.RoR2.EquipmentSlot.orig_FireBossHunter orig, EquipmentSlot self)
        {
            if (self.currentTarget.rootObject && self.currentTarget.rootObject.name.Contains("Brother"))
            {
                return false;
            }
            return orig(self);
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;
            if (damageInfo.procCoefficient <= 0)
            {
                return;
            }

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var sunderBehavior = attackerBody.GetComponent<SunderBehaviour>();
            if (!sunderBehavior)
            {
                return;
            }

            var victimBody = report.victimBody;
            if (!victimBody)
            {
                return;
            }

            var stack = attackerBody.inventory.GetItemCount(instance.ItemDef);
            if (stack > 0)
            {
                if (victimBody.netId != sunderBehavior.LastID)
                {
                    sunderBehavior.LastID = victimBody.netId;
                }
                sunderBehavior.HitCount++;
                if (sunderBehavior.HitCount >= 3)
                {
                    sunderBehavior.HitCount = 0;
                    DamageInfo extraDamageInfo = new()
                    {
                        damage = attackerBody.damage * (3 + sunderBehavior.ProcCount * 3),
                        attacker = damageInfo.attacker,
                        procCoefficient = 0,
                        position = victimBody.corePosition,
                        crit = false,
                        damageColorIndex = betrayalDamageColour,
                        damageType = DamageType.Silent
                    };
                    victimBody.healthComponent.TakeDamage(extraDamageInfo);
                    sunderBehavior.ProcCount++;
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
*/