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
using System.Runtime.CompilerServices;

namespace Sandswept.Survivors.Electrician.VFX
{
    public class GalvanicBolt
    {
        public static GameObject muzzleFlashDefault;
        public static GameObject muzzleFlashCovenant;

        public static GameObject projectileDefault;
        public static GameObject projectileCovenant;

        public static GameObject impactDefault;
        public static GameObject impactCovenant;

        public static void Init()
        {
            muzzleFlashDefault = CreateMuzzleFlashRecolor("Default", new Color32(255, 213, 0, 255));
            muzzleFlashCovenant = CreateMuzzleFlashRecolor("Covenant", new Color32(150, 67, 238, 255));

            projectileDefault = CreateProjectileRecolor("Default", new Color32(0, 77, 255, 255), new Color32(255, 202, 23, 255), new Color32(248, 171, 0, 255), new Color32(0, 77, 255, 255), new Color32(255, 191, 0, 255));
            projectileCovenant = CreateProjectileRecolor("Covenant", new Color32(98, 28, 113, 255), new Color32(119, 106, 230, 255), new Color32(223, 31, 208, 255), new Color32(255, 0, 230, 255), new Color32(0, 0, 255, 255));

            impactDefault = CreateImpactRecolor("Default", new Color32(255, 213, 0, 255), new Color32(255, 213, 0, 255));
            impactCovenant = CreateImpactRecolor("Covenant", new Color32(98, 28, 113, 255), new Color32(120, 70, 255, 255));
        }

        public static GameObject CreateMuzzleFlashRecolor(string name, Color32 sparksColor)
        {
            var muzzleFlash = Main.assets.LoadAsset<GameObject>("ElectricinMuzzleFlash.prefab").InstantiateClone("Galvanic Bolt Muzzle Flash " + name, false);

            var transform = muzzleFlash.transform;
            var sparks = transform.Find("Particles, Sparks").GetComponent<ParticleSystemRenderer>();

            var newSparksMaterial = new Material(sparks.sharedMaterials[0]);
            newSparksMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newSparksMaterial.SetColor("_TintColor", sparksColor);
            newSparksMaterial.SetFloat("_Boost", 5.534081f);
            newSparksMaterial.SetFloat("_AlphaBoost", 0.068f);
            newSparksMaterial.SetFloat("_AlphaBias", 0.2367508f);

            VFXUtils.MultiplyScale(sparks.gameObject, 1.2f);
            VFXUtils.MultiplyDuration(sparks.gameObject, 1.5f);

            var newSparksMaterials = new Material[2] { newSparksMaterial, newSparksMaterial };
            sparks.materials = newSparksMaterials;

            var flashes = transform.Find("Particles, Flashes").GetComponent<ParticleSystemRenderer>();

            var newFlashesMaterial = new Material(flashes.material);
            newFlashesMaterial.SetTexture("_MainTex", Paths.Texture2D.texRocketFlare);
            newFlashesMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newFlashesMaterial.SetColor("_TintColor", sparksColor);
            newFlashesMaterial.SetFloat("_Boost", 1.9f);
            newFlashesMaterial.SetFloat("_AlphaBoost", 2f);
            newFlashesMaterial.SetFloat("_AlphaBias", 0f);

            flashes.material = newFlashesMaterial;

            VFXUtils.MultiplyScale(flashes.gameObject, 1.2f);

            // god what a fucking piece of shit unity is, imagine having to do this thing that should be fucking default
            var sparksMain = sparks.GetComponent<ParticleSystem>().main;
            sparksMain.scalingMode = ParticleSystemScalingMode.Hierarchy;
            var flashesMain = flashes.GetComponent<ParticleSystem>().main;
            flashesMain.scalingMode = ParticleSystemScalingMode.Hierarchy;

            ContentAddition.AddEffect(muzzleFlash);
            return muzzleFlash;
        }

        public static GameObject CreateProjectileRecolor(string name, Color32 ballColor, Color32 radiusIndicatorColor, Color32 lightningColor, Color32 beamStartColor, Color32 beamEndColor)
        {
            var projectile = Main.assets.LoadAsset<GameObject>("GalvanicBallProjectile.prefab").GetComponent<ProjectileController>().ghostPrefab.InstantiateClone("Galvanic Bolt Projectile Ghost " + name, false);
            var actualProjectile = Main.assets.LoadAsset<GameObject>("GalvanicBallProjectile.prefab").InstantiateClone("Galvanic Bolt Projectile " + name, true);
            actualProjectile.GetComponent<ProjectileController>().ghostPrefab = projectile;
            projectile.FindComponent<MeshRenderer>("Radius").sharedMaterial = Paths.Material.matTeamAreaIndicatorIntersectionPlayer;

            var transform = projectile.transform;
            var sphere = transform.Find("Sphere").GetComponent<MeshRenderer>();

            var newGalvanicBallMaterial = new Material(sphere.sharedMaterials[0]);
            newGalvanicBallMaterial.SetColor("_EmColor", ballColor);

            var newGalvanicOverlayMaterial = new Material(sphere.sharedMaterials[1]);
            newGalvanicOverlayMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newGalvanicOverlayMaterial.SetColor("_TintColor", lightningColor);
            newGalvanicOverlayMaterial.SetFloat("_OffsetAmount", 0.1f);

            var newSphereMaterials = new Material[2] { newGalvanicBallMaterial, newGalvanicOverlayMaterial };
            sphere.sharedMaterials = newSphereMaterials;

            var radius = transform.Find("Sphere/Radius").GetComponent<MeshRenderer>();

            var newRadiusMaterial = new Material(radius.material);
            newRadiusMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newRadiusMaterial.SetColor("_TintColor", radiusIndicatorColor);
            newRadiusMaterial.SetFloat("_InvFade", 0.25f);
            newRadiusMaterial.SetFloat("_SoftPower", 0.15f);
            newRadiusMaterial.SetFloat("_Boost", 5f);
            newRadiusMaterial.SetFloat("_RimPower", 10.6f);
            newRadiusMaterial.SetFloat("_RimStrength", 0f);
            newRadiusMaterial.SetFloat("_AlphaBoost", 1.2f);
            newRadiusMaterial.SetFloat("_IntersectionStrength", 1.5f);

            radius.material = newRadiusMaterial;

            var particleHolder = transform.Find("Particle Holder");
            var sparks = particleHolder.Find("Particle, Sparks");
            sparks.localScale = new Vector3(2f, 2f, 1f);

            var sparksPSR = sparks.GetComponent<ParticleSystemRenderer>();

            var newSparksFlareMaterial = new Material(sparksPSR.sharedMaterials[0]);
            newSparksFlareMaterial.SetColor("_TintColor", lightningColor);
            newSparksFlareMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);

            var newSparksLightningMaterial = new Material(sparksPSR.sharedMaterials[1]);
            newSparksLightningMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newSparksLightningMaterial.SetColor("_TintColor", lightningColor);
            newSparksLightningMaterial.SetFloat("_InvFade", 0.1f);
            newSparksLightningMaterial.SetFloat("_Boost", 1.5f);
            newSparksLightningMaterial.SetFloat("_AlphaBoost", 4.648962f);
            newSparksLightningMaterial.SetFloat("_AlphaBias", 0.2042867f);

            var newSparksMaterials = new Material[2] { newSparksFlareMaterial, newSparksLightningMaterial };
            sparksPSR.materials = newSparksMaterials;

            var trail = particleHolder.Find("Trail").GetComponent<TrailRenderer>();

            trail.endWidth = 0.5f;
            trail.startColor = beamStartColor;
            trail.endColor = beamEndColor;
            trail.material = Main.lineRendererBase;
            trail.numCapVertices = 1;
            // trail.textureMode = LineTextureMode.Tile;
            trail.time = 0.15f;

            var light = particleHolder.Find("Light").GetComponent<Light>();
            light.color = lightningColor;
            light.intensity = 15f;
            light.range = 9f;

            ContentAddition.AddNetworkedObject(actualProjectile);
            PrefabAPI.RegisterNetworkPrefab(actualProjectile);
            ContentAddition.AddProjectile(actualProjectile);

            Utils.Projectile.BlacklistAttackDirectionFix(actualProjectile);

            return actualProjectile;
        }

        public static GameObject CreateImpactRecolor(string name, Color32 smallDetailsColor, Color32 ringColor)
        {
            var impact = PrefabAPI.InstantiateClone(Paths.GameObject.LoaderGroundSlam, "Galvanic Bolt Impact " + name, false);

            foreach (ShakeEmitter ughShakesButt in impact.GetComponents<ShakeEmitter>())
            {
                ughShakesButt.enabled = false;
            }
            var effectComponent = impact.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_loader_m1_impact";

            var transform = impact.transform;

            var pointLight = transform.Find("Point Light").GetComponent<Light>();
            pointLight.range = 8f;
            pointLight.color = smallDetailsColor;

            var flash = transform.Find("Flash").gameObject;
            var flashCenter = transform.Find("Flash Center").gameObject;
            var sparksSingle = transform.Find("Sparks, Single").gameObject;
            var omniDirectional = transform.Find("Omni, Directional").gameObject;
            var debris = transform.Find("Debris").gameObject;

            VFXUtils.RecolorMaterialsAndLights(flash, smallDetailsColor, smallDetailsColor, true);
            VFXUtils.RecolorMaterialsAndLights(flashCenter, smallDetailsColor, smallDetailsColor, true);
            VFXUtils.RecolorMaterialsAndLights(sparksSingle, smallDetailsColor, smallDetailsColor, true);
            VFXUtils.RecolorMaterialsAndLights(debris, smallDetailsColor, smallDetailsColor, true);
            /*
            VFXUtils.RecolorMaterialsAndLights(omniDirectional, smallDetailsColor, smallDetailsColor, true);
            var omniDirectionalMain = omniDirectional.GetComponent<ParticleSystem>().main;
            var omniDirectionalStartColor = omniDirectionalMain.startColor;
            omniDirectionalStartColor.color = Color.white;
            */
            // nvm what a PoS that doesnt want to be recolored
            // I bet Wayland developers made him

            omniDirectional.SetActive(false);

            var ring = transform.Find("Ring").GetComponent<ParticleSystemRenderer>();
            var newRingMaterial = new Material(Paths.Material.matLoaderTrailImpact);
            newRingMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newRingMaterial.SetColor("_TintColor", ringColor);
            newRingMaterial.SetFloat("_Boost", 6f);

            ring.material = newRingMaterial;

            var sphereExpanding = transform.Find("Sphere, Expanding").GetComponent<ParticleSystemRenderer>();

            var newSphereMaterial = new Material(Paths.Material.matLoaderSlamSphere);
            newSphereMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newSphereMaterial.SetColor("_TintColor", ringColor);
            newSphereMaterial.SetFloat("_InvFade", 3.2f);
            newSphereMaterial.SetFloat("_SoftPower", 20f);
            newSphereMaterial.SetFloat("_Boost", 5f);
            newSphereMaterial.SetFloat("_RimPower", 12.95549f);
            newSphereMaterial.SetFloat("_RimStrength", 5f);
            newSphereMaterial.SetFloat("_AlphaBoost", 4.7f);
            newSphereMaterial.SetFloat("_IntersectionStrength", 1.38f);
            sphereExpanding.material = newSphereMaterial;

            ContentAddition.AddEffect(impact);
            return impact;
        }
    }
}