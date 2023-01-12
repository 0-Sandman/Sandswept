/*using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Sandswept.Items
{
    public class CemJar : ItemBase<CemJar>
    {
        public class BuffCount : MonoBehaviour
        {
            public int count;
        }
        public class JarToken : MonoBehaviour
        {
            public static Dictionary<CharacterBody, JarToken> ownerToComponentDict = new Dictionary<CharacterBody, JarToken>();
            public static Dictionary<TeamIndex, List<JarToken>> marksPerTeam = new Dictionary<TeamIndex, List<JarToken>>();

            public CharacterBody attacker;

            public CharacterBody body;

            public bool isActive = true;

            public TeamComponent teamComponent;

            public static JarToken GetForBody(CharacterBody owner)
            {
                if (!ownerToComponentDict.ContainsKey(owner))
                {
                    ownerToComponentDict[owner] = owner.gameObject.AddComponent<JarToken>();
                }
                return ownerToComponentDict[owner];
            }

            public static bool ExistsForBody(CharacterBody owner)
            {
                return ownerToComponentDict.ContainsKey(owner);
            }

            public void Awake()
            {
                body = GetComponent<CharacterBody>();
                teamComponent = GetComponent<TeamComponent>();
                if (!marksPerTeam.ContainsKey(teamComponent.teamIndex))
                {
                    marksPerTeam[teamComponent.teamIndex] = new List<JarToken>();
                }
                marksPerTeam[teamComponent.teamIndex].Add(this);
            }

            public void OnDestroy()
            {
                TeamIndex teamIndex = teamComponent.teamIndex;
                if (marksPerTeam.ContainsKey(teamIndex))
                {
                    marksPerTeam[teamIndex].Remove(this);
                }
            }

            public void FixedUpdate()
            {
                if (!isActive)
                {
                    Destroy(this);
                }
            }
        }

        public class BuffToken : MonoBehaviour
        {
            public int buffCount;

            public void Start()
            {
                On.RoR2.CharacterBody.RemoveBuff_BuffDef += RemoveCount;
            }

            public void Destroy()
            {
                On.RoR2.CharacterBody.RemoveBuff_BuffDef -= RemoveCount;
            }

            private void RemoveCount(On.RoR2.CharacterBody.orig_RemoveBuff_BuffDef orig, CharacterBody self, BuffDef buffDef)
            {
                if (buffDef = CeremonialDef)
                {
                    if (buffCount > 0)
                    {
                        buffCount--;
                    }
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
            TeamComponent.onJoinTeamGlobal += TeamComponent_onJoinTeamGlobal;
            TeamComponent.onLeaveTeamGlobal += TeamComponent_onLeaveTeamGlobal;
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
                var buffCount = attacker.gameObject.GetComponent<BuffCount>();

                Debug.Log("buh");

                if (!JarToken.marksPerTeam.ContainsKey(self.teamComponent.teamIndex))
                {
                    return;
                }
                if (buffCount.count >= 3)
                {
                    buffCount.count = 0;
                    Debug.Log("duh");

                    foreach (CharacterBody item in JarToken.marksPerTeam[self.teamComponent.teamIndex].Select((JarToken x) => x.body))
                    {
                        Debug.Log("guh");
                        if (item.HasBuff(CeremonialDef))
                        {
                            Debug.Log("should proc");

                            var stacks = GetCount(attacker);
                            item.ClearTimedBuffs(CeremonialDef);
                            self.ClearTimedBuffs(CeremonialDef);
                            item.AddTimedBuff(CeremonialCooldown, 5f);

                            DamageInfo extraDamageInfo = new DamageInfo();
                            extraDamageInfo.damage = attacker.damage * (3 + stacks);
                            extraDamageInfo.attacker = attacker.gameObject;
                            extraDamageInfo.procCoefficient = 0;
                            extraDamageInfo.position = item.corePosition;
                            extraDamageInfo.crit = false;
                            extraDamageInfo.damageColorIndex = jarDamageColour;
                            extraDamageInfo.damageType = DamageType.Silent;
                            item.healthComponent.TakeDamage(extraDamageInfo);

                            JarToken.ownerToComponentDict.Remove(item);
                            token.isActive = false;
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
            var buffCount = attackerBody.gameObject.GetComponent<BuffCount>();
            if (damageInfo.procCoefficient > 0)
            {
                if (stacks > 0 && victim && !victimBody.HasBuff(CeremonialDef) && !victimBody.HasBuff(CeremonialCooldown))
                {
                    if (!buffCount)
                    {
                        attackerBody.gameObject.AddComponent<BuffCount>();
                        BuffApply(orig, self, damageInfo, victim);
                    }
                    JarToken forBody = JarToken.GetForBody(victimBody);
                    buffCount.count++;
                    forBody.attacker = attackerBody;
                    victimBody.AddTimedBuff(CeremonialDef, 3f + (0.5f * (stacks - 1f)));
                }
                if (stacks > 0 && victim && victimBody.HasBuff(CeremonialDef) && !victimBody.HasBuff(CeremonialCooldown))
                {
                    victimBody.AddTimedBuff(CeremonialDef, 3f + (0.5f * (stacks - 1f)), 1);
                }
            }
            orig(self, damageInfo, victim);
        }

        private void TeamComponent_onJoinTeamGlobal(TeamComponent teamComponent, TeamIndex newTeamIndex)
        {
            if (JarToken.ExistsForBody(teamComponent.body))
            {
                if (!JarToken.marksPerTeam.ContainsKey(newTeamIndex))
                {
                    JarToken.marksPerTeam[newTeamIndex] = new List<JarToken>();
                }
                JarToken.marksPerTeam[newTeamIndex].Add(JarToken.GetForBody(teamComponent.body));
            }
        }

        private void TeamComponent_onLeaveTeamGlobal(TeamComponent teamComponent, TeamIndex newTeamIndex)
        {
            if (JarToken.ExistsForBody(teamComponent.body) && JarToken.marksPerTeam.ContainsKey(newTeamIndex))
            {
                JarToken.marksPerTeam[newTeamIndex].Remove(JarToken.GetForBody(teamComponent.body));
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
    }
}*/
