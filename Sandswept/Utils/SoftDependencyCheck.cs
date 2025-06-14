using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using RoR2;
using On.RoR2;
using Sandswept.Survivors.Ranger;
using Sandswept.Survivors.Electrician;

namespace Sandswept.Utils
{
    public static class CustomEmoteAPICheck
    {
        private static bool? _enabled;
        private static string bonemapperName;


        public static bool enabled
        {
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.weliveinasociety.CustomEmotesAPI");
                }
                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SurvivorCatalog_Init(On.RoR2.SurvivorCatalog.orig_Init orig)
        {
            orig();

            var skele = Main.dgoslingAssets.LoadAsset<GameObject>("mdlRangerEmote");
            EmotesAPI.CustomEmotesAPI.ImportArmature(Ranger.instance.Body, skele);

            var boneMapper = skele.GetComponentInChildren<BoneMapper>();
            boneMapper.scale = 0.9f;

            bonemapperName = boneMapper.name;

            EmotesAPI.CustomEmotesAPI.animChanged += CustomEmoteAPICheck.CustomEmotesAPI_animChanged;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void CustomEmotesAPI_animChanged(string newAnimation, BoneMapper mapper)
        {
            if (mapper.name == bonemapperName)
            {
                GameObject gun = mapper.transform.parent.Find("Gun").gameObject;
                if (newAnimation != "none")
                {
                    gun.SetActive(false);
                }
                else
                    gun.SetActive(true);
            }

        }
    }
}