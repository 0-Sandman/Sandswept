using RoR2.Achievements;

namespace Sandswept.Unlocks
{
    [RegisterAchievement("ITEM_SANDSWEPT_HALLOWED_ICHOR", "ITEM_SANDSWEPT_HALLOWED_ICHOR", null, 5, null)]
    public class HallowedIchorUnlock : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();

        }

        public override void OnUninstall()
        {
            // On.RoR2.ShrineRestackBehavior.AddShrineStack -= OnOrderInteract;
            base.OnUninstall();
        }
    }
}