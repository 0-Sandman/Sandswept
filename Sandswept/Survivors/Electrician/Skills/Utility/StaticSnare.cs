using System;

namespace Sandswept.Survivors.Electrician.Skills
{
    public class StaticSnare : SkillBase<StaticSnare>
    {
        public override string Name => "Static Snare";
        public override string Description => "<style=cIsUtility>Lightweight.</style> Deploy a pylon that <style=cIsDamage>zaps</style> targets for <style=cIsDamage>200% damage</style>, and damages targets between you and it for <style=cIsDamage>200% damage per second</style>. Reactivate to <style=cIsUtility>zip</style> to the pylon for <style=cIsDamage>600% damage</style>. <style=cIsUtility>Hold to manually detonate the pylon.</style>";
        public override Type ActivationStateType => typeof(States.StaticSnare);
        public override string ActivationMachineName => "Weapon";
        public override float Cooldown => 8f;
        public override int MaxStock => 1;
        public override Sprite Icon => Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texElectricianSkillIcon_utl.png");
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;
        public override string[] Keywords => new string[] { "KEYWORD_LIGHTWEIGHT" };
        public override float GetProcCoefficientData() => 1f;
    }
}