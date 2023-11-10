using System;

namespace Sandswept.Skills.Ranger.Skilldefs
{
    public class Release : SkillBase<Release>
    {
        public override string Name => "Release";

        public override string Description => "$rcLose all Charge$ec. Fire a piercing blast for $sd600% damage$se, increasing up to $sd1500%$se at full $rcCharge$ec.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.Release);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 6f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("Blast.png");

        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;
    }
}