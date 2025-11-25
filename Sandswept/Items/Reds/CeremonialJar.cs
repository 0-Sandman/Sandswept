using System.Collections.Generic;
using System.Linq;
using HG;
using LookingGlass.ItemStatsNameSpace;
using TMPro;

namespace Sandswept.Items.Reds
{
    [ConfigSection("Items :: Ceremonial Jar")]
    public class CeremonialJar : ItemBase<CeremonialJar>
    {
        public override string ItemLangTokenName => "CEREMONIAL_JAR";

        public override string ItemPickupDesc => "Enemies hit with any skill get linked. Linked enemies take massive damage.";

        public override string ItemFullDescription => $"Enemies hit with any $suskill$se get $sdlinked$se, up to $sd{linkedEnemiesRequirement}$se times. Linked enemies take $sd{baseDamage * 100f}%$se $ss(+{stackDamage * 100f}% per stack)$se $sdbase damage$se each and cannot be $sdlinked$se for $sd{linkedEnemyCooldown}$se seconds afterward.".AutoFormat();

        public override string ItemLore =>
        """
        "In the early days, the Tar was localized to only a few of the dunepeople's smaller towns and villages. This slow start was merely a silent beginning to its takeover, though, ending with the crusades of those now called the "tainted generation," for whom the Tar had been present since birth.

        As the Tar's cultural relevance in those isolated places grew, it stopped being a symbiotic equal, and became a divine superior. It was worshipped as a god, one whose existence was undeniable, and whose gifts were powerful, and the tainted generation took it upon themselves to crusade through all of Aphelia and spread it.

        At first, the crusades were met with only skepticism and rejection from those not used to the Tar's presence. Some doubted the Tar's existence; those who didn't, doubted its power; and those who did neither, were rightfully wary of its influence. With an inability to demonstrate the Tar's existence or potency without travelling all the way back to their hometowns, the crusaders only recruited a scarce few. With their new handful of allies, though, they discovered a way to transport the Tar, and thus, its first vessel was created.

        Though simple in design, this original clay jar was the cornerstone to the Tar epidemic. The original jar was worshipped as an idol. The dunepeople fed what they had to it, allowing the Tar to spread forth. Brought from town to town, village to village, city to city, the vessel crushed all doubts of the Tar's power or existence, quickly integrating itself into all of the dunepeople's struggling civilization. Those who continued to warn against the Tar's influence were bribed with its power, and those who couldn't be bribed were fed to Aphelia's new god."

        - Tragedy of Aphelia
        """;
        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Main.hifuSandswept.LoadAsset<GameObject>("CeremonialJarHolder.prefab");

        public override Sprite ItemIcon => Main.hifuSandswept.LoadAsset<Sprite>("texCeremonialJar.png");

        public override string ItemName => "Ceremonial Jar";

        public override ItemTag[] ItemTags => [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.CanBeTemporary];

        public static BuffDef linkedBuff;
        public static BuffDef cooldownBuff;
        public static DamageColorIndex JarDamageColor = DamageColourHelper.RegisterDamageColor(new Color32(74, 63, 58, 255));

        [ConfigField("Linked Enemies Requirement", "", 3)]
        public static int linkedEnemiesRequirement;

        [ConfigField("Base Damage", "Decimal.", 20f)]
        public static float baseDamage;

        [ConfigField("Stack Damage", "Decimal.", 20f)]
        public static float stackDamage;

        [ConfigField("Linked Enemy Cooldown", "", 5f)]
        public static float linkedEnemyCooldown;

        [ConfigField("Proc Coefficient", "", 1f)]
        public static float procCoefficient;

        public static Material matCeremonialJarTar;
        public static GameObject vfx;

        public override float modelPanelParametersMinDistance => 5f;
        public override float modelPanelParametersMaxDistance => 12f;

        public override void Init()
        {
            base.Init();
            SetUpVFX();
            SetUpBuffs();
        }

        public void SetUpVFX()
        {
            vfx = PrefabAPI.InstantiateClone(Paths.GameObject.Bandit2SmokeBomb, "Ceremonial Jar VFX", false);

            vfx.GetComponent<EffectComponent>().applyScale = true;
            VFXUtils.OdpizdzijPierdoloneGownoKurwaCoZaJebanyKurwaSmiecToKurwaDodalPizdaKurwaJebanaKurwa(vfx);

            var tarColor = new Color32(64, 45, 35, 255);
            VFXUtils.RecolorMaterialsAndLights(vfx, tarColor, tarColor, true, true);

            var transform = vfx.transform.Find("Core");
            transform.localScale = Vector3.one / 12f;// base radius at 1 scale is 12m according to bandit's util value
            transform.localPosition = Vector3.zero;

            var sparks = transform.Find("Sparks");
            var sparksPS = sparks.GetComponent<ParticleSystem>();
            var sparksMain = sparksPS.main;
            sparksMain.maxParticles = 400;
            var sparksEmission = sparksPS.emission;
            var burst = new ParticleSystem.Burst(0f, 400, 400, 1, 0.01f);
            burst.probability = 1f;
            sparksEmission.SetBurst(0, burst);

            var sparksPSR = sparks.GetComponent<ParticleSystemRenderer>();
            sparksPSR.material.SetTexture("_MainTex", Paths.Texture2D.texGlowSkullMask);

            var pointLight = transform.Find("Point Light").GetComponent<Light>();
            var lightColor = new Color(-0.09f, -0.075f, -0.066f, 1);
            pointLight.color = lightColor;
            // pointLight.range = 20f;

            ContentAddition.AddEffect(vfx);

            VFXUtils.MultiplyDuration(vfx, 2f);

            matCeremonialJarTar = new Material(Paths.Material.matHuntressFlashBright);

            matCeremonialJarTar.SetColor("_TintColor", new Color32(52, 27, 9, 183));
            matCeremonialJarTar.SetTexture("_MainTex", null);
            matCeremonialJarTar.SetTexture("_RemapTex", Paths.Texture2D.texRampClaySwordSwing);
            matCeremonialJarTar.SetFloat("_InvFade", 0f);
            matCeremonialJarTar.SetFloat("_Boost", 1f);
            matCeremonialJarTar.SetFloat("_AlphaBoost", 0f);
            matCeremonialJarTar.SetFloat("_AlphaBias", 1f);
            matCeremonialJarTar.SetInt("_Cull", 1);
            matCeremonialJarTar.SetInt("_DstBlend", 5);
            matCeremonialJarTar.SetInt("_VertexOffsetOn", 1);
            matCeremonialJarTar.SetFloat("_OffsetAmount", 0.3f);
        }

        public void SetUpBuffs()
        {
            linkedBuff = ScriptableObject.CreateInstance<BuffDef>();
            linkedBuff.name = "Ceremonial Jar Link";
            linkedBuff.canStack = false;
            linkedBuff.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texGaySex.png");

            cooldownBuff = ScriptableObject.CreateInstance<BuffDef>();
            cooldownBuff.name = "Ceremonial Jar Cooldown";
            cooldownBuff.canStack = false;
            cooldownBuff.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texLesbianFurry.png");

            ContentAddition.AddBuffDef(linkedBuff);
            ContentAddition.AddBuffDef(cooldownBuff);
        }

        public override void Hooks()
        {
            base.Hooks();
            On.RoR2.GlobalEventManager.OnHitEnemy += OnHitEnemy;
        }

        public override object GetItemStatsDef()
        {
            ItemStatsDef itemStatsDef = new();
            itemStatsDef.descriptions.Add("Link Damage: ");
            itemStatsDef.valueTypes.Add(ItemStatsDef.ValueType.Damage);
            itemStatsDef.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
            itemStatsDef.calculateValuesNew = (luck, stack, procChance) =>
            {
                List<float> values = new()
                {
                    baseDamage + stackDamage * (stack - 1)
                };

                return values;
            };

            return itemStatsDef;
        }

        public void OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo info, GameObject victim)
        {
            orig(self, info, victim);

            if (!victim)
            {
                return;
            }

            var attacker = info.attacker;

            if (!info.damageType.IsDamageSourceSkillBased)
            {
                return;
            }

            if (!attacker)
            {
                return;
            }

            if (!attacker.TryGetComponent<CharacterBody>(out var attackerBody))
            {
                return;
            }

            if (GetCount(attackerBody) <= 0)
            {
                return;
            }

            if (!victim.TryGetComponent<CharacterBody>(out var victimBody))
            {
                return;
            }

            if (victimBody.HasBuff(cooldownBuff) || attackerBody == victimBody)
            {
                // Debug.Log("returning because cd");
                return;
            }

            victimBody.SetBuffCount(linkedBuff.buffIndex, 1);
            victimBody.SetBuffCount(RoR2Content.Buffs.ClayGoo.buffIndex, 1);
            // Debug.Log("adding buff");

            List<CharacterBody> linkedVictimBodies = new();

            for (int i = 0; i < CharacterBody.readOnlyInstancesList.Count; i++)
            {
                var body = CharacterBody.readOnlyInstancesList[i];
                if (body.HasBuff(linkedBuff))
                {
                    linkedVictimBodies.Add(CharacterBody.readOnlyInstancesList[i]);
                }
            }

            if (linkedVictimBodies.Count >= linkedEnemiesRequirement)
            {
                linkedVictimBodies.ForEach(linkedVictimBody =>
                {
                    linkedVictimBody.SetBuffCount(linkedBuff.buffIndex, 0);
                    linkedVictimBody.SetBuffCount(RoR2Content.Buffs.ClayGoo.buffIndex, 0);
                    linkedVictimBody.AddTimedBuff(cooldownBuff, linkedEnemyCooldown);

                    DamageInfo info = new()
                    {
                        damage = attackerBody.damage * (baseDamage + (stackDamage * (GetCount(attackerBody) - 1))),
                        crit = attackerBody.RollCrit(),
                        damageColorIndex = JarDamageColor,
                        attacker = attackerBody.gameObject,
                        position = linkedVictimBody.corePosition,
                        procCoefficient = procCoefficient
                    };

                    linkedVictimBody.healthComponent.TakeDamage(info);

                    EffectManager.SpawnEffect(vfx, new EffectData()
                    {
                        scale = 8f + linkedVictimBody.bestFitActualRadius * 4f,
                        origin = linkedVictimBody.corePosition
                    }, true);

                    Util.PlaySound("Play_bison_headbutt_attack_hit", linkedVictimBody.gameObject);

                    for (int i = 0; i < 3; i++)
                    {
                        Util.PlaySound("Play_clayGrenadier_impact", linkedVictimBody.gameObject);
                    }

                    Util.PlaySound("Play_bison_headbutt_attack_hit", attackerBody.gameObject);
                    Util.PlaySound("Play_clayGrenadier_impact", attackerBody.gameObject);
                    Util.PlaySound("Play_arenaCrab_swim_stroke", attackerBody.gameObject);
                });
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();

            var itemDisplay = SetUpFollowerIDRS(0.45f, 20f, true, 3f, false, 0f, true, 22f);

            return new ItemDisplayRuleDict(new ItemDisplayRule()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(-1f, 0.75f, 1.5f),
                localScale = new Vector3(0.2f, 0.2f, 0.2f),

                followerPrefab = itemDisplay,
                limbMask = LimbFlags.None,
                followerPrefabAddress = new AssetReferenceGameObject("")
            });
        }
    }
}