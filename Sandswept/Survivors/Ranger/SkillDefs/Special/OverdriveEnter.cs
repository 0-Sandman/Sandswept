using System;

namespace Sandswept.Survivors.Ranger.SkillDefs.Special
{
    public class OverdriveEnter : SkillBase<OverdriveEnter>
    {
        public override string Name => "Overdrive";

        public override string Description => "$suAgile$se. $sdTransform$se your rifle, replacing all of your skills with $sdoverdriven forms$se.".AutoFormat();

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