namespace Sandswept.Skills.Ranger.VFX
{
    public static class HeatSinkVFX
    {
        public static Material dashMat1;
        public static Material dashMat2;

        public static void Init()
        {
            dashMat1 = Object.Instantiate(Assets.Material.matHuntressFlashBright);

            dashMat1.SetColor("_TintColor", new Color32(191, 49, 3, 255));

            dashMat2 = Object.Instantiate(Assets.Material.matHuntressFlashExpanded);

            dashMat2.SetColor("_TintColor", new Color32(148, 46, 0, 255));
        }
    }
}