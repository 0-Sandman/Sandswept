using System;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class GalvanicBolt : SkillBase<GalvanicBolt>
    {
        public override string Name => "Galvanic Bolt";
        public override string Description => "Blast out a galvanic ball that <style=cIsUtility>sticks</style> into terrain, zapping nearby enemies for <style=cIsDamage>200% damage</style> periodically. <style=cIsUtility>Explodes</style> for <style=cIsDamage>200% damage</style> on impact.";
        public override Type ActivationStateType => typeof(States.GalvanicBolt);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 1.8f;
        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;
        public override string[] Keywords => null;
    }
}