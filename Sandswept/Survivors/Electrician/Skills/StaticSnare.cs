using System;
using Sandswept.Survivors;
using Sandswept.Survivors.Electrician;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class StaticSnare : SkillBase<StaticSnare>
    {
        public override string Name => "Static Snare";
        public override string Description => "<style=cIsUtility>Lightweight.</style> Toss out a pylon that shocks nearby targets for <style=cIsDamage>300% damage</style> periodically, and creates a chain between you that damages targets for <style=cIsDamage>200% damage per second</style>. Re-activate to zip to the pylon, dealing <style=cIsDamage>800% damage</style> when passing through targets.";
        public override Type ActivationStateType => typeof(States.StaticSnare);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 8f;
        public override int MaxStock => 1;
        public override Sprite Icon => null;
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;
        public override string[] Keywords => new string[] { "KEYWORD_LIGHTWEIGHT" };
    }
}