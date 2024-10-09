using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.VFX
{
    public static class SidestepVFX
    {
        public static Material dashMat1Default;
        public static Material dashMat2Default;

        public static Material dashMat1Major;
        public static Material dashMat2Major;

        public static Material dashMat1Renegade;
        public static Material dashMat2Renegade;

        public static Material dashMat1MileZero;
        public static Material dashMat2MileZero;

        public static Material dashMat1Sandswept;
        public static Material dashMat2Sandswept;

        public static void Init()
        {
            dashMat1Default = CreateMat1Recolor(new Color32(3, 191, 100, 255));

            dashMat2Default = CreateMat2Recolor(new Color32(0, 148, 74, 255));

            //

            dashMat1Major = CreateMat1Recolor(new Color32(70, 56, 204, 255));

            dashMat2Major = CreateMat2Recolor(new Color32(65, 61, 170, 255));

            //

            dashMat1Renegade = CreateMat1Recolor(new Color32(204, 56, 182, 255));

            dashMat2Renegade = CreateMat2Recolor(new Color32(170, 54, 155, 255));

            //

            dashMat1MileZero = CreateMat1Recolor(new Color32(191, 3, 6, 255));

            dashMat2MileZero = CreateMat2Recolor(new Color32(148, 0, 5, 255));

            //

            dashMat1Sandswept = CreateMat1Recolor(new Color32(249, 197, 143, 255));

            dashMat2Sandswept = CreateMat2Recolor(new Color32(87, 87, 87, 255));
        }

        public static Material CreateMat1Recolor(Color32 blueEquivalent)
        {
            var mat = Object.Instantiate(Paths.Material.matHuntressFlashBright);

            mat.SetColor("_TintColor", blueEquivalent);

            return mat;
        }

        public static Material CreateMat2Recolor(Color32 blueEquivalent)
        {
            var mat = Object.Instantiate(Paths.Material.matHuntressFlashExpanded);

            mat.SetColor("_TintColor", blueEquivalent);

            return mat;
        }
    }
}