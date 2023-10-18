namespace Sandswept.Skills.Ranger.VFX
{
    public static class ReleaseVFX
    {
        public static GameObject tracerPrefab;
        public static GameObject impactPrefab;

        public static void Init()
        {
            tracerPrefab = PrefabAPI.InstantiateClone(Assets.GameObject.TracerHuntressSnipe, "Release Tracer", false);

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
            newMat.SetColor("_TintColor", new Color32(51, 234, 140, 255));
            newMat.SetTexture("_MainTex", Assets.Texture2D.texCrosshairBullets1);

            tracerHead.material = newMat;

            //

            // beam object

            var beamObject = tracerPrefab.transform.GetChild(2);
            // beamObject.transform.localScale = Vector3.one * 3f;

            var destroyOnTimer2 = beamObject.AddComponent<DestroyOnTimer>();
            destroyOnTimer2.duration = 3f;

            var particleSystem = beamObject.GetComponent<ParticleSystem>();

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
            newMat2.SetColor("_TintColor", new Color32(92, 224, 125, 255));

            // particleSystemRenderer.material = newMat2;

            var newMat3 = Object.Instantiate(Assets.Material.matHuntressSwingTrail);
            newMat3.SetColor("_TintColor", new Color32(0, 255, 99, 255));
            newMat3.SetFloat("_Boost", 1.94f);
            newMat3.SetFloat("_AlphaBoost", 4.6f);
            newMat3.SetFloat("_AlphaBias", 0.093f);
            newMat3.SetColor("_CutoffScroll", new Color(15f, 0.02f, 0f, 0f));

            particleSystemRenderer.sharedMaterials = new Material[] { newMat2, newMat3 };

            //

            ContentAddition.AddEffect(tracerPrefab);

            impactPrefab = PrefabAPI.InstantiateClone(Assets.GameObject.OmniExplosionCrowstorm, "Release Impact", false);

            var effectComponent = impactPrefab.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_lunar_wisp_attack2_explode";

            var trans = impactPrefab.transform;

            // scaled hitsparks 1

            var geen1 = new Color32(23, 234, 129, 255);
            var geen2 = new Color32(23, 211, 148, 255);

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

            var scaledHitsparks1 = trans.GetChild(0).GetComponent<ParticleSystem>().main;
            scaledHitsparks1.startColor = new ParticleSystem.MinMaxGradient(minGradient, maxGradient);

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