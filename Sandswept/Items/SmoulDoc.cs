using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static Sandswept.Utils.Components.MaterialControllerComponents;

namespace Sandswept.Items
{
    internal class SmoulDoc : ItemBase<SmoulDoc>
    {
        public class InfoStorage : MonoBehaviour
        {
            public CharacterBody body;
            public int stacks;
        }

        public static BuffDef SmoulderingDocumentDebuff;

        public override string ItemName => "Smouldering Document";

        public override string ItemLangTokenName => "SMOULDERING_DOCUMENT";

        public override string ItemPickupDesc => "DoTs reduce enemy armour and attack speed.";

        public override string ItemFullDescription => "<style=cIsDamage>8%</style> chance to <style=cIsDamage>ignite</style> enemies on hit for <style=cIsDamage>100%</style> damage. <style=cIsDamage>DoT</style> effects <style=cIsDamage>burden</style> enemies, ruducing their <style=cIsDamage>armour</style> by <style=cIsDamage>10</style> <style=cStack>(+5 per stack)</style> and <style=cIsDamage>attack speed</style> by <style=cIsDamage>10%</style> <style=cStack>(+5% per stack)</style>.";

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
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            RecalculateStatsAPI.GetStatCoefficients += ApplyArmourChange;
            On.RoR2.DotController.InflictDot_refInflictDotInfo += DotCheck;
        }

        public void ApplyArmourChange(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(SmoulderingDocumentDebuff))
            {
                var token = sender.gameObject.GetComponent<InfoStorage>();
                args.armorAdd += -10f + ((token.stacks - 1) * -5);
                args.attackSpeedMultAdd *= 0.9f - ((token.stacks - 1) * 0.05f);
            }
        }

        private void DotCheck(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo)
        {
            CharacterBody attacker = inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
            CharacterBody victim = inflictDotInfo.victimObject.GetComponent<CharacterBody>();

            int stacks = GetCount(attacker);

            if (stacks > 0)
            {
                var behaviour = (bool)victim.gameObject.GetComponent<InfoStorage>() ? victim.gameObject.GetComponent<InfoStorage>() : victim.gameObject.AddComponent<InfoStorage>();
                behaviour.body = victim;
                behaviour.stacks = GetCount(attacker);
                if (inflictDotInfo.dotIndex == DotController.DotIndex.Burn)
                {
                    victim.AddTimedBuff(SmoulderingDocumentDebuff, (float)inflictDotInfo.totalDamage / inflictDotInfo.attackerObject.GetComponent<CharacterBody>().damage);
                    return;
                }
                victim.AddTimedBuff(SmoulderingDocumentDebuff, inflictDotInfo.duration);
            }
            orig(ref inflictDotInfo);
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if ((bool)damageInfo.attacker)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                CharacterBody victimBody = victim.GetComponent<CharacterBody>();

                if (attackerBody && victimBody)
                {
                    int stack = GetCount(attackerBody);
                    if (stack > 0)
                    {
                        if (Util.CheckRoll(8f * damageInfo.procCoefficient, attackerBody.master))
                        {
                            InflictDotInfo inflictDotInfo = default;
                            inflictDotInfo.victimObject = victim;
                            inflictDotInfo.attackerObject = damageInfo.attacker;
                            inflictDotInfo.totalDamage = attackerBody.damage;
                            inflictDotInfo.duration = default;
                            inflictDotInfo.dotIndex = DotController.DotIndex.Burn;
                            inflictDotInfo.damageMultiplier = 1f;
                            InflictDotInfo dotInfo = inflictDotInfo;
                            if ((bool)attackerBody?.inventory)
                            {
                                StrengthenBurnUtils.CheckDotForUpgrade(attackerBody.inventory, ref dotInfo);
                            }
                            DotController.InflictDot(ref dotInfo);
                        }
                    }
                }
            } 
            orig(self, damageInfo, victim);
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
