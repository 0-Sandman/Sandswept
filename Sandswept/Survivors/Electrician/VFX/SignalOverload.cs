using System;
using System.Collections;
using System.Linq;
using EntityStates.Chef;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Sandswept.Survivors.Electrician.Achievements;
using Sandswept.Survivors.Electrician.Skills;
using Sandswept.Survivors.Electrician.States;
using Sandswept.Utils.Components;
using UnityEngine.SceneManagement;
using RoR2.Stats;
using UnityEngine;

namespace Sandswept.Survivors.Electrician.VFX
{
    public class SignalOverload
    {
        public static GameObject signalOverloadIndicatorDefault;
        public static GameObject signalOverloadIndicatorCovenant;

        public static Material matShieldBreakDefault;
        public static Material matShieldBreakCovenant;


        public static void Init()
        {
            signalOverloadIndicatorDefault = CreateIndicatorRecolor("Default", new Color32(255, 179, 0, 255), new Color32(255, 150, 0, 255));
            signalOverloadIndicatorCovenant = CreateIndicatorRecolor("Covenant", new Color32(255, 179, 0, 255), new Color32(255, 150, 0, 255));

            matShieldBreakDefault = CreateOverlayRecolor(new Color32(0, 77, 255, 255));
            matShieldBreakCovenant = CreateOverlayRecolor(new Color32(0, 0, 255, 255));
        }

        public static Material CreateOverlayRecolor(Color32 color)
        {
            var mat = Object.Instantiate(Paths.Material.matHuntressFlashExpanded);
            mat.SetFloat("_InvFade", 1f);
            mat.SetFloat("_Boost", 1.5f);
            mat.SetFloat("_AlphaBoost", 2f);
            mat.SetFloat("_AlphaBias", 0f);
            mat.SetInt("_Cull", 0);
            mat.SetColor("_TintColor", color);

            return mat;
        }

        public static GameObject CreateIndicatorRecolor(string name, Color32 sphereFillColor, Color32 sphereOutlineColor)
        {
            var tempestSphereIndicator = PrefabAPI.InstantiateClone(Paths.GameObject.VagrantNovaAreaIndicator, "Signal Overload Base Indicator", false);
            // tempestSphereIndicator.GetComponentInChildren<ParticleSystemRenderer>().gameObject.SetActive(false);
            tempestSphereIndicator.transform.Find("Particle System").gameObject.SetActive(false);

            var pointLight = tempestSphereIndicator.transform.Find("Point Light").GetComponent<Light>();
            pointLight.color = sphereOutlineColor;
            pointLight.intensity = 100f;
            pointLight.GetComponent<LightIntensityCurve>().enabled = false;
            pointLight.GetComponent<LightScaleFromParent>().enabled = false;
            pointLight.range = 20f;

            var newSphereFillMaterial = new Material(Paths.Material.matWarbannerSphereIndicator);
            newSphereFillMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone3);
            newSphereFillMaterial.SetColor("_TintColor", sphereFillColor);
            newSphereFillMaterial.SetFloat("_InvFade", 30f);
            newSphereFillMaterial.SetFloat("_SoftPower", 0.1f);
            newSphereFillMaterial.SetFloat("_Boost", 0.8f);
            newSphereFillMaterial.SetFloat("_RimPower", 4.405039f);
            newSphereFillMaterial.SetFloat("_RimStrength", 2.607298f);
            newSphereFillMaterial.SetFloat("_AlphaBoost", 2.607298f);
            newSphereFillMaterial.SetFloat("_IntersectionStrength", 1.553762f);

            var newSphereOutlineMaterial = new Material(Paths.Material.matLightningSphere);
            newSphereOutlineMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone3);
            newSphereOutlineMaterial.SetColor("_TintColor", sphereOutlineColor);
            newSphereOutlineMaterial.SetFloat("_InvFade", 1f);
            newSphereOutlineMaterial.SetFloat("_SoftPower", 0.85f);
            newSphereOutlineMaterial.SetFloat("_Boost", 2f);
            newSphereOutlineMaterial.SetFloat("_RimPower", 3.17f);
            newSphereOutlineMaterial.SetFloat("_RimStrength", 0.23f);
            newSphereOutlineMaterial.SetFloat("_AlphaBoost", 1f);
            newSphereOutlineMaterial.SetFloat("_IntersectionStrength", 6.4f);

            var newTempestSphereMaterials = new Material[2] { newSphereFillMaterial, newSphereOutlineMaterial };

            tempestSphereIndicator.GetComponent<MeshRenderer>().sharedMaterials = newTempestSphereMaterials;

            // tempestSphereIndicator.RemoveComponent<ObjectScaleCurve>();
            tempestSphereIndicator.GetComponent<ObjectScaleCurve>().enabled = false;

            // yield return new WaitForSeconds(0.1f);
            tempestSphereIndicator.transform.localScale = new(14f, 14f, 14f);
            // tempestSphereIndicator.RemoveComponent<AnimateShaderAlpha>();
            tempestSphereIndicator.GetComponent<AnimateShaderAlpha>().enabled = false;

            var signalOverloadIndicator = PrefabAPI.InstantiateClone(tempestSphereIndicator, "Signal Overload Indicator " + name, false);
            signalOverloadIndicator.GetComponent<MeshRenderer>().sharedMaterials = new Material[]
            {
                Paths.Material.matLightningSphere
            };

            ContentAddition.AddEffect(signalOverloadIndicator);

            return signalOverloadIndicator;
        }
    }
}
