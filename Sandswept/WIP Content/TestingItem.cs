/*using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace Sandswept.Items
{
    public class TestingItem : ItemBase<TestingItem>
    {
        public override string ItemName => "Testing Item";

        public override string ItemLangTokenName => "TESTING_ITEM";

        public override string ItemPickupDesc => "Put whatever you want on this, its here to test effects";

        public override string ItemFullDescription => "";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.NoTier;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("CemJarPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("CemJarIcon.png");


        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (GetCount(damageInfo.attacker.GetComponent<CharacterBody>()) > 0)
            {
                var victimBody = victim.GetComponent<CharacterBody>();

                Debug.Log(victimBody.netId);
            }
            orig(self, damageInfo, victim);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}*/
