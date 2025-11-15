using System;

namespace Sandswept.Survivors.Ranger.SkillDefs.Special
{
    public class HeatSink : SkillBase<HeatSink>
    {
        public override string Name => "Heat Sink";

        public override string Description => "$suAgile$se. $sdIgnite$se. $suConsume all heat and exit overdrive$se. Release a $sdfire nova$se around you that deals $sd300%$se damage. $suDamage increases with heat spent$se.".AutoFormat();

        // 16m radius?
        public override Type ActivationStateType => typeof(States.Special.HeatSink);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texRangerSkillIcon_hspc.png");
        public override int StockToConsume => 0;

        public override bool MustKeyPress => true;
        public override bool Agile => true;

        public override bool FullRestockOnAssign => false;

        public override string[] Keywords => [Utils.Keywords.Agile, Utils.Keywords.Ignite];
        public override float GetProcCoefficientData() => 1f;
    }
}