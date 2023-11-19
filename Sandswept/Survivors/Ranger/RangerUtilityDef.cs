using JetBrains.Annotations;

namespace Sandswept.Skills.Ranger
{
    public class RangerUtilityDef : SkillDef
    {
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            skillSlot.stock = Mathf.Max(1, Mathf.CeilToInt(skillSlot.maxStock / 2));
            Main.ModLogger.LogError("max stock" + skillSlot.maxStock);
            Main.ModLogger.LogError("max stock / 2" + skillSlot.maxStock / 2);
            Main.ModLogger.LogError("max stock / 2 ceiled to int" + Mathf.CeilToInt(skillSlot.maxStock / 2));
            Main.ModLogger.LogError("max stock / 2 ceiled to int and math max" + Mathf.Max(1, Mathf.CeilToInt(skillSlot.maxStock / 2)));
            return base.OnAssigned(skillSlot);
        }
    }
}