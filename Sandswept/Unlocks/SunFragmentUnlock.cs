using RoR2.Achievements;

namespace Sandswept.Unlocks
{
    [RegisterAchievement("SUN_FRAGMENT", "ITEM_SUN_FRAGMENT", null, null)]
    public class Tracker : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            On.RoR2.ScrapperController.BeginScrapping += ScrapperController_BeginScrapping;
        }

        private void ScrapperController_BeginScrapping(On.RoR2.ScrapperController.orig_BeginScrapping orig, ScrapperController self, int intPickupIndex)
        {
            var pickupDef = PickupCatalog.GetPickupDef(new PickupIndex(intPickupIndex));
            if (self.interactor != null && pickupDef != null)
            {
                if (pickupDef == PickupCatalog.FindPickupIndex(RoR2Content.Items.ParentEgg.itemIndex).pickupDef)
                {
                    Grant();
                }
            }
            orig(self, intPickupIndex);
        }

        public override void OnUninstall()
        {
            On.RoR2.ScrapperController.BeginScrapping -= ScrapperController_BeginScrapping;
        }
    }
}