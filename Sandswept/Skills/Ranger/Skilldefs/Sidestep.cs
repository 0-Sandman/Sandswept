using JetBrains.Annotations;
using System;

namespace Sandswept.Skills.Ranger.Skilldefs
{
    public class Sidestep : SkillBase<Sidestep>
    {
        public override string Name => "Sidestep";

        public override string Description => "$suAgile$se. Quickly $susidestep$se a short distance. Taking damage while in Sidestep generates $rc10 Charge$ec.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.Sidestep);

        public override string ActivationMachineName => "Dash";

        public override float Cooldown => 5f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("Dash.png");
        public override string[] Keywords => new string[] { "KEYWORD_AGILE" };
        public override bool Agile => true;
        public override bool IsCombat => false;
    }
}