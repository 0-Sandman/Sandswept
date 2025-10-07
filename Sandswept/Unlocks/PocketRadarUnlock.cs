using RoR2.Achievements;

namespace Sandswept.Unlocks
{
    [RegisterAchievement("ITEM_SANDSWEPT_POCKET_RADAR", "ITEM_SANDSWEPT_POCKET_RADAR", null, 5, null)]
    public class PocketRadarUnlock : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            GlobalEventManager.OnInteractionsGlobal += OnInteract;
        }

        private void OnInteract(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            var body = base.localUser.cachedBodyObject;
            if (!body)
            {
                return;
            }

            var bodyInteractor = body.GetComponent<Interactor>();
            if (!bodyInteractor)
            {
                return;
            }

            if (bodyInteractor != interactor)
            {
                return;
            }

            var genericDisplayNameProvider = interactableObject.GetComponent<GenericDisplayNameProvider>();
            if (!genericDisplayNameProvider)
            {
                return;
            }

            if (genericDisplayNameProvider.displayToken != "CHEST1_STEALTHED_NAME") // sorry
            {
                return;
            }

            Grant();
        }

        public override void OnUninstall()
        {
            GlobalEventManager.OnInteractionsGlobal -= OnInteract;
            base.OnUninstall();
        }
    }
}