using static RoR2.DotController;
using static R2API.DotAPI;

namespace Sandswept.Items.Greens
{
    internal class BleedingWitness : ItemBase<BleedingWitness>
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

        public static DotDef WitnessedDef;

        public DotIndex WitnessIndex;

        public override string ItemName => "Bleeding Witness";

        public override string ItemLangTokenName => "BLEEDING_WITNESS";

        public override string ItemPickupDesc => "Your bleed effects deal a percentage of the enemy's maximum health.";

        public override string ItemFullDescription => StringExtensions.AutoFormat("$sd5%$se chance to $sdbleed$se enemies for $sd120%$se base damage. Your $sdbleed$se effects additionally deal $sd1%$se $ss(+1% per stack)$se of the enemy's $sdmaximum health$se as damage.");

        public override string ItemLore => "no";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("WitnessPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("WitnessIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateItem();
            CreateDot();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.DotController.InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 += OnBleedProc;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            var stack = GetCount(body);
            if (stack > 0 && !body.GetComponent<WitnessToken>())
            {
                body.AddComponent<WitnessToken>();
            }
            else if (stack <= 0 && body.GetComponent<WitnessToken>())
            {
                body.RemoveComponent<WitnessToken>();
            }
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var victim = report.victim;
            if (!victim)
            {
                return;
            }

            int stacks = GetCount(attackerBody);

            if (stacks > 0)
            {
                if (Util.CheckRoll(5f * damageInfo.procCoefficient, attackerBody.master))
                {
                    InflictDotInfo inflictDotInfo = default;
                    inflictDotInfo.victimObject = victim.gameObject;
                    inflictDotInfo.attackerObject = attackerBody.gameObject;
                    inflictDotInfo.totalDamage = attackerBody.damage * 1.2f;
                    inflictDotInfo.dotIndex = DotIndex.Bleed;
                    inflictDotInfo.duration = 3f;
                    inflictDotInfo.damageMultiplier = 1f;
                    InflictDotInfo dotInfo = inflictDotInfo;
                    InflictDot(ref dotInfo);
                }
            }
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
            CustomDotBehaviour behaviour = delegate (DotController self, DotStack dotStack)
            {
                if (!self.victimBody.HasBuff(RoR2Content.Buffs.Bleeding) && !self.victimBody.HasBuff(RoR2Content.Buffs.SuperBleed))
                {
                    self.RemoveDotStackAtServer((int)WitnessIndex);
                }
                var attackerBody = dotStack.attackerObject.GetComponent<CharacterBody>();

                CharacterBody attacker = dotStack.attackerObject.GetComponent<CharacterBody>();
                CharacterBody victim = self.victimObject.GetComponent<CharacterBody>();

                var token = attacker.gameObject.GetComponent<WitnessToken>();

                dotStack.damage = self.victimHealthComponent ? self.victimHealthComponent.fullCombinedHealth * (0.01f * GetCount(attackerBody)) : 0;
            };
            WitnessIndex = RegisterDotDef(WitnessedDef, behaviour);
        }

        public void OnBleedProc(On.RoR2.DotController.orig_InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 orig, GameObject victimObject, GameObject attackerObject, DotIndex dotIndex, float duration, float damageMultiplier, uint? maxStacksFromAttacker)
        {
            var attacker = attackerObject.GetComponent<CharacterBody>();
            if (attacker)
            {
                var stacks = GetCount(attacker);

                if (stacks > 0)
                {
                    if (dotIndex == DotIndex.Bleed || dotIndex == DotIndex.SuperBleed)
                    {
                        InflictDotInfo inflictDotInfo = new()
                        {
                            victimObject = victimObject,
                            attackerObject = attackerObject,
                            totalDamage = null,
                            dotIndex = WitnessIndex,
                            duration = duration,
                            damageMultiplier = 1f,
                            maxStacksFromAttacker = 1
                        };

                        InflictDot(ref inflictDotInfo);
                    }
                }
            }
            orig(victimObject, attackerObject, dotIndex, duration, damageMultiplier, maxStacksFromAttacker);
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