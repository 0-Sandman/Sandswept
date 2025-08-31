using JetBrains.Annotations;
using System;

namespace Sandswept.Survivors.Ranger.SkillDefs.Utility
{
    public class Sidestep : SkillBase<Sidestep>
    {
        public override string Name => "Sidestep";

        public override string Description => "$suAgile$se. Quickly $susidestep$se a short distance. Getting hit during $rcSidestep$ec generates $rc10 Charge$ec.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Utility.Sidestep);

        public override string ActivationMachineName => "Dash";

        public override float Cooldown => 5f;

        public override Sprite Icon => Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texRangerSkillIcon_utl.png");
        public override string[] Keywords => new string[] { Utils.Keywords.Agile, Utils.Keywords.OverdriveFormUtility };

        public override bool FullRestockOnAssign => true;
        public override bool Agile => true;
        public override bool IsCombat => false;

        public override bool MustKeyPress => true;

        public override void CreateSkillDef()
        {
            skillDef = ScriptableObject.CreateInstance<RangerUtilityDef>();
        }
    }
}