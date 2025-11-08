using System;

namespace Sandswept.Survivors.Ranger.SkillDefs.Secondary
{
    public class Exhaust : SkillBase<Exhaust>
    {
        public override string Name => "Exhaust";

        public override string Description => "$sdIgnite$se. $suReduce current heat by 30%$se. Fire $sda burst of heat$se for $sd4x200% damage$se. $sdBurst count increases with heat spent$se.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Secondary.Exhaust);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 5f;

        public override Sprite Icon => Main.hifuSandswept.LoadAsset<Sprite>("texExhaust.png");
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