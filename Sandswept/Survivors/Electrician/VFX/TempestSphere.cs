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
    public class TempestSphere : MonoBehaviour
    {

        public static GameObject CreateProjectileRecolor(string name, Color32 beamStartColor, Color32 beamEndColor, Color32 sphereFillColor, Color32 sphereOutlineColor, Color32 smallSphereFillColor)
        {
            var projectile = PrefabAPI.InstantiateClone(Main.assets.LoadAsset<GameObject>("TempestSphereProjectile.prefab"), "Tempest Sphere Projectile " + name);

            ContentAddition.AddProjectile(projectile);

            GameObject sphereVFX = new("joe sigma");
            sphereVFX.transform.position = Vector3.zero;
            sphereVFX.transform.localPosition = Vector3.zero;

            var tempestSphereIndicator = PrefabAPI.InstantiateClone(Paths.GameObject.VagrantNovaAreaIndicator, "TempestSphereIndicator", false);
            // tempestSphereIndicator.GetComponentInChildren<ParticleSystemRenderer>().gameObject.SetActive(false);
            tempestSphereIndicator.transform.Find("Particle System").gameObject.SetActive(false);

            var pointLight = tempestSphereIndicator.transform.Find("Point Light").GetComponent<Light>();
            pointLight.color = sphereOutlineColor;
            pointLight.intensity = 100f;
            pointLight.range = 16f;
            pointLight.GetComponent<LightIntensityCurve>().enabled = false;

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

            var newOrbCoreOutlineMaterial = new Material(Paths.Material.matLoaderLightningTile);
            newOrbCoreOutlineMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone3);
            newOrbCoreOutlineMaterial.SetColor("_TintColor", sphereFillColor);
            newOrbCoreOutlineMaterial.SetFloat("_InvFade", 1.3f);
            newOrbCoreOutlineMaterial.SetFloat("_Boost", 4f);
            newOrbCoreOutlineMaterial.SetFloat("_AlphaBoost", 3.67f);
            newOrbCoreOutlineMaterial.SetFloat("_AlphaBias", 0f);

            var newOrbCoreFillMaterial = new Material(Paths.Material.matJellyfishLightningSphere);
            newOrbCoreFillMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone3);
            newOrbCoreFillMaterial.SetColor("_TintColor", smallSphereFillColor);
            newOrbCoreFillMaterial.SetFloat("_InvFade", 0.8f);
            newOrbCoreFillMaterial.SetFloat("_SoftPower", 1f);
            newOrbCoreFillMaterial.SetFloat("_Boost", 1.86f);
            newOrbCoreFillMaterial.SetFloat("_RimPower", 1.5f);
            newOrbCoreFillMaterial.SetFloat("_RimStrength", 0.7f);
            newOrbCoreFillMaterial.SetFloat("_AlphaBoost", 6f);
            newOrbCoreFillMaterial.SetFloat("_AlphaBias", 2f);

            var newOrbCoreMaterials = new Material[2] { newOrbCoreOutlineMaterial, newOrbCoreFillMaterial };

            GameObject tempestOrb = PrefabAPI.InstantiateClone(Paths.GameObject.VoidSurvivorChargeMegaBlaster, "TempestOrb", false);

            tempestOrb.transform.Find("Base").gameObject.SetActive(false);
            tempestOrb.transform.Find("Base (1)").gameObject.SetActive(false);
            var tempestOrbPointLight = tempestOrb.transform.Find("Point light");
            tempestOrbPointLight.GetComponent<FlickerLight>().enabled = false;
            tempestOrbPointLight.GetComponent<LightIntensityCurve>().enabled = false;
            var tempestOrbPointLightLight = tempestOrbPointLight.GetComponent<Light>();
            tempestOrbPointLightLight.color = beamStartColor;
            tempestOrbPointLightLight.intensity = 500f;
            tempestOrbPointLightLight.range = 6f;

            var sparksIn = tempestOrb.transform.Find("Sparks, In").gameObject;
            var sparksMisc = tempestOrb.transform.Find("Sparks, Misc").gameObject;
            sparksIn.GetComponent<ParticleSystemRenderer>().sharedMaterial = Paths.Material.matLoaderCharging;
            sparksMisc.GetComponent<ParticleSystemRenderer>().sharedMaterial = Paths.Material.matIceOrbCore;

            VFXUtils.RecolorMaterialsAndLights(sparksIn, smallSphereFillColor, smallSphereFillColor, true);
            VFXUtils.RecolorMaterialsAndLights(sparksMisc, smallSphereFillColor, smallSphereFillColor, true);

            var orbCore = tempestOrb.transform.Find("OrbCore");
            orbCore.localScale = Vector3.one * 0.5f;
            orbCore.GetComponent<MeshRenderer>().sharedMaterials = newOrbCoreMaterials;

            // new Material[] { Paths.Material.matLoaderLightningTile, Paths.Material.matJellyfishLightningSphere };
            // tempestOrb.transform.RemoveComponent<ObjectScaleCurve>();
            tempestOrb.transform.GetComponent<ObjectScaleCurve>().enabled = false;
            // yield return new WaitForSeconds(0.1f);
            tempestOrb.transform.localScale = Vector3.one * 3f;

            tempestSphereIndicator.transform.parent = sphereVFX.transform;
            tempestOrb.transform.parent = sphereVFX.transform;
            sphereVFX.transform.SetParent(projectile.transform);
            tempestSphereIndicator.transform.position = Vector3.zero;
            tempestSphereIndicator.transform.localPosition = Vector3.zero;
            tempestOrb.transform.position = Vector3.zero;
            tempestOrb.transform.localPosition = Vector3.zero;

            var lineRenderer = projectile.transform.Find("LR").GetComponent<LineRenderer>();

            var newLineMaterial = new Material(Paths.Material.matLightningLongYellow);
            newLineMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newLineMaterial.SetColor("_TintColor", Color.white);
            newLineMaterial.SetFloat("_Boost", 2f);
            newLineMaterial.SetFloat("_AlphaBoost", 1.66f);
            newLineMaterial.SetFloat("_AlphaBias", 0.2f);

            // lineRenderer.startWidth = 2f;
            // lineRenderer.endWidth = 1f;
            lineRenderer.startColor = beamStartColor;
            lineRenderer.endColor = beamEndColor;
            lineRenderer.material = newLineMaterial;

            // projectile.GetComponentInChildren<LineRenderer>().sharedMaterial = Paths.Material.matLightningLongYellow;

            var detachAndCollapse = projectile.AddComponent<DetachAndCollapse>();
            detachAndCollapse.collapseTime = 0.4f;
            detachAndCollapse.target = sphereVFX.transform;

            return projectile;
        }
    }
}