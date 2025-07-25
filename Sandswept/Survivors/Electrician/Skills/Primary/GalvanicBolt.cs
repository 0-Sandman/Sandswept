using System;
using LookingGlass.ItemStatsNameSpace;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class GalvanicBolt : SkillBase<GalvanicBolt>
    {
        public override string Name => "Galvanic Bolt";
        public override string Description => "Blast out a <style=cIsDamage>galvanic ball</style> that <style=cIsUtility>sticks</style> into terrain, <style=cIsDamage>zapping</style> nearby enemies for <style=cIsDamage>100% damage</style> periodically. <style=cIsDamage>Explodes</style> for <style=cIsDamage>400% damage</style> on impact.";
        public override Type ActivationStateType => typeof(States.GalvanicBolt);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 1.55f;
        public override Sprite Icon => Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texElectricianSkillIcon_m1.png");
        public override int StockToConsume => 1;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;
        public override string[] Keywords => null;
        public override float GetProcCoefficientData() => 1f;
    }
}