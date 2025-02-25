using System.Collections.Generic;
using System.Linq;
using HG;
using TMPro;

namespace Sandswept.Items.Reds
{
    [ConfigSection("Items :: Ceremonial Jar")]
    public class CeremonialJar : ItemBase<CeremonialJar>
    {
        public override string ItemLangTokenName => "CEREMONIAL_JAR";

        public override string ItemPickupDesc => "Enemies hit with any skill get linked. Linked enemies take massive damage.";

        public override string ItemFullDescription => $"Enemies hit with any $suskill$se get $sdlinked$se, up to $sd{linkedEnemiesRequirement}$se times. Linked enemies take $sd{baseDamage * 100f}%$se $ss(+{stackDamage * 100f}% per stack)$se $sdbase damage$se each and cannot be $sdlinked$se for $sd{linkedEnemyCooldown}$se seconds afterward.".AutoFormat();

        public override string ItemLore => "\"In the early days, the Tar was localized to only a few of the dunepeople's smaller towns and villages. This slow start was merely a silent beginning to its takeover, though, ending with the crusades of those now called the \"tainted generation,\" for whom the Tar had been present since birth.\r\n\r\nAs the Tar's cultural relevance in those isolated places grew, it stopped being a symbiotic equal, and became a divine superior. It was worshipped as a god, one whose existence was undeniable, and whose gifts were powerful, and the tainted generation took it upon themselves to crusade through all of Aphelia and spread it.\r\n\r\nAt first, the crusades were met with only skepticism and rejection from those not used to the Tar's presence. Some doubted the Tar's existence; those who didn't, doubted its power; and those who did neither, were rightfully wary of its influence. With an inability to demonstrate the Tar's existence or potency without travelling all the way back to their hometowns, the crusaders only recruited a scarce few. With their new handful of allies, though, they discovered a way to transport the Tar, and thus, its first vessel was created.\r\n\r\nThough simple in design, this original clay jar was the cornerstone to the Tar epidemic. The original jar was worshipped as an idol. The dunepeople fed what they had to it, allowing the Tar to spread forth. Brought from town to town, village to village, city to city, the vessel crushed all doubts of the Tar's power or existence, quickly integrating itself into all of the dunepeople's struggling civilization. Those who continued to warn against the Tar's influence were bribed with its power, and those who couldn't be bribed were fed to Aphelia's new god.\"\r\n\r\n- Tragedy of Aphelia\r\n";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("CeremonialJarHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texCeremonialJar.png");

        public override string ItemName => "Ceremonial Jar";

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };

        public static BuffDef CereJarLinkedBuff;
        public static BuffDef CereJarCDBuff;
        public static DamageColorIndex JarDamageColor = DamageColourHelper.RegisterDamageColor(new Color32(0, 255, 204, 255));
        public static GameObject JarVFX;

        [ConfigField("Linked Enemies Requirement", "", 3)]
        public static int linkedEnemiesRequirement;

        [ConfigField("Base Damage", "Decimal.", 15f)]
        public static float baseDamage;

        [ConfigField("Stack Damage", "Decimal.", 15f)]
        public static float stackDamage;

        [ConfigField("Linked Enemy Cooldown", "", 5f)]
        public static float linkedEnemyCooldown;

        [ConfigField("Proc Coefficient", "", 0.33f)]
        public static float procCoefficient;

        public override float modelPanelParametersMinDistance => 5f;
        public override float modelPanelParametersMaxDistance => 12f;

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

        public void SetupBuffs()
        {
            CereJarLinkedBuff = ScriptableObject.CreateInstance<BuffDef>();
            CereJarLinkedBuff.name = "Ceremonial Jar Link";
            CereJarLinkedBuff.canStack = false;
            CereJarLinkedBuff.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texGaySex.png");

            CereJarCDBuff = ScriptableObject.CreateInstance<BuffDef>();
            CereJarCDBuff.name = "Ceremonial Jar Cooldown";
            CereJarCDBuff.canStack = false;
            CereJarCDBuff.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texLesbianFurry.png");

            ContentAddition.AddBuffDef(CereJarLinkedBuff);
            ContentAddition.AddBuffDef(CereJarCDBuff);
        }

        public override void Hooks()
        {
            base.Hooks();
            On.RoR2.GlobalEventManager.OnHitEnemy += OnHitEnemy;
        }

        public void OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo info, GameObject victim)
        {
            orig(self, info, victim);

            GameObject attackerGO = info.attacker;

            if (!info.damageType.IsDamageSourceSkillBased)
            {
                return;
            }

            if (!attackerGO || !attackerGO.GetComponent<CharacterBody>())
            {
                return;
            }

            CharacterBody attacker = attackerGO.GetComponent<CharacterBody>();

            if (GetCount(attacker) <= 0)
            {
                return;
            }

            CharacterBody victimBody = victim.GetComponent<CharacterBody>();

            if (victimBody.HasBuff(CereJarCDBuff))
            {
                // Debug.Log("returning because cd");
                return;
            }

            victimBody.SetBuffCount(CereJarLinkedBuff.buffIndex, 1);
            // Debug.Log("adding buff");

            List<CharacterBody> bodies = new();

            for (int i = 0; i < CharacterBody.readOnlyInstancesList.Count; i++)
            {
                if (CharacterBody.readOnlyInstancesList[i].HasBuff(CereJarLinkedBuff))
                {
                    bodies.Add(CharacterBody.readOnlyInstancesList[i]);
                }
            }

            if (bodies.Count >= linkedEnemiesRequirement)
            {
                bodies.ForEach(x =>
                {
                    x.SetBuffCount(CereJarLinkedBuff.buffIndex, 0);
                    x.AddTimedBuff(CereJarCDBuff, linkedEnemyCooldown);

                    DamageInfo info = new()
                    {
                        damage = attacker.damage * (baseDamage + (stackDamage * (GetCount(attacker) - 1))),
                        crit = false,
                        damageColorIndex = JarDamageColor,
                        attacker = attacker.gameObject,
                        position = x.corePosition,
                        procCoefficient = procCoefficient
                    };

                    x.healthComponent.TakeDamage(info);

                    EffectManager.SpawnEffect(Paths.GameObject.IgniteExplosionVFX, new EffectData
                    {
                        scale = 2f,
                        origin = x.corePosition
                    }, true);
                });
            }
        }

        public void SetupVFX()
        {
            JarVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifyStack3Effect.prefab").WaitForCompletion().InstantiateClone("CeremonialJarEffect", false);
            JarVFX.name = "CeremonialJarEffect";

            Object.Destroy(JarVFX.transform.Find("Visual").Find("Stack 2").gameObject);

            Object.Destroy(JarVFX.transform.Find("Visual").Find("Stack 3").gameObject);

            GameObject stack = JarVFX.transform.Find("Visual").Find("Stack 1").gameObject;
            stack.name = "Donuts";
            stack.GetComponent<MeshFilter>().mesh = Main.prodAssets.LoadAsset<Mesh>("assets/jardonuts.obj");

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
            mat.SetTexture("_RemapTex", Main.prodAssets.LoadAsset<Texture2D>("assets/jarramp.png"));
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