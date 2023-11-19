using System;
using Sandswept.Skills.Ranger;
using Sandswept.Survivors;

namespace Sandswept.Survivors.Ranger.Skilldefs
{
    public class Release : SkillBase<Release>
    {
        public override string Name => "Release";

        public override string Description => "$rcLose all Charge$ec. Fire a piercing blast for $sd400% damage$se, increasing up to $sd1600%$se at full $rcCharge$ec.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Release);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 6f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("Blast.png");

        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;

        public override bool FullRestockOnAssign => true;

        public override string[] Keywords => new string[] { Utils.Keywords.OverdriveFormSecondary };

        public override void CreateSkillDef()
        {
            skillDef = ScriptableObject.CreateInstance<RangerSecondaryDef>();
        }
    }
}