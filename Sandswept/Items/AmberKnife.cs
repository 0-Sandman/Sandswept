using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static R2API.RecalculateStatsAPI;

namespace Sandswept.Items
{
    public class AmberKnife : ItemBase<AmberKnife>
    {
        public override string ItemName => "Amber Knife";

        public override string ItemLangTokenName => "AMBER_KNIFE";

        public override string ItemPickupDesc => "Gain temporary barrier on crit";

        public override string ItemFullDescription => "Gain <style=cIsDamage>5%</style> <style=cStack>(+5% per stack)</style> <style=cIsDamage>critical chance</style>. <style=cIsDamage>Critical Strikes</style> grant a <style=cIsHealing>temporary barrier</style> for <style=cIsHealing>10</style> <style=cStack>(+5 per stack)</style> <style=cIsHealing>health</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("AmberKnifePrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("AmberKnifeIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCrit += GiveBarrier;
            GetStatCoefficients += GiveCrit;
        }

        public void GiveCrit(CharacterBody sender, StatHookEventArgs args)
        {
            int stacks = GetCount(sender);
            if (stacks > 0)
            {
                args.critAdd += 5f * stacks;
            }
        }

        public void GiveBarrier(On.RoR2.GlobalEventManager.orig_OnCrit orig, GlobalEventManager self, CharacterBody body, DamageInfo damageInfo, CharacterMaster master, float procCoefficient, ProcChainMask procChainMask)
        {
            orig(self, body, damageInfo, master, procCoefficient, procChainMask);

            CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();

            int stacks = GetCount(attackerBody);

            if (stacks > 0 && damageInfo.procCoefficient > 0)
            {
                attackerBody.healthComponent.AddBarrier(5 + (5 * stacks));
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

    }
}
