using JetBrains.Annotations;

namespace Sandswept.Survivors.Ranger
{
    public class RangerChargeLockDef : SkillDef
    {
        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return base.IsReady(skillSlot) && skillSlot.characterBody?.GetBuffCount(Buffs.Charge.instance.BuffDef) > 0;
        }
    }
}