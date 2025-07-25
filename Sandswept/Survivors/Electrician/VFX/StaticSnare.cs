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

        public static GameObject staticSnareIndicatorDefault;
        public static GameObject staticSnareIndicatorCovenant;

        public static void Init()
        {
            staticSnareDefault = CreateProjectileRecolor("Default", new Color32(255, 191, 0, 255), new Color32(0, 77, 255, 255));
            staticSnareCovenant = CreateProjectileRecolor("Covenant", new Color32(0, 0, 255, 255), new Color32(255, 0, 230, 255));

            lightningVFXDefault = CreateZipRecolor("Default", new Color32(0, 77, 255, 255), new Color32(255, 168, 0, 255), new Color32(255, 182, 0, 255));
            lightningVFXCovenant = CreateZipRecolor("Covenant", new Color32(0, 0, 255, 255), new Color32(223, 31, 208, 255), new Color32(173, 0, 255, 255));

            staticSnareIndicatorDefault = CreateIndicatorRecolor("Default", new Color32(255, 214, 0, 255));
            staticSnareIndicatorCovenant = CreateIndicatorRecolor("Covenant", new Color32(255, 217, 251, 255));
        }

        public static GameObject CreateIndicatorRecolor(string name, Color32 indicatorColor)
        {
            var indicator = PrefabAPI.InstantiateClone(Paths.GameObject.BossPositionIndicator, "Static Snare Indicator " + name, false);

            var newSprite = Main.sandsweptHIFU.LoadAsset<Sprite>("texVoltIndicator" + name + ".png");

            var transform = indicator.transform;
            transform.localScale = Vector3.one * 0.1f;
            var outsideFrameArrow = transform.Find("OutsideFrameArrow");
            outsideFrameArrow.localScale = Vector3.one;

            var outsideSprite = outsideFrameArrow.Find("Sprite").GetComponent<SpriteRenderer>();

            outsideSprite.color = indicatorColor;
            outsideSprite.sprite = newSprite;
            outsideSprite.sortingOrder = -999;
            outsideSprite.transform.localScale = Vector3.one;

            outsideSprite.transform.GetChild(0).gameObject.SetActive(false); // fuck the second arrow

            var insideFrameMarker = transform.Find("InsideFrameMarker");
            var insideSprite = insideFrameMarker.Find("Sprite").GetComponent<SpriteRenderer>();
            insideSprite.transform.localScale = Vector3.one;
            insideSprite.color = indicatorColor;
            insideSprite.sprite = newSprite;
            insideSprite.sortingOrder = -999;
            insideSprite.GetComponent<ObjectScaleCurve>().timeMax = 1.5f;

            return indicator;
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
            // lineRenderer.textureMode = LineTextureMode.Tile;

            ContentAddition.AddNetworkedObject(projectile);
            PrefabAPI.RegisterNetworkPrefab(projectile);
            ContentAddition.AddProjectile(projectile);

            return projectile;
        }

        public static GameObject CreateZipRecolor(string name, Color32 smallSparksColor, Color32 largeSparksColor, Color32 lightColor)
        {
            var lightningZipOrb = PrefabAPI.InstantiateClone(Paths.GameObject.BeamSphereGhost, "Lightning Zip Orb VFX " + name, false);
            lightningZipOrb.RemoveComponent<ProjectileGhostController>();

            var transform = lightningZipOrb.transform;

            var pointLight = transform.Find("Point light").GetComponent<Light>();
            pointLight.GetComponent<LightIntensityCurve>().enabled = false;
            pointLight.color = lightColor;
            pointLight.intensity = 30f;
            pointLight.range = 20f;

            var fire = transform.Find("Fire");
            fire.localScale = Vector3.one;
            var firePSR = fire.GetComponent<ParticleSystemRenderer>();

            var newFireMaterial = new Material(Paths.Material.matLoaderLightningTile);
            newFireMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newFireMaterial.SetColor("_TintColor", smallSparksColor);
            newFireMaterial.SetFloat("_Boost", 12f);

            firePSR.material = newFireMaterial;

            transform.Find("Lightning").gameObject.SetActive(false);

            var beams = fire.Find("Beams");
            beams.localScale = Vector3.one * 1.2f;

            var beamsPSR = beams.GetComponent<ParticleSystemRenderer>();
            var beamsMain = beams.GetComponent<ParticleSystem>().main;
            beamsMain.startColor = new ParticleSystem.MinMaxGradient(Color.white);

            var newBeamsMaterial = new Material(Paths.Material.matLoaderLightningTile);

            newBeamsMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newBeamsMaterial.SetColor("_TintColor", largeSparksColor);
            newBeamsMaterial.SetFloat("_Boost", 6f);

            beamsPSR.material = newBeamsMaterial;

            return lightningZipOrb;
        }

    }
}
