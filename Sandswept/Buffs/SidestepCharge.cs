using Sandswept.Survivors.Ranger;
using Sandswept.Survivors.Ranger.VFX;

namespace Sandswept.Buffs
{
    public class SidestepCharge : BuffBase<SidestepCharge>
    {
        public override string BuffName => "In Sidestep (Unprocessed In Concretion reference norway??)";

        public override Color Color => new Color32(45, 187, 143, 255);

        public override Sprite BuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("texBuffSidestep.png");
        public override bool CanStack => false;
        public static Material overlayMat1 = SidestepVFX.dashMat1Default;
        public static Material overlayMat2 = SidestepVFX.dashMat2Default;

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
                body.SetBuffCount(Charge.instance.BuffDef.buffIndex, Mathf.Min(Survivors.Ranger.Projectiles.DirectCurrent.maxCharge, body.GetBuffCount(Charge.instance.BuffDef.buffIndex) + 10));
                damageInfo.rejected = true;
            }
            orig(self, damageInfo);
        }
    }
}