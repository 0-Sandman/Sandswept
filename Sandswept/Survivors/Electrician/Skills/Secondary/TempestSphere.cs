using System;
using Sandswept.Survivors.Electrician.States;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class TempestSphere : SkillBase<TempestSphere>
    {
        public override string Name => "Tempest Sphere";
        public override string Description => "Send forth an orb of energy that <style=cIsDamage>damages</style> targets within for <style=cIsDamage>300% damage per second</style>. <style=cIsUtility>Release to lock the orb in place</style>.";
        public override Type ActivationStateType => typeof(TempestSphereCharge);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 7f;
        public override Sprite Icon => Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texElectricianSkillIcon_m2.png");
        public override int StockToConsume => 1;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;
        public override string[] Keywords => null;
        public override float GetProcCoefficientData() => 1f;
    }
}