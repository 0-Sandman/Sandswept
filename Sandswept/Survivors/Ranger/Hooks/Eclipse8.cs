using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.Hooks
{
    public static class Eclipse8
    {
        public static void Init()
        {
            IL.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private static void HealthComponent_TakeDamage(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(0.4f)))
            {
                c.Index++;
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<float, DamageInfo, float>>((orig, self) =>
                {
                    if (self.HasModdedDamageType(Main.HeatSelfDamage))
                    {
                        return 0f;
                    }
                    return orig;
                });
            }
        }
    }
}