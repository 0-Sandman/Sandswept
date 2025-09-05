using RoR2.Achievements;

namespace Sandswept.Unlocks
{
    [RegisterAchievement("ITEM_SANDSWEPT_HALLOWED_ICHOR", "ITEM_SANDSWEPT_HALLOWED_ICHOR", null, 5, null)]
    public class HallowedIchorUnlock : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            TeleporterInteraction.onTeleporterChargedGlobal += OnTeleporterCharged;

        }
        private void OnTeleporterCharged(TeleporterInteraction interaction)
        {
            if (RoR2.Stage.instance && RoR2.Stage.instance.sceneDef.cachedName == "skymeadow")
            {
                // shizo stuff I don't wanna implement rn
            }
        }

        public override void OnUninstall()
        {
            // On.RoR2.ShrineRestackBehavior.AddShrineStack -= OnOrderInteract;
            base.OnUninstall();
        }
    }
}