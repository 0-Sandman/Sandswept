using System;

namespace Sandswept.Survivors.Electrician.SkillsDefs.Primary
{
    public class GalvanicBolt : SkillBase<GalvanicBolt>
    {
        public override string Name => "Galvanic Bolt";
        public override string Description => "Put yo <style=cIsUtility>balls</style> in enemy <style=cIsDamage>jaws</style>.";
        public override Type ActivationStateType => typeof(States.Primary.GalvanicBolt);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 0f;
        public override Sprite Icon => null;
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;
        public override string[] Keywords => null;
    }
}