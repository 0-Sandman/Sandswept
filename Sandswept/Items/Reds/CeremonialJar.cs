using System.Collections.Generic;
using System.Linq;
using HG;
using TMPro;

// ay are we sure this a red? this feels very underwhelming esp. in single stack
namespace Sandswept.Items.Reds
{
    public class CeremonialJar : ItemBase<CeremonialJar>
    {
        public static BuffDef CeremonialDef;

        public static BuffDef CeremonialCooldown;

        public static GameObject JarVFX;

        public static DamageAPI.ModdedDamageType jarDamageType;

        public static DamageColorIndex jarDamageColour = DamageColourHelper.RegisterDamageColor(new Color32(0, 255, 204, 255));

        public override string ItemName => "Ceremonial Jar";

        public override string ItemLangTokenName => "CEREMONIAL_JAR";

        public override string ItemPickupDesc => "Hits link enemies, link multiple to damage them";

        public override string ItemFullDescription => "<color=#00ffcc>J</color>";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.MainAssets.LoadAsset<GameObject>("CemJarPrefab.prefab");

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("CemJarIcon.png");

        public static List<CharacterBody> list = new();

        public static Dictionary<TeamIndex, List<JarToken>> AppliedBuff = new();

        public override void Init(ConfigFile config)
        {
            jarDamageType = DamageAPI.ReserveDamageType();

            // full on 6hr code incoming

            JarVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifyStack3Effect.prefab").WaitForCompletion().InstantiateClone("CeremonialJarEffect", false);
            JarVFX.name = "CeremonialJarEffect";

            Object.Destroy(JarVFX.transform.Find("Visual").Find("Stack 2").gameObject);
            Object.Destroy(JarVFX.transform.Find("Visual").Find("Stack 3").gameObject);
            GameObject stack = JarVFX.transform.Find("Visual").Find("Stack 1").gameObject;
            stack.name = "Donuts";
            stack.GetComponent<MeshFilter>().mesh = Main.Asset2s.LoadAsset<Mesh>("assets/jardonuts.obj");
            ObjectScaleCurve osc = stack.AddComponent<ObjectScaleCurve>();
            osc.enabled = false;
            AnimationCurve curve = new();
            curve.AddKey(0, 1);
            curve.keys[0].inTangent = 4;
            curve.keys[0].outTangent = 4;
            curve.AddKey(0.5f, 4);
            curve.keys[1].inTangent = 0;
            curve.keys[1].outTangent = 0;
            osc.curveX = curve;
            osc.curveY = curve;
            osc.curveZ = curve;
            osc.overallCurve = curve;
            Object.Destroy(stack.GetComponent<AnimateShaderAlpha>());
            GuhAlpha guh = stack.AddComponent<GuhAlpha>();
            AnimationCurve curve2 = new();
            curve2.AddKey(0, 1);
            curve2.AddKey(0.5f, 0);
            guh.alphaCurve = curve2;
            guh.timeMax = 0.5f;
            guh.destroyOnEnd = true;
            guh.enabled = false;
            Material mat = Object.Instantiate(stack.GetComponent<MeshRenderer>().material);
            mat.name = "matJarDonuts";
            mat.SetTexture("_RemapTex", Main.Asset2s.LoadAsset<Texture2D>("assets/jarramp.png"));
            stack.GetComponent<MeshRenderer>().material = mat;

            GameObject parts = Object.Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/LevelUpEffect.prefab").WaitForCompletion().transform.Find("Dust Explosion").gameObject);
            parts.name = "Particles";
            Color c_sandsweep = new Color(0.1333f, 1f, 0.5f, 1f);
            ParticleSystem ps = parts.GetComponent<ParticleSystem>();
            ps.colorOverLifetime.color.gradient.colorKeys[0].color = c_sandsweep;
            ps.colorOverLifetime.color.gradientMax.colorKeys[0].color = c_sandsweep;
            ps.loop = true;
            ps.gravityModifier = 0.2f;
            ps.emissionRate = 20;
            ps.maxParticles = 20;
            parts.transform.localScale *= 0.75f;
            parts.transform.Translate(0f, 0f, -2f);
            parts.transform.parent = JarVFX.transform;

            // JarVFX.AddComponent<EffectComponent>().applyScale = true;
            // Main.EffectPrefabs.Add(JarVFX);

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
            On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += UpdateVFX;
        }

        public void CreateBuff()
        {
            CeremonialDef = ScriptableObject.CreateInstance<BuffDef>();
            CeremonialDef.name = "Linked";
            CeremonialDef.canStack = false;
            CeremonialDef.isDebuff = true;
            CeremonialDef.iconSprite = Main.MainAssets.LoadAsset<Sprite>("LinkedIcon.png");
            ContentAddition.AddBuffDef(CeremonialDef);
            // BuffCatalog.buffDefs.AddItem(CeremonialDef);

            CeremonialCooldown = ScriptableObject.CreateInstance<BuffDef>();
            CeremonialCooldown.name = "Cleansed";
            CeremonialCooldown.buffColor = Color.white;
            CeremonialCooldown.canStack = false;
            CeremonialCooldown.isDebuff = false;
            CeremonialCooldown.isCooldown = true;
            CeremonialCooldown.iconSprite = Main.MainAssets.LoadAsset<Sprite>("Cleansed.png");
            ContentAddition.AddBuffDef(CeremonialCooldown);
            // BuffCatalog.buffDefs.AddItem(CeremonialCooldown);
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
            orig(self, buffDef, duration, maxStacks);
            if (buffDef == CeremonialDef)
            {
                var token = GetToken(self);

                var attacker = token.attacker;

                if (list.Contains(self))
                {
                    if (AppliedBuff[self.teamComponent.teamIndex].Count >= 3)
                    {
                        UpdateVFX((_) => { }, self); // final enemy have vfx as well
                        foreach (CharacterBody body in AppliedBuff[self.teamComponent.teamIndex].Select((x) => x.body))
                        {
                            var stacks = GetCount(attacker);

                            body.AddTimedBuff(CeremonialCooldown, 5f);

                            DamageInfo extraDamageInfo = new()
                            {
                                damage = attacker.damage * 5f + 2.5f * (stacks - 1),
                                attacker = attacker.gameObject,
                                procCoefficient = 0,
                                position = body.corePosition,
                                crit = false,
                                damageColorIndex = jarDamageColour,
                                damageType = DamageType.Silent
                            };
                            body.healthComponent.TakeDamage(extraDamageInfo);

                            EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.igniteOnKillExplosionEffectPrefab, new EffectData
                            {
                                origin = body.corePosition,
                                scale = body.radius * 2
                            }, transmit: false);
                            if (!JarVFXList.ContainsKey(body) || !(bool)JarVFXList[body]) continue;
                            GameObject _JarVFX = JarVFXList[body].gameObject;
                            GameObject stack = _JarVFX.transform.Find("Visual").Find("Donuts").gameObject;
                            ArrayUtils.ArrayAppend(ref _JarVFX.GetComponent<TemporaryVisualEffect>().exitComponents, stack.GetComponent<GuhAlpha>());
                            ArrayUtils.ArrayAppend(ref _JarVFX.GetComponent<TemporaryVisualEffect>().exitComponents, stack.GetComponent<ObjectScaleCurve>());
                            _JarVFX.transform.Find("Particles").gameObject.SetActive(false);
                            _JarVFX.GetComponent<DestroyOnTimer>().duration = 0.5f;
                        }
                        AppliedBuff[self.teamComponent.teamIndex].RemoveAll((x) => x.body);
                    }
                }
            }
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
                        victimBody.AddTimedBuff(CeremonialDef, 3f + 0.5f * (stacks - 1f));
                        var token = GetToken(victimBody);
                        token.attacker = attackerBody;
                        token.timer = 3f + 0.5f * (stacks - 1f);
                    }
                }
                if (stacks > 0 && victim.GetComponent<JarToken>())
                {
                    var token = GetToken(victimBody);
                    token.timer = 3f + 0.5f * (stacks - 1f);
                    victimBody.AddTimedBuff(CeremonialDef, 3f + 0.5f * (stacks - 1f), 1);
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

        private Dictionary<CharacterBody, TemporaryVisualEffect> JarVFXList = new();

        public void UpdateVFX(On.RoR2.CharacterBody.orig_UpdateAllTemporaryVisualEffects orig, CharacterBody self)
        {
            orig(self);
            TemporaryVisualEffect _JarVFX;
            if (JarVFXList.ContainsKey(self)) _JarVFX = JarVFXList[self];
            else _JarVFX = new();
            self.UpdateSingleTemporaryVisualEffect(ref _JarVFX, JarVFX, self.radius, self.HasBuff(CeremonialDef));
            if (!self.HasBuff(CeremonialDef)) return;
            ParticleSystem ps = _JarVFX.transform.Find("Particles").GetComponent<ParticleSystem>();
            ps.maxParticles = (int)(ps.maxParticles * self.radius);
            ps.emissionRate *= self.radius;
            JarVFXList[self] = _JarVFX;
        }

        public class GuhAlpha : AnimateShaderAlpha
        {
            private void Update()
            {
                if (!pauseTime) time = Mathf.Min(timeMax, time + Time.deltaTime);
                float num = alphaCurve.Evaluate(time / timeMax);
                Material[] array = materials;
                for (int i = 0; i < array.Length; i++)
                {
                    _ = array[i];
                    _propBlock = new MaterialPropertyBlock();
                    targetRenderer.GetPropertyBlock(_propBlock);
                    _propBlock.SetColor("_TintColor", Color.white.AlphaMultiplied(num));
                    targetRenderer.SetPropertyBlock(_propBlock);
                }
                if (time >= timeMax)
                {
                    if (disableOnEnd) enabled = false;
                    if (destroyOnEnd) Destroy(gameObject);
                }
            }
        }
    }

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
            if (!CeremonialJar.AppliedBuff.ContainsKey(teamComponent.teamIndex))
            {
                CeremonialJar.AppliedBuff[teamComponent.teamIndex] = new List<JarToken>();
            }
            CeremonialJar.AppliedBuff[teamComponent.teamIndex].Add(this);
            if (!CeremonialJar.list.Contains(body))
            {
                CeremonialJar.list.Add(body);
            }
        }

        public void FixedUpdate()
        {
            timer -= Time.fixedDeltaTime;
            if (timer <= 0 || body.HasBuff(CeremonialJar.CeremonialCooldown))
            {
                Destroy(this);
            }
        }

        public void OnDestroy()
        {
            body.ClearTimedBuffs(CeremonialJar.CeremonialDef);
            TeamIndex teamIndex = teamComponent.teamIndex;
            if (CeremonialJar.AppliedBuff.ContainsKey(teamIndex))
            {
                CeremonialJar.AppliedBuff[teamIndex].Remove(this);
                if (CeremonialJar.list.Contains(body))
                {
                    CeremonialJar.list.Remove(body);
                }
            }
        }
    }
}