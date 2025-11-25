using IL.RoR2.ContentManagement;
using Sandswept.Utils.Components;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;

namespace Sandswept.Survivors.Ranger.VFX
{
    public static class ReleaseVFX
    {
        public static GameObject tracerPrefabDefault;
        public static GameObject impactPrefabDefault;

        public static GameObject tracerPrefabMajor;
        public static GameObject impactPrefabMajor;

        public static GameObject tracerPrefabRenegade;
        public static GameObject impactPrefabRenegade;

        public static GameObject tracerPrefabMileZero;
        public static GameObject impactPrefabMileZero;

        public static GameObject tracerPrefabSandswept;
        public static GameObject impactPrefabSandswept;

        public static void Init()
        {
            tracerPrefabDefault = CreateTracerRecolor("Default", new Color32(0, 255, 183, 255), new Color32(51, 147, 234, 255));
            impactPrefabDefault = CreateImpactRecolor("Default", new Color32(23, 234, 129, 255), new Color32(23, 211, 148, 255));

            tracerPrefabMajor = CreateTracerRecolor("Major", new Color32(109, 95, 209, 255), new Color32(51, 63, 234, 255));
            impactPrefabMajor = CreateImpactRecolor("Major", new Color32(23, 43, 234, 255), new Color32(35, 23, 211, 255));

            tracerPrefabRenegade = CreateTracerRecolor("Renegade", new Color32(219, 103, 159, 255), new Color32(246, 59, 183, 255));
            impactPrefabRenegade = CreateImpactRecolor("Renegade", new Color32(247, 32, 182, 255), new Color32(225, 34, 136, 255));

            tracerPrefabMileZero = CreateTracerRecolor("Mile Zero", new Color32(255, 0, 0, 255), new Color32(255, 0, 0, 255));
            impactPrefabMileZero = CreateImpactRecolor("Mile Zero", new Color32(255, 0, 0, 255), new Color32(255, 0, 0, 255));

            tracerPrefabSandswept = CreateTracerRecolor("Sandswept", new Color32(214, 159, 79, 255), new Color32(150, 150, 150, 255));
            impactPrefabSandswept = CreateImpactRecolor("Sandswept", new Color32(214, 159, 79, 255), new Color32(150, 150, 150, 255));
        }

        public static GameObject CreateTracerRecolor(string name, Color32 primaryColor, Color32 emissionAndLightColor)
        {
            var tracerGameObject = Paths.GameObject.TracerRailgunSuper.InstantiateClone("Release Tracer " + name, false);
            tracerGameObject.AddComponent<ReleaseVFXIntensityController>();

            var childLocator = tracerGameObject.AddComponent<ChildLocator>();
            foreach (Transform child in tracerGameObject.transform.GetComponentsInChildren<Transform>())
            {
                childLocator.AddChild(child.name, child);
            }

            tracerGameObject.GetOrAddComponent<VFXAttributes>((x) =>
            {
                x.DoNotCullPool = true;
                x.DoNotPool = true;
                x.vfxPriority = VFXAttributes.VFXPriority.Always;
            });

            var kurwaJebanyTracerComponentKurwaKtoToPisal = tracerGameObject.AddComponent<TracerComponentSucks>();

            var tracer = tracerGameObject.GetComponent<Tracer>();
            tracer.onTailReachedDestination = new UnityEvent();
            tracer.length = 10000f;
            tracer.speed = 500f;

            var tailObject = tracer.tailTransform.gameObject;

            // var slowLightHead = VFXUtils.AddLight(tracerGameObject, primaryColor, 5f, 70f, 1f);
            // var slowLightTail = VFXUtils.AddLight(tailObject, primaryColor, 5f, 70f, 1f);
            // Four Lights

            // var szmatoJebanaKurwa = tracer.GetComponent<EffectManagerHelper>();
            // szmatoJebanaKurwa.enabled = false;
            // kurwa ale jebana szmata pierdolona kurwa dodaje sie pizda jebana automatycznie gdzies kurwa w chuju zajebana spierdolona dziwka kurwa jebane kurwa spaghetti code ja pierdole

            var pizdoKurwaPierdolona = tracer.AddComponent<VFXAttributes>();
            pizdoKurwaPierdolona.DoNotPool = true;
            pizdoKurwaPierdolona.DoNotCullPool = true;

            // kurwa mac jebane kurwa spaghetti code kurwa jak mozna kurwa zrobic ze jebany tracer usuwa sie kurwa
            // i potem kurwa dawac jakies zjebane workaroundy zamiast naprawic problem kurwa?? po co stwarzac sobie problem ja pierdole

            var transform = tracerGameObject.transform;

            transform.Find("TracerHead/HarshGlow, Billboard").GetComponent<ParticleSystemRenderer>().enabled = false;

            var postProcessing = transform.Find("StartTransform/PP");
            postProcessing.gameObject.SetActive(false);
            postProcessing.GetComponent<PostProcessDuration>().enabled = false;
            postProcessing.GetComponent<PostProcessVolume>().enabled = false;
            postProcessing.GetComponent<SphereCollider>().enabled = false;

            var fx = transform.Find("FX");
            //fx.GetComponent<Animator>().speed = 0.75f;
            fx.GetComponent<Animator>().enabled = false;
            // jednak trzeba kurwa wylaczyc smiecia

            var brief = fx.Find("Brief");

            var beamFlashBriefly = brief.Find("Beam, Flash Briefly");
            var beamFlashAnimateShaderAlpha = beamFlashBriefly.AddComponent<AnimateShaderAlpha>();
            beamFlashAnimateShaderAlpha.alphaCurve = new AnimationCurve(new Keyframe(1f, 1f), new Keyframe(1f, 0f));
            beamFlashAnimateShaderAlpha.timeMax = 0.1f;

            var beamDistortion = fx.Find("Longer/Beam, Distortion");
            var beamDistortionDestroyOnTimer = beamDistortion.AddComponent<DestroyOnTimer>();
            beamDistortionDestroyOnTimer.duration = 1f;

            VFXUtils.RecolorMaterialsAndLights(tracerGameObject, primaryColor, emissionAndLightColor, true);
            VFXUtils.MultiplyScale(tracerGameObject, 2f);
            VFXUtils.MultiplyDuration(tracerGameObject, 2f, 1.5f);

            // slowLightHead.color = primaryColor;
            // slowLightTail.color = primaryColor;

            var beamLingerMaterial = fx.Find("Longer/Beam, Linger").GetComponent<LineRenderer>().material;
            beamLingerMaterial.SetFloat("_Boost", 4f);

            var beamGlow = fx.Find("Longer/Beam, Glow");

            var beamGlowMaterial = beamGlow.GetComponent<LineRenderer>().material;
            beamGlowMaterial.SetTexture("_MainTex", Paths.Texture2D.texAlphaGradient2Mask);
            beamGlowMaterial.SetInt("_CloudOffsetOn", 0);
            beamGlowMaterial.DisableKeyword("CLOUDOFFSET");
            beamGlowMaterial.SetFloat("_AlphaBias", 0.26f);

            var tracerHead = transform.Find("TracerHead").gameObject;

            VFXUtils.AddLight(tracerHead, primaryColor, 5f, 60f, 2f);
            VFXUtils.AddLight(tracerGameObject, primaryColor, 8f, 40f, 0.3f);
            VFXUtils.AddLight(tailObject, primaryColor, 8f, 40f, 0.3f);

            var mdlRailgunnerBeam = fx.Find("mdlRailgunnerBeam");
            mdlRailgunnerBeam.transform.localScale = Vector3.one * 0.05f;

            ContentAddition.AddEffect(tracerGameObject);

            return tracerGameObject;
        }

        public static GameObject CreateImpactRecolor(string name, Color32 primaryColor, Color32 emissionAndLightColor)
        {
            var impact = Paths.GameObject.ImpactRailgun.InstantiateClone("Release Impact " + name, false);

            var effectComponent = impact.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_lunar_wisp_attack2_explode";

            var pizdoKurwaPierdolona = impact.GetComponent<VFXAttributes>();
            pizdoKurwaPierdolona.DoNotPool = true;

            VFXUtils.RecolorMaterialsAndLights(impact, primaryColor, emissionAndLightColor, true);
            VFXUtils.MultiplyScale(impact, 1.25f);
            VFXUtils.MultiplyDuration(impact, 2.5f);

            ContentAddition.AddEffect(impact);

            return impact;
        }

        public static GameObject CreateImpactRecolor(string name, Color32 saturatedAquaEquivalent, Color32 lessSaturatedAquaEquivalent, Color32 saturatedBlueEquivalent)
        {
            // saturatedAquaEquivalent = new Color32(23,234,129,255);
            // lessSaturatedAquaEquivalent = new Color32(23,211,148,255);
            // saturatedBlueEquivalent = new Color32(0,255,210,255);

            var impact = Paths.GameObject.OmniExplosionCrowstorm.InstantiateClone("Release Impact " + name, false);

            var effectComponent = impact.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_lunar_wisp_attack2_explode";

            var trans = impact.transform;

            for (int j = 0; j < trans.childCount; j++)
            {
                var child = trans.GetChild(j);
                child.localScale = Vector3.one * 2f;
            }

            // scaled hitsparks 1

            var geen1 = saturatedAquaEquivalent;
            var geen2 = lessSaturatedAquaEquivalent;

            var alphas = new GradientAlphaKey[2];
            alphas[0] = new GradientAlphaKey(1f, 0f);
            alphas[1] = new GradientAlphaKey(1f, 1f);

            var minGradient = new Gradient();
            var minColors = new GradientColorKey[2];
            minColors[0] = new GradientColorKey(geen1, 0f);
            minColors[1] = new GradientColorKey(geen2, 0f);

            var maxGradient = new Gradient();
            var maxColors = new GradientColorKey[2];
            maxColors[0] = new GradientColorKey(geen2, 0f);
            maxColors[1] = new GradientColorKey(geen1, 0f);

            minGradient.SetKeys(minColors, alphas);
            maxGradient.SetKeys(maxColors, alphas);

            var scaledHitsparks1 = trans.GetChild(0);

            var scaledHitsparks1PSR = scaledHitsparks1.GetComponent<ParticleSystemRenderer>();

            var newScaledHitsparks1Mat = Object.Instantiate(Paths.Material.matOmniHitspark1Huntress);
            newScaledHitsparks1Mat.SetColor("_TintColor", saturatedBlueEquivalent);
            newScaledHitsparks1Mat.SetFloat("_Boost", 2.216648f);
            newScaledHitsparks1Mat.SetFloat("_AlphaBoost", 4.214276f);
            newScaledHitsparks1Mat.SetFloat("_AlphaBias", 0.2612987f);

            scaledHitsparks1PSR.material = newScaledHitsparks1Mat;

            var scaledHitsparks1PS = scaledHitsparks1.GetComponent<ParticleSystem>();
            var scaledHitsparks1Main = scaledHitsparks1PS.main;
            scaledHitsparks1Main.startColor = new ParticleSystem.MinMaxGradient(minGradient, maxGradient);
            var scaledHitsparks1StartLifetime = scaledHitsparks1Main.startLifetime;
            scaledHitsparks1StartLifetime.constantMin = 0.4f;
            scaledHitsparks1StartLifetime.constantMax = 0.5f;

            var emission = scaledHitsparks1PS.emission;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 7, 9));

            var unscaledHitsparks1 = trans.GetChild(1);
            var unscaledHitsparks1PSR = unscaledHitsparks1.GetComponent<ParticleSystemRenderer>();
            unscaledHitsparks1PSR.material = Paths.Material.matOmniHitspark1GreaterWisp;

            //

            // scaled smoke billboard

            var scaledSmokeBillboard = trans.GetChild(2).GetComponent<ParticleSystem>().main.startColor;
            scaledSmokeBillboard.color = geen1;

            //

            // scaled smoke ring mesh

            var scaledSmokeRingMesh = trans.GetChild(3);
            var startColor2 = scaledSmokeRingMesh.GetComponent<ParticleSystem>().main.startColor;
            startColor2.color = geen1;

            var particleSystemRenderer2 = scaledSmokeRingMesh.GetComponent<ParticleSystemRenderer>();

            var newMat4 = Object.Instantiate(Paths.Material.matCrowstormFeatherRepeated);

            newMat4.SetColor("_TintColor", geen2);
            newMat4.SetTexture("_MainTex", Paths.Texture2D.texShockwaveRing3Mask);
            // newMat4.SetTexture("_RemapTex", ramp);

            particleSystemRenderer2.material = newMat4;

            //

            // unscaled smoke billboard

            var unscaledSmokeBillboard = trans.GetChild(4).GetComponent<ParticleSystem>().main.startColor;
            unscaledSmokeBillboard.color = geen2;

            //

            // area indicator ring billboard

            var areaIndicatorRingBillboard = trans.GetChild(5).GetComponent<ParticleSystemRenderer>();

            var newMat5 = Object.Instantiate(Paths.Material.matOmniRing2Loader);

            // newMat5.SetTexture("_RemapTex", ramp);

            areaIndicatorRingBillboard.material = newMat5;

            //

            // area indicator ring random billboard

            var areaIndicatorRingRandomBillboard = trans.GetChild(6).GetComponent<ParticleSystemRenderer>();

            areaIndicatorRingRandomBillboard.material = newMat5;

            //

            // physics sparks

            var physicsSparks = trans.GetChild(7).GetComponent<ParticleSystem>().main;
            physicsSparks.startColor = new ParticleSystem.MinMaxGradient(minGradient, maxGradient);

            //

            // dash bright

            var dashBright = trans.GetChild(10);
            var startColor3 = dashBright.GetComponent<ParticleSystem>().main.startColor;
            startColor3.color = geen1;

            var particleSystemRenderer3 = dashBright.GetComponent<ParticleSystemRenderer>();

            var newMat6 = Object.Instantiate(Paths.Material.matCrowstormFeather);
            newMat6.SetColor("_TintColor", geen2);
            newMat6.SetTexture("_MainTex", Paths.Texture2D.texShockwaveRing3Mask);
            // newMat6.SetTexture("_RemapTex", ramp);

            particleSystemRenderer3.material = newMat6;

            //

            // chunks billboards

            var chunksBillboards = trans.GetChild(13).GetComponent<ParticleSystemRenderer>();
            chunksBillboards.material = newMat6;

            ContentAddition.AddEffect(impact);

            return impact;
        }
    }

    public class ReleaseVFXIntensityController : MonoBehaviour
    {
        public EffectComponent effectComponent;
        public EffectData effectData;
        public ChildLocator childLocator;
        public Tracer tracer;
        public Light light;

        public void Start()
        {
            childLocator = GetComponent<ChildLocator>();
            effectComponent = GetComponent<EffectComponent>();
            tracer = GetComponent<Tracer>();
            light = GetComponent<Light>();

            effectData = effectComponent.effectData;

            tracer.beamDensity = 0.1f + (effectData.genericUInt / 200f);
            tracer.speed = 300 + (10 * effectData.genericUInt);
            light.range = 20f + effectData.genericUInt;
            var scale = 1f + effectData.genericUInt / (6f + 2 / 3f);
            transform.localScale = new Vector3(scale, scale, 4f);
            childLocator.FindChild("TracerHead").GetComponent<Light>().range = 40f + effectData.genericUInt;
            childLocator.FindChild("TracerTail").GetComponent<Light>().range = 20f + effectData.genericUInt;
            childLocator.FindChild("Beam, Linger").GetComponent<LineRenderer>().widthMultiplier = 0.5f + (effectData.genericUInt / 40f);

            switch (effectData.genericUInt)
            {
                case < 5:
                    DisableTransform("BeamParticles, Rings");
                    DisableTransform("Beam, Flash Briefly");
                    DisableTransform("Beam, Distortion");
                    DisableTransform("Beam, Glow");
                    break;

                case >= 5 and < 10:
                    DisableTransform("Beam, Flash Briefly");
                    DisableTransform("Beam, Distortion");
                    DisableTransform("Beam, Glow");
                    break;

                case >= 10 and < 15:
                    DisableTransform("Beam, Distortion");
                    DisableTransform("Beam, Glow");
                    break;
            }
        }

        public void DisableTransform(string transformToDisable)
        {
            childLocator.FindChild(transformToDisable).gameObject.SetActive(false);
        }
    }
}