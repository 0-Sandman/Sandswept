using System;

namespace Sandswept.Survivors.Ranger.SkillDefs.Special
{
    public class OverdriveEnter : SkillBase<OverdriveEnter>
    {
        public override string Name => "Overdrive";

        public override string Description => "<style=cIsUtility>Agile</style>. <style=cIsDamage>Transform your rifle</style>, replacing all of your skills with <style=cIsDamage>scorching forms</style>. <color=#36D7A9>Charge</color> amplifies <style=cIsDamage>base damage gain</style> while in full heat.";

        public override Type ActivationStateType => typeof(States.Special.OverdriveEnter);

        public override string ActivationMachineName => "Overdrive";

        public override float Cooldown => 13f;

        public override Sprite Icon => Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texRangerSkillIcon_spc.png");
        public override string[] Keywords => new string[] { Utils.Keywords.Agile, Utils.Keywords.OverdriveFormSpecial };

        public override bool Agile => true;
        public override bool IsCombat => false;

        public override bool FullRestockOnAssign => false;

        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;

        /*
        public override void CreateSkillDef()
        {
            skillDef = ScriptableObject.CreateInstance<RangerChargeLockDef>();
        }
        */
    }
}