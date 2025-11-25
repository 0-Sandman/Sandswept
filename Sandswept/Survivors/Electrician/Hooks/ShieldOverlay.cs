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
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdfld<HealthComponent>("shield"),
                x => x.MatchLdcR4(0f),
                x => x.MatchCgt()))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, CharacterModel, bool>>((orig, characterModel) =>
                {
                    var characterBody = characterModel.body;
                    if (characterBody && characterBody.bodyIndex == Electrician.ElectricianIndex)
                    {
                        return characterBody.healthComponent.shield > 0 && characterBody.maxShield > (characterBody.baseMaxShield + characterBody.levelMaxShield * (characterBody.level - 1));
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