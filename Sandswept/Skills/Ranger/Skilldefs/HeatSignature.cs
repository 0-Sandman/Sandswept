using System;

namespace Sandswept.Skills.Ranger.Skilldefs
{
    public class HeatSignature : SkillBase<HeatSignature>
    {
        public override string Name => "Heat Signature";

        public override string Description => "$suAgile$se. Toggle to summon $sdfire$se in your wake that deals $sd300% damage per second$se. Increase $sumovement speed$se by $su30%$se. $srGain 20% Heat per second$se.".AutoFormat();

        // 16m radius?
        public override Type ActivationStateType => typeof(States.Ranger.HeatSignature);

        public override string ActivationMachineName => "Dash";

        public override float Cooldown => 8f;

        public override bool BeginCooldownOnSkillEnd => true;
        public override bool MustKeyPress => true;

        public override bool Agile => true;

        public override Sprite Icon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texHeatSignature.png");
        public override string[] Keywords => new string[] { "KEYWORD_AGILE" };
    }
}