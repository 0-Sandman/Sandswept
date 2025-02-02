using System;

namespace Sandswept.Survivors.Megalomaniac.Skills
{
    public class Expel : SkillBase<Expel>
    {
        public override string Name => "Expel";
        public override string Description => "<style=cDeath>40% HP.</style> Tear off your core and lob it as a bomb that explodes in a large radius for <style=cIsDamage>600% damage</style> and sends out flame waves for <style=cIsDamage>8x300% damage</style>.";
        public override Type ActivationStateType => typeof(States.Expel);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 12f;
        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;
        public override string[] Keywords => null;
    }
}