using System;

namespace Sandswept.Buffs
{
    public class SidestepBuff : BuffBase<SidestepBuff>
    {
        public override string BuffName => "Sidestep Boost";

        public override Color Color => new Color32(45, 188, 148, 255);

        public override Sprite BuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texBuffSidestep2.png");

        public override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += BoostDamage;
        }

        public void BoostDamage(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (NetworkServer.active && body.HasBuff(BuffDef))
            {
                args.damageMultAdd += 2f;
            }
        }
    }
}