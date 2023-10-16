using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Skills.Ranger.VFX
{
    public static class SidestepVFX
    {
        public static Material dashMat1;
        public static Material dashMat2;

        public static void Init()
        {
            dashMat1 = Object.Instantiate(Utils.Assets.Material.matHuntressFlashBright);

            dashMat1.SetColor("_TintColor", new Color32(3, 191, 100, 255));

            dashMat2 = Object.Instantiate(Utils.Assets.Material.matHuntressFlashExpanded);

            dashMat2.SetColor("_TintColor", new Color32(0, 148, 74, 255));
        }
    }
}