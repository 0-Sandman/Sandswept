using System;

namespace Sandswept.Survivors.Ranger.SkillDefs.Secondary
{
    public class Char : SkillBase<Char>
    {
        public override string Name => "Char";

        public override string Description => "$sdIgnite$se. Fire off a $sdblazing ball$se for $sd600%$se damage that $sdengulfs$se the ground on impact for $sd250%$se damage per second. $suReduce$se $srheat$se by $su50%$se.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Secondary.Char);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 4f;

        public override Sprite Icon => Main.Assets.LoadAsset<Sprite>("b2.png");
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