using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.VFX
{
    public static class EnflameVFX
    {
        public static GameObject tracerPrefabDefault;
        public static GameObject tracerHeatedPrefabDefault;

        public static GameObject tracerPrefabMajor;
        public static GameObject tracerHeatedPrefabMajor;

        public static GameObject tracerPrefabRenegade;
        public static GameObject tracerHeatedPrefabRenegade;

        public static GameObject tracerPrefabMileZero;
        public static GameObject tracerHeatedPrefabMileZero;

        public static GameObject tracerPrefabRacecar;
        public static GameObject tracerHeatedPrefabRacecar;

        public static GameObject tracerPrefabSandswept;
        public static GameObject tracerHeatedPrefabSandswept;

        public static void Init()
        {
            tracerPrefabDefault = CreateTracerRecolor("Default", new Color32(255, 27, 0, 255));
            tracerHeatedPrefabDefault = CreateTracerRecolor("Default", new Color32(255, 7, 0, 255), 9f, 4f, 10f, 1.2f, 0.65f, true);

            tracerPrefabMajor = CreateTracerRecolor("Major", new Color32(22, 28, 56, 255));
            tracerHeatedPrefabMajor = CreateTracerRecolor("Major", new Color32(18, 39, 113, 255), 9f, 4f, 10f, 1.2f, 0.65f, true);

            tracerPrefabRenegade = CreateTracerRecolor("Renegade", new Color32(144, 25, 68, 255));
            tracerHeatedPrefabRenegade = CreateTracerRecolor("Renegade", new Color32(152, 30, 141, 255), 9f, 4f, 10f, 1.2f, 0.65f, true);

            tracerPrefabMileZero = CreateTracerRecolor("Mile Zero", new Color32(64, 0, 0, 255));
            tracerHeatedPrefabMileZero = CreateTracerRecolor("Mile Zero", new Color32(19, 0, 0, 255), 9f, 4f, 10f, 1.2f, 0.65f, true);

            tracerPrefabRacecar = CreateTracerRecolor("Racecar", new Color32(25, 144, 129, 255));
            tracerHeatedPrefabRacecar = CreateTracerRecolor("Racecar", new Color32(118, 202, 205, 255), 9f, 4f, 10f, 1.2f, 0.65f, true);

            tracerPrefabSandswept = CreateTracerRecolor("Sandswept", new Color32(255, 132, 0, 255));
            tracerHeatedPrefabSandswept = CreateTracerRecolor("Sandswept", new Color32(255, 172, 87, 255), 9f, 4f, 10f, 1.2f, 0.65f, true);
        }

        public static GameObject CreateTracerRecolor(string name, Color32 whiteEquivalent, float length = 6f, float widthMultiplier = 3f, float brightnessBoost = 10f, float alphaBoost = 1.2f, float alphaBias = 0.65f, bool heated = false)
        {
            // whiteEquivalent = new Color32(255,255,255,255);
            var tracer = Paths.GameObject.TracerCommandoShotgun.InstantiateClone((heated ? "Heated " : "") + "Enflame Tracer " + name, false);

            var tracerComponent = tracer.GetComponent<Tracer>();
            tracerComponent.length = length;
            tracerComponent.speed = 240f;

            var effectComponent = tracer.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_wHeavyShoot1";

            tracer.GetOrAddComponent<VFXAttributes>((x) =>
            {
                x.DoNotCullPool = true;
                x.DoNotPool = true;
                x.vfxPriority = VFXAttributes.VFXPriority.Always;
            });

            var lineRenderer = tracer.GetComponent<LineRenderer>();
            lineRenderer.widthMultiplier = widthMultiplier;
            lineRenderer.numCapVertices = 5;
            lineRenderer.numCornerVertices = 5;
            /*
            var geenGradient = new Gradient();

            var alphas = new GradientAlphaKey[1];
            alphas[0] = new GradientAlphaKey(1.0f, 0.0f);

            var colors = new GradientColorKey[3];
            colors[0] = new GradientColorKey(new Color32(115, 38, 0, 255), 0f);
            colors[1] = new GradientColorKey(new Color32(255, 132, 0, 255), 0.912f);
            colors[2] = new GradientColorKey(Color.white, 1f);

            geenGradient.SetKeys(colors, alphas);

            lineRenderer.colorGradient = geenGradient;
            */
            /*
            var newMat = Object.Instantiate();
            newMat.SetColor("_TintColor", new Color32(255, 146, 0, 255));
            newMat.SetFloat("_Boost", 6.071373f);
            newMat.SetFloat("_AlphaBoost", 1.116706f);
            newMat.SetFloat("_AlphaBias", 0.08291277f);
            newMat.SetTexture("_RemapTex", Paths.Texture2D.texRampBandit);
            */

            var gradient = new Gradient();

            var alphas = new GradientAlphaKey[2];
            alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
            alphas[1] = new GradientAlphaKey(0.2f, 1f);

            var colors = new GradientColorKey[2];
            colors[0] = new GradientColorKey(new Color32(101, 52, 255, 255), 0f);
            colors[1] = new GradientColorKey(new Color32(255, 255, 255, 51), 1f);

            gradient.SetKeys(colors, alphas);

            lineRenderer.colorGradient = gradient;

            var newMat = new Material(Paths.Material.matBandit2TracerTrail);
            newMat.SetColor("_TintColor", whiteEquivalent);
            newMat.SetTexture("_RemapTex", Main.hifuSandswept.LoadAsset<Texture2D>("texRampEnflame.png"));
            newMat.SetFloat("_Boost", brightnessBoost);
            newMat.SetFloat("_AlphaBoost", alphaBoost);
            newMat.SetFloat("_AlphaBias", alphaBias);
            newMat.SetFloat("_DepthOffset", -10f);
            // newMat.SetTexture("_MainTex", Paths.Texture2D.texMageLaserMask);

            lineRenderer.material = newMat;

            ContentAddition.AddEffect(tracer);

            return tracer;
        }
    }
}