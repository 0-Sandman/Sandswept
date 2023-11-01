using System.Collections.Generic;
using System.Linq;
using HG;
using TMPro;

namespace Sandswept.Items.Reds {
    public class CeremonialJar : ItemBase<CeremonialJar>
    {
        public override string ItemLangTokenName => "CEREMONIAL_JAR";

        public override string ItemPickupDesc => "Link enemies on hit. Linked enemies take massive damage.";

        public override string ItemFullDescription => "On hit, $sdlink$se enemies up to $sd3$se times. $sdLinked$se enemies take $sd1500%$se $ss(+750% per stack)$se base damage each.".AutoFormat();

        public override string ItemLore => "texLesbianFurry.png";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("Assets/Sandswept/CeremonialJarHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texCeremonialJar.png");

        public override string ItemName => "Ceremonial Jar";

        public static BuffDef CereJarLinkedBuff;
        public static BuffDef CereJarCDBuff;
        public static DamageColorIndex JarDamageColor = DamageColourHelper.RegisterDamageColor(new Color32(0, 255, 204, 255));
        public static GameObject JarVFX;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Init(ConfigFile config)
        {
            base.Init(config);
            SetupVFX();
            SetupBuffs();
        }

        public void SetupBuffs() {
            CereJarLinkedBuff = ScriptableObject.CreateInstance<BuffDef>();
            CereJarLinkedBuff.name = "Ceremonial Jar Link";
            CereJarLinkedBuff.canStack = false;
            CereJarLinkedBuff.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texGaySex.png");

            CereJarCDBuff = ScriptableObject.CreateInstance<BuffDef>();
            CereJarCDBuff.name = "Ceremonial Jar Cooldown";
            CereJarCDBuff.canStack = false;
            CereJarCDBuff.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texLesbianFurry.png");

            ContentAddition.AddBuffDef(CereJarLinkedBuff);
            ContentAddition.AddBuffDef(CereJarCDBuff);
        }

        public override void Hooks()
        {
            base.Hooks();
            On.RoR2.GlobalEventManager.OnHitEnemy += OnHitEnemy;
        }

        public void OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo info, GameObject victim) {
            orig(self, info, victim);

            GameObject attackerGO = info.attacker;

            if (!attackerGO || !attackerGO.GetComponent<CharacterBody>()) {
                return;
            }

            CharacterBody attacker = attackerGO.GetComponent<CharacterBody>();

            if (GetCount(attacker) <= 0) {
                return;
            }

            CharacterBody victimBody = victim.GetComponent<CharacterBody>();

            if (victimBody.HasBuff(CereJarCDBuff)) {
                return;
            }

            victimBody.AddTimedBuff(CereJarLinkedBuff, 5f);

            List<CharacterBody> bodies = new();

            for (int i = 0; i < CharacterBody.readOnlyInstancesList.Count; i++) {
                if (CharacterBody.readOnlyInstancesList[i].HasBuff(CereJarLinkedBuff)) {
                    bodies.Add(CharacterBody.readOnlyInstancesList[i]);
                }
            }

            if (bodies.Count >= 3) {
                bodies.ForEach(x => {
                    x.RemoveBuff(CereJarLinkedBuff);
                    x.AddTimedBuff(CereJarCDBuff, 5f);

                    DamageInfo info = new();
                    info.damage = attacker.damage * (15 + (7 * (GetCount(attacker) - 1)));
                    info.crit = false;
                    info.damageColorIndex = JarDamageColor;
                    info.attacker = attacker.gameObject;
                    info.position = x.corePosition;
                    info.procCoefficient = 0;

                    x.healthComponent.TakeDamage(info);

                    EffectManager.SpawnEffect(Assets.GameObject.IgniteExplosionVFX, new EffectData {
                        scale = 2f,
                        origin = x.corePosition
                    }, true);
                });
            }
        }

        public void SetupVFX() {
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
        }
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