using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.VFX
{
    public static class HeatVFX
    {
        public static Material heatMatDefault;

        public static Material heatMatMajor;

        public static Material heatMatRenegade;

        public static Material heatMatMileZero;

        public static void Init()
        {
            heatMatDefault = CreateMatRecolor(new Color32(191, 80, 3, 100));

            //

            heatMatMajor = CreateMatRecolor(new Color32(0, 35, 148, 100));

            //

            heatMatRenegade = CreateMatRecolor(new Color32(78, 2, 132, 100));

            //

            heatMatMileZero = CreateMatRecolor(new Color32(50, 0, 0, 100));
        }

        // heat vfx color = first heat signature vfx color + hue shift 17 to the right or left in paint.net :smirk_cat: except mile zero lmoa

        public static Material CreateMatRecolor(Color32 blueEquivalent)
        {
            var mat = Object.Instantiate(Paths.Material.matHuntressFlashExpanded);

            mat.SetColor("_TintColor", blueEquivalent);
            mat.SetInt("_Cull", 1);

            return mat;
        }
    }
}