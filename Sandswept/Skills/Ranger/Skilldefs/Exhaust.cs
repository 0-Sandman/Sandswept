using System;

namespace Sandswept.Skills.Ranger.Skilldefs
{
    public class Exhaust : SkillBase<Exhaust>
    {
        public override string Name => "Exhaust";

        public override string Description => "$sdIgnite$se. Fire a short-range heat spread for $sd8x140% damage$se. Gain $sr20% heat$se.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Ranger.Exhaust);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 4f;

        public override Sprite Icon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texExhaust.png");

        public override void Init()
        {
            base.Init();
        }
    }
}