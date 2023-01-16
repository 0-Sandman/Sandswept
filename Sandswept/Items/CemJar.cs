using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Sandswept.Items
{
    public class CemJar : ItemBase<CemJar>
    {
        public class JarToken : MonoBehaviour
        {
            public CharacterBody attacker;
            public CharacterBody body;
            public bool active = true;
            public float timer;

            public void FixedUpdate()
            {
                if (!active)
                {
                    Destroy(this);
                }

                timer -= Time.fixedDeltaTime;
                if (timer <= 0 || body.HasBuff(CeremonialCooldown))
                {
                    var token = attacker.gameObject.GetComponent<BuffToken>();
                    token.list.Remove(body);
                    active = false;
                }
            }
        }

        public class BuffToken : MonoBehaviour
        {
            public List<CharacterBody> list = new List<CharacterBody>();

            public void Start()
            {
                On.RoR2.CharacterBody.RemoveBuff_BuffDef += RemoveCount;
                On.RoR2.CharacterBody.OnDeathStart += CharacterBody_OnDeathStart;
            }

            private void CharacterBody_OnDeathStart(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self)
            {
                if (self.gameObject.GetComponent<JarToken>())
                {
                    list.Remove(self);
                }
                orig(self);
            }

            public void Destroy()
            {
                On.RoR2.CharacterBody.RemoveBuff_BuffDef -= RemoveCount;
                On.RoR2.CharacterBody.OnDeathStart -= CharacterBody_OnDeathStart;
            }

            private void RemoveCount(On.RoR2.CharacterBody.orig_RemoveBuff_BuffDef orig, CharacterBody self, BuffDef buffDef)
            {
                if (buffDef = CeremonialDef)
                {
                    list.Remove(self);
                }
                orig(self, buffDef);
            }
        }

        public static BuffDef CeremonialDef;

        public static BuffDef CeremonialCooldown;

        public static DamageAPI.ModdedDamageType jarDamageType;

        public static DamageColorIndex jarDamageColour = DamageColourHelper.RegisterDamageColor(new Color32(175, 255, 30, 255));

        public override string ItemName => "Ceremonial Jar";

        public override string ItemLangTokenName => "CEREMONIAL_JAR";

        public override string ItemPickupDesc => "Hits link enemies, link multiple to damage them";

        public override string ItemFullDescription => "";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("CemJarPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("CemJarIcon.png");

        public int buffCount = 0;

        public override void Init(ConfigFile config)
        {
            jarDamageType = DamageAPI.ReserveDamageType();
            CreateLang();
            CreateItem();
            CreateBuff();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += BuffApply;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int += BuffCheck;
        }

        public void CreateBuff()
        {
            CeremonialDef = ScriptableObject.CreateInstance<BuffDef>();
            CeremonialDef.name = "Linked";
            CeremonialDef.buffColor = new Color32(245, 153, 80, 255);
            CeremonialDef.canStack = false;
            CeremonialDef.isDebuff = false;
            CeremonialDef.iconSprite = Main.MainAssets.LoadAsset<Sprite>("Linked.png");
            ContentAddition.AddBuffDef(CeremonialDef);

            CeremonialCooldown = ScriptableObject.CreateInstance<BuffDef>();
            CeremonialCooldown.name = "Cleansed";
            CeremonialCooldown.buffColor = new Color32(140, 153, 60, 255);
            CeremonialCooldown.canStack = false;
            CeremonialCooldown.isDebuff = false;
            CeremonialCooldown.iconSprite = Main.MainAssets.LoadAsset<Sprite>("Cleansed.png");
            ContentAddition.AddBuffDef(CeremonialCooldown);
        }

        private void BuffCheck(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float_int orig, CharacterBody self, BuffDef buffDef, float duration, int maxStacks)
        {
            if (buffDef = CeremonialDef)
            {
                var token = self.gameObject.GetComponent<JarToken>();

                var attacker = token.attacker;
                var buffCount = attacker.gameObject.GetComponent<BuffToken>();

                Debug.Log("buh");

                if (buffCount.list.Count < 3 && !buffCount.list.Contains(self))
                {
                    buffCount.list.Add(self);
                    Debug.Log(buffCount.list.Count);

                    if (buffCount.list.Count == 3)
                    {
                        foreach (CharacterBody body in buffCount.list)
                        {
                            Debug.Log("should proc");

                            var stacks = GetCount(attacker);
                            body.RemoveBuff(CeremonialDef);
                            body.AddTimedBuff(CeremonialCooldown, 5f);

                            DamageInfo extraDamageInfo = new DamageInfo
                            {
                                damage = attacker.damage * (3 + stacks),
                                attacker = attacker.gameObject,
                                procCoefficient = 0,
                                position = self.corePosition,
                                crit = false,
                                damageColorIndex = jarDamageColour,
                                damageType = DamageType.Silent
                            };
                            self.healthComponent.TakeDamage(extraDamageInfo);
                        }
                    }
                }
            }
            orig(self, buffDef, duration, maxStacks);
        }

        private void BuffApply(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            var victimBody = victim.GetComponent<CharacterBody>();

            var stacks = GetCount(attackerBody);

            var buffToken = attackerBody.gameObject.GetComponent<BuffToken>();
            if (damageInfo.procCoefficient > 0)
            {
                if (stacks > 0 && victim && !victimBody.HasBuff(CeremonialDef) && !victimBody.HasBuff(CeremonialCooldown))
                {
                    if (!buffToken)
                    {
                        attackerBody.gameObject.AddComponent<BuffToken>();
                        BuffApply(orig, self, damageInfo, victim);
                        return;
                    }

                    if (!buffToken.list.Contains(victimBody))
                    {
                        var token = victim.AddComponent<JarToken>();
                        token.body = victimBody;
                        token.attacker = attackerBody;
                        token.timer = 3f + (0.5f * (stacks - 1f));

                        if (buffToken.list.Count < 3)
                        {
                            victimBody.AddTimedBuff(CeremonialDef, 3f + (0.5f * (stacks - 1f)));
                        }
                    }
                }
                if (stacks > 0 && victim && victimBody.HasBuff(CeremonialDef) && !victimBody.HasBuff(CeremonialCooldown))
                {
                    var token = victim.GetComponent<JarToken>();
                    token.timer = 3f + (0.5f * (stacks - 1f));
                    victimBody.AddTimedBuff(CeremonialDef, 3f + (0.5f * (stacks - 1f)), 1);
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
