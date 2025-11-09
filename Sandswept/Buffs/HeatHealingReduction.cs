using System;
using Sandswept.Survivors.Ranger.SkillDefs.Passive;

namespace Sandswept.Buffs
{
    public class HeatHealingReduction : BuffBase<HeatHealingReduction>
    {
        public override string BuffName => "Heat Healing Reduction";

        public override Color Color => new Color32(246, 119, 32, 255);
        public override bool CanStack => true;

        public override Sprite BuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("texHeatHealingReduction.png");

        public override void Hooks()
        {
            base.Hooks();
            On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
        }

        private float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            var body = self.body;
            var buffCount = body.GetBuffCount(BuffDef);

            var reduction = 1f - (buffCount * 0.01f);

            reduction = Mathf.Max(0, reduction);

            if (nonRegen)
            {
                amount *= reduction;
            }
            else {
                if (!procChainMask.HasModdedProc(OverchargedProtection.RangerPassiveOverchargedProtection.OverchargeRegen)) {
                    amount *= reduction;
                }
            }

            return orig(self, amount, procChainMask, nonRegen);
        }
    }
}