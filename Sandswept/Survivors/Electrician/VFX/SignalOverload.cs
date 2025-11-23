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
        public static GameObject indicatorDefault;
        public static GameObject indicatorCovenant;

        public static Material matShieldBreakDefault;
        public static Material matShieldBreakCovenant;

        public static GameObject beamDefault;
        public static GameObject beamCovenant;

        public static GameObject impactDefault;
        public static GameObject impactCovenant;

        public static void Init()
        {
            indicatorDefault = CreateIndicatorRecolor("Default", new Color32(0, 77, 255, 255), new Color32(0, 42, 255, 255), new Color32(140, 69, 0, 255), new Color32(0, 77, 255, 255));
            indicatorCovenant = CreateIndicatorRecolor("Covenant", new Color32(255, 179, 0, 255), new Color32(255, 150, 0, 255), new Color32(13, 0, 42, 255), new Color32(0, 0, 255, 255));

            matShieldBreakDefault = CreateOverlayRecolor(new Color32(0, 77, 255, 255));
            matShieldBreakCovenant = CreateOverlayRecolor(new Color32(0, 0, 255, 255));

            beamDefault = CreateBeamRecolor("Default", new Color32(0, 42, 255, 255), new Color32(0, 77, 255, 255));
            beamCovenant = CreateBeamRecolor("Covenant", new Color32(82, 0, 255, 255), new Color32(141, 87, 255, 255));

            impactDefault = VFX.GalvanicBolt.CreateImpactRecolor("Signal Overload Version Default", new Color32(0, 77, 255, 255), new Color32(0, 42, 255, 255));
            impactCovenant = VFX.GalvanicBolt.CreateImpactRecolor("Signal Overload Version Covenant", new Color32(141, 87, 255, 255), new Color32(82, 0, 255, 255));
        }

        public static GameObject CreateBeamRecolor(string name, Color32 beamColor, Color32 sparksColor)
        {
            var beam = PrefabAPI.InstantiateClone(Main.assets.LoadAsset<GameObject>("ElectricianChargeBeam.prefab"), "Signal Overload Beam VFX " + name, false);

            var beamLR = beam.GetComponent<LineRenderer>();
            beamLR.material = Main.lineRendererBase;
            beamLR.startColor = beamColor;
            beamLR.endColor = beamColor;
            // beamLR.textureMode = LineTextureMode.Tile;

            var transform = beam.transform;

            var laser = transform.Find("LaserTitan 3 (1)");
            var flare = laser.Find("Start/Flare (1)");
            var flarePSR = flare.GetComponent<ParticleSystemRenderer>();

            var newFlareMaterial = new Material(flarePSR.material);
            newFlareMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newFlareMaterial.SetFloat("_AlphaBias", 0.1f);
            newFlareMaterial.SetFloat("_AlphaBoost", 5f);
            newFlareMaterial.SetColor("_TintColor", beamColor);

            flarePSR.material = newFlareMaterial;

            var flare2 = flare.Find("Flare (3)").GetComponent<ParticleSystemRenderer>();

            var newFlare2Material = new Material(flare2.sharedMaterials[0]);
            newFlare2Material.SetColor("_TintColor", sparksColor);
            newFlare2Material.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newFlare2Material.SetFloat("_Boost", 1.21542f);
            newFlare2Material.SetFloat("_AlphaBoost", 0.7732428f);
            newFlare2Material.SetFloat("_AlphaBias", 0f);

            var newFlare2Materials = new Material[2] { newFlare2Material, newFlare2Material };

            flare2.sharedMaterials = newFlare2Materials;

            var newSparksMaterial = new Material(newFlare2Material);
            newSparksMaterial.SetTexture("_MainTex", Paths.Texture2D.texLightning2Mask);
            newSparksMaterial.SetTextureScale("_MainTex", new Vector2(2f, 1f));
            newSparksMaterial.SetFloat("_AlphaBoost", 1.5f);

            var sparksWiggly1 = flare.Find("Sparks,Wiggly").GetComponent<ParticleSystemRenderer>();
            sparksWiggly1.material = newSparksMaterial;

            var fire = laser.Find("End/EndEffect/Particles/Fire");
            var firePSR = fire.GetComponent<ParticleSystemRenderer>();

            firePSR.material = newFlareMaterial;

            var fireElectric = fire.Find("Fire, Electric").GetComponent<ParticleSystemRenderer>();
            fireElectric.material = newFlareMaterial;

            var sparksWiggly2 = fire.Find("Sparks,Wiggly").GetComponent<ParticleSystemRenderer>();
            sparksWiggly2.material = newFlare2Material;

            return beam;
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

        public static GameObject CreateIndicatorRecolor(string name, Color32 sphereFillColor, Color32 sphereOutlineColor, Color32 hugeSphereColor, Color32 lightColor)
        {
            var tempestSphereIndicator = PrefabAPI.InstantiateClone(Paths.GameObject.VagrantNovaAreaIndicator, "Signal Overload Base Indicator " + name, false);
            // tempestSphereIndicator.GetComponentInChildren<ParticleSystemRenderer>().gameObject.SetActive(false);
            tempestSphereIndicator.transform.Find("Particle System").gameObject.SetActive(false);

            var pointLight = tempestSphereIndicator.transform.Find("Point Light").GetComponent<Light>();
            pointLight.color = lightColor;
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

            var signalOverloadIndicator = PrefabAPI.InstantiateClone(tempestSphereIndicator, "Signal Overload Huge Indicator " + name, false);

            var newIndicatorMaterial = new Material(Paths.Material.matLightningSphere);
            newIndicatorMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritoneSmoothed);
            newIndicatorMaterial.SetFloat("_InvFade", 1f);
            newIndicatorMaterial.SetFloat("_SoftPower", 0.85f);
            newIndicatorMaterial.SetFloat("_Boost", 1.718147f);
            newIndicatorMaterial.SetFloat("_RimPower", 8.398069f);
            newIndicatorMaterial.SetFloat("_RimStrength", 5f);
            newIndicatorMaterial.SetFloat("_AlphaBoost", 1f);
            newIndicatorMaterial.SetFloat("_IntersectionStrength", 20f);
            newIndicatorMaterial.SetTexture("_Cloud1Tex", Paths.Texture2D.texCloudWaterFoam1);
            newIndicatorMaterial.SetTextureScale("_Cloud1Tex", Vector2.one * 2f);
            newIndicatorMaterial.SetColor("_TintColor", hugeSphereColor);

            var hugeIndicatorMaterials = new Material[2] { newIndicatorMaterial, newIndicatorMaterial };

            var hugeIndicatorMeshRenderer = signalOverloadIndicator.GetComponent<MeshRenderer>();
            hugeIndicatorMeshRenderer.sharedMaterials = hugeIndicatorMaterials;

            var indicatorPointLight = signalOverloadIndicator.transform.Find("Point Light").GetComponent<Light>();
            indicatorPointLight.GetComponent<LightIntensityCurve>().enabled = false;
            indicatorPointLight.GetComponent<LightScaleFromParent>().enabled = false;
            indicatorPointLight.color = lightColor;
            indicatorPointLight.intensity = 30f;
            indicatorPointLight.range = 20f;

            // ContentAddition.AddEffect(signalOverloadIndicator);

            return signalOverloadIndicator;
        }
    }
}
