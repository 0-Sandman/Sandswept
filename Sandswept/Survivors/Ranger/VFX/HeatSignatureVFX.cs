namespace Sandswept.Survivors.Ranger.VFX
{
    public static class HeatSignatureVFX
    {
        public static Material heatDashMat1Default;
        public static Material heatDashMat2Default;

        public static Material heatDashMat1Major;
        public static Material heatDashMat2Major;

        public static Material heatDashMat1Renegade;
        public static Material heatDashMat2Renegade;

        public static Material heatDashMat1MileZero;
        public static Material heatDashMat2MileZero;

        public static Material heatDashMat1Sandswept;
        public static Material heatDashMat2Sandswept;

        public static void Init()
        {
            heatDashMat1Default = CreateMat1Recolor(new Color32(191, 27, 3, 200));

            heatDashMat2Default = CreateMat2Recolor(new Color32(148, 29, 0, 200));

            //

            heatDashMat1Major = CreateMat1Recolor(new Color32(0, 77, 148, 200));

            heatDashMat2Major = CreateMat2Recolor(new Color32(0, 119, 148, 200));

            //

            heatDashMat1Renegade = CreateMat1Recolor(new Color32(53, 2, 132, 200));

            heatDashMat2Renegade = CreateMat2Recolor(new Color32(47, 0, 102, 200));

            //

            heatDashMat1MileZero = CreateMat1Recolor(new Color32(0, 0, 0, 200));

            heatDashMat2MileZero = CreateMat2Recolor(new Color32(99, 0, 0, 200));

            //

            heatDashMat1Sandswept = CreateMat1Recolor(new Color32(249, 197, 143, 200));

            heatDashMat2Sandswept = CreateMat2Recolor(new Color32(150, 150, 150, 200));
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