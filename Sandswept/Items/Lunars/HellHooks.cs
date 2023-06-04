/*
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace Sandswept.Items
{
    public class HellHooks : ItemBase<HellHooks>
    {
        public static BuffDef HooksMark;
        public override string ItemName => "Hooks from Below";

        public override string ItemLangTokenName => "HELL_HOOKS";

        public override string ItemPickupDesc => "Curse enemies when hit";

        public override string ItemFullDescription => "Funny description";

        public override string ItemLore => "Jaysian is a funny cuck";

        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("HellHooks.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("HellHooksIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            CreateBuff();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += DebuffApply;
        }

        private void DebuffApply(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.attacker)
            {
                var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
                var count = GetCount(self.body);
                if (count > 0)
                {
                    damageInfo.damage = damageInfo.damage * (1f + 0.25f * count);
                    if (!attacker.HasBuff(HooksMark))
                    {
                        attacker.AddBuff(HooksMark);
                    }
                }
            }
            orig(self, damageInfo);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
*/