using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static Sandswept.Main;
using static RoR2.DotController;
using static R2API.DotAPI;

namespace Sandswept.Items
{
    internal class BloodMask : ItemBase<BloodMask>
    {
        public class WitnessToken : MonoBehaviour
        {
            public CharacterBody body;
            public int stacks;

            public void Start()
            {
                body = GetComponent<CharacterBody>();
                stacks = body.inventory.GetItemCount(instance.ItemDef);
            }
        }
        public static DotDef WitnessedDef;

        public static DotIndex WitnessedIndex;

        public override string ItemName => "Bleeding Witness";

        public override string ItemLangTokenName => "BLEEDING_WITNESS";

        public override string ItemPickupDesc => "Your bleed effects deal max health based damage";

        public override string ItemFullDescription => "<style=cIsDamage>5%</style> chance to <style=cIsDamage>bleed</style> enemies for <style=cIsDamage>120%</style> damage. Your <style=cIsDamage>bleed</style> effects additionally deal <style=cIsHealth>1%</style> <style=cStack>(+1% per stack)</style> <style=cIsHealth>max health</style> as damage.";

        public override string ItemLore => "no";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("WitnessPrefab.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("WitnessIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateDot();
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.DotController.InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 += OnBleedProc;
            On.RoR2.GlobalEventManager.OnHitEnemy += OnHit;
        }

        public void CreateDot()
        {
            WitnessedDef = new DotDef
            {
                associatedBuff = null,
                damageCoefficient = 1f,
                damageColorIndex = DamageColorIndex.Bleed,
                interval = 0.25f
            };
            DotDef dotDef = WitnessedDef;
            CustomDotBehaviour behaviour = delegate (DotController self, DotStack dotStack)
            {
                DotStack dotInfo = dotStack;

                if (!self.victimBody.HasBuff(RoR2Content.Buffs.Bleeding) || !self.victimBody.HasBuff(RoR2Content.Buffs.SuperBleed))
                {
                    self.RemoveDotStackAtServer((int)dotInfo.dotIndex);
                }

                CharacterBody attacker = dotStack.attackerObject.GetComponent<CharacterBody>();
                CharacterBody victim = self.victimObject.GetComponent<CharacterBody>();

                var token = attacker.gameObject.GetComponent<WitnessToken>();

                dotInfo.damage = (self.victimHealthComponent ? (self.victimHealthComponent.fullCombinedHealth * (0.01f * token.stacks)) : 0);
            };
            WitnessedIndex = RegisterDotDef(dotDef, behaviour, null);
        }

        private void OnHit(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            int stacks = GetCount(attacker);

            if (stacks > 0 && victim)
            {
                if (Util.CheckRoll(5f * damageInfo.procCoefficient, attacker.master))
                {
                    InflictDotInfo inflictDotInfo = default;
                    inflictDotInfo.victimObject = victim;
                    inflictDotInfo.attackerObject = damageInfo.attacker;
                    inflictDotInfo.totalDamage = attacker.damage * 1.2f;
                    inflictDotInfo.dotIndex = DotIndex.Bleed;
                    inflictDotInfo.duration = 3f;
                    inflictDotInfo.damageMultiplier = 1f;
                    InflictDotInfo dotInfo = inflictDotInfo;
                    InflictDot(ref dotInfo);
                }
            }
            orig(self, damageInfo, victim);
        }

        public void OnBleedProc(On.RoR2.DotController.orig_InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 orig, GameObject victimObject, GameObject attackerObject, DotIndex dotIndex, float duration, float damageMultiplier, uint? maxStacksFromAttacker)
        {
            orig(victimObject, attackerObject, dotIndex, duration, damageMultiplier, maxStacksFromAttacker);

            var attacker = attackerObject.GetComponent<CharacterBody>();

            var stacks = GetCount(attacker);

            if (stacks > 0)
            {
                attacker.gameObject.AddComponent<WitnessToken>();

                if (dotIndex == DotIndex.Bleed || dotIndex == DotIndex.SuperBleed)
                {
                    InflictDotInfo inflictDotInfo = default;
                    inflictDotInfo.victimObject = victimObject;
                    inflictDotInfo.attackerObject = attackerObject;
                    inflictDotInfo.totalDamage = null;
                    inflictDotInfo.dotIndex = WitnessedIndex;
                    inflictDotInfo.duration = duration;
                    inflictDotInfo.damageMultiplier = 1f;
                    inflictDotInfo.maxStacksFromAttacker = 1;
                    InflictDotInfo dotInfo = inflictDotInfo;
                    InflictDot(ref dotInfo);
                }
            }
        }
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
