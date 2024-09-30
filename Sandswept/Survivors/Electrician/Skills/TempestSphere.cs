using System;
using Sandswept.Survivors;
using Sandswept.Survivors.Electrician;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class TempestSphere : SkillBase<TempestSphere>
    {
        public override string Name => "Tempest Sphere";
        public override string Description => "Throw out yo <style=cDeath>ball</style>.";
        public override Type ActivationStateType => typeof(States.TempestSphereCharge);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 7f;
        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;
        public override string[] Keywords => null;
    }
}