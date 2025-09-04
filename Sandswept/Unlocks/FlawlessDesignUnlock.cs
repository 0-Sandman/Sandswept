using RoR2.Achievements;

namespace Sandswept.Unlocks
{
    [RegisterAchievement("EQUIPMENT_SANDSWEPT_FLAWLESS_DESIGN", "EQUIPMENT_SANDSWEPT_FLAWLESS_DESIGN", null, 5, null)]
    public class FlawlessDesignUnlock : BaseAchievement
    {
        public static List<ItemDef> requiredScrap = new();
        public static List<GameEndingDef> acceptableEndings = new(); // feel free to add anything here aside from actual, full-on losses
        public override void OnInstall()
        {
            base.OnInstall();
            acceptableEndings.Add(RoR2Content.GameEndings.MainEnding);
            acceptableEndings.Add(RoR2Content.GameEndings.LimboEnding);
            acceptableEndings.Add(RoR2Content.GameEndings.ObliterationEnding);
            acceptableEndings.Add(DLC1Content.GameEndings.VoidEnding);
            acceptableEndings.Add(DLC2Content.GameEndings.RebirthEndingDef); // bit of a stretch but hey

            requiredScrap.Add(RoR2Content.Items.ScrapWhite);
            // requiredScrap.Add(RoR2Content.Items.ScrapGreen); special case because of regenerating scrap
            requiredScrap.Add(RoR2Content.Items.ScrapRed);
            requiredScrap.Add(RoR2Content.Items.ScrapYellow);

            Run.onClientGameOverGlobal += OnClientGameOverGlobal;
        }
        public void OnClientGameOverGlobal(Run run, RunReport report)
        {
            if (ShouldGrant(report))
            {
                Grant();
            }
        }

        public bool ShouldGrant(RunReport runReport)
        {
            var body = localUser.cachedBody;
            if (acceptableEndings.Contains(runReport.gameEnding) && body)
            {
                // Main.ModLogger.LogError("found body");
                var inventory = body.inventory;
                if (inventory)
                {
                    // Main.ModLogger.LogError("found inventory");

                    var passesListCheck = true;

                    foreach (var scrap in requiredScrap)
                    {
                        if (inventory.GetItemCount(scrap) <= 0)
                        {
                            passesListCheck = false;
                            // Main.ModLogger.LogError("failed to pass required scrap check");
                        }
                    }

                    var passesSpecialCaseCheck = inventory.GetItemCount(RoR2Content.Items.ScrapGreen) > 0 || inventory.GetItemCount(DLC1Content.Items.RegeneratingScrap) > 0;

                    if (!passesSpecialCaseCheck)
                    {
                        // Main.ModLogger.LogError("failed to pass special case scrap check");
                    }

                    return passesListCheck && passesSpecialCaseCheck;
                }
            }
            return false;
        }

        public override void OnUninstall()
        {
            Run.onClientGameOverGlobal -= OnClientGameOverGlobal;
            base.OnUninstall();
        }
    }
}