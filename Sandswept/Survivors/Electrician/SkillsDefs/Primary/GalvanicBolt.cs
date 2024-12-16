using System;

namespace Sandswept.Survivors.Electrician.SkillsDefs.Primary
{
    public class GalvanicBolt : SkillBase<GalvanicBolt>
    {
        public override string Name => "Galvanic Bolt";
        public override string Description => "Blast out a galvanic ball that <style=cIsUtility>sticks</style> into terrain, zapping nearby enemies for <style=cIsDamage>150% damage</style> periodically. <style=cIsDamage>Explodes</style> for <style=cIsDamage>200% damage</style> on impact.";
        public override Type ActivationStateType => typeof(States.Primary.GalvanicBolt);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 1.55f;
        public override Sprite Icon => Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texElectricianSkillIcon_m1.png");
        public override int StockToConsume => 1;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;
        public override string[] Keywords => null;
    }
}