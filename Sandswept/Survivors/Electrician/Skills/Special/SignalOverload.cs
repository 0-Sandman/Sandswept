using System;
using Sandswept.Survivors.Electrician.States;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class SignalOverload : SkillBase<SignalOverload>
    {
        public override string Name => Random.Range(0f, 100f) >= 98f ? "Sigma Overload" : "Signal Overload";
        public override string Description => "<style=cIsDamage>Grounding.</style> Briefly wind up and drain your $shshield$se, unleashing a $sdbeam of energy$se for $sd3$se seconds that $supulls$se enemies inward and $sdzaps$se them in a large radius for $sd330%-800% damage per second$se, scaling with $shshield$se drained.".AutoFormat();
        // round up this mf from 792% to 800% >:)
        public override Type ActivationStateType => typeof(SignalOverloadCharge);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 12f;
        public override Sprite Icon => Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texElectricianSkillIcon_spc.png");
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;
        public override string[] Keywords => new string[] { "KEYWORD_GROUNDING" };
        public override float GetProcCoefficientData() => 1f;
    }
}