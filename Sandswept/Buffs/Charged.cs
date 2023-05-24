using System;
using BepInEx.Configuration;

namespace Sandswept.Buffs {
    public class Charged : BuffBase<Charged>
    {
        public override string BuffName => "Charged";

        public override Color Color => Color.cyan;

        public override Sprite BuffIcon => null;
        public override bool CanStack => true;

        public override void Init()
        {
            base.Init();
            RecalculateStatsAPI.GetStatCoefficients += (body, args) => {
                if (NetworkServer.active && body) {
                    args.attackSpeedMultAdd += 0.025f * body.GetBuffCount(BuffDef);
                }
            };
        }
    }
}