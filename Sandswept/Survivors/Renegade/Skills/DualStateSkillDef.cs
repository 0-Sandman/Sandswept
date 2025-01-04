using System;
using JetBrains.Annotations;

namespace Sandswept.Survivors.Renegade {
    public class DualStateSkillDef : SteppedSkillDef {
        public SerializableEntityStateType primaryState;
        public SerializableEntityStateType secondaryState;

        public override void OnFixedUpdate([NotNull] GenericSkill skillSlot, float deltaTime)
        {
            base.OnFixedUpdate(skillSlot, deltaTime);

            if (!skillSlot.characterBody.characterMotor.isGrounded) {
                activationState = secondaryState;
            }
            else {
                activationState = primaryState;
            }
        }
    }
}