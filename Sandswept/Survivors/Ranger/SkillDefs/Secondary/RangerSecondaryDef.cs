using JetBrains.Annotations;

namespace Sandswept.Survivors.Ranger.SkillDefs.Secondary
{
    public class RangerSecondaryDef : SkillDef
    {
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            skillSlot.stock = Mathf.Max(1, Mathf.FloorToInt(skillSlot.maxStock / 2f));
            return base.OnAssigned(skillSlot);
        }
    }
}