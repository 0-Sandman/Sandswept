using System;
using JetBrains.Annotations;

namespace Sandswept.Survivors.Renegade {
    public class AmmoSkillDef : SkillDef {
        public override void OnFixedUpdate([NotNull] GenericSkill skillSlot, float deltaTime)
        {
            // skip fixedupdate so we dont automatically reload
        }
    }
}