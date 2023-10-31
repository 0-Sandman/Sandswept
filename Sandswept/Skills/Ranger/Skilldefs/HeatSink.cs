using System;

namespace Sandswept.Skills.Ranger.Skilldefs
{
    public class HeatSink : SkillBase<HeatSink>
    {
        public override string Name => "Heat Sink";

        public override string Description => "Release a $sdfire nova$se around you that deals $sd500%$se base damage, increasing by up to $sd1000%$se in full heat. $srConsumes all heat$se, $srdamages you$se for $sr25% max health$se and $srexits overdrive$se.".AutoFormat();

        // 16m radius?
        public override Type ActivationStateType => typeof(States.Ranger.HeatSink);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texHeatSink.png");
        public override int StockToConsume => 0;
    }
}