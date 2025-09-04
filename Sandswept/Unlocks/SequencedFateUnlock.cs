using RoR2.Achievements;

namespace Sandswept.Unlocks
{
    [RegisterAchievement("ITEM_SANDSWEPT_SEQUENCED_FATE", "ITEM_SANDSWEPT_SEQUENCED_FATE", null, 5, null)]
    public class SequencedFateUnlock : BaseAchievement
    {
        public static List<string> acceptableStageNames = new();
        public override void OnInstall()
        {
            base.OnInstall();

            acceptableStageNames.Add("moon");
            acceptableStageNames.Add("moon2");
            acceptableStageNames.Add("moon3");

            On.RoR2.ShrineRestackBehavior.AddShrineStack += OnOrderInteract;

        }

        private void OnOrderInteract(On.RoR2.ShrineRestackBehavior.orig_AddShrineStack orig, RoR2.ShrineRestackBehavior self, RoR2.Interactor interactor)
        {
            orig(self, interactor);
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

            if (RoR2.Stage.instance && acceptableStageNames.Contains(RoR2.Stage.instance.sceneDef.cachedName))
            {
                Grant();
            }
        }

        public override void OnUninstall()
        {
            On.RoR2.ShrineRestackBehavior.AddShrineStack -= OnOrderInteract;
            base.OnUninstall();
        }
    }
}