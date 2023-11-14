/*
using RoR2.Achievements;

namespace Sandswept.Unlocks
{
    [RegisterAchievement("SKIN_MAJOR", "Skins.Ranger.Major", null, null)]
    public class RangerMajorSkinUnlock : BaseAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("RangerBody");
        }

        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            SetServerTracked(true);
        }

        public override void OnBodyRequirementBroken()
        {
            SetServerTracked(false);
            base.OnBodyRequirementBroken();
        }

        public static readonly int requirement = 10;

        public class RangerMajorSkinUnlockServer : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                RoR2Application.onFixedUpdate += RoR2Application_onFixedUpdate;
            }

            private void RoR2Application_onFixedUpdate()
            {
                var currentBody = base.GetCurrentBody();
                if (currentBody && requirement <= currentBody.multiKillCount)
                {
                    Grant();
                }
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
                RoR2Application.onFixedUpdate -= RoR2Application_onFixedUpdate;
            }
        }
    }
}
*/