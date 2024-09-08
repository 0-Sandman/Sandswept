using RoR2.Achievements;

namespace Sandswept.Unlocks
{
    [RegisterAchievement("ITEM_SANDSWEPT_SUN_FRAGMENT", "ITEM_SANDSWEPT_SUN_FRAGMENT", null, 5, null)]
    public class Tracker : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            On.RoR2.ScrapperController.BeginScrapping += ScrapperController_BeginScrapping;
        }

        private void ScrapperController_BeginScrapping(On.RoR2.ScrapperController.orig_BeginScrapping orig, ScrapperController self, int intPickupIndex)
        {
            orig(self, intPickupIndex);
            PickupIndex planula = PickupCatalog.FindPickupIndex(RoR2Content.Items.ParentEgg.itemIndex);

            if (new PickupIndex(intPickupIndex) == planula) {
                Grant();
            }
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
            On.RoR2.ScrapperController.BeginScrapping -= ScrapperController_BeginScrapping;
        }
    }
}