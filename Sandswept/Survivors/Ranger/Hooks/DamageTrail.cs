using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.Hooks
{
    public static class DamageTrail
    {
        public static void Init()
        {
            IL.RoR2.DamageTrail.DoDamage += DamageTrail_DoDamage;
        }

        private static void DamageTrail_DoDamage(ILContext il)
        {
            ILCursor c = new(il);
            int ld1 = 14; // matches healthcomponent
            int ld2 = 4; // matches damageinfo

            bool found = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(14),
                x => x.MatchLdloc(4),
                x => x.MatchCallOrCallvirt<HealthComponent>(nameof(HealthComponent.TakeDamage))
            );

            if (found)
            {
                c.Emit(OpCodes.Ldloc, 4);
                c.Emit(OpCodes.Ldloc, 14);
                c.EmitDelegate<Action<DamageInfo, HealthComponent>>((info, hc) =>
                {
                    info.damageType = DamageType.IgniteOnHit;
                    info.procCoefficient = 1;

                    GlobalEventManager.instance.OnHitEnemy(info, hc.gameObject);
                });
            }
            else
            {
                Main.ModLogger.LogError("Couldnt apply IL hook for Damage Trail");
            }
        }
    }
}