using JetBrains.Annotations;
using Sandswept.Skills.Ranger;
using Sandswept.Survivors;
using System;

namespace Sandswept.Survivors.Ranger.Skilldefs
{
    public class Sidestep : SkillBase<Sidestep>
    {
        public override string Name => "Sidestep";

        public override string Description => "$suAgile$se. Quickly $susidestep$se a short distance. Getting hit during Sidestep generates $rc5 Charge$ec.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Sidestep);

        public override string ActivationMachineName => "Dash";

        public override float Cooldown => 5f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("Dash.png");
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