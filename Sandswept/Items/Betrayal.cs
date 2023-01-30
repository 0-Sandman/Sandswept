using BepInEx.Configuration;
using R2API;
using RoR2;
using Sandswept.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace Sandswept.Items
{
    public class Betrayal : ItemBase<Betrayal>
    {
        public class SunderBehaviour : MonoBehaviour
        {
            public int HitCount;
            public int ProcCount;
            public NetworkInstanceId LastID;

            public void Start()
            {
                On.RoR2.GlobalEventManager.OnHitEnemy += EnemyHit;
            }

            private void EnemyHit(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
            {
                if (damageInfo.procCoefficient > 0)
                {

                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                var victimBody = victim.GetComponent<CharacterBody>();

                    if (attackerBody.inventory.GetItemCount(instance.ItemDef) > 0)
                    {
                        if (victimBody.netId != LastID)
                        {
                            LastID = victimBody.netId;
                            HitCount = 0;
                            ProcCount = 0;
                        }
                        HitCount++;
                        if (HitCount == 3)
                        {
                            HitCount = 0;
                            DamageInfo extraDamageInfo = new DamageInfo
                            {
                                damage = attackerBody.damage * (3 + ProcCount),
                                attacker = damageInfo.attacker,
                                procCoefficient = 0,
                                position = victimBody.corePosition,
                                crit = false,
                                damageColorIndex = betrayalDamageColour,
                                damageType = DamageType.Silent
                            };
                            victimBody.healthComponent.TakeDamage(extraDamageInfo);
                            ProcCount++;
                        }
                    }
                }
                orig(self, damageInfo, victim);
            }
        }
        public static DamageColorIndex betrayalDamageColour = DamageColourHelper.RegisterDamageColor(new Color32(175, 255, 30, 255));

        public override string ItemName => "Betrayal";

        public override string ItemLangTokenName => "BETRAYAL";

        public override string ItemPickupDesc => "Repeated strikes on enemies crush them";

        public override string ItemFullDescription => "Hitting an enemy 3 consecutive times sunders them for 300% (+300% per proc) damage.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Boss;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("BetrayalPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("BetrayalIcon.png");


        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            if (GetCount(self) > 0 && !self.gameObject.GetComponent<SunderBehaviour>())
            {
                self.gameObject.AddComponent<SunderBehaviour>();
            }
            orig(self);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
