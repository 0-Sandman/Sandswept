using System;
using System.Collections.Generic;
using System.Text;
using RoR2.Achievements;

namespace Sandswept.Survivors.Electrician.Achievements
{
    [RegisterAchievement("ElectricianRepair", "Unlocks.Electrician.Sandswept", null, 3, typeof(RepairElecAchievementServer))]
    public class RepairElecAchievement : BaseAchievement
    {
        public class RepairElecAchievementServer : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                BrokenElecController.OnUserUnlock += OnUnlocked;
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
                BrokenElecController.OnUserUnlock -= OnUnlocked;
            }

            public void OnUnlocked(CharacterBody body)
            {
                CharacterBody current = serverAchievementTracker.networkUser.GetCurrentBody();

                if (current && body == current)
                {
                    base.Grant();
                }
            }
        }

        public override void OnInstall()
        {
            base.OnInstall();
            SetServerTracked(true);
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
        }
    }
}