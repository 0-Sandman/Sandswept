using RoR2.Achievements;

namespace Sandswept.Unlocks
{
    [RegisterAchievement("ITEM_SANDSWEPT_SUN_FRAGMENT", "ITEM_SANDSWEPT_SUN_FRAGMENT", null, 5, null)]
    public class SunFragmentUnlock : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            On.RoR2.ScrapperController.BeginScrapping_UniquePickup += OnScrap;
        }

        private void OnScrap(On.RoR2.ScrapperController.orig_BeginScrapping_UniquePickup orig, ScrapperController self, UniquePickup pickupToTake)
        {
            orig(self, pickupToTake);
            PickupIndex planula = PickupCatalog.FindPickupIndex(RoR2Content.Items.ParentEgg.itemIndex);

            if (pickupToTake.pickupIndex == planula)
            {
                Grant();
            }
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
            On.RoR2.ScrapperController.BeginScrapping_UniquePickup -= OnScrap;
        }
    }
}