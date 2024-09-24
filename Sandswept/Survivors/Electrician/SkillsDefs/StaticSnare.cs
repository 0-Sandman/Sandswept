using System;
using Sandswept.Survivors;
using Sandswept.Survivors.Electrician;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class StaticSnare : SkillBase<StaticSnare>
    {
        public override string Name => "Static Snare";
        public override string Description => "Toss out a pair of yo <style=cDeath>balls</style>.";
        public override Type ActivationStateType => typeof(States.StaticSnare);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 8f;
        public override int MaxStock => 2;
        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;
        public override string[] Keywords => null;
    }
}