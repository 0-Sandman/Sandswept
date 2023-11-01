using System;

namespace Sandswept.Skills.Ranger.Skilldefs
{
    public class OverdriveEnter : SkillBase<OverdriveEnter>
    {
        public override string Name => "Overdrive";

        public override string Description => "$suAgile$se. Transform your rifle into a $sdrapid-fire machine gun$se and fire a rapid stream of $sdbullets$se for $sd200% damage$se. $srContinuous fire will cause overheating, increasing spread, ignite chance and damaging you.$se".AutoFormat();

        // add custom keyword that says it can overheat, but gains increased damage (up to 2x) and spread (up to 3x) at max heat, linearly
        public override Type ActivationStateType => typeof(States.Ranger.OverdriveEnter);

        public override string ActivationMachineName => "Overdrive";

        public override float Cooldown => 12f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("OverheatPrimary.png");
        public override string[] Keywords => new string[] { Utils.Keywords.Agile };

        public override bool Agile => true;
        public override bool IsCombat => false;
    }
}