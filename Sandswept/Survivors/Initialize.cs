
using Sandswept.Survivors.Electrician.Crate;
using Sandswept.Survivors.Electrician.VFX;
using Sandswept.Survivors.Ranger.Hooks;
using Sandswept.Survivors.Ranger.Pod;
using Sandswept.Survivors.Ranger.Projectiles;
using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Survivors
{
    public static class Initialize
    {
        public static void Init()
        {
            if (SurvivorBase.DefaultEnabledCallback(Ranger.Ranger.instance))
            {
                RangerPod.Init();
                Ranger.Crosshairs.Ranger.Init();

                RangerVFX.Init();

                DirectCurrent.Init();
                AltSecondaries.Init();
                ChargeGain.Init();

                Material matRanger = Main.assets.LoadAsset<Material>("matRanger.mat");
                matRanger.shader = Utils.Assets.Shader.HopooGamesDeferredStandard;
                matRanger.SetColor("_EmColor", Color.white);
                matRanger.SetFloat("_EmPower", 2.5f);
                matRanger.EnableKeyword("DITHER");
            }

            if (SurvivorBase.DefaultEnabledCallback(Electrician.Electrician.instance))
            {
                VoltCrate.Init();
                Electrician.Hooks.ShieldOverlay.Init();
                VoltVFX.Init();
            }
        }
    }
}