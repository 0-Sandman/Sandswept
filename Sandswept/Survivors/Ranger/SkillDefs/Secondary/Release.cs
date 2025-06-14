using System;

namespace Sandswept.Survivors.Ranger.SkillDefs.Secondary
{
    public class Release : SkillBase<Release>
    {
        public override string Name => "Release";

        public override string Description => "$rcConsume all Charge$ec. Fire a piercing blast for $sd600% damage$se, increasing up to $sd1800%$se at full $rcCharge$ec.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Secondary.Release);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 6f;

        public override Sprite Icon => Main.assets.LoadAsset<Sprite>("Blast.png");

        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;

        public override bool FullRestockOnAssign => true;

        public override string[] Keywords => new string[] { Utils.Keywords.OverdriveFormSecondary };

        public override void CreateSkillDef()
        {
            skillDef = ScriptableObject.CreateInstance<RangerSecondaryDef>();
        }

        public override float GetProcCoefficientData() => 1f;
    }
}