using RoR2.Achievements;

namespace Sandswept.Survivors
{
    public class BaseMasteryAchievement : BaseAchievement
    {
        [SystemInitializer(typeof(SearchableAttribute.OptInAttribute))]
        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            Run.onClientGameOverGlobal += OnClientGameOverGlobal;
        }

        [SystemInitializer(typeof(SearchableAttribute.OptInAttribute))]
        public override void OnBodyRequirementBroken()
        {
            Run.onClientGameOverGlobal -= OnClientGameOverGlobal;
            base.OnBodyRequirementBroken();
        }

        [SystemInitializer(typeof(SearchableAttribute.OptInAttribute))]
        private void OnClientGameOverGlobal(Run run, RunReport runReport)
        {
            if (!runReport.gameEnding)
            {
                return;
            }
            if (runReport.gameEnding.isWin)
            {
                var difficultyIndex = runReport.ruleBook.FindDifficulty();
                DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(difficultyIndex);
                if (difficultyDef != null && difficultyDef.countsAsHardMode)
                {
                    Grant();
                }
            }
        }
    }
}