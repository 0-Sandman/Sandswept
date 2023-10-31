using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Skills.Ranger.VFX
{
    public static class DirectCurrentVFX
    {
        public static GameObject ghostPrefab;

        public static void Init()
        {
            ghostPrefab = PrefabAPI.InstantiateClone(Assets.GameObject.TracerCommandoShotgun, "Direct Current Ghost", false);

            var projectileGhostControlller = ghostPrefab.AddComponent<ProjectileGhostController>();
            projectileGhostControlller.authorityTransform = ghostPrefab.transform;

            var vfxAttributes = ghostPrefab.AddComponent<VFXAttributes>();
            vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Always;
            vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Medium;

            var tracer = ghostPrefab.GetComponent<Tracer>();
            // Main.ModLogger.LogError("tracer is " + tracer); exists
            tracer.length = 16f; // 14 vaniller
            tracer.speed = 140f; // 160 vaniller, 140 to be accurate with projectile speed

            // ghostPrefab.RemoveComponent<EffectComponent>();

            var effectComponent = ghostPrefab.GetComponent<EffectComponent>();
            // Main.ModLogger.LogError("effect component is " + effectComponent); exists
            effectComponent.soundName = "Play_wHeavyShoot1";
            effectComponent.effectData = new EffectData() { origin = ghostPrefab.transform.position };

            var lineRenderer = ghostPrefab.GetComponent<LineRenderer>();

            var geenGradient = new Gradient();

            var alphas = new GradientAlphaKey[1];
            alphas[0] = new GradientAlphaKey(1.0f, 0.0f);

            var colors = new GradientColorKey[3];
            colors[0] = new GradientColorKey(new Color32(0, 115, 82, 255), 0f);
            colors[1] = new GradientColorKey(new Color32(0, 255, 230, 255), 0.912f);
            colors[2] = new GradientColorKey(Color.white, 1f);

            geenGradient.SetKeys(colors, alphas);

            lineRenderer.colorGradient = geenGradient;

            var newMat = Object.Instantiate(Assets.Material.matCommandoShotgunTracerCore);
            newMat.SetColor("_TintColor", new Color32(0, 255, 195, 255));
            newMat.SetFloat("_Boost", 4.77f);

            lineRenderer.material = newMat;

            ContentAddition.AddEffect(ghostPrefab);
        }
    }
}