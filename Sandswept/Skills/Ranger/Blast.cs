using System;

namespace Sandswept.Skills.Ranger {
    public class Blast : SkillBase<Blast>
    {
        public override string Name => "Release";

        public override string Description => "$srLose all stacks of Charge$se. Fire a blast for $sd500% damage$se and an additional $sd100%$se for every $suCharge$se lost.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.Blast);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 5;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("Blast.png");

    }
}