namespace Sandswept.Skills.Ranger.VFX
{
    public static class ExhaustVFX
    {
        public static GameObject tracerPrefab;
        public static GameObject impactPrefab;

        // replace with railgunner m2/special later

        public static void Init()
        {
            tracerPrefab = PrefabAPI.InstantiateClone(Assets.GameObject.TracerHuntressSnipe, "Exhaust Tracer", false);

            var destroyOnTimer = tracerPrefab.AddComponent<DestroyOnTimer>();
            destroyOnTimer.duration = 3f;

            // tracer head

            var tracerHead = tracerPrefab.transform.GetChild(0).GetComponent<LineRenderer>();

            var animateShaderAlpha = tracerHead.GetComponent<AnimateShaderAlpha>();
            animateShaderAlpha.timeMax = 0.25f;

            var gradient = new Gradient();
            var colors = new GradientColorKey[2];
            colors[0] = new GradientColorKey(Color.white, 0f);
            colors[1] = new GradientColorKey(new Color32(95, 209, 177, 255), 0f);

            var alphas = new GradientAlphaKey[2];
            alphas[0] = new GradientAlphaKey(1f, 0f);
            alphas[1] = new GradientAlphaKey(1f, 1f);

            gradient.SetKeys(colors, alphas);

            tracerHead.colorGradient = gradient;

            var newMat = Object.Instantiate(Assets.Material.matHuntressArrowBig);
            newMat.SetColor("_TintColor", new Color32(234, 122, 51, 255));
            newMat.SetTexture("_MainTex", Assets.Texture2D.texCrosshairBullets1);

            tracerHead.material = newMat;

            //

            // beam object

            var beamObject = tracerPrefab.transform.GetChild(2);

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
            newMat2.SetColor("_TintColor", new Color32(224, 112, 92, 255));

            // particleSystemRenderer.material = newMat2;

            var newMat3 = Object.Instantiate(Assets.Material.matHuntressSwingTrail);
            newMat3.SetTexture("_RemapTex", Main.hifuSandswept.LoadAsset<Texture2D>("Assets/Sandswept/texRampGay.png"));
            newMat3.SetColor("_TintColor", new Color32(255, 126, 0, 255));
            newMat3.SetFloat("_SoftFactor", 1f);
            newMat3.SetFloat("_Boost", 1.277907f);
            newMat3.SetFloat("_AlphaBoost", 0f);
            newMat3.SetFloat("_AlphaBias", 0.2317166f);
            newMat3.SetColor("_CutoffScroll", new Color(15f, 0.02f, 0f, 0f));

            particleSystemRenderer.sharedMaterials = new Material[] { newMat2, newMat3 };

            //

            ContentAddition.AddEffect(tracerPrefab);

            impactPrefab = PrefabAPI.InstantiateClone(Assets.GameObject.OmniExplosionCrowstorm, "Exhaust Impact", false);

            var effectComponent = impactPrefab.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_lunar_wisp_attack2_explode";

            var trans = impactPrefab.transform;

            for (int j = 0; j < trans.childCount; j++)
            {
                var child = trans.GetChild(j);
                child.localScale = Vector3.one * 2f;
            }

            // scaled hitsparks 1

            var geen1 = new Color32(234, 107, 23, 255);
            var geen2 = new Color32(211, 129, 23, 255);

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

            var newScaledHitsparks1Mat = GameObject.Instantiate(Assets.Material.matOmniHitspark1Huntress);
            newScaledHitsparks1Mat.SetColor("_TintColor", new Color32(255, 184, 0, 255));
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

            var ramp = Assets.Texture2D.texRampAncientWisp;

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

            ContentAddition.AddEffect(impactPrefab);
        }
    }
}