using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.SkillDefs.Passive
{
    public class RangerPassiveDef: SkillDef
    {
        public event Action onHook = null;
        public event Action onUnhook = null;
        public Action<Run> unhook;
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            onHook?.Invoke();
            unhook = run =>
            {
                onUnhook?.Invoke();
                Run.onRunDestroyGlobal -= unhook;
            };
            Run.onRunDestroyGlobal += unhook;
            return base.OnAssigned(skillSlot);
        }
    }
}
