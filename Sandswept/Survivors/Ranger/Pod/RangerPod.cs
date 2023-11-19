using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.Pod
{
    public static class RangerPod
    {
        public static GameObject prefab;

        public static void Init()
        {
            var podMat = Assets.Material.matEscapePod;

            var rangerPodMat = Object.Instantiate(Assets.Material.matEscapePod);
            rangerPodMat.SetTexture("_MainTex", Main.hifuSandswept.LoadAsset<Texture2D>("Assets/Sandswept/texRangerPod.png"));
            rangerPodMat.SetColor("_TintColor", Color.white);
            rangerPodMat.SetFloat("_Smoothness", 0.5f);
            rangerPodMat.SetFloat("_SpecularExponent", 1.082716f);

            prefab = PrefabAPI.InstantiateClone(Assets.GameObject.SurvivorPod, "Ranger Pod GIGACHAD");

            var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                var meshRenderer = meshRenderers[i];
                if (meshRenderer.material == podMat)
                {
                    meshRenderer.material = rangerPodMat;
                }
            }

            PrefabAPI.RegisterNetworkPrefab(prefab);
        }
    }
}