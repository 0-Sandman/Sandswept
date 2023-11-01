using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept2.Skills.Ranger.VFX
{
    public static class DirectCurrentVFX
    {
        public static GameObject ghostPrefab;

        public static void Init()
        {
            ghostPrefab = PrefabAPI.InstantiateClone(Assets.GameObject.LunarSunProjectileGhost, "Direct Current Ghost", false);

            var ramp = Main.hifuSandswept.LoadAsset<Texture2D>("Assets/Sandswept/texRampDirectCurrent.png");
            var fresnel = Main.hifuSandswept.LoadAsset<Texture2D>("Assets/Sandswept/texRampDirectCurrentFresnel.png");

            var green = new Color32(0, 255, 167, 255);

            var mdl = ghostPrefab.transform.GetChild(0);
            var objectScaleCurve = mdl.GetComponent<ObjectScaleCurve>();
            objectScaleCurve.overallCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.15f, 0.75f), new Keyframe(1f, 2.5f));

            var backdrop = mdl.GetChild(0).GetComponent<ParticleSystemRenderer>();

            var newBackdropMat = Object.Instantiate(Assets.Material.matLunarSunProjectileBackdrop);

            newBackdropMat.SetTexture("_RemapTex", ramp);
            newBackdropMat.SetInt("_Cull", 1); // used to appear as a white square behind terrain so I fixed it

            backdrop.material = newBackdropMat;

            var quad = mdl.GetChild(1).GetComponent<MeshRenderer>();

            var newQuadMat = Object.Instantiate(Assets.Material.matLunarSunProjectile);

            newQuadMat.SetColor("_EmColor", green); // 0, 187, 255, 255
            newQuadMat.SetTexture("_FresnelRamp", fresnel);

            quad.material = newQuadMat;

            var particles = ghostPrefab.transform.GetChild(1);

            var closeParticles = particles.GetChild(0).GetComponent<ParticleSystem>().main.startColor;
            closeParticles.colorMin = green;
            closeParticles.colorMax = new Color32(0, 141, 197, 255);

            var distantParticles = particles.GetChild(1).GetComponent<ParticleSystem>().main.startColor;
            distantParticles.color = green;

            var pop = particles.GetChild(2).GetComponent<ParticleSystem>().main.startColor;
            pop.color = green;

            var trail = particles.GetChild(3).GetComponent<TrailRenderer>();
            trail.startWidth = 0.6f;
            trail.endWidth = 0.25f;
            trail.widthMultiplier = 0.5f;
            trail.time = 0.2f;

            var newTrailMat = Object.Instantiate(Assets.Material.matLunarSunProjectileTrail);

            newTrailMat.SetTexture("_RemapTex", ramp);
            newTrailMat.SetFloat("_Boost", 1f);
            newTrailMat.SetFloat("_AlphaBoost", 4.710526f);
            newTrailMat.SetFloat("_AlphaBias", 0.3349282f);
            newTrailMat.SetTexture("_MainTex", Assets.Texture2D.texAlphaGradient1);
            newTrailMat.SetColor("_TintColor", new Color32(111, 170, 151, 154));

            trail.material = newTrailMat;
        }
    }
}