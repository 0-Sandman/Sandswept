using Sandswept.Survivors.Ranger.SkillDefs.Secondary;

namespace Sandswept.Survivors.Ranger.SkillDefs.Secondary
{
    public class Galvanize : SkillBase<Galvanize>
    {
        public override string Name => "Galvanize";

        public override string Description => "$rcLose all Charge$ec. Fire $sd3$se orbs of electricity, increasing up to $sd10$se at full $rcCharge$ec for $sd40%$se damage per second each.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Secondary.Galvanize);

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