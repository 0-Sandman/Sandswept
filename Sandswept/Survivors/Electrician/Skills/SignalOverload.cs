using System;
using Sandswept.Survivors;
using Sandswept.Survivors.Electrician;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class SignalOverload : SkillBase<SignalOverload>
    {
        public override string Name => (UnityEngine.Random.Range(0f, 100f) >= 99f) ? "Sigma Overload" : "Signal Overload";
        public override string Description => "<style=cIsDamage>Grounding.</style> Charge up and beam energy forward, zapping enemies in a large radius around impact for <style=cIsDamage>1800% damage</style> and <style=cIsUtility>pulling them inward</style> over <style=cIsDamage>4 seconds</style>.";
        public override Type ActivationStateType => typeof(States.SignalOverloadCharge);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 12f;
        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;
        public override string[] Keywords => new string[] { "KEYWORD_GROUNDING" };
    }
}