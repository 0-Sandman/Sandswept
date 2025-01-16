/*
using BepInEx.Configuration;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using RoR2;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.AddressableAssets;
using R2API;
using System.Collections.Generic;
using Sandswept.Artifacts;
using Sandswept;

namespace Sandswept.Artifacts
{
    [ConfigSection("Artifacts :: Blindness")]
    internal class ArtifactOfBlindness : ArtifactBase<ArtifactOfBlindness>
    {
        public override string ArtifactName => "Artifact of Blindness";

        public override string ArtifactLangTokenName => "BLINDNESS";

        public override string ArtifactDescription => "Severely reduce ally vision. Monsters are regenerative and enraged outside of any ally's vision.";

        public override Sprite ArtifactEnabledIcon => Main.hifuSandswept.LoadAsset<Sprite>("texArtifactOfBlindnessEnabled.png");

        public override Sprite ArtifactDisabledIcon => Main.hifuSandswept.LoadAsset<Sprite>("texArtifactOfBlindnessDisabled.png");

        public static RampFog rampFog;
        public static ChromaticAberration chromaticAberration;

        // public static LensDistortion ld;
        public static Vignette vignette;

        public static GameObject ppHolder;
        public static BuffDef regenBuff;
        public static BuffDef speedBuff;
        public static BuffDef aspdBuff;
        private static readonly string[] blacklistedScenes = { "artifactworld", "crystalworld", "eclipseworld", "infinitetowerworld", "intro", "loadingbasic", "lobby", "logbook", "mysteryspace", "outro", "PromoRailGunner", "PromoVoidSurvivor", "splash", "title", "voidoutro" };

        public static GameObject indicator;

        [ConfigField("Fog Radius", "", 30f)]
        public static float fogRadius;

        [ConfigField("Movement Speed Increase", "Decimal.", 1.5f)]
        public static float movementSpeedIncrease;

        [ConfigField("Attack Speed Increase", "Decimal.", 0.35f)]
        public static float attackSpeedIncrease;

        [ConfigField("Cooldown Reduction", "Decimal.", 0.35f)]
        public static float cooldownReduction;

        [ConfigField("Flat Regeneration Increase", "", 5f)]
        public static float flatRegenerationIncrease;

        [ConfigField("Percent Regeneration Increase", "", 0.01f)]
        public static float percentRegenerationIncrease;

        [ConfigField("Final Boss Stat Multiplier", "", 0.5f)]
        public static float finalBossStatMultiplier;

        public static List<BodyIndex> finalBossBodyIndices = new();

        public override void Init(ConfigFile config)
        {
            indicator = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/NearbyDamageBonus/NearbyDamageBonusIndicator.prefab").WaitForCompletion(), "Fog Visual", true);
            var radiusTrans = indicator.transform.Find("Radius, Spherical");
            radiusTrans.localScale = new Vector3(fogRadius * 2f, fogRadius * 2f, fogRadius * 2f);

            var indicatorMat = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/NearbyDamageBonus/matNearbyDamageBonusRangeIndicator.mat").WaitForCompletion());
            var cloudTexture = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/PerlinNoise.png").WaitForCompletion();
            indicatorMat.SetTexture("_MainTex", cloudTexture);
            indicatorMat.SetTexture("_Cloud1Tex", cloudTexture);
            indicatorMat.SetColor("_TintColor", new Color32(142, 113, 84, 60));

            radiusTrans.GetComponent<MeshRenderer>().material = indicatorMat;

            PrefabAPI.RegisterNetworkPrefab(indicator);

            ppHolder = new("SandsweptArtifactOfBlindnessPP");
            Object.DontDestroyOnLoad(ppHolder);
            ppHolder.layer = LayerIndex.postProcess.intVal;
            ppHolder.AddComponent<ArtifactOfBlindnessPostProcessingController>();
            PostProcessVolume pp = ppHolder.AddComponent<PostProcessVolume>();
            Object.DontDestroyOnLoad(pp);
            pp.isGlobal = true;
            pp.weight = 0f;
            pp.priority = float.MaxValue - 0.5f;
            PostProcessProfile ppProfile = ScriptableObject.CreateInstance<PostProcessProfile>();
            Object.DontDestroyOnLoad(ppProfile);
            ppProfile.name = "SandsweptArtifactOfBlindness";

            rampFog = ppProfile.AddSettings<RampFog>();
            rampFog.enabled.value = true;
            rampFog.SetAllOverridesTo(true);
            rampFog.fogColorStart.value = new Color32(105, 83, 43, 0);
            rampFog.fogColorMid.value = new Color32(70, 61, 51, 213);
            rampFog.fogColorEnd.value = new Color32(62, 50, 39, 255);
            rampFog.skyboxStrength.value = 0.08f;
            rampFog.fogPower.value = 0.35f;
            rampFog.fogIntensity.value = 1f;
            rampFog.fogZero.value = 0f;
            rampFog.fogOne.value = 0.01f;

            chromaticAberration = ppProfile.AddSettings<ChromaticAberration>();
            chromaticAberration.enabled.value = true;
            chromaticAberration.SetAllOverridesTo(true);
            chromaticAberration.intensity.value = 0.15f;
            chromaticAberration.fastMode.value = false;

            vignette = ppProfile.AddSettings<Vignette>();
            vignette.enabled.value = true;
            vignette.SetAllOverridesTo(true);
            vignette.intensity.value = 0.15f;
            vignette.roundness.value = 1f;
            vignette.smoothness.value = 0.2f;
            vignette.rounded.value = false;
            vignette.color.value = new Color32(255, 255, 255, 255);

            On.RoR2.BodyCatalog.Init += BodyCatalog_Init;

            pp.sharedProfile = ppProfile;
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        private System.Collections.IEnumerator BodyCatalog_Init(On.RoR2.BodyCatalog.orig_Init orig)
        {
            yield return orig();

            finalBossBodyIndices.Add(BodyCatalog.FindBodyIndex("BrotherBody(Clone)"));
            finalBossBodyIndices.Add(BodyCatalog.FindBodyIndex("BrotherHurtBody(Clone)"));
            finalBossBodyIndices.Add(BodyCatalog.FindBodyIndex("ScavLunar1Body(Clone)"));
            finalBossBodyIndices.Add(BodyCatalog.FindBodyIndex("ScavLunar2Body(Clone)"));
            finalBossBodyIndices.Add(BodyCatalog.FindBodyIndex("ScavLunar3Body(Clone)"));
            finalBossBodyIndices.Add(BodyCatalog.FindBodyIndex("ScavLunar4Body(Clone)"));
            finalBossBodyIndices.Add(BodyCatalog.FindBodyIndex("VoidRaidCrabBody(Clone)"));
            finalBossBodyIndices.Add(BodyCatalog.FindBodyIndex("MiniVoidRaidCrabBodyBase(Clone)"));
            finalBossBodyIndices.Add(BodyCatalog.FindBodyIndex("MiniVoidRaidCrabBodyPhase1(Clone)"));
            finalBossBodyIndices.Add(BodyCatalog.FindBodyIndex("MiniVoidRaidCrabBodyPhase2(Clone)"));
            finalBossBodyIndices.Add(BodyCatalog.FindBodyIndex("MiniVoidRaidCrabBodyPhase3(Clone)"));
            finalBossBodyIndices.Add(BodyCatalog.FindBodyIndex("FalseSonBossBody(Clone)"));
        }

        public override void Hooks()
        {
            CreateBuff();
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
            if (instance.ArtifactEnabled)
            {
                if (body.teamComponent.teamIndex == TeamIndex.Player)
                {
                    var fogSphere = body.GetComponent<ArtifactOfBlindnessFogSphereController>();
                    if (fogSphere == null)
                    {
                        var fogSphereInstance = body.gameObject.AddComponent<ArtifactOfBlindnessFogSphereController>();
                    }
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.HasBuff(regenBuff) || sender.HasBuff(speedBuff) || sender.HasBuff(aspdBuff))
            {
                if (finalBossBodyIndices.Contains(sender.bodyIndex))
                {
                    args.baseAttackSpeedAdd += attackSpeedIncrease * finalBossStatMultiplier;
                    args.cooldownMultAdd += cooldownReduction * finalBossStatMultiplier;
                    args.moveSpeedMultAdd += movementSpeedIncrease * finalBossStatMultiplier;
                    args.baseRegenAdd += (sender.healthComponent.combinedHealth * percentRegenerationIncrease * finalBossStatMultiplier) + flatRegenerationIncrease * finalBossStatMultiplier + (flatRegenerationIncrease * finalBossStatMultiplier) / 5f * (sender.level - 1);
                }
                else
                {
                    args.baseAttackSpeedAdd += attackSpeedIncrease;
                    args.cooldownMultAdd += cooldownReduction;
                    args.moveSpeedMultAdd += movementSpeedIncrease;
                    args.baseRegenAdd += (sender.healthComponent.combinedHealth * percentRegenerationIncrease) + flatRegenerationIncrease + flatRegenerationIncrease / 5f * (sender.level - 1);
                }
            }
        }

        private void CreateBuff()
        {
            regenBuff = ScriptableObject.CreateInstance<BuffDef>();
            speedBuff = ScriptableObject.CreateInstance<BuffDef>();
            aspdBuff = ScriptableObject.CreateInstance<BuffDef>();

            regenBuff.canStack = false;
            regenBuff.isDebuff = false;
            regenBuff.name = "Furry Yaoi";
            regenBuff.iconSprite = Paths.BuffDef.bdCrocoRegen.iconSprite;
            regenBuff.buffColor = new Color32(142, 113, 84, 255);

            speedBuff.canStack = false;
            speedBuff.isDebuff = false;
            speedBuff.name = "Furry Yuri";
            speedBuff.iconSprite = Paths.BuffDef.bdCloakSpeed.iconSprite;
            speedBuff.buffColor = new Color32(142, 113, 84, 255);

            var aspdIconTexture = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/AttackSpeedOnCrit/texBuffAttackSpeedOnCritIcon.tif").WaitForCompletion();
            aspdBuff.canStack = false;
            aspdBuff.isDebuff = false;
            aspdBuff.name = "Hello Chat";
            aspdBuff.iconSprite = Paths.BuffDef.bdAttackSpeedOnCrit.iconSprite;
            aspdBuff.buffColor = new Color32(142, 113, 84, 255);

            ContentAddition.AddBuffDef(regenBuff);
            ContentAddition.AddBuffDef(speedBuff);
            ContentAddition.AddBuffDef(aspdBuff);
        }

        private void Run_onRunDestroyGlobal(Run obj)
        {
            var ppVolume = ppHolder.GetComponent<PostProcessVolume>();
            var sceneName = SceneManager.GetActiveScene().name;
            if (!blacklistedScenes.Contains(sceneName))
            {
                if (ppVolume)
                {
                    ppVolume.weight = 0f;
                }
            }
        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            var ppVolume = ppHolder.GetComponent<PostProcessVolume>();
            ppVolume.weight = 0f;

            var sceneName = SceneManager.GetActiveScene().name;
            if (instance.ArtifactEnabled && !blacklistedScenes.Contains(sceneName))
            {
                ppVolume.weight = 1f;
            }

            orig(self);
        }
    }

    public class ArtifactOfBlindnessPostProcessingController : MonoBehaviour
    {
        public PostProcessVolume volume;

        public void Start()
        {
            volume = GetComponent<PostProcessVolume>();
        }
    }

    public class ArtifactOfBlindnessFogSphereController : MonoBehaviour
    {
        public CharacterBody bodyComponent;
        public float checkInterval = 0.1f;
        public float timer;
        public float radius = ArtifactOfBlindness.fogRadius;
        public static List<ArtifactOfBlindnessFogSphereController> fogList = new();
        public bool anyEnemiesOutside = true;
        public Vector3 myPosition;
        public Vector3 enemyPosition;
        public GameObject radiusIndicator;
        public Light light = null;
        public Material indicatorMat;
        public Transform radiusTrans;

        public void Awake()
        {
            fogList.Add(this);
        }

        public void OnDestroy()
        {
            fogList.Remove(this);
        }

        public void Start()
        {
            bodyComponent = gameObject.GetComponent<CharacterBody>();

            enableRadiusIndicator = true;
            radiusTrans = radiusIndicator.transform.GetChild(1);
            radiusTrans.localScale = new Vector3(ArtifactOfBlindness.fogRadius * 2f, ArtifactOfBlindness.fogRadius * 2f, ArtifactOfBlindness.fogRadius * 2f);
            indicatorMat = radiusTrans.GetComponent<MeshRenderer>().sharedMaterial;

            if (light == null)
            {
                light = gameObject.AddComponent<Light>();
                light.color = Color.white;
                light.range = 13f;
                light.intensity = 0.2f;
                light.type = LightType.Point;
                light.shadows = LightShadows.None;
            }
        }

        public void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (timer >= checkInterval)
            {
                timer = 0f;

                for (int i = 0; i < CharacterBody.instancesList.Count; i++)
                {
                    var cachedBody = CharacterBody.instancesList[i];
                    if (cachedBody && cachedBody.teamComponent.teamIndex != TeamIndex.Player)
                    {
                        enemyPosition = cachedBody.transform.position;
                        foreach (ArtifactOfBlindnessFogSphereController controller in fogList)
                        {
                            // todo: remove useless fucks like birdsharks, pots and the like from getting checked (idk how)
                            myPosition = controller.bodyComponent.transform.position;
                            if (Vector3.Distance(enemyPosition, myPosition) < radius)
                            {
                                anyEnemiesOutside = false;
                                break;
                            }
                            else
                            {
                                anyEnemiesOutside = true;
                            }

                            // the idea is to group all spheres and run them on the server
                        }

                        if (anyEnemiesOutside)
                        {
                            AddBuffs(cachedBody);
                        }
                        else
                        {
                            RemoveBuffs(cachedBody);
                        }
                    }
                }
            }
        }

        private void AddBuffs(CharacterBody body)
        {
            bool hasAnyBuff = body.HasBuff(ArtifactOfBlindness.regenBuff) || body.HasBuff(ArtifactOfBlindness.speedBuff) || body.HasBuff(ArtifactOfBlindness.aspdBuff);
            if (!hasAnyBuff)
            {
                body.AddBuff(ArtifactOfBlindness.regenBuff);
                body.AddBuff(ArtifactOfBlindness.speedBuff);
                body.AddBuff(ArtifactOfBlindness.aspdBuff);
            }
        }

        private void RemoveBuffs(CharacterBody body)
        {
            bool hasAnyBuff = body.HasBuff(ArtifactOfBlindness.regenBuff) || body.HasBuff(ArtifactOfBlindness.speedBuff) || body.HasBuff(ArtifactOfBlindness.aspdBuff);
            if (hasAnyBuff)
            {
                body.RemoveBuff(ArtifactOfBlindness.regenBuff);
                body.RemoveBuff(ArtifactOfBlindness.speedBuff);
                body.RemoveBuff(ArtifactOfBlindness.aspdBuff);
            }
        }

        private bool enableRadiusIndicator
        {
            get
            {
                return radiusIndicator;
            }
            set
            {
                if (enableRadiusIndicator != value)
                {
                    if (value)
                    {
                        radiusIndicator = Instantiate(ArtifactOfBlindness.indicator, bodyComponent.corePosition, Quaternion.identity);
                        radiusIndicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject, null);
                    }
                    else
                    {
                        Object.Destroy(radiusIndicator);
                        radiusIndicator = null;
                    }
                }
            }
        }
    }
}
*/