using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.VFX
{
    public static class HeatVFX
    {
        public static Material heatMatDefault;
        public static Color32 heatMatDefaultColor = new(191,80,3,100);

        public static Material heatMatMajor;
        public static Color32 heatMatMajorColor = new(0,35,148,100);

        public static Material heatMatRenegade;
        public static Color32 heatMatRenegadeColor = new(78,2,132,100);

        public static Material heatMatMileZero;
        public static Color32 heatMatMileZeroColor = new(50,0,0,100);

        public static Material heatMatSandswept;
        public static Color32 heatMatSandsweptColor = new(150,150,150,100);

        public static void Init()
        {
            heatMatDefault = CreateMatRecolor(heatMatDefaultColor);

            //

            heatMatMajor = CreateMatRecolor(heatMatMajorColor);

            //

            heatMatRenegade = CreateMatRecolor(heatMatRenegadeColor);

            //

            heatMatMileZero = CreateMatRecolor(heatMatMileZeroColor);

            //

            heatMatSandswept = CreateMatRecolor(heatMatSandsweptColor);
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