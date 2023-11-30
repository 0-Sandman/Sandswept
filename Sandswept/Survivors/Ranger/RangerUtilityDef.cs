using JetBrains.Annotations;

namespace Sandswept.Skills.Ranger
{
    public class RangerUtilityDef : SkillDef
    {
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            skillSlot.stock = Mathf.Max(1, Mathf.CeilToInt(skillSlot.maxStock / 2f));
            return base.OnAssigned(skillSlot);
        }
    }
}