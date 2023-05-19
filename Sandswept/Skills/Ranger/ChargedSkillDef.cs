using System;
using JetBrains.Annotations;

namespace Sandswept.Skills {
    public class ChargedSkillDef : SkillDef {
        public static float ReductionPerCharge = 1f;
        public override float GetRechargeInterval([NotNull] GenericSkill skillSlot)
        {
            CharacterBody body = skillSlot.GetComponent<CharacterBody>();
            if (!body) {
                return baseRechargeInterval;
            }

            return Mathf.Max(0.5f, baseRechargeInterval - (ReductionPerCharge * body.GetBuffCount(Buffs.Charged.instance.BuffDef)));
        }
    }
}