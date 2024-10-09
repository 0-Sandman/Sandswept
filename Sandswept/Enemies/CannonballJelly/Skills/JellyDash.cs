using System;
using Sandswept.Survivors;

namespace Sandswept.Enemies.CannonJellyfish {
    public class JellyDash : SkillBase<JellyDash>
    {
        public override string Name => "JellyDash";

        public override string Description => "you shouldnt see this.";

        public override Type ActivationStateType => typeof(States.JellyDash);

        public override string ActivationMachineName => "Body";

        public override float Cooldown => 3f;

        public override Sprite Icon => null;
    }
}