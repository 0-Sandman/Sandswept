using System;

namespace Sandswept.Skills.Ranger.Skilldefs
{
    public class OverdriveEnter : SkillBase<OverdriveEnter>
    {
        public override string Name => "Overdrive";

        public override string Description => "<color=#36D7A9>Lose all Charge</color>. <style=cIsDamage>Transform your rifle</style>, replacing all of your skills with <style=cIsDamage>scorching forms</style>, lasts longer with each <color=#36D7A9>Charge</style>.";

        public override Type ActivationStateType => typeof(States.Ranger.OverdriveEnter);

        public override string ActivationMachineName => "Overdrive";

        public override float Cooldown => 12f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("OverheatPrimary.png");
        public override string[] Keywords => new string[] { Utils.Keywords.Agile };

        public override bool Agile => true;
        public override bool IsCombat => false;

        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;

        public override void CreateSkillDef()
        {
            skillDef = ScriptableObject.CreateInstance<RangerChargeLockDef>();
        }
    }
}