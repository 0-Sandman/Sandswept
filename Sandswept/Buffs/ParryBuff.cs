using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Buffs
{
    public class ParryBuff : BuffBase<ParryBuff>
    {
        public override string BuffName => "Galvanic Cell Shield - Parry";

        public override Color Color => Color.magenta;

        public override Sprite BuffIcon => Utils.Assets.BuffDef.bdHiddenInvincibility.iconSprite;

        public override bool CanStack => false;
        public override bool IsDebuff => false;

        public override void Init()
        {
            base.Init();
        }
    }
}