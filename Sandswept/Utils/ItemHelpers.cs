using RoR2;
using Sandswept.Utils.Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Sandswept.Utils
{
    public static class ItemHelpers
    {
        /// <summary>
        /// A helper that will set up the RendererInfos of a GameObject that you pass in.
        /// <para>This allows it to go invisible when your character is not visible, as well as letting overlays affect it.</para>
        /// </summary>
        /// <param name="obj">The GameObject/Prefab that you wish to set up RendererInfos for.</param>
        /// <param name="debugmode">Do we attempt to attach a material shader controller instance to meshes in this?</param>
        /// <returns>Returns an array full of RendererInfos for GameObject.</returns>
        public static CharacterModel.RendererInfo[] ItemDisplaySetup(GameObject obj, bool debugmode = false)
        {
            List<Renderer> AllRenderers = new List<Renderer>();

            var meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers.Length > 0) { AllRenderers.AddRange(meshRenderers); }

            var skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderers.Length > 0) { AllRenderers.AddRange(skinnedMeshRenderers); }

            CharacterModel.RendererInfo[] renderInfos = new CharacterModel.RendererInfo[AllRenderers.Count];

            for (int i = 0; i < AllRenderers.Count; i++)
            {
                if (debugmode)
                {
                    var controller = AllRenderers[i].gameObject.AddComponent<MaterialControllerComponents.HGControllerFinder>();
                    controller.Renderer = AllRenderers[i];
                }

                renderInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = AllRenderers[i] is SkinnedMeshRenderer ? AllRenderers[i].sharedMaterial : AllRenderers[i].material,
                    renderer = AllRenderers[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false //We allow the mesh to be affected by overlays like OnFire or PredatoryInstinctsCritOverlay.
                };
            }

            return renderInfos;
        }

        /// <summary>
        /// Refreshes stacks of a timed buff on a body for a specified duration. Will refresh the entire stack pool of the buff at once.
        /// </summary>
        /// <param name="body">The body to check.</param>
        /// <param name="buffDef">The buff to refresh.</param>
        /// <param name="duration">The duration all stacks should have.</param>
        public static void RefreshTimedBuffs(CharacterBody body, BuffDef buffDef, float duration)
        {
            if (!body || body.GetBuffCount(buffDef) <= 0) { return; }
            foreach (var buff in body.timedBuffs)
            {
                if (buffDef.buffIndex == buff.buffIndex)
                {
                    buff.timer = duration;
                }
            }
        }

        /// <summary>
        /// Refreshes stacks of a timed buff on a body for a specified duration, but spreads their time to decay after a set start duration and interval afterwards.
        /// <para>Will refresh the entire stack pool of the buff at once.</para>
        /// </summary>
        /// <param name="body">The body to check.</param>
        /// <param name="buffDef">The buff to refresh.</param>
        /// <param name="taperStart">How long should we wait before beginning to decay buffs?</param>
        /// <param name="taperDuration">The duration between stack decay.</param>
        public static void RefreshTimedBuffs(CharacterBody body, BuffDef buffDef, float taperStart, float taperDuration)
        {
            if (!body || body.GetBuffCount(buffDef) <= 0) { return; }
            int i = 0;
            foreach (var buff in body.timedBuffs)
            {
                if (buffDef.buffIndex == buff.buffIndex)
                {
                    buff.timer = taperStart + i * taperDuration;
                    i++;
                }
            }
        }

        /// <summary>
        /// Finds the associated DotController from a buff, if applicable.
        /// </summary>
        /// <param name="buff">The buff to check all dots against.</param>
        /// <returns>A dotindex of the DotController the target buff is associated with, else, it will return an invalid index.</returns>
        public static DotController.DotIndex FindAssociatedDotForBuff(BuffDef buff)
        {
            RoR2.DotController.DotIndex index = (RoR2.DotController.DotIndex)Array.FindIndex(DotController.dotDefs, (dotDef) => dotDef.associatedBuff == buff);

            return index;
        }
    }
}