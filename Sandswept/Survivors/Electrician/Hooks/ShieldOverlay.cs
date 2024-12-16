using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Electrician.Hooks
{
    public static class ShieldOverlay
    {
        public static void Init()
        {
            IL.RoR2.CharacterModel.UpdateOverlays += UpdateOverlays;
            IL.RoR2.CharacterModel.UpdateOverlayStates += UpdateOverlayStates;
        }

        private static void UpdateOverlayStates(ILContext il)
        {
            ILCursor c = new(il);
            TryModifyOverlay(c, 1);
        }

        private static void UpdateOverlays(ILContext il)
        {
            ILCursor c = new(il);
            TryModifyOverlay(c, 2);
        }

        private static void TryModifyOverlay(ILCursor c, int hookNumber)
        {
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdfld<HealthComponent>("shield"),
                x => x.MatchLdcR4(0f)))
            {
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, CharacterModel, float>>((orig, characterModel) =>
                {
                    var characterBody = characterModel.body;
                    if (characterBody.bodyIndex == Electrician.ElectricianIndex)
                    {
                        // Main.ModLogger.LogError("playing electrician, updating shield overlay - hook #1");
                        return characterBody.baseMaxShield + characterBody.levelMaxShield * (characterBody.level - 1);
                    }
                    return orig;
                });
            }
            else
            {
                Main.ModLogger.LogError("Failed to apply Electrician Shield Overlay " + hookNumber + "hook");
            }
        }
    }
}