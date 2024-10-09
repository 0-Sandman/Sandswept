using System;

namespace Sandswept.Survivors.Ranger.SkillDefs.Special
{
    public class HeatSink : SkillBase<HeatSink>
    {
        public override string Name => "Heat Sink";

        public override string Description => "$suAgile$se. $sdIgnite$se. Release a $sdfire nova$se around you that deals $sd300%$se damage, increasing up to $sd900%$se in full heat. $suConsume all heat$se, gaining an $sdattack speed$se boost, and $srexit overdrive$se.".AutoFormat();

        // 16m radius?
        public override Type ActivationStateType => typeof(States.Special.HeatSink);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Main.hifuSandswept.LoadAsset<Sprite>("texHeatSink.png");
        public override int StockToConsume => 0;

        public override bool MustKeyPress => true;
        public override bool Agile => true;

        public override bool FullRestockOnAssign => false;

        public override string[] Keywords => new string[] { Utils.Keywords.Agile, Utils.Keywords.Ignite };
    }
}