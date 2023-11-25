using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.VFX
{
    public class InitVFX
    {
        public static void Init()
        {
            DirectCurrentVFX.Init();
            EnflameVFX.Init();
            ExhaustVFX.Init();
            HeatSignatureVFX.Init();
            HeatSinkVFX.Init();
            HeatVFX.Init();
            ReleaseVFX.Init();
            SidestepVFX.Init();
        }
    }
}