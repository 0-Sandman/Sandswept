using RoR2.Achievements;
using RoR2;
using BepInEx.Configuration;

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
            if (self.interactor != null)
            {
                if (intPickupIndex == 146)
                {
                    Grant();
                }
            }
            orig(self, intPickupIndex);
        }
    }
}
