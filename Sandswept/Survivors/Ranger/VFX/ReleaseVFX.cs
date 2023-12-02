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

        public static void Init()
        {
            tracerPrefabDefault = CreateTracerRecolor("Default", new Color32(95, 209, 177, 255), new Color32(51, 234, 149, 255), new Color32(95, 224, 125, 255), new Color32(0, 121, 255, 255), true, 2.675917f, 0.8582755f, 10f);
            impactPrefabDefault = CreateImpactRecolor("Default", new Color32(23, 234, 129, 255), new Color32(23, 211, 148, 255), new Color32(0, 255, 210, 255));

            tracerPrefabMajor = CreateTracerRecolor("Major", new Color32(95, 125, 209, 255), new Color32(51, 133, 234, 255), new Color32(95, 192, 224, 255), new Color32(4, 0, 255, 255), false, 10.65977f, 0.4565004f, 0.06634249f);
            impactPrefabMajor = CreateImpactRecolor("Major", new Color32(23, 124, 234, 255), new Color32(23, 83, 211, 255), new Color32(0, 41, 255, 255));

            tracerPrefabRenegade = CreateTracerRecolor("Renegade", new Color32(219, 103, 159, 255), new Color32(246, 59, 183, 255), new Color32(234, 102, 230, 255), new Color32(176, 31, 187, 255), false, 2.474493f, 0.5483652f, 1.758069f);
            impactPrefabRenegade = CreateImpactRecolor("Renegade", new Color32(247, 32, 182, 255), new Color32(225, 34, 136, 255), new Color32(255, 9, 107, 255));

            tracerPrefabMileZero = CreateTracerRecolor("Mile Zero", new Color32(209, 95, 95, 255), new Color32(234, 51, 84, 255), new Color32(224, 95, 157, 255), new Color32(255, 0, 2, 255), false, 20f, 4.072613f, 0.1128651f);
            impactPrefabMileZero = CreateImpactRecolor("Mile Zero", new Color32(234, 23, 68, 255), new Color32(211, 23, 33, 255), new Color32(255, 27, 0, 255));
        }

        public static GameObject CreateTracerRecolor(string name, Color32 lightBlueEquivalent, Color32 lightAquaEquivalent, Color32 lightGreenEquivalent, Color32 tintColor, bool altRamp = false, float brightnessBoost = 20f, float alphaBias = 0.2612987f, float alphaBoost = 0.5506042f)
        {
            // 0 255 141 255
            // lightBlueEquivalent = new Color32(95, 209, 177, 255);
            // lightAquaEquivalent = new Color32(51,234,149,255);
            // lightGreenEquivalent = new Color32(92,224,125,255);
            // tintColor = new Color32(0, 1, 255, 255);

            var tracer = Assets.GameObject.TracerHuntressSnipe.InstantiateClone("Release Tracer " + name, false);

            tracer.AddComponent<VFXAttributes>();

            var destroyOnTimer = tracer.AddComponent<DestroyOnTimer>();
            destroyOnTimer.duration = 3f;

            // tracer head

            var tracerHead = tracer.transform.GetChild(0).GetComponent<LineRenderer>();

            var animateShaderAlpha = tracerHead.GetComponent<AnimateShaderAlpha>();
            animateShaderAlpha.timeMax = 0.25f;

            var gradient = new Gradient();
            var colors = new GradientColorKey[2];
            colors[0] = new GradientColorKey(Color.white, 0f);
            colors[1] = new GradientColorKey(lightBlueEquivalent, 1f);

            var alphas = new GradientAlphaKey[2];
            alphas[0] = new GradientAlphaKey(1f, 0f);
            alphas[1] = new GradientAlphaKey(1f, 1f);

            gradient.SetKeys(colors, alphas);

            tracerHead.colorGradient = gradient;

            var newMat = Object.Instantiate(Assets.Material.matHuntressArrowBig);
            newMat.SetColor("_TintColor", lightAquaEquivalent);
            newMat.SetTexture("_MainTex", Assets.Texture2D.texCrosshairBullets1);

            tracerHead.material = newMat;

            //

            // beam object

            var beamObject = tracer.transform.GetChild(2);

            var destroyOnTimer2 = beamObject.AddComponent<DestroyOnTimer>();
            destroyOnTimer2.duration = 3f;

            var particleSystem = beamObject.GetComponent<ParticleSystem>();

            var main = particleSystem.main;
            main.startSize = 1f;

            var startColor = particleSystem.main.startColor;
            startColor.mode = ParticleSystemGradientMode.Color;
            startColor.color = Color.white;

            var noise = particleSystem.noise;
            noise.quality = ParticleSystemNoiseQuality.Medium;
            noise.rotationAmount = 0.5f;
            noise.sizeAmount = 0.2f;
            noise.positionAmount = 2f;

            var particleSystemRenderer = beamObject.GetComponent<ParticleSystemRenderer>();

            var newMat2 = Object.Instantiate(Assets.Material.matHuntressSwingTrail);
            newMat2.SetColor("_TintColor", lightGreenEquivalent);

            // particleSystemRenderer.material = newMat2;

            var newMat3 = Object.Instantiate(Assets.Material.matHuntressSwingTrail);
            newMat3.SetTexture("_RemapTex", altRamp ? Assets.Texture2D.texRampBandit : Main.hifuSandswept.LoadAsset<Texture2D>("Assets/Sandswept/texRampGay.png"));
            newMat3.SetColor("_TintColor", tintColor);
            newMat3.SetFloat("_SoftFactor", 0.8866442f);
            newMat3.SetFloat("_Boost", brightnessBoost);
            newMat3.SetFloat("_AlphaBoost", alphaBoost);
            newMat3.SetFloat("_AlphaBias", alphaBias);
            newMat3.SetColor("_CutoffScroll", new Color(15f, 0.02f, 0f, 0f));

            particleSystemRenderer.sharedMaterials = new Material[] { newMat2, newMat3 };

            ContentAddition.AddEffect(tracer);

            return tracer;
        }

        public static GameObject CreateImpactRecolor(string name, Color32 saturatedAquaEquivalent, Color32 lessSaturatedAquaEquivalent, Color32 saturatedBlueEquivalent)
        {
            // saturatedAquaEquivalent = new Color32(23,234,129,255);
            // lessSaturatedAquaEquivalent = new Color32(23,211,148,255);
            // saturatedBlueEquivalent = new Color32(0,255,210,255);

            var impact = Assets.GameObject.OmniExplosionCrowstorm.InstantiateClone("Release Impact " + name, false);

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

            var newScaledHitsparks1Mat = Object.Instantiate(Assets.Material.matOmniHitspark1Huntress);
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
            unscaledHitsparks1PSR.material = Assets.Material.matOmniHitspark1GreaterWisp;

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

            var newMat4 = Object.Instantiate(Assets.Material.matCrowstormFeatherRepeated);

            newMat4.SetColor("_TintColor", geen2);
            newMat4.SetTexture("_MainTex", Assets.Texture2D.texShockwaveRing3Mask);
            // newMat4.SetTexture("_RemapTex", ramp);

            particleSystemRenderer2.material = newMat4;

            //

            // unscaled smoke billboard

            var unscaledSmokeBillboard = trans.GetChild(4).GetComponent<ParticleSystem>().main.startColor;
            unscaledSmokeBillboard.color = geen2;

            //

            // area indicator ring billboard

            var areaIndicatorRingBillboard = trans.GetChild(5).GetComponent<ParticleSystemRenderer>();

            var newMat5 = Object.Instantiate(Assets.Material.matOmniRing2Loader);

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

            var newMat6 = Object.Instantiate(Assets.Material.matCrowstormFeather);
            newMat6.SetColor("_TintColor", geen2);
            newMat6.SetTexture("_MainTex", Assets.Texture2D.texShockwaveRing3Mask);
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
}