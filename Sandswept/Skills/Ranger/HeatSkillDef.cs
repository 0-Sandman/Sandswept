using System;
using JetBrains.Annotations;

namespace Sandswept.Skills.Ranger {
    public class HeatSkillDef : SkillDef {
        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            return false;
        }
    }
}