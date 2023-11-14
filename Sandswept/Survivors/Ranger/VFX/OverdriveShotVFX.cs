using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.VFX
{
    public static class OverdriveShotVFX
    {
        public static GameObject tracerPrefab;

        public static void Init()
        {
            tracerPrefab = Assets.GameObject.TracerCommandoShotgun.InstantiateClone("Enflame Shot Tracer", false);

            var tracer = tracerPrefab.GetComponent<Tracer>();
            tracer.length = 16f; // 14
            tracer.speed = 240f; // 160

            var effectComponent = tracerPrefab.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_wHeavyShoot1";

            var lineRenderer = tracerPrefab.GetComponent<LineRenderer>();

            var geenGradient = new Gradient();

            var alphas = new GradientAlphaKey[1];
            alphas[0] = new GradientAlphaKey(1.0f, 0.0f);

            var colors = new GradientColorKey[3];
            colors[0] = new GradientColorKey(new Color32(115, 38, 0, 255), 0f);
            colors[1] = new GradientColorKey(new Color32(255, 132, 0, 255), 0.912f);
            colors[2] = new GradientColorKey(Color.white, 1f);

            geenGradient.SetKeys(colors, alphas);

            lineRenderer.colorGradient = geenGradient;

            var newMat = Object.Instantiate(Assets.Material.matCommandoShotgunTracerCore);
            newMat.SetColor("_TintColor", new Color32(255, 146, 0, 255));
            newMat.SetFloat("_Boost", 6.071373f);
            newMat.SetFloat("_AlphaBoost", 1.116706f);
            newMat.SetFloat("_AlphaBias", 0.08291277f);
            newMat.SetTexture("_RemapTex", Assets.Texture2D.texRampBandit);

            lineRenderer.material = newMat;

            ContentAddition.AddEffect(tracerPrefab);
        }
    }
}