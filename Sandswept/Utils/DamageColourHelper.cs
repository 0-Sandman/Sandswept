using RoR2;
using UnityEngine;
using System.Collections.Generic;

namespace Sandswept.Utils
{
    public static class DamageColourHelper
    {
        internal static void Init()
        {
            On.RoR2.DamageColor.FindColor += DamageColor_FindColor;
        }

        public static List<DamageColorIndex> registeredColorIndicies = new List<DamageColorIndex>();

        private static Color DamageColor_FindColor(On.RoR2.DamageColor.orig_FindColor orig, DamageColorIndex colorIndex)
        {
            if (registeredColorIndicies.Contains(colorIndex)) return DamageColor.colors[(int)colorIndex];
            return orig(colorIndex);
        }

        public static DamageColorIndex RegisterDamageColor(Color color)
        {
            int nextColorIndex = DamageColor.colors.Length;
            DamageColorIndex newDamageColorIndex = (DamageColorIndex)nextColorIndex;
            HG.ArrayUtils.ArrayAppend(ref DamageColor.colors, color);
            registeredColorIndicies.Add(newDamageColorIndex);
            return newDamageColorIndex;
        }
    }
}