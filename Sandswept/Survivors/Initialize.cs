using Sandswept.Elites.VFX;
using Sandswept.Survivors.Ranger.Hooks;
using Sandswept.Survivors.Ranger.Pod;
using Sandswept.Survivors.Ranger.Projectiles;
using Sandswept.Survivors.Ranger.VFX;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors
{
    public static class Initialize
    {
        public static void Init()
        {
            RangerPod.Init();
            Ranger.Crosshairs.Ranger.Init();

            RangerVFX.Init();
            EliteVFX.Init();

            DirectCurrent.Init();
            TheFuckingBFG.Init();
            Sandswept.Survivors.Ranger.Projectiles.Char.Init();
            ChargeGain.Init();
            // Based.Init();

            Material matRanger = Main.assets.LoadAsset<Material>("matRanger.mat");
            matRanger.shader = Utils.Assets.Shader.HGStandard;
            matRanger.SetColor("_EmColor", Color.white);
            matRanger.SetFloat("_EmPower", 2.5f);
            matRanger.EnableKeyword("DITHER");

            Electrician.Hooks.ShieldOverlay.Init();
        }
    }
}