namespace Sandswept.Survivors.Ranger.VFX
{
    public static class ExhaustVFX
    {
        public static GameObject tracerPrefabDefault;
        public static GameObject impactPrefabDefault;

        public static GameObject tracerPrefabMajor;
        public static GameObject impactPrefabMajor;

        public static GameObject tracerPrefabRenegade;
        public static GameObject impactPrefabRenegade;

        public static GameObject tracerPrefabMileZero;
        public static GameObject impactPrefabMileZero;

        public static GameObject tracerPrefabRacecar;
        public static GameObject impactPrefabRacecar;

        public static GameObject tracerPrefabSandswept;
        public static GameObject impactPrefabSandswept;

        // replace with railgunner m2/special later

        public static void Init()
        {
            tracerPrefabDefault = CreateTracerRecolor("Default", new Color32(95, 209, 177, 255), new Color32(234, 122, 51, 255), new Color32(158, 14, 0, 255));
            impactPrefabDefault = CreateImpactRecolor("Default", new Color32(234, 107, 23, 255), new Color32(211, 129, 23, 94), new Color32(76, 41, 8, 255));

            tracerPrefabMajor = CreateTracerRecolor("Major", new Color32(95, 125, 209, 255), new Color32(51, 78, 234, 255), new Color32(0, 70, 158, 255), true, 20f, 0f, 0.2559297f);
            impactPrefabMajor = CreateImpactRecolor("Major", new Color32(23, 66, 234, 255), new Color32(23, 30, 211, 94), new Color32(0, 132, 255, 255), 20f, 0.761f);

            tracerPrefabRenegade = CreateTracerRecolor("Renegade", new Color32(219, 103, 159, 255), new Color32(143, 51, 234, 255), new Color32(32, 0, 158, 255), true);
            impactPrefabRenegade = CreateImpactRecolor("Renegade", new Color32(125, 23, 234, 255), new Color32(145, 23, 211, 94), new Color32(6, 0, 255, 255));

            tracerPrefabMileZero = CreateTracerRecolor("Mile Zero", new Color32(127, 0, 0, 255), new Color32(0, 0, 0, 255), new Color32(255, 0, 0, 255), false, 2.207824f, 1.515893f, 0.397718f);
            impactPrefabMileZero = CreateImpactRecolor("Mile Zero", new Color32(0, 0, 0, 255), new Color32(127, 0, 0, 94), new Color32(4, 0, 0, 255));

            tracerPrefabRacecar = CreateTracerRecolor("Racecar", new Color32(127, 0, 0, 255), new Color32(0, 0, 0, 255), new Color32(255, 0, 0, 255), false, 2.207824f, 1.515893f, 0.397718f);
            impactPrefabRacecar = CreateImpactRecolor("Racecar", new Color32(0, 0, 0, 255), new Color32(127, 0, 0, 94), new Color32(4, 0, 0, 255));

            tracerPrefabSandswept = CreateTracerRecolor("Sandswept", new Color32(255, 255, 255, 255), new Color32(150, 150, 150, 255), new Color32(255, 255, 255, 255), false, 2.207824f, 1.515893f, 0.397718f, true);
            impactPrefabSandswept = CreateImpactRecolor("Sandswept", new Color32(230, 230, 230, 255), new Color32(249, 197, 143, 94), new Color32(87, 87, 87, 255));
        }

        public static GameObject CreateTracerRecolor(string name, Color32 aquaEquivalent, Color32 orangeEquivalent, Color32 darkRedEquivalent, bool altRamp = false, float brightnessBoost = 1.277907f, float alphaBoost = 0f, float alphaBias = 0.2317166f, bool sandsweptRamp = false)
        {
            // aquaEquivalent = new Color32(95, 209, 177, 255);
            // orangeEquivalent = new Color32(234, 122, 51, 255);
            // salmonEquivalent = new Color32(158, 14, 0, 255);
            var tracer = Paths.GameObject.TracerHuntressSnipe.InstantiateClone("Exhaust Tracer " + name, false);

            tracer.GetOrAddComponent<VFXAttributes>((x) =>
            {
                x.DoNotPool = true;
                x.DoNotCullPool = true;
                x.vfxPriority = VFXAttributes.VFXPriority.Always;
            });

            var destroyOnTimer = tracer.AddComponent<DestroyOnTimer>();
            destroyOnTimer.duration = 3f;

            // tracer head

            var tracerHead = tracer.transform.GetChild(0).GetComponent<LineRenderer>();

            var animateShaderAlpha = tracerHead.GetComponent<AnimateShaderAlpha>();
            animateShaderAlpha.timeMax = 0.25f;

            var gradient = new Gradient();
            var colors = new GradientColorKey[2];
            colors[0] = new GradientColorKey(Color.white, 0f);
            colors[1] = new GradientColorKey(aquaEquivalent, 0f);

            var alphas = new GradientAlphaKey[2];
            alphas[0] = new GradientAlphaKey(1f, 0f);
            alphas[1] = new GradientAlphaKey(1f, 1f);

            gradient.SetKeys(colors, alphas);

            tracerHead.colorGradient = gradient;

            var newMat = Object.Instantiate(Paths.Material.matHuntressArrowBig);
            newMat.SetColor("_TintColor", orangeEquivalent);
            newMat.SetTexture("_MainTex", Paths.Texture2D.texCrosshairBullets1);

            tracerHead.material = newMat;

            //

            // beam object

            var beamObject = tracer.transform.GetChild(2);

            var destroyOnTimer2 = beamObject.AddComponent<DestroyOnTimer>();
            destroyOnTimer2.duration = 3f;

            var particleSystem = beamObject.GetComponent<ParticleSystem>();

            var main = particleSystem.main;
            main.startSize = 0.1f;
            main.startLifetime = 0.8f;

            var startColor = particleSystem.main.startColor;
            startColor.mode = ParticleSystemGradientMode.Color;
            startColor.color = Color.white;

            var noise = particleSystem.noise;
            noise.quality = ParticleSystemNoiseQuality.Medium;
            noise.rotationAmount = 0.5f;
            noise.sizeAmount = 0.2f;
            noise.positionAmount = 2f;

            var particleSystemRenderer = beamObject.GetComponent<ParticleSystemRenderer>();

            var beamObjectPS = beamObject.GetComponent<ParticleSystem>();
            var beamObjectMain = beamObjectPS.main;
            var startLifetime = beamObjectMain.startLifetime;
            startLifetime.constant = 1f;
            var colorOverLifetime = beamObjectPS.colorOverLifetime;

            var gradient2 = new Gradient();
            var colors2 = new GradientColorKey[2];
            colors2[0] = new GradientColorKey(Color.white, 0f);
            colors2[1] = new GradientColorKey(Color.black, 1f);

            var alphas2 = new GradientAlphaKey[2];
            alphas2[0] = new GradientAlphaKey(1f, 0f);
            alphas2[1] = new GradientAlphaKey(0f, 1f);

            gradient2.SetKeys(colors2, alphas2);

            colorOverLifetime.color = gradient2;

            var newMat2 = Object.Instantiate(Paths.Material.matHuntressSwingTrail);
            newMat2.SetColor("_TintColor", new Color32(224, 112, 92, 255));

            // particleSystemRenderer.material = newMat2;

            var newMat3 = Object.Instantiate(Paths.Material.matHuntressSwingTrail);

            var p2 = "texRampGay";

            var p2_2 = altRamp ? "2.png" : ".png";

            var inConcretion = p2 + p2_2;
            if (sandsweptRamp)
            {
                newMat3.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            }
            else
            {
                newMat3.SetTexture("_RemapTex", Main.hifuSandswept.LoadAsset<Texture2D>(inConcretion));
            }

            newMat3.SetColor("_TintColor", darkRedEquivalent);
            newMat3.SetFloat("_SoftFactor", 1f);
            newMat3.SetFloat("_Boost", brightnessBoost);
            newMat3.SetFloat("_AlphaBoost", alphaBoost);
            newMat3.SetFloat("_AlphaBias", alphaBias);
            newMat3.SetColor("_CutoffScroll", new Color(15f, 0.02f, 0f, 0f));

            particleSystemRenderer.sharedMaterials = new Material[] { newMat2, newMat3 };

            //
            /*
            var animateShaderAlpha2 = beamObject.AddComponent<AnimateShaderAlpha>();
            animateShaderAlpha2.targetRenderer = particleSystemRenderer;
            animateShaderAlpha2.alphaCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
            animateShaderAlpha2.timeMax = 1f;
            doesnt even do anything
             */

            ContentAddition.AddEffect(tracer);

            return tracer;
        }

        public static GameObject CreateImpactRecolor(string name, Color32 hotPinkEquivalent, Color32 redEquivalent, Color32 spikeColor, float brightnessBoost = 11.08f, float alphaBoost = 4.3f)
        {
            // hotPinkEquivalent = new Color32(226, 27, 128, 255);
            // redEquivalent = new Color32(209, 21, 15, 96);
            // spikeColor = new Color32(255,255,255,255);
            var impact = Paths.GameObject.ImpactRailgunLight.InstantiateClone("Exhaust Impact " + name, false);

            var trans = impact.transform;

            var beamParticles = trans.GetChild(0);
            beamParticles.gameObject.SetActive(false);

            var shockwave = trans.GetChild(1);
            var shockwavePS = shockwave.GetComponent<ParticleSystem>();
            var shockwaveMain = shockwavePS.main;
            var shockwaveColor = shockwaveMain.startColor;
            shockwaveColor.color = hotPinkEquivalent;
            var shockwavePSR = shockwave.GetComponent<ParticleSystemRenderer>();

            var newShockwaveMaterial = new Material(Paths.Material.matRailgunRings);
            newShockwaveMaterial.SetColor("_TintColor", redEquivalent);

            shockwavePSR.material = newShockwaveMaterial;

            var flashWhite = trans.GetChild(2);
            var flashWhitePSR = flashWhite.GetComponent<ParticleSystemRenderer>();

            var newFlashWhiteMat = new Material(Paths.Material.matRailgunTracerHeadLight);
            newFlashWhiteMat.SetTexture("_RemapTex", Paths.Texture2D.texRampTritoneSmoothed);
            newFlashWhiteMat.SetColor("_TintColor", new Color32(hotPinkEquivalent.r, hotPinkEquivalent.g, hotPinkEquivalent.b, 143));
            newFlashWhiteMat.SetFloat("_Boost", 1f);
            newFlashWhiteMat.SetFloat("_AlphaBias", 0.8453865f);
            flashWhitePSR.material = newFlashWhiteMat;
            // flashWhite.gameObject.SetActive(false);

            var daggers = trans.GetChild(3).GetComponent<ParticleSystemRenderer>();

            var daggersPS = daggers.GetComponent<ParticleSystem>().main.startLifetime;
            daggersPS.constantMin = 1f;
            daggersPS.constantMax = 1f;

            var newMat = new Material(Paths.Material.matRailgunImpactSpikesLight);
            newMat.SetColor("_TintColor", spikeColor);
            newMat.SetFloat("_Boost", brightnessBoost);
            newMat.SetFloat("_AlphaBoost", alphaBoost);
            newMat.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);

            daggers.material = newMat;

            var flashWhite3 = trans.GetChild(7).GetComponent<ParticleSystem>().main.startColor;
            flashWhite3.color = redEquivalent;

            ContentAddition.AddEffect(impact);
            // 9,0,0
            return impact;
        }
    }
}