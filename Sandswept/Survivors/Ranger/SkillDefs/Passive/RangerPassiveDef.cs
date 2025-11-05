using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.SkillDefs.Passive
{
    public class RangerPassiveDef : SkillDef
    {
        public event Func<GenericSkill, BaseSkillInstanceData> onAssigned = null;
        public event Action<GenericSkill> onUnassigned = null;
        public Action<Run> unhook;
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return onAssigned?.Invoke(skillSlot) ?? base.OnAssigned(skillSlot);
        }
        public override void OnUnassigned([NotNull] GenericSkill skillSlot)
        {
            onUnassigned?.Invoke(skillSlot);
        }
    }
}
