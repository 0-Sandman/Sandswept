using System;

namespace Sandswept.Buffs
{
    public class SidestepBuff : BuffBase<SidestepBuff>
    {
        public override string BuffName => "Sidestep Boost";

        public override Color Color => Color.blue;

        public override Sprite BuffIcon => null;
    }
}