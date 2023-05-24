using BepInEx.Configuration;
using R2API;
using RoR2;
using Sandswept.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;

namespace Sandswept.Items
{
    public class CemJar : ItemBase<CemJar>
    {
        public class JarToken : MonoBehaviour
        {
            public CharacterBody attacker;
            public CharacterBody body;
            public TeamComponent teamComponent;

            public float timer = 99;

            public void Awake()
            {
                body = GetComponent<CharacterBody>();
                teamComponent = GetComponent<TeamComponent>();
                if (!AppliedBuff.ContainsKey(teamComponent.teamIndex))
                {
                    AppliedBuff[teamComponent.teamIndex] = new List<JarToken>();
                }
                AppliedBuff[teamComponent.teamIndex].Add(this);
                if (!list.Contains(body))
                {
                    list.Add(body);
                }
            }

            public void FixedUpdate()
            {
                timer -= Time.fixedDeltaTime;
                if (timer <= 0 || body.HasBuff(CeremonialCooldown))
                {
                    Destroy(this);
                }
            }

            public void OnDestroy()
            {
                body.ClearTimedBuffs(CeremonialDef);
                TeamIndex teamIndex = teamComponent.teamIndex;
                if (AppliedBuff.ContainsKey(teamIndex))
                {
                    AppliedBuff[teamIndex].Remove(this);
                    if (list.Contains(body))
                    {
                        list.Remove(body);
                    }
                }
            }
        }

        public static BuffDef CeremonialDef;

        public static BuffDef CeremonialCooldown;

        public static DamageAPI.ModdedDamageType jarDamageType;

        public static DamageColorIndex jarDamageColour = DamageColourHelper.RegisterDamageColor(new Color32(0, 255, 204, 255));

        public override string ItemName => "Ceremonial Jar";

        public override string ItemLangTokenName => "CEREMONIAL_JAR";

        public override string ItemPickupDesc => "Hits link enemies, link multiple to damage them";

        public override string ItemFullDescription => "<color=#00ffcc></color>";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("CemJarPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("CemJarIcon.png");

        public static List<CharacterBody> list = new List<CharacterBody>();

        public static Dictionary<TeamIndex, List<JarToken>> AppliedBuff = new Dictionary<TeamIndex, List<JarToken>>();

        public override void Init(ConfigFile config)
        {
            //jarDamageType = DamageAPI.ReserveDamageType();
            CreateLang();
            CreateItem();
            CreateBuff();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += BuffApply;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int += BuffCheck;
            On.RoR2.CharacterBody.OnDeathStart += DeathRemove;
        }

        public void CreateBuff()
        {
            CeremonialDef = ScriptableObject.CreateInstance<BuffDef>();
            CeremonialDef.name = "Linked";
            CeremonialDef.canStack = false;
            CeremonialDef.isDebuff = false;
            CeremonialDef.iconSprite = Main.MainAssets.LoadAsset<Sprite>("LinkedIcon.png");
            ContentAddition.AddBuffDef(CeremonialDef);
            BuffCatalog.buffDefs.AddItem(CeremonialDef);

            CeremonialCooldown = ScriptableObject.CreateInstance<BuffDef>();
            CeremonialCooldown.name = "Cleansed";
            CeremonialCooldown.buffColor = Color.white;
            CeremonialCooldown.canStack = false;
            CeremonialCooldown.isDebuff = false;
            CeremonialCooldown.isCooldown = true;
            CeremonialCooldown.iconSprite = Main.MainAssets.LoadAsset<Sprite>("Cleansed.png");
            ContentAddition.AddBuffDef(CeremonialCooldown);
        }

        private void DeathRemove(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self)
        {
            if ((bool)self.gameObject.GetComponent<JarToken>())
            {
                list.Remove(self);
                AppliedBuff[self.teamComponent.teamIndex].Remove(GetToken(self));
            }
            orig(self);
        }

        private void BuffCheck(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float_int orig, CharacterBody self, BuffDef buffDef, float duration, int maxStacks)
        {
            if (buffDef == CeremonialDef)
            {
                var token = GetToken(self);

                var attacker = token.attacker;

                if (list.Contains(self))
                {
                    if (AppliedBuff[self.teamComponent.teamIndex].Count == 3)
                    {
                        foreach (CharacterBody body in AppliedBuff[self.teamComponent.teamIndex].Select((JarToken x) => x.body))
                        {
                            var stacks = GetCount(attacker);

                            body.AddTimedBuff(CeremonialCooldown, 5f);

                            DamageInfo extraDamageInfo = new DamageInfo
                            {
                                damage = attacker.damage * (5f + (2.5f * --stacks)),
                                attacker = attacker.gameObject,
                                procCoefficient = 0,
                                position = body.corePosition,
                                crit = false,
                                damageColorIndex = jarDamageColour,
                                damageType = DamageType.Silent
                            };
                            body.healthComponent.TakeDamage(extraDamageInfo);
                        }
                        AppliedBuff[self.teamComponent.teamIndex].RemoveAll((JarToken x) => x.body);
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

            if (damageInfo.procCoefficient > 0)
            {
                if (stacks > 0 && !victimBody.HasBuff(CeremonialDef) && !victimBody.HasBuff(CeremonialCooldown))
                {
                    if (list.Count < 3)
                    {
                        victimBody.AddTimedBuff(CeremonialDef, 3f + (0.5f * (stacks - 1f)));
                        var token = GetToken(victimBody);
                        token.attacker = attackerBody;
                        token.timer = 3f + (0.5f * (stacks - 1f));
                    }
                }
                if (stacks > 0 && victim.GetComponent<JarToken>())
                {
                    var token = GetToken(victimBody);
                    token.timer = 3f + (0.5f * (stacks - 1f));
                    victimBody.AddTimedBuff(CeremonialDef, 3f + (0.5f * (stacks - 1f)), 1);
                }
            }
            orig(self, damageInfo, victim);
        }

        public static JarToken GetToken(CharacterBody body)
        {
            if (!body.gameObject.GetComponent<JarToken>())
            {
                body.gameObject.AddComponent<JarToken>();
            }
            return body.gameObject.GetComponent<JarToken>();
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}
