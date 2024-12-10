using System;
using Sandswept.Survivors;

namespace Sandswept.Enemies.CannonballJellyfish.SkillDefs
{
    public class JellyDash : SkillBase<JellyDash>
    {
        public override string Name => "JellyDash";

        public override string Description => "omg hiii :3 :3 :3";

        public override Type ActivationStateType => typeof(States.JellyDash);

        public override string ActivationMachineName => "Body";

        public override float Cooldown => 7f;

        public override Sprite Icon => null;
    }
}