using System;
using Sandswept.Equipment.Standard;
using Sandswept.Items.Greens;
using static Sandswept.Items.Greens.MakeshiftPlate;

namespace Sandswept
{
    public class SandsweptTemporaryEffects : MonoBehaviour
    {
        public TemporaryVisualEffect makeshiftPlate;

        public void UpdateTemporaryEffects(CharacterBody body)
        {
            body.UpdateSingleTemporaryVisualEffect(ref makeshiftPlate, platingOverlayPrefab, MathHelpers.BestBestFitRadius(body), body.GetComponent<PlatingManager>() && body.GetComponent<PlatingManager>().CurrentPlating > 0 && body.inventory && body.inventory.GetItemCount(MakeshiftPlate.instance.ItemDef) > 0);
        }

        internal static void ApplyHooks()
        {
            On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += OnUpdateVisualEffects;
        }

        private static void OnUpdateVisualEffects(On.RoR2.CharacterBody.orig_UpdateAllTemporaryVisualEffects orig, CharacterBody self)
        {
            orig(self);

            SandsweptTemporaryEffects effects = self.GetComponent<SandsweptTemporaryEffects>();
            if (!effects) effects = self.AddComponent<SandsweptTemporaryEffects>();

            effects.UpdateTemporaryEffects(self);
        }
    }
}