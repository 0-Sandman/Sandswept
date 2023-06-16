using System;

namespace Sandswept.Buffs
{
    public class SidestepBuff : BuffBase<SidestepBuff>
    {
        public override string BuffName => "Sidestep Boost";

        public override Color Color => Color.blue;

        public override Sprite BuffIcon => null;

        public override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += BoostDamage;
        }

        public void BoostDamage(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args) {
            if (NetworkServer.active && body.HasBuff(BuffDef)) {
                args.damageMultAdd += 2;
            }
        }
    }
}