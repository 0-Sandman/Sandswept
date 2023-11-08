using System;

namespace Sandswept.Buffs
{
    public class Scorched : BuffBase<Scorched>
    {
        public override string BuffName => "Scorched";

        public override Color Color => new Color32(246, 119, 32, 255);
        public override bool CanStack => true;

        public override Sprite BuffIcon => null;

        public override void Hooks()
        {
            base.Hooks();
            // On.RoR2.CharacterBody.RecalculateStats += ScorchAway;
            On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
            GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender)
            {
                args.armorAdd -= sender.armor * 0.1f * sender.GetBuffCount(BuffDef);
            }
        }

        private float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            var body = self.body;
            amount = amount * (10 - body.GetBuffCount(BuffDef)) * 0.1f;
            return orig(self, amount, procChainMask, nonRegen);
        }

        public void ScorchAway(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            self.armor -= 10 * self.GetBuffCount(BuffDef);
        }
    }
}