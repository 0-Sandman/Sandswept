using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static Sandswept.Main;
using static Sandswept.Items.SmoulDoc.DocBehaviour;
using static Sandswept.Utils.Components.MaterialControllerComponents;

namespace Sandswept.Items
{
    internal class SmoulDoc : ItemBase<SmoulDoc>
    {
        public class DocBehaviour : MonoBehaviour
        {
            public class ProcStorage : MonoBehaviour
            {
                public int procCount;
            }
            public CharacterBody body;
            public int stacks;

            public void Start()
            {
                var procSet = body.gameObject.GetComponent<ProcStorage>();
                procSet.procCount = 1;
                body.armor += (-10f + (stacks - 1) * -5);
                body.attackSpeed *= (0.9f - (stacks - 1) * 0.05f);
                On.RoR2.DotController.OnDotStackRemovedServer += DotRemove;
                RecalculateStatsAPI.GetStatCoefficients += ApplyArmourChange;
                On.RoR2.DotController.InflictDot_refInflictDotInfo += DotInflict;
            }

            public void OnDestroy() 
            {
                On.RoR2.DotController.OnDotStackRemovedServer -= DotRemove;
                RecalculateStatsAPI.GetStatCoefficients -= ApplyArmourChange;
                On.RoR2.DotController.InflictDot_refInflictDotInfo -= DotInflict;
                body.RemoveBuff(SmoulderingDocumentDebuff);
            }

            public void DotInflict(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo)
            {
                orig(ref inflictDotInfo);

                var stacks = inflictDotInfo.victimObject.GetComponent<ProcStorage>();
                ++stacks.procCount;
            }

            public void DotRemove(On.RoR2.DotController.orig_OnDotStackRemovedServer orig, DotController self, object dotStack)
            {
                orig(self, dotStack);
                
                CharacterBody body = self.victimBody.GetComponent<CharacterBody>();

                var stacks = body.gameObject.GetComponent<ProcStorage>();
                --stacks.procCount;
                
                if(stacks.procCount == 0 && body.HasBuff(SmoulderingDocumentDebuff))
                {
                    Destroy(this);
                }
            }

            public void ApplyArmourChange(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (sender.HasBuff(SmoulderingDocumentDebuff))
                {
                    args.armorAdd += (-10f + (stacks - 1) * -5);
                    args.attackSpeedMultAdd *= (0.9f - (stacks - 1) * 0.05f);
                }
            }
        }

        public static BuffDef SmoulderingDocumentDebuff;

        public override string ItemName => "Smouldering Document";

        public override string ItemLangTokenName => "SMOULDERING_DOCUMENT";

        public override string ItemPickupDesc => "DoTs reduce enemy armour and attack speed.";

        public override string ItemFullDescription => "<style=cIsDamage>8%</style> chance to <style=cIsDamage>ignite</style> enemies on hit for <style=cIsDamage>100%</style> damage. <style=cIsDamage>DoT</style> effects <style=cIsDamage>Burden</style> enemies, ruducing their <style=cIsDamage>armour</style> by <style=cIsDamage>10</style> <style=cStack>(+5 per stack)</style> and <style=cIsDamage>attack speed</style> by <style=cIsDamage>10%</style> <style=cStack>(+5% per stack)</style>.";

        public override string ItemLore => "<style=cStack>[insert sad story about corporate exploitation here]</style>";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("DocumentPrefab.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("DocumentIcon.png");


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
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.RoR2.DotController.InflictDot_refInflictDotInfo += DotCheck;
        }

        private void DotCheck(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo)
        {
            orig(ref inflictDotInfo);

            CharacterBody attacker = inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
            CharacterBody victim = inflictDotInfo.victimObject.GetComponent<CharacterBody>();

            DocBehaviour behaviour = victim.gameObject.GetComponent<DocBehaviour>();
            int stacks = GetCount(attacker);

            if (stacks > 0 && !behaviour)
            {
                victim.AddBuff(SmoulderingDocumentDebuff);
                behaviour = victim.gameObject.AddComponent<DocBehaviour>();
                behaviour.body = victim;
                behaviour.stacks = GetCount(attacker);
                victim.gameObject.AddComponent<ProcStorage>();
                
            }
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            GameObject attacker = damageInfo.attacker;
            if ((bool)attacker)
            {
                CharacterBody balls = attacker.GetComponent<CharacterBody>();
                CharacterBody cum = victim.GetComponent<CharacterBody>();

                float burnTotal = 1f * balls.damage;

                if ((bool)balls && (bool)cum)
                {
                    int stack = GetCount(balls);
                    if (stack > 0)
                    {
                        if (Util.CheckRoll(8f * damageInfo.procCoefficient))
                        {
                            InflictDotInfo inflictDotInfo = default(InflictDotInfo);
                            inflictDotInfo.victimObject = victim;
                            inflictDotInfo.attackerObject = attacker;
                            inflictDotInfo.totalDamage = burnTotal;
                            inflictDotInfo.dotIndex = DotController.DotIndex.Burn;
                            inflictDotInfo.damageMultiplier = 1f;
                            InflictDotInfo dotInfo = inflictDotInfo;
                            if ((bool)balls?.inventory)
                            {
                                StrengthenBurnUtils.CheckDotForUpgrade(balls.inventory, ref dotInfo);
                            }
                            DotController.InflictDot(ref dotInfo);
                        }
                    }
                }
            } orig(self, damageInfo, victim);
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
