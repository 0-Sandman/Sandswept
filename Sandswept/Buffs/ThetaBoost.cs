using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Buffs
{
    [ConfigSection("Enemies :: Theta Construct")]
    public class ThetaBoost : BuffBase<ThetaBoost>
    {
        public override string BuffName => "Theta Construct Shielding";

        public override Color Color => Color.yellow;

        public override Sprite BuffIcon => Main.assets.LoadAsset<Sprite>("texBuffThetaConstruct.png");

        public override bool CanStack => true;
        public override bool IsDebuff => false;

        [ConfigField("Shield Buff Attack Speed Gain", "", 0.35f)]
        public static float shieldBuffAttackSpeedGain;

        [ConfigField("Shield Buff Cooldown Reduction", "Decimal.", 0.25f)]
        public static float shieldBuffCooldownReduction;

        public override void Init()
        {
            base.Init();

            RecalculateStatsAPI.GetStatCoefficients += HandleSpeedBuff;
        }

        private void HandleSpeedBuff(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(BuffDef))
            {
                int c = sender.GetBuffCount(BuffDef);
                args.attackSpeedMultAdd += shieldBuffAttackSpeedGain * c;
                args.cooldownMultAdd -= shieldBuffCooldownReduction * c;
            }
        }
    }
}