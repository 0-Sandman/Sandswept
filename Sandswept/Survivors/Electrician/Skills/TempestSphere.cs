using System;
using Sandswept.Survivors;
using Sandswept.Survivors.Electrician;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class TempestSphere : SkillBase<TempestSphere>
    {
        public override string Name => "Tempest Sphere";
        public override string Description => "winds up briefly before launching out a slow-moving orb that rapidly damages and slows anything inside. the orb travels forward as long as m2 is held, and locks in place when m2 is released";
        public override Type ActivationStateType => typeof(States.TempestSphereCharge);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 7f;
        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;
        public override string[] Keywords => null;
    }
}