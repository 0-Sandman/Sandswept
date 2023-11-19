using JetBrains.Annotations;

namespace Sandswept.Skills.Ranger
{
    public class RangerSecondaryDef : SkillDef
    {
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            skillSlot.stock = Mathf.Max(1, Mathf.FloorToInt(skillSlot.maxStock / 2));
            return base.OnAssigned(skillSlot);
        }
    }
}