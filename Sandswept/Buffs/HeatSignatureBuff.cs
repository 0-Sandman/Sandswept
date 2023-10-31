using System;

namespace Sandswept.Buffs
{
    public class HeatSignatureBuff : BuffBase<HeatSignatureBuff>
    {
        public override string BuffName => "Sidestep Boost";

        public override Color Color => new Color32(188, 96, 45, 255);

        public override Sprite BuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texBuffSidestep.png");

        public override void Hooks()
        {
            base.Hooks();
            GetStatCoefficients += Grantspeed;
        }

        public void Grantspeed(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (NetworkServer.active && body.HasBuff(BuffDef))
            {
                args.moveSpeedMultAdd += 0.4f;
            }
        }
    }
}