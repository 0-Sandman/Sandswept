using Sandswept.Skills.Ranger.VFX;

namespace Sandswept.Buffs
{
    public class SidestepCharge : BuffBase<SidestepCharge>
    {
        public override string BuffName => "In Sidestep (Unprocessed In Concretion reference norway??)";

        public override Color Color => new Color32(45, 187, 143, 255);

        public override Sprite BuffIcon => Assets.BuffDef.bdArmorBoost.iconSprite;
        public override bool CanStack => false;
        public static Material overlayMat1 = SidestepVFX.dashMat1;
        public static Material overlayMat2 = SidestepVFX.dashMat2;

        public override void Init()
        {
            base.Init();
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var body = self.body;
            if (body.HasBuff(instance.BuffDef) && damageInfo.procCoefficient > 0f && damageInfo.attacker != null)
            {
                body.SetBuffCount(Charge.instance.BuffDef.buffIndex, 10);
                // damageInfo.rejected = true;
            }
            orig(self, damageInfo);
        }
    }
}