﻿using System;
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

        public static void Init()
        {
            dashMat1Default = Object.Instantiate(Assets.Material.matHuntressFlashBright);

            dashMat1Default.SetColor("_TintColor", new Color32(3, 191, 100, 255));

            dashMat2Default = Object.Instantiate(Assets.Material.matHuntressFlashExpanded);

            dashMat2Default.SetColor("_TintColor", new Color32(0, 148, 74, 255));

            //

            dashMat1Major = Object.Instantiate(Assets.Material.matHuntressFlashBright);

            dashMat1Major.SetColor("_TintColor", new Color32(70, 56, 204, 255));

            dashMat2Major = Object.Instantiate(Assets.Material.matHuntressFlashExpanded);

            dashMat2Major.SetColor("_TintColor", new Color32(65, 61, 170, 255));

            //

            dashMat1Renegade = Object.Instantiate(Assets.Material.matHuntressFlashBright);

            dashMat1Renegade.SetColor("_TintColor", new Color32(204, 56, 182, 255));

            dashMat2Renegade = Object.Instantiate(Assets.Material.matHuntressFlashExpanded);

            dashMat2Renegade.SetColor("_TintColor", new Color32(170, 54, 155, 255));

            //

            dashMat1MileZero = Object.Instantiate(Assets.Material.matHuntressFlashBright);

            dashMat1MileZero.SetColor("_TintColor", new Color32(191, 3, 6, 255));

            dashMat2MileZero = Object.Instantiate(Assets.Material.matHuntressFlashExpanded);

            dashMat2MileZero.SetColor("_TintColor", new Color32(148, 0, 5, 255));
        }
    }
}