using System;
using Sandswept.Survivors;
using Sandswept.Survivors.Electrician;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class StaticSnare : SkillBase<StaticSnare>
    {
        public override string Name => "Static Snare";
        public override string Description => "throw out a pylon that occasionally blasts for damage and is tethered to you. enemies hit by the tether are damaged. activate the skill again to travel along the tether (you become like a ball of light) damaging everything in your path, once you arrive at the pylon it explodes";
        public override Type ActivationStateType => typeof(States.StaticSnare);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 8f;
        public override int MaxStock => 1;
        public override Sprite Icon => null;
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;
        public override string[] Keywords => null;
    }
}