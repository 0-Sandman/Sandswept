using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Buffs
{
    public class MegaloCore : BuffBase<MegaloCore>
    {
        public override string BuffName => "Lunar Energy";

        public override Color Color => Color.blue;

        public override Sprite BuffIcon => null;

        public override bool CanStack => true;
        public override bool IsDebuff => false;

        public static float attackSpeedGain = 0.025f;
        public static float regenGain = 0.025f;

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
                args.attackSpeedMultAdd += attackSpeedGain * c;
                args.regenMultAdd += regenGain * c;
            }
        }
    }
}