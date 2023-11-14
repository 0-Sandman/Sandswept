namespace Sandswept.Survivors.Ranger.VFX
{
    public static class HeatSignatureVFX
    {
        public static Material dashMat1;
        public static Material dashMat2;

        public static void Init()
        {
            dashMat1 = Object.Instantiate(Assets.Material.matHuntressFlashBright);

            dashMat1.SetColor("_TintColor", new Color32(191, 27, 3, 150));

            dashMat2 = Object.Instantiate(Assets.Material.matHuntressFlashExpanded);

            dashMat2.SetColor("_TintColor", new Color32(148, 29, 0, 150));
        }
    }
}