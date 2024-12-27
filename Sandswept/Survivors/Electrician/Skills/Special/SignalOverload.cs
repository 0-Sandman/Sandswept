using System;
using Sandswept.Survivors.Electrician.States;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class SignalOverload : SkillBase<SignalOverload>
    {
        public override string Name => Random.Range(0f, 100f) >= 99f ? "Sigma Overload" : "Signal Overload";
        public override string Description => "<style=cIsDamage>Grounding.</style> Wind up briefly and beam energy forward, <style=cIsDamage>zapping</style> enemies in a large radius for <style=cIsDamage>1600% damage</style> and <style=cIsUtility>pulling them inward</style> over <style=cIsDamage>4 seconds</style>. Charging drains <style=cDeath>shield</style>, and skill effect ramps from <style=cDeath>0.4x</style> to <style=cIsDamage>1.5x</style> based on shield drained.";
        public override Type ActivationStateType => typeof(SignalOverloadCharge);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 12f;
        public override Sprite Icon => Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texElectricianSkillIcon_spc.png");
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;
        public override string[] Keywords => new string[] { "KEYWORD_GROUNDING" };
    }
}