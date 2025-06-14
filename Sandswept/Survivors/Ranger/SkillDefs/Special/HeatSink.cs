using System;

namespace Sandswept.Survivors.Ranger.SkillDefs.Special
{
    public class HeatSink : SkillBase<HeatSink>
    {
        public override string Name => "Heat Sink";

        public override string Description => "$suAgile$se. $sdIgnite$se. Release a $sdfire nova$se around you that deals $sd300%$se damage, increasing up to $sd900%$se in $srfull heat$se. $suConsume all$se $srheat$se, gaining an $sdattack speed$se boost, and $suexit overdrive$se.".AutoFormat();

        // 16m radius?
        public override Type ActivationStateType => typeof(States.Special.HeatSink);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texRangerSkillIcon_hspc.png");
        public override int StockToConsume => 0;

        public override bool MustKeyPress => true;
        public override bool Agile => true;

        public override bool FullRestockOnAssign => false;

        public override string[] Keywords => new string[] { Utils.Keywords.Agile, Utils.Keywords.Ignite };
        public override float GetProcCoefficientData() => 1f;
    }
}