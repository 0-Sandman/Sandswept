using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace Sandswept.Items
{
    public class KillOnChance : ItemBase<KillOnChance>
    {
        public override string ItemName => "Graceful Hand";

        public override string ItemLangTokenName => "Graceful Hand";

        public override string ItemPickupDesc => "Chance to trigger On-Kill effects on hit.";

        public override string ItemFullDescription => "4% chance to trigger On-Kill effects. Enemies effected by this become immune to it for 10 seconds.";

        public override string ItemLore => "Literally witches ring, L komrade specter";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("Funny.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("Funny.png");

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += ProcDeath;
        }

        private void ProcDeath(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            var victimBody = victim.GetComponent<CharacterBody>();
            var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            var stacks = GetCount(attackerBody);

            if (stacks > 0)
            {
                if (Util.CheckRoll(3f + (1 * --stacks) * damageInfo.procCoefficient))
                {
                    DamageReport damageReport = new DamageReport(damageInfo, victimBody.healthComponent, damageInfo.damage, victimBody.healthComponent.combinedHealth);
                    GlobalEventManager.instance.OnCharacterDeath(damageReport);
                }
            }
            orig(self, damageInfo, victim);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
