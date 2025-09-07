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
            dashMat1Default = CreateMat1Recolor(new Color32(2, 126, 7, 255));

            dashMat2Default = CreateMat2Recolor(new Color32(0, 127, 64, 255));

            //

            dashMat1Major = CreateMat1Recolor(new Color32(46, 37, 135, 255));

            dashMat2Major = CreateMat2Recolor(new Color32(56, 52, 146, 255));

            //

            dashMat1Renegade = CreateMat1Recolor(new Color32(135, 37, 121, 255));

            dashMat2Renegade = CreateMat2Recolor(new Color32(146, 46, 133, 255));

            //

            dashMat1MileZero = CreateMat1Recolor(new Color32(126, 2, 4, 255));

            dashMat2MileZero = CreateMat2Recolor(new Color32(127, 0, 4, 255));

            //

            dashMat1Sandswept = CreateMat1Recolor(new Color32(142, 105, 52, 255));

            dashMat2Sandswept = CreateMat2Recolor(new Color32(127, 96, 51, 255));
        }

        public static Material CreateMat1Recolor(Color32 blueEquivalent)
        {
            var mat = new Material(Paths.Material.matHuntressFlashBright);

            mat.SetColor("_TintColor", blueEquivalent);

            return mat;
        }

        public static Material CreateMat2Recolor(Color32 blueEquivalent)
        {
            var mat = new Material(Paths.Material.matHuntressFlashExpanded);

            mat.SetColor("_TintColor", blueEquivalent);

            return mat;
        }
    }
}