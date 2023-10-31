using System;

namespace Sandswept.Skills.Ranger.Skilldefs
{
    public class HeatSignature : SkillBase<HeatSignature>
    {
        public override string Name => "Heat Signature";

        public override string Description => "$suAgile$se. Summon $sdfire$se in your wake that $sdignites$se and deals $sd300% damage per second$se. Increase $sumovement speed$se by $su40%$se. $srConsume 30% heat per second$se. $srTaking damage increases this skill's cooldown$se by $sr2s$se.".AutoFormat();

        // 16m radius?
        public override Type ActivationStateType => typeof(States.Ranger.HeatSignature);

        public override string ActivationMachineName => "Dash";

        public override float Cooldown => 4f;

        public override Sprite Icon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texHeatSignature.png");
        public override string[] Keywords => new string[] { "KEYWORD_AGILE" };
    }
}