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
    public class StaticSnare
    {

        public static GameObject staticSnareDefault;
        public static GameObject staticSnareCovenant;

        public static GameObject lightningVFXDefault;
        public static GameObject lightningVFXCovenant;

        public static void Init()
        {
            staticSnareDefault = CreateProjectileRecolor("Default", new Color32(255, 191, 0, 255), new Color32(0, 0, 255, 255));
            staticSnareCovenant = CreateProjectileRecolor("Covenant", new Color32(0, 0, 255, 255), new Color32(255, 0, 230, 255));

            lightningVFXDefault = CreateZipRecolor("Default", new Color32(255, 156, 0, 255));
            lightningVFXCovenant = CreateZipRecolor("Covenant", new Color32(223, 31, 208, 255));

        }

        public static GameObject CreateProjectileRecolor(string name, Color32 beamStartColor, Color32 beamEndColor)
        {
            var projectile = PrefabAPI.InstantiateClone(Main.assets.LoadAsset<GameObject>("TripwireMineProjectile.prefab"), "Static Snare Projectile " + name, true);

            var transform = projectile.transform;

            var lineRenderer = transform.Find("LineRenderer").GetComponent<LineRenderer>();

            lineRenderer.endWidth = 0.5f;
            lineRenderer.startColor = beamStartColor;
            lineRenderer.endColor = beamEndColor;
            lineRenderer.material = Main.lineRendererBase;

            var seat = transform.Find("seat");

            ContentAddition.AddNetworkedObject(projectile);
            PrefabAPI.RegisterNetworkPrefab(projectile);
            ContentAddition.AddProjectile(projectile);

            return projectile;
        }
        
        public static GameObject CreateZipRecolor(string name, Color32 vfxColor)
        {
            var lightningZipOrb = PrefabAPI.InstantiateClone(Paths.GameObject.BeamSphereGhost, "Lightning Zip Orb VFX " + name, false);
            lightningZipOrb.RemoveComponent<ProjectileGhostController>();

            var transform = lightningZipOrb.transform;

            var pointLight = transform.Find("Point light").GetComponent<Light>();
            pointLight.GetComponent<LightIntensityCurve>().enabled = false;
            pointLight.color = vfxColor;
            pointLight.intensity = 30f;
            pointLight.range = 20f;

            var fire = transform.Find("Fire");
            fire.localScale = Vector3.one * 0.5f;
            var firePSR = fire.GetComponent<ParticleSystemRenderer>();

            VFXUtils.RecolorMaterialsAndLights(fire.gameObject, vfxColor, vfxColor, true);

            var newFireMaterial = new Material(Paths.Material.matLoaderLightningTile);
            newFireMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);

            firePSR.material = newFireMaterial;

            transform.Find("Lightning").gameObject.SetActive(false);

            var beams = fire.Find("Beams");
            beams.localScale = Vector3.one * 1.2f;

            return lightningZipOrb;
        }

    }
}
