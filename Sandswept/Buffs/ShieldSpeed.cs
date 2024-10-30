using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Buffs
{
    public class ShieldSpeed : BuffBase<ShieldSpeed>
    {
        public override string BuffName => "Shield Speed";

        public override Color Color => Color.blue;

        public override Sprite BuffIcon => Utils.Assets.BuffDef.bdCloakSpeed.iconSprite;

        public override bool CanStack => false;
        public override bool IsDebuff => false;
        public static float ShieldSpeedMult = 0.4f;

        public override void Init()
        {
            base.Init();

            RecalculateStatsAPI.GetStatCoefficients += HandleSpeedBuff;
        }

        private void HandleSpeedBuff(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(BuffDef)) {
                args.moveSpeedMultAdd += ShieldSpeedMult;
            }
        }
    }
}
