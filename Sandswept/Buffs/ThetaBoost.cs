using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Buffs
{
    public class ThetaBoost : BuffBase<ThetaBoost>
    {
        public override string BuffName => "Theta Shielding";

        public override Color Color => Color.yellow;

        public override Sprite BuffIcon => Main.Assets.LoadAsset<Sprite>("texBuffThetaConstruct.png");

        public override bool CanStack => false;
        public override bool IsDebuff => false;
        public static float AtkSpeedMult = 0.35f;
        public static float CdMult = 0.25f;

        public override void Init()
        {
            base.Init();

            RecalculateStatsAPI.GetStatCoefficients += HandleSpeedBuff;
        }

        private void HandleSpeedBuff(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(BuffDef)) {
                args.attackSpeedMultAdd += AtkSpeedMult;
                args.cooldownMultAdd -= CdMult;
            }
        }
    }
}
