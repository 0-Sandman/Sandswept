using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors
{
    public static class SelfDamageHook
    {
        public static void Init()
        {
            IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
        }

        private static void HealthComponent_TakeDamageProcess(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(0.4f)))
            {
                c.Index++;
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<float, DamageInfo, float>>((orig, self) =>
                {
                    if (self.HasModdedDamageType(Main.eclipseSelfDamage))
                    {
                        return 0f;
                    }
                    return orig;
                });
            }
            else
            {
                Main.ModLogger.LogError("Failed to apply Ranger Self Damage Eclipse 8 hook");
            }
        }
    }
}