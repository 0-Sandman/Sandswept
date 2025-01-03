using System;

namespace Sandswept.Survivors.Ranger.SkillDefs.Secondary
{
    public class Exhaust : SkillBase<Exhaust>
    {
        public override string Name => "Exhaust";

        public override string Description => "$sdIgnite$se. Fire a short-range heat burst for $sd8x180% damage$se. Increase $srheat$se by $sr15%$se.".AutoFormat();

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