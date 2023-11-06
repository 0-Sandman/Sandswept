using System;

namespace Sandswept.Skills.Ranger.Skilldefs
{
    public class Exhaust : SkillBase<Exhaust>
    {
        public override string Name => "Exhaust";

        public override string Description => "Fire a shotgun blast for every $sr20%$se of $srheat$se that deals $sd5x150%$se damage$se.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.Exhaust);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 8f;

        public override Sprite Icon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texRangerIcon.png");
    }
}