using System;

namespace Sandswept.Skills.Ranger.Skilldefs
{
    public class Release : SkillBase<Release>
    {
        public override string Name => "Release";

        public override string Description => "$srLose all Charge$se. Fire a piercing blast for $sd500% damage$se and an additional $sd100%$se for every $rcCharge$erc lost.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.Release);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 5f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("Blast.png");
    }
}