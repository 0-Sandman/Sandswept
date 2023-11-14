using System;
using Sandswept.Survivors;

namespace Sandswept.Survivors.Ranger.Skilldefs
{
    public class HeatSink : SkillBase<HeatSink>
    {
        public override string Name => "Heat Sink";

        public override string Description => "$suAgile$se. $sdIgnite$se. Release a $sdfire nova$se around you that ignites and deals $sd300%$se damage, increasing up to $sd900%$se in full heat. $suConsumes all heat$se and $srexits overdrive$se.".AutoFormat();

        // 16m radius?
        public override Type ActivationStateType => typeof(States.HeatSink);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texHeatSink.png");
        public override int StockToConsume => 0;

        public override bool MustKeyPress => true;
        public override bool Agile => true;

        public override string[] Keywords => new string[] { Utils.Keywords.Agile, Utils.Keywords.Ignite };
    }
}