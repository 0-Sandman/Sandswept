using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static Sandswept.Buffs.Witnessed;
using static Sandswept.Main;
using static RoR2.DotController;

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
                body = this.GetComponent<CharacterBody>();
                stacks = body.inventory.GetItemCount(instance.ItemDef);
            }
        }

        public override string ItemName => "Bleeding Witness";

        public override string ItemLangTokenName => "BLEEDING_WITNESS";

        public override string ItemPickupDesc => "Your bleed effects deal max health based damage";

        public override string ItemFullDescription => "<style=cIsDamage>5%</style> chance to <style=cIsDamage>bleed</style> enemies on hit. Your <style=cIsDamage>bleed</style> effects additionally deal <style=cIsHealth>1%</style> <style=cStack>(+1% per stack)</style> <style=cIsHealth>max health</style> as damage.";

        public override string ItemLore => "no";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("WitnessPrefab.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("WitnessIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.DotController.InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 += OnBleedProc;
        }

        public void OnBleedProc(On.RoR2.DotController.orig_InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 orig, GameObject victimObject, GameObject attackerObject, DotIndex dotIndex, float duration, float damageMultiplier, uint? maxStacksFromAttacker)
        {
            orig(victimObject, attackerObject, dotIndex, duration, damageMultiplier, maxStacksFromAttacker);

            var victim = victimObject.GetComponent<CharacterBody>();
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
