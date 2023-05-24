using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static RoR2.DotController;
using static R2API.DotAPI;
using static RoR2.OverlapAttack;

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
            }
        }
        public DotDef WitnessDef;

        public DotIndex WitnessIndex;

        public override string ItemName => "Bleeding Witness";

        public override string ItemLangTokenName => "BLEEDING_WITNESS";

        public override string ItemPickupDesc => "Your bleed effects deal max health based damage";

        public override string ItemFullDescription => "<style=cIsDamage>5%</style> chance to <style=cIsDamage>bleed</style> enemies for <style=cIsDamage>120%</style> damage. Your <style=cIsDamage>bleed</style> effects additionally deal <style=cIsHealth>1%</style> <style=cStack>(+1% per stack)</style> <style=cIsHealth>max health</style> as damage.";

        public override string ItemLore => "no";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("WitnessPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("WitnessIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateDot();
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.DotController.InflictDot_refInflictDotInfo += OnBleedProc;
            On.RoR2.GlobalEventManager.OnHitEnemy += OnHit;
        }

        public void CreateDot()
        {
            WitnessDef = new DotDef
            {
                associatedBuff = null,
                damageCoefficient = 1f,
                damageColorIndex = DamageColorIndex.Bleed,
                interval = 0.25f
            };
            CustomDotBehaviour behaviour = delegate (DotController self, DotStack dotStack)
            {
                if (!self.victimBody.HasBuff(RoR2Content.Buffs.Bleeding) && !self.victimBody.HasBuff(RoR2Content.Buffs.SuperBleed))
                {
                    self.RemoveDotStackAtServer((int)WitnessIndex);
                }
                var attackerBody = dotStack.attackerObject.GetComponent<CharacterBody>();

                dotStack.damage = self.victimHealthComponent ? self.victimHealthComponent.fullCombinedHealth * (0.01f * GetCount(attackerBody)) : 0;
            };
            WitnessIndex = RegisterDotDef(WitnessDef, behaviour);
        }

        public void OnHit(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.attacker)
            {
                var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
                int stacks = GetCount(attacker);

                if (stacks > 0 && (bool)victim)
                {
                    if (Util.CheckRoll(5f * damageInfo.procCoefficient, attacker.master))
                    {
                        InflictDotInfo inflictDotInfo = new InflictDotInfo
                        {
                            victimObject = victim,
                            attackerObject = damageInfo.attacker,
                            totalDamage = attacker.damage * 1.2f,
                            dotIndex = DotIndex.Bleed,
                            duration = 3f,
                            damageMultiplier = 1f
                        };
                        InflictDot(ref inflictDotInfo);
                    }
                }
            }
            orig(self, damageInfo, victim);
        }

        public void OnBleedProc(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo)
        {
            orig(ref inflictDotInfo);

            if (inflictDotInfo.attackerObject != inflictDotInfo.victimObject && inflictDotInfo.dotIndex != WitnessIndex)
            {
                Debug.Log("Cum");
                Debug.Log((int)WitnessIndex);
                var attacker = inflictDotInfo.attackerObject.GetComponent<CharacterBody>();

                var stacks = GetCount(attacker);

                if (stacks > 0)
                {
                    Debug.Log("Ass");
                    if (!attacker.gameObject.GetComponent<WitnessToken>())
                    {
                        attacker.gameObject.AddComponent<WitnessToken>();
                    }
                    if (inflictDotInfo.dotIndex == DotIndex.Bleed || inflictDotInfo.dotIndex == DotIndex.SuperBleed)
                    {
                        Debug.Log("Dick");
                        InflictDotInfo dotInfo = new InflictDotInfo
                        {
                            victimObject = inflictDotInfo.victimObject,
                            attackerObject = inflictDotInfo.attackerObject,
                            totalDamage = null,
                            dotIndex = WitnessIndex,
                            duration = inflictDotInfo.duration,
                            damageMultiplier = 1f,
                            maxStacksFromAttacker = 1
                        };
                        InflictDot(ref dotInfo);
                    }
                }
            }
        }
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
