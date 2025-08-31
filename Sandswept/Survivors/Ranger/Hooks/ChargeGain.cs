using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.Hooks
{
    public static class ChargeGain
    {
        public static void Init()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private static void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var damageInfo = report.damageInfo;

            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var buffCount = attackerBody.GetBuffCount(Buffs.Charge.instance.BuffDef);

            if (damageInfo.HasModdedDamageType(Projectiles.DirectCurrent.chargeOnHit) && buffCount <= (Projectiles.DirectCurrent.maxCharge - 1))
            {
                attackerBody.AddBuff(Buffs.Charge.instance.BuffDef);
            }

            if (damageInfo.HasModdedDamageType(Projectiles.DirectCurrent.chargeOnHitDash))
            {
                attackerBody.SetBuffCount(Buffs.Charge.instance.BuffDef.buffIndex, Mathf.Min(Projectiles.DirectCurrent.maxCharge, buffCount + 6));
            }
        }
    }
}