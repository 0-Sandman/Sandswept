using RoR2.UI;
using System.Collections;

namespace Sandswept.Utils
{
    public class VFXUtils
    {
        public static void RecolorMaterialsAndLights(GameObject gameObject, Color32 primaryColor, Color32 emissionAndLightColor, bool fuckRamps)
        {
            // for itself
            RecolorMaterialsAndLightsInternal(gameObject, primaryColor, emissionAndLightColor, fuckRamps);

            // and for all children recursively
            foreach (Transform child in gameObject.transform.GetComponentsInChildren<Transform>())
            {
                RecolorMaterialsAndLightsInternal(child.gameObject, primaryColor, emissionAndLightColor, fuckRamps);
            }
        }

        public static void MultiplyScale(GameObject gameObject, float scaleMultiplier)
        {
            // for itself
            MultiplyScaleInternal(gameObject, scaleMultiplier);

            // and for all children recursively
            foreach (Transform child in gameObject.transform.GetComponentsInChildren<Transform>())
            {
                MultiplyScaleInternal(child.gameObject, scaleMultiplier);
            }
        }

        private static void MultiplyScaleInternal(GameObject gameObject, float scaleMultiplier)
        {
            if (gameObject.GetComponent<ParticleSystem>() == null && gameObject.GetComponent<LineRenderer>() == null)
            {
                gameObject.transform.localScale *= scaleMultiplier;
            }

            foreach (ParticleSystem particleSystem in gameObject.GetComponents<ParticleSystem>())
            {
                var particleSystemMain = particleSystem.main;
                var particleSystemStartSize = particleSystemMain.startSize;
                float finalSize = particleSystemStartSize.constant;
                if (particleSystemStartSize.mode == ParticleSystemCurveMode.TwoConstants)
                {
                    finalSize = (particleSystemStartSize.constantMin + particleSystemStartSize.constantMax) / 2f;
                }
                particleSystemStartSize.mode = ParticleSystemCurveMode.Constant;
                particleSystemStartSize.constant = finalSize * scaleMultiplier;
            }
            foreach (LineRenderer lineRenderer in gameObject.GetComponents<LineRenderer>())
            {
                lineRenderer.startWidth *= scaleMultiplier;
                lineRenderer.endWidth *= scaleMultiplier;
            }
            /*
            foreach (Tracer tracer in gameObject.GetComponents<Tracer>())
            {
                tracer.beamDensity *= scaleMultiplier;
            }
            */
        }

        private static void RecolorMaterialsAndLightsInternal(GameObject gameObject, Color32 primaryColor, Color32 emissionAndLightColor, bool convertToGrayscaleColors)
        {
            foreach (ParticleSystem particleSystem in gameObject.GetComponents<ParticleSystem>())
            {
                var particleSystemMain = particleSystem.main;
                var particleSystemStartColor = particleSystemMain.startColor;
                particleSystemStartColor.mode = ParticleSystemGradientMode.Color;
                particleSystemStartColor.color = primaryColor;

                var particleSystemColorOverLifetime = particleSystem.colorOverLifetime;
                if (particleSystemColorOverLifetime.color.gradient != null && particleSystemColorOverLifetime.color.gradient.colorKeys.Length > 0)
                {
                    for (int i = 0; i < particleSystemColorOverLifetime.color.gradient.colorKeys.Length; i++)
                    {
                        var particleSystemColorKey = particleSystemColorOverLifetime.color.gradient.colorKeys[i];
                        particleSystemColorKey.color = Color.white;
                    }
                }
            }

            foreach (Renderer renderer in gameObject.GetComponents<Renderer>())
            {
                foreach (Material material in renderer.materials)
                {
                    switch (material.shader.name)
                    {
                        case "Hopoo Games/FX/Cloud Remap":
                        case "Hopoo Games/FX/Cloud Intersection Remap":
                            material.SetColor("_TintColor", primaryColor);
                            material.SetColor("_EmissionColor", emissionAndLightColor);
                            if (convertToGrayscaleColors)
                            {
                                material.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
                            }
                            break;

                        case "Hopoo Games/Deferred/Standard":
                            material.SetColor("_Color", primaryColor);
                            material.SetColor("_EmColor", emissionAndLightColor);
                            break;
                    }
                }
            }

            foreach (LineRenderer lineRenderer in gameObject.GetComponents<LineRenderer>())
            {
                lineRenderer.startColor = primaryColor;
                lineRenderer.endColor = emissionAndLightColor;
            }

            foreach (Light light in gameObject.GetComponents<Light>())
            {
                light.color = emissionAndLightColor;
            }
        }

        public static void MultiplyDuration(GameObject gameObject, float durationMultiplier, float maximumDuration = -999f)
        {
            // for itself
            MultiplyDurationInternal(gameObject, durationMultiplier, maximumDuration);

            // and for all children recursively
            foreach (Transform child in gameObject.transform.GetComponentsInChildren<Transform>())
            {
                MultiplyDurationInternal(child.gameObject, durationMultiplier, maximumDuration);
            }
        }

        private static void MultiplyDurationInternal(GameObject gameObject, float durationMultiplier, float maximumDuration = -999f)
        {
            foreach (ParticleSystem particleSystem in gameObject.GetComponents<ParticleSystem>())
            {
                var particleSystemMain = particleSystem.main;
                var particleSystemLifetime = particleSystemMain.startLifetime;
                float finalDuration = particleSystemLifetime.constant;
                if (particleSystemLifetime.mode == ParticleSystemCurveMode.TwoConstants)
                {
                    finalDuration = (particleSystemLifetime.constantMin + particleSystemLifetime.constantMax) / 2f;
                }
                particleSystemLifetime.mode = ParticleSystemCurveMode.Constant;
                particleSystemLifetime.constant = finalDuration * durationMultiplier;
            }

            foreach (DestroyOnTimer destroyOnTimer in gameObject.GetComponents<DestroyOnTimer>())
            {
                destroyOnTimer.duration *= durationMultiplier;
            }

            foreach (AnimateShaderAlpha animateShaderAlpha in gameObject.GetComponents<AnimateShaderAlpha>())
            {
                animateShaderAlpha.timeMax *= durationMultiplier;
                if (animateShaderAlpha.GetComponent<DestroyOnTimer>() != null)
                {
                    animateShaderAlpha.timeMax = Mathf.Min(animateShaderAlpha.timeMax, animateShaderAlpha.GetComponent<DestroyOnTimer>().duration);
                }
                if (maximumDuration > 0f)
                {
                    animateShaderAlpha.timeMax = maximumDuration;
                }
            }

            foreach (PostProcessDuration postProcessDuration in gameObject.GetComponents<PostProcessDuration>())
            {
                postProcessDuration.maxDuration *= durationMultiplier;
            }

            foreach (ObjectScaleCurve objectScaleCurve in gameObject.GetComponents<ObjectScaleCurve>())
            {
                objectScaleCurve.timeMax *= durationMultiplier;
            }

            foreach (AnimateUIAlpha animateUIAlpha in gameObject.GetComponents<AnimateUIAlpha>())
            {
                animateUIAlpha.timeMax *= durationMultiplier;
            }

            foreach (AnimateImageAlpha animateImageAlpha in gameObject.GetComponents<AnimateImageAlpha>())
            {
                animateImageAlpha.timeMax *= durationMultiplier;
            }

            foreach (LightIntensityCurve lightIntensityCurve in gameObject.GetComponents<LightIntensityCurve>())
            {
                lightIntensityCurve.timeMax *= durationMultiplier;
            }
        }
    }

    public class TracerComponentSucks : MonoBehaviour
    {
        public Tracer tracer;

        public void OnEnable()
        {
            tracer = GetComponent<Tracer>();
        }

        public void LateUpdate()
        {
            tracer.distanceTraveled = Mathf.Min(tracer.distanceTraveled, tracer.totalDistance - 1f);
        }
    }
}