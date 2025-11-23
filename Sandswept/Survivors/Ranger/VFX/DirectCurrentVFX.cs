using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.VFX
{
    public static class DirectCurrentVFX
    {
        public static GameObject ghostPrefabDefault;
        public static GameObject impactPrefabDefault;
        public static GameObject hitsparkPrefabDefault;
        public static GameObject muzzleFlashPrefabDefault;
        public static GameObject muzzleFlashPrefabDefaultOverdrive;

        public static GameObject ghostPrefabMajor;
        public static GameObject impactPrefabMajor;
        public static GameObject hitsparkPrefabMajor;
        public static GameObject muzzleFlashPrefabMajor;
        public static GameObject muzzleFlashPrefabMajorOverdrive;

        public static GameObject ghostPrefabRenegade;
        public static GameObject impactPrefabRenegade;
        public static GameObject hitsparkPrefabRenegade;
        public static GameObject muzzleFlashPrefabRenegade;
        public static GameObject muzzleFlashPrefabRenegadeOverdrive;

        public static GameObject ghostPrefabMileZero;
        public static GameObject impactPrefabMileZero;
        public static GameObject hitsparkPrefabMileZero;
        public static GameObject muzzleFlashPrefabMileZero;
        public static GameObject muzzleFlashPrefabMileZeroOverdrive;

        public static GameObject ghostPrefabRacecar;
        public static GameObject impactPrefabRacecar;
        public static GameObject hitsparkPrefabRacecar;
        public static GameObject muzzleFlashPrefabRacecar;
        public static GameObject muzzleFlashPrefabRacecarOverdrive;

        public static GameObject ghostPrefabSandswept;
        public static GameObject impactPrefabSandswept;
        public static GameObject hitsparkPrefabSandswept;
        public static GameObject muzzleFlashPrefabSandswept;
        public static GameObject muzzleFlashPrefabSandsweptOverdrive;

        public static void Init()
        {
            ghostPrefabDefault = CreateGhostRecolor("Default", new Color32(0, 255, 167, 255), new Color32(0, 141, 197, 255), new Color32(111, 170, 151, 255));
            impactPrefabDefault = CreateImpactRecolor("Default", new Color32(34, 34, 34, 255), new Color32(14, 32, 161, 255), new Color32(25, 67, 255, 255),
                new Color32(0, 108, 238, 255), new Color32(0, 57, 147, 255), new Color32(20, 255, 168, 255), new Color32(0, 133, 255, 255), new Color32(88, 229, 255, 255),
                new Color32(54, 71, 214, 255), new Color32(58, 218, 219, 255), new Color32(255, 255, 255, 255), new Color32(39, 140, 144, 255), new Color32(49, 166, 180, 255),
                new Color32(0, 255, 62, 255), new Color32(0, 255, 90, 255), 1.301445f, 0.07936508f);
            hitsparkPrefabDefault = CreateHitsparkRecolor("Default", new Color32(0, 255, 167, 255));
            muzzleFlashPrefabDefault = CreateMuzzleFlashRecolor("Default", new Color32(0, 255, 167, 255));
            muzzleFlashPrefabDefaultOverdrive = CreateMuzzleFlashRecolor("Default", new Color32(255, 151, 0, 255), true);

            ghostPrefabMajor = CreateGhostRecolor("Major", new Color32(43, 94, 134, 255), new Color32(0, 49, 197, 255), new Color32(100, 20, 217, 255));
            impactPrefabMajor = CreateImpactRecolor("Major", new Color32(34, 34, 34, 255), new Color32(65, 14, 161, 255), new Color32(90, 25, 255, 255),
                new Color32(3, 0, 238, 255), new Color32(12, 0, 147, 255), new Color32(34, 20, 255, 255), new Color32(0, 14, 255, 255), new Color32(88, 151, 255, 255),
                new Color32(112, 54, 214, 255), new Color32(95, 65, 243, 255), new Color32(255, 255, 255, 255), new Color32(96, 71, 255, 255),
                new Color32(49, 105, 180, 255), new Color32(193, 67, 184, 255), new Color32(108, 0, 255, 255));
            hitsparkPrefabMajor = CreateHitsparkRecolor("Major", new Color32(43, 94, 134, 255));
            muzzleFlashPrefabMajor = CreateMuzzleFlashRecolor("Major", new Color32(48, 0, 255, 255));
            muzzleFlashPrefabMajorOverdrive = CreateMuzzleFlashRecolor("Major", new Color32(0, 24, 255, 255), true);

            ghostPrefabRenegade = CreateGhostRecolor("Renegade", new Color32(219, 51, 232, 255), new Color32(187, 0, 197, 255), new Color32(217, 20, 98, 255));
            impactPrefabRenegade = CreateImpactRecolor("Renegade", new Color32(34, 34, 34, 255), new Color32(161, 14, 81, 255), new Color32(255, 25, 144, 255),
                new Color32(238, 0, 187, 255), new Color32(147, 0, 106, 255), new Color32(255, 20, 194, 255), new Color32(255, 0, 218, 255), new Color32(225, 88, 255, 255),
                new Color32(214, 54, 124, 255), new Color32(85, 65, 243, 255), new Color32(255, 255, 255, 255), new Color32(255, 71, 193, 255),
                new Color32(150, 49, 180, 255), new Color32(234, 44, 85, 255), new Color32(255, 0, 38, 255), 0.9672766f, 0.1169591f);
            hitsparkPrefabRenegade = CreateHitsparkRecolor("Renegade", new Color32(219, 51, 232, 255));
            muzzleFlashPrefabRenegade = CreateMuzzleFlashRecolor("Renegade", new Color32(219, 51, 232, 255));
            muzzleFlashPrefabRenegadeOverdrive = CreateMuzzleFlashRecolor("Renegade", new Color32(255, 17, 174, 255), true);

            ghostPrefabMileZero = CreateGhostRecolor("Mile Zero", new Color32(255, 0, 50, 255), new Color32(197, 0, 13, 255), new Color32(217, 21, 20, 255));
            impactPrefabMileZero = CreateImpactRecolor("Mile Zero", new Color32(34, 34, 34, 255), new Color32(167, 0, 0, 255), new Color32(255, 0, 3, 255),
                new Color32(250, 0, 0, 255), new Color32(154, 0, 0, 255), new Color32(255, 0, 0, 255), new Color32(255, 0, 0, 255), new Color32(255, 19, 19, 255),
                new Color32(216, 31, 25, 255), new Color32(255, 0, 0, 255), new Color32(255, 255, 255, 255), new Color32(255, 37, 39, 255),
                new Color32(181, 25, 29, 255), new Color32(170, 5, 2, 255), new Color32(255, 0, 0, 255), 2.639934f, 0.1044277f);
            hitsparkPrefabMileZero = CreateHitsparkRecolor("Mile Zero", new Color32(255, 0, 13, 255));
            muzzleFlashPrefabMileZero = CreateMuzzleFlashRecolor("Mile Zero", new Color32(255, 0, 13, 255));
            muzzleFlashPrefabMileZeroOverdrive = CreateMuzzleFlashRecolor("Mile Zero", new Color32(21, 0, 0, 255), true);

            ghostPrefabSandswept = CreateGhostRecolor("Sandswept", new Color32(249, 197, 143, 255), new Color32(214, 159, 79, 255), new Color32(87, 87, 87, 255));
            impactPrefabSandswept = CreateImpactRecolor("Sandswept", new Color32(34, 34, 34, 255), new Color32(214, 159, 79, 255), new Color32(249, 197, 143, 255),
                new Color32(214, 159, 79, 255), new Color32(255, 255, 255, 255), new Color32(150, 150, 150, 255), new Color32(255, 255, 255, 255), new Color32(87, 87, 87, 255),
                new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255), new Color32(100, 70, 36, 255),
                new Color32(59, 45, 23, 255), new Color32(214, 159, 79, 255), new Color32(249, 197, 143, 255));
            hitsparkPrefabSandswept = CreateHitsparkRecolor("Sandswept", new Color32(249, 197, 143, 255));
            muzzleFlashPrefabSandswept = CreateMuzzleFlashRecolor("Sandswept", new Color32(249, 197, 143, 255));
            muzzleFlashPrefabSandsweptOverdrive = CreateMuzzleFlashRecolor("Sandswept", new Color32(255, 181, 66, 255), true);

            /*
            ghostPrefabRacecar = CreateGhostRecolor("Racecar", new Color32(0, 255, 248, 255), new Color32(0, 180, 197, 255), new Color32(111, 170, 170, 255));
            impactPrefabRacecar = CreateImpactRecolor("Racecar", new Color32(34, 34, 34, 255), new Color32(14, 161, 158, 255), new Color32(25, 186, 255, 255),
                new Color32(0, 211, 238, 255), new Color32(0, 113, 147, 255), new Color32(20, 190, 255, 255), new Color32(0, 214, 255, 255), new Color32(88, 229, 255, 255),
                new Color32(54, 178, 214, 255), new Color32(243, 211, 65, 255), new Color32(255, 255, 255, 255), new Color32(39, 140, 144, 255), new Color32(49, 166, 180, 255),
                new Color32(0, 255, 223, 255), new Color32(0, 255, 251, 255), 1.301445f, 0.07936508f);
            */
        }

        public static GameObject CreateMuzzleFlashRecolor(string name, Color32 mainColor, bool overdrive = false)
        {
            var prefab = PrefabAPI.InstantiateClone(Paths.GameObject.Muzzleflash1, "Muzzle Flash " + name + (overdrive ? " Overdrive" : ""), false);

            prefab.GetOrAddComponent<VFXAttributes>((x) =>
            {
                x.DoNotPool = true;
                x.DoNotCullPool = true;
            });

            VFXUtils.RecolorMaterialsAndLights(prefab, mainColor, mainColor, true);
            VFXUtils.MultiplyDuration(prefab, 1.66f);

            var starburst = prefab.transform.Find("Starburst");

            var starburstPSR = starburst.GetComponent<ParticleSystemRenderer>();
            starburstPSR.material.SetTexture("_MainTex", overdrive ? Paths.Texture2D.texBirdHeartRuinMask : Paths.Texture2D.texBbDecalMask1);
            starburstPSR.material.SetFloat("_Boost", overdrive ? 5f : 4f);
            starburstPSR.material.SetFloat("_AlphaBoost", 7.25f);
            starburstPSR.material.SetFloat("_AlphaBias", 0.22f);

            var starburstPS = starburst.GetComponent<ParticleSystem>();

            var main = starburstPS.main;
            main.maxParticles = overdrive ? 6 : 4;

            var shape = starburstPS.shape;
            if (overdrive)
            {
                shape.arcMode = ParticleSystemShapeMultiModeValue.Random;
                shape.radiusMode = ParticleSystemShapeMultiModeValue.Random;
                shape.meshSpawnMode = ParticleSystemShapeMultiModeValue.Random;
            }

            for (int i = 0; i < prefab.transform.childCount; i++)
            {
                var child = prefab.transform.GetChild(i);
                child.localPosition = new Vector3(0f, -0.33f, -0.15f);
                child.localEulerAngles = new Vector3(90f, 0f, 0f);
                child.localScale *= 1.33f;
            }

            prefab.transform.Find("Point light").GetComponent<Light>().range = 7f;

            ContentAddition.AddEffect(prefab);

            return prefab;
        }

        public static GameObject CreateHitsparkRecolor(string name, Color32 mainColor)
        {
            var prefab = PrefabAPI.InstantiateClone(Paths.GameObject.HitsparkCommando, "Direct Current Hitspark " + name, false);
            prefab.GetOrAddComponent<VFXAttributes>((x) =>
            {
                x.DoNotPool = true;
                x.DoNotCullPool = true;
            });

            prefab.GetComponent<EffectComponent>().applyScale = true;

            VFXUtils.OdpizdzijPierdoloneGownoKurwaCoZaJebanyKurwaSmiecToKurwaDodalPizdaKurwaJebanaKurwa(prefab);
            VFXUtils.MultiplyScale(prefab, 1.5f);
            VFXUtils.MultiplyDuration(prefab, 3f);
            VFXUtils.RecolorMaterialsAndLights(prefab, mainColor, mainColor, true);

            var hitflash = prefab.transform.Find("Particles/HitFlash").GetComponent<ParticleSystemRenderer>();
            hitflash.material.SetTexture("_MainTex", Paths.Texture2D.texCrackedGroundImpactMask);
            hitflash.material.SetFloat("_Boost", 8f);
            hitflash.material.SetFloat("_AlphaBoost", 6f);
            hitflash.material.SetFloat("_AlphaBias", 0f);

            var triangleSparks = prefab.transform.Find("Particles/TriangleSparks").GetComponent<ParticleSystemRenderer>();
            triangleSparks.material.SetInt("_ZTest", 7); // GreaterEqual so that it appears in front of bodies it hits

            ContentAddition.AddEffect(prefab);

            return prefab;
        }

        public static GameObject CreateImpactRecolor(string name, Color32 darkGreenEquivalent, Color32 darkBlueEquivalent, Color32 saturatedBlueEquivalent, Color32 brightBlueEquivalent, Color32 desaturatedBlueEquivalent, Color32 saturatedBlueEquivalent2, Color32 saturatedBlueEquivalent3, Color32 saturatedBlueEquivalent4, Color32 saturatedDarkBlueEquivalent, Color32 lavenderEquivalent, Color32 tintColor, Color32 saturatedBlueEquivalent5, Color32 lightAquaEquivalent, Color32 lightGrayEquivalent, Color32 whiteEquivalent, float alphaBoost = 3.39f, float alphaBias = 0f)
        {
            // darkGreenEquivalent = new Color32(17,17,17,255);
            // darkBlueEquivalent = new Color32(14,32,161,255);
            // saturatedBlueEquivalent = new Color32(25,67,255,255);
            // brightBlueEquivalent = new Color32(0,108,238,255);
            // desaturatedBlueEquivalent = new Color32(0,57,147,255);
            // saturatedBlueEquivalent2 = new Color32(20,116,255,255);
            // saturatedBlueEquivalent3 = new Color32(0,133,255,255);
            // saturatedBlueEquivalent4 = new Color32(88,229,255,255);
            // saturatedDarkBlueEquivalent = new Color32(54,71,214,255);
            // lavenderEquivalent = new Color32(65,70,243,255);
            // tintColor = new Color32(255,255,255,255);
            // saturatedBlueEquivalent5 = new Color32(71,132,255,255);
            // lightAquaEquivalent = new Color32(49, 166, 180,255);
            // lightGrayEquivalent = new Color32(191,191,191,255);
            // whiteEquivalent = new Color32(255,255,255,255);

            var trimmedName = name.Replace(" ", "");

            var impact = Paths.GameObject.OmniImpactVFXLightningMage.InstantiateClone("Direct Current Impact " + name, false);

            var effectComponent = impact.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_engi_M1_explo";
            effectComponent.applyScale = true;

            var trans = impact.transform;

            var scaledHitspark1 = trans.GetChild(0);
            var scaledHitspark1PS = scaledHitspark1.GetComponent<ParticleSystem>().main.startColor;
            scaledHitspark1PS.colorMin = darkBlueEquivalent;
            scaledHitspark1PS.colorMax = saturatedBlueEquivalent;

            var scaledHitspark1PSR = scaledHitspark1.GetComponent<ParticleSystemRenderer>();

            var newMat4 = Object.Instantiate(Paths.Material.matOmniHitspark1Generic);
            newMat4.SetColor("_TintColor", lightGrayEquivalent);

            scaledHitspark1PSR.material = newMat4;

            var scaledHitspark3 = trans.GetChild(1);
            var scaledHitspark3PS = scaledHitspark3.GetComponent<ParticleSystem>().main.startColor;
            scaledHitspark3PS.colorMin = darkBlueEquivalent;
            scaledHitspark3PS.colorMax = saturatedBlueEquivalent;

            var scaledHitspark3PSR = scaledHitspark3.GetComponent<ParticleSystemRenderer>();

            var newMat5 = Object.Instantiate(Paths.Material.matOmniHitspark3Generic);
            newMat5.SetColor("_TintColor", lightGrayEquivalent);

            scaledHitspark3PSR.material = newMat5;

            var omniEffect = impact.GetComponent<OmniEffect>();
            omniEffect.omniEffectGroups[1].omniEffectElements[0].particleSystemOverrideMaterial = newMat4; //hitspark1
            omniEffect.omniEffectGroups[4].omniEffectElements[1].particleSystemOverrideMaterial = newMat5; //hitspark3

            var flashHard = trans.GetChild(2).GetComponent<ParticleSystem>().main.startColor;
            flashHard.color = brightBlueEquivalent;

            var dashBright = trans.GetChild(3);
            var dashBrightPS = dashBright.GetComponent<ParticleSystem>().main.startColor;
            dashBrightPS.color = desaturatedBlueEquivalent;

            var dashBrightPSR = dashBright.GetComponent<ParticleSystemRenderer>();

            var newMat6 = Object.Instantiate(Paths.Material.matTracerBright);
            newMat6.SetColor("_TintColor", whiteEquivalent);

            dashBrightPSR.material = newMat6;

            var impactShockwave = trans.GetChild(5).GetComponent<ParticleSystem>().main.startColor;
            impactShockwave.color = saturatedBlueEquivalent2;

            var flashBlue = trans.GetChild(6).GetComponent<ParticleSystem>().main.startColor;
            flashBlue.color = saturatedBlueEquivalent3;

            var matrixDynamic = trans.GetChild(7);
            var matrixPS = matrixDynamic.GetComponent<ParticleSystem>().main.startColor;

            var colors = new GradientColorKey[2];
            colors[0] = new GradientColorKey(saturatedBlueEquivalent4, 0f);
            colors[1] = new GradientColorKey(saturatedDarkBlueEquivalent, 1f);

            var alphas = new GradientAlphaKey[2];
            alphas[0] = new GradientAlphaKey(1f, 0f);
            alphas[1] = new GradientAlphaKey(1f, 1f);

            var matrixGradient = new Gradient();
            matrixGradient.SetKeys(colors, alphas);

            matrixPS.gradient = matrixGradient;

            var matrixPSR = matrixDynamic.GetComponent<ParticleSystemRenderer>();

            var newTrailMat = new Material(Paths.Material.matLightningLongBlue);

            newTrailMat.SetColor("_TintColor", lavenderEquivalent);
            newTrailMat.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone2);

            matrixPSR.trailMaterial = newTrailMat;

            var matrixDirectional = trans.GetChild(8);

            var matrixDPS = matrixDirectional.GetComponent<ParticleSystem>().main.startColor;

            matrixDPS.gradient = matrixGradient;

            var matrixDPSR = matrixDirectional.GetComponent<ParticleSystemRenderer>();

            // var newMat2 = Object.Instantiate(Paths.Material.matMageMatrixDirectionalLightning);
            // newMat2.SetColor("_TintColor", tintColor);
            // newMat2.SetTexture("_RemapTex", Main.hifuSandswept.LoadAsset<Texture2D>("texRampDirectCurrentMatrix" + trimmedName + ".png"));

            var newMat2 = new Material(Paths.Material.matMageMatrixDirectionalLightning); // the version above does not have a transparent bg so it looks weird in-game
            newMat2.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone2);
            newMat2.SetColor("_TintColor", lightGrayEquivalent);

            matrixDPSR.material = newMat2;

            var flashDirectional = trans.GetChild(9).GetComponent<ParticleSystem>().main.startColor;

            flashDirectional.gradient = matrixGradient;

            var pointLight = trans.GetChild(11).GetComponent<Light>();
            pointLight.color = saturatedBlueEquivalent5;
            pointLight.range = 1.667f;

            var matrixBillboard = trans.GetChild(12);
            var matrixMain = matrixBillboard.GetComponent<ParticleSystem>().main;
            var matrixStartColor = matrixMain.startColor;
            matrixStartColor.color = saturatedBlueEquivalent3;

            var matrixBPSR = matrixBillboard.GetComponent<ParticleSystemRenderer>();

            var newMat3 = Object.Instantiate(Paths.Material.matMageMatrixLightning);
            newMat3.SetColor("_TintColor", tintColor);
            newMat3.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone2);

            matrixBPSR.material = newMat3;

            var sphereExpanding = impact.transform.Find("Sphere, Expanding");
            var sphereExpandingMain = sphereExpanding.GetComponent<ParticleSystem>().main;
            var sphereExpandingStartColor = sphereExpandingMain.startColor;
            sphereExpandingStartColor.color = lightAquaEquivalent;

            var sphereExpandingPSR = sphereExpanding.GetComponent<ParticleSystemRenderer>();

            var newMat = Object.Instantiate(Paths.Material.matLightningSphere);

            newMat.SetColor("_TintColor", darkGreenEquivalent);
            newMat.SetTexture("_RemapTex", Main.hifuSandswept.LoadAsset<Texture2D>("texRampDirectCurrentImpact" + trimmedName + ".png"));
            newMat.SetFloat("_AlphaBoost", alphaBoost);
            newMat.SetFloat("_AlphaBias", alphaBias);

            sphereExpandingPSR.material = newMat;

            for (int i = 0; i < impact.transform.childCount; i++)
            {
                var trans2 = impact.transform.GetChild(i);
                if (i >= 12)
                {
                    continue;
                }
                trans2.localScale *= 0.33f;
            }

            matrixBillboard.localScale = Vector3.one;
            matrixMain.scalingMode = ParticleSystemScalingMode.Hierarchy; // dont use other garbage or I will kill you
            sphereExpanding.localScale = Vector3.one;
            sphereExpandingMain.scalingMode = ParticleSystemScalingMode.Hierarchy;

            // so the scaling is very much fucked and garbage, but I fixed it

            ContentAddition.AddEffect(impact);
            return impact;
        }

        public static GameObject CreateGhostRecolor(string name, Color32 saturatedAquaEquivalent, Color32 saturatedBlueEquivalent, Color32 mutedAquaEquivalent)
        {
            // saturatedAquaEquivalent = new Color32(0, 255, 167, 255);
            // saturatedBlueEquivalent = new Color32(0,141,197,255);
            // mutedAquaEquivalent = new Color32(111,170,151,154);

            var trimmedName = name.Replace(" ", "");

            var ghost = Paths.GameObject.LunarSunProjectileGhost.InstantiateClone("Direct Current Ghost " + name, false);

            Main.ModLogger.LogDebug(ghost);
            Main.ModLogger.LogDebug(ghost.transform);
            Main.ModLogger.LogDebug(ghost.transform.GetChild(0));

            var ramp = Main.hifuSandswept.LoadAsset<Texture2D>("texRampDirectCurrent" + trimmedName + ".png");
            var fresnel = Main.hifuSandswept.LoadAsset<Texture2D>("texRampDirectCurrentFresnel" + trimmedName + ".png");

            var green = saturatedAquaEquivalent;

            var mdl = ghost.transform.GetChild(0);
            var objectScaleCurve = mdl.GetComponent<ObjectScaleCurve>();
            objectScaleCurve.overallCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.15f, 0.75f), new Keyframe(1f, 2.5f));

            var backdrop = mdl.GetChild(0).GetComponent<ParticleSystemRenderer>();

            var newBackdropMat = Object.Instantiate(Paths.Material.matLunarSunProjectileBackdrop);

            newBackdropMat.SetTexture("_RemapTex", ramp);
            newBackdropMat.SetInt("_Cull", 1); // used to appear as a white square behind terrain so I fixed it

            backdrop.material = newBackdropMat;

            var quad = mdl.GetChild(1).GetComponent<MeshRenderer>();

            VFXUtils.AddLight(quad.gameObject, saturatedAquaEquivalent, 1.5f, 13f, 5f);

            quad.transform.localScale = Vector3.one;
            var setRandomScale = quad.GetComponent<SetRandomScale>();
            setRandomScale.minimumScale = 0.75f;
            setRandomScale.maximumScale = 1f;

            var newQuadMat = new Material(Paths.Material.matLunarSunProjectile);

            newQuadMat.SetColor("_Color", Color.white);
            newQuadMat.SetColor("_EmColor", green); // 0, 187, 255, 255
            newQuadMat.SetTexture("_FresnelRamp", fresnel);
            newQuadMat.SetFloat("_EmPower", 3f);
            newQuadMat.SetFloat("_FresnelPower", 1f);
            newQuadMat.SetFloat("_FresnelBoost", 20f);

            quad.material = newQuadMat;

            var particles = ghost.transform.GetChild(1);

            var closeParticles = particles.GetChild(0).GetComponent<ParticleSystem>().main.startColor;
            closeParticles.colorMin = green;
            closeParticles.colorMax = saturatedBlueEquivalent;

            var distantParticles = particles.GetChild(1).GetComponent<ParticleSystem>().main.startColor;
            distantParticles.color = green;

            var pop = particles.GetChild(2).GetComponent<ParticleSystem>().main.startColor;
            pop.color = green;

            var trail = particles.GetChild(3).GetComponent<TrailRenderer>();

            VFXUtils.AddLight(trail.gameObject, saturatedAquaEquivalent, 1f, 26f, 5f);

            trail.startWidth = 0.6f;
            trail.endWidth = 0.25f;
            trail.widthMultiplier = 0.5f;
            trail.time = 0.2f;

            var newTrailMat = Object.Instantiate(Paths.Material.matLunarSunProjectileTrail);

            newTrailMat.SetTexture("_RemapTex", ramp);
            newTrailMat.SetFloat("_Boost", 7f);
            newTrailMat.SetFloat("_AlphaBoost", 4.710526f);
            newTrailMat.SetFloat("_AlphaBias", 0.3349282f);
            newTrailMat.SetTexture("_MainTex", Paths.Texture2D.texAlphaGradient1);
            newTrailMat.SetColor("_TintColor", mutedAquaEquivalent);

            trail.material = newTrailMat;

            ghost.AddComponent<DetachTrailOnDestroy>().targetTrailRenderers = [trail];

            var onSpawnPopParticle = particles.Find("OnSpawnPopParticle").gameObject;
            VFXUtils.RecolorMaterialsAndLights(onSpawnPopParticle, mutedAquaEquivalent, mutedAquaEquivalent, true);
            VFXUtils.MultiplyDuration(onSpawnPopParticle, 1.4f);
            VFXUtils.MultiplyScale(onSpawnPopParticle, 1.25f);

            return ghost;
        }
    }
}