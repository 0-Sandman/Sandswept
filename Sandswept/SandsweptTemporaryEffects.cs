using System;
using Sandswept.Equipment.Standard;

namespace Sandswept {
    public class SandsweptTemporaryEffects : MonoBehaviour {
        public TemporaryVisualEffect GalvanicShield;
        public void UpdateTemporaryEffects(CharacterBody body) {
            body.UpdateSingleTemporaryVisualEffect(ref GalvanicShield, GalvanicCellShield.ShieldEffect, body.bestFitActualRadius * 1.5f, body.HasBuff(Buffs.ParryBuff.instance.BuffDef), "Pelvis");
        }

        internal static void ApplyHooks() {
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