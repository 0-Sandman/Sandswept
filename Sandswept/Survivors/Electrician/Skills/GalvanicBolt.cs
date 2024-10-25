using System;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class GalvanicBolt : SkillBase<GalvanicBolt>
    {
        public override string Name => "Galvanic Bolt";
        public override string Description => "launches out balls with a slow fire rate which stick into terrain and repeatedly zap up to 3 nearby enemies. if the ball impacts an enemy and not terrain, it makes a mini explosion and drops to the ground, halting all momentum";
        public override Type ActivationStateType => typeof(States.GalvanicBolt);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 1.8f;
        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;
        public override string[] Keywords => null;
    }
}