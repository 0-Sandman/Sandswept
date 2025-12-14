using System;

namespace Sandswept.Survivors.Ranger.SkillDefs.Secondary
{
    public class Char : SkillBase<Char>
    {
        public override string Name => "Char";

        public override string Description => "$sdIgnite$se. $suReduce current heat by 50%$se. Fire off a blazing orb for $sd200%$se damage that $sdengulfs$se the ground on impact for $sd200% damage per second$se. $suDamage increases with heat spent$se.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Secondary.Char);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 4f;

        public override Sprite Icon => Main.assets.LoadAsset<Sprite>("b2.png");
        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;

        public override string[] Keywords => [Utils.Keywords.Ignite];

        public override bool FullRestockOnAssign => true;

        public override void Init()
        {
            base.Init();
        }

        public override void CreateSkillDef()
        {
            skillDef = ScriptableObject.CreateInstance<RangerSecondaryDef>();
        }

        public override float GetProcCoefficientData() => 1f;
    }
}