using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Skills.Ranger.Hooks
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

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchStfld("RoR2.DamageInfo", "damageType")))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<int, RoR2.DamageTrail, int>>((orig, self) =>
                {
                    if (self.gameObject.name == "RangerHeatTrail(Clone)")
                    {
                        return (int)DamageType.IgniteOnHit;
                    }
                    return orig;
                });
            }
        }
    }
}