using System;

namespace Sandswept.Survivors.Ranger.SkillDefs.Secondary
{
    public class Exhaust : SkillBase<Exhaust>
    {
        public override string Name => "Exhaust";

        public override string Description => "$sdIgnite$se. Fire $sdtwo$se short bursts of $srheat$se for $sd4x200% damage$se each. $sdBurst count$se increases up to $sdfour$se while in $srfull heat$se. $suReduce$se $srheat$se by $su25%$se.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Secondary.Exhaust);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 4f;

        public override Sprite Icon => Main.hifuSandswept.LoadAsset<Sprite>("texExhaust.png");
        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;

        public override string[] Keywords => new string[] { Utils.Keywords.Ignite };

        public override bool FullRestockOnAssign => true;

        public override void Init()
        {
            base.Init();
        }

        public override void CreateSkillDef()
        {
            skillDef = ScriptableObject.CreateInstance<RangerSecondaryDef>();
        }
    }
}