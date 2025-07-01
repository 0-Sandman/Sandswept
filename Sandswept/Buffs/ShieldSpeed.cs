using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Buffs
{
    public class ShieldSpeed : BuffBase<ShieldSpeed>
    {
        public override string BuffName => "VOL-T Shield Break Movement Speed Bonus";

        public override Color Color => new Color32(0, 77, 255, 255);

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
            if (sender.HasBuff(BuffDef))
            {
                args.moveSpeedMultAdd += ShieldSpeedMult;
            }
        }
    }
}