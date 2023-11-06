using System;
using JetBrains.Annotations;
using Sandswept.Components;

namespace Sandswept.Skills.Ranger {
    public class RangerStunDef : SkillDef {
        protected class InstanceData : BaseSkillInstanceData {
            public RangerHeatManager heatManager;

            public InstanceData(RangerHeatManager heatManager) {
                this.heatManager = heatManager;
            }
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData(skillSlot.GetComponent<RangerHeatManager>());
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return base.IsReady(skillSlot) && !((InstanceData)skillSlot.skillInstanceData).heatManager.isInStun;
        }
    }
}