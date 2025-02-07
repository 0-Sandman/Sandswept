using RoR2.Achievements;
using Sandswept.Interactables.Regular;

namespace Sandswept.Unlocks
{
    [RegisterAchievement("ITEM_SANDSWEPT_TEMPORAL_TRANSISTOR", "ITEM_SANDSWEPT_TEMPORAL_TRANSISTOR", null, 5, null)]
    public class TemporalTransistorUnlock : BaseAchievement
    {
        public CharacterBody interactorBody;

        public override void OnInstall()
        {
            base.OnInstall();
            GlobalEventManager.OnInteractionsGlobal += GlobalEventManager_OnInteractionsGlobal;
            On.RoR2.CombatSquad.TriggerDefeat += CombatSquad_TriggerDefeat;
        }

        private void GlobalEventManager_OnInteractionsGlobal(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            if (interactor && interactableObject && interactableObject.GetComponent<ShrineOfTheFutureController>() && !interactor.GetComponent<TemporalTransistorJumpTracker>())
            {
                interactorBody = interactor.GetComponent<CharacterBody>();
                interactor.AddComponent<TemporalTransistorJumpTracker>();
                // Main.ModLogger.LogError("interactor body is " + interactorBody);
            }
        }

        private void CombatSquad_TriggerDefeat(On.RoR2.CombatSquad.orig_TriggerDefeat orig, CombatSquad self)
        {
            orig(self);
            if (interactorBody && self.GetComponent<ShrineOfTheFutureController>())
            {
                // Main.ModLogger.LogError("combat squad defeat: is shrine of the future");
                if (interactorBody.TryGetComponent<TemporalTransistorJumpTracker>(out var temporalTransistorJumpTracker) && !temporalTransistorJumpTracker.hasJumped)
                {
                    Grant();
                }
            }
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
            On.RoR2.CombatSquad.TriggerDefeat -= CombatSquad_TriggerDefeat;
            GlobalEventManager.OnInteractionsGlobal -= GlobalEventManager_OnInteractionsGlobal;
        }
    }

    public class TemporalTransistorJumpTracker : MonoBehaviour
    {
        public CharacterBody body;
        public bool hasJumped = false;

        public void Start()
        {
            body = GetComponent<CharacterBody>();
            // Main.ModLogger.LogError("tracker: body is " + body);
            if (body)
            {
                body.onJump += OnJump;
            }
        }

        public void OnJump()
        {
            // Main.ModLogger.LogError("has jumped !!");
            hasJumped = true;
        }
    }
}