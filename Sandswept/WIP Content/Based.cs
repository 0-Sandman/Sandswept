using HarmonyLib;
using System.Text.RegularExpressions;
using TMPro;

namespace Sandswept.WIP_Content
{
    public class Based
    {
        public static void Init()
        {
            Main.ModLogger.LogDebug("Based -prod");
            Harmony harmony = new(Main.ModName);
            JoePatch.JOE = Main.Asset2s.LoadAsset<TMP_SpriteAsset>("assets/resources/sprites/joe160.asset");
            On.RoR2.UI.ChatBox.SubmitChat += (orig, self) => { self.inputField.text = Regex.Replace(self.inputField.text, @":joe_\w+:", "<sprite name=\"$0\">"); orig(self); };
            On.RoR2.Util.EscapeRichTextForTextMeshPro += (orig, str) => { 
                return Regex.Replace(orig(str), @"<\/noparse><noparse><<\/noparse><noparse>sprite name="":(joe_\w+):"">", "</noparse><sprite name=\":$1:\"><noparse>");
            };
            harmony.PatchAll(typeof(JoePatch));
            Main.ModLogger.LogDebug("Chad Emoji -prod");
            RoR2Application.onLoad += () =>
            {
                foreach (PickupDef allPickup in PickupCatalog.allPickups)
                    Main.ModLogger.LogDebug(allPickup.internalName);
            };
        }

        public class JoePatch
        {
            public static TMP_SpriteAsset JOE;

            [HarmonyPatch(typeof(TextMeshProUGUI), nameof(TextMeshProUGUI.OnPreRenderCanvas))]
            public static void Prefix(TextMeshProUGUI __instance)
            {
                if (__instance.text.Contains("<sprite name=\":joe_"))
                {
                    __instance.spriteAsset = JOE;
                    __instance.m_spriteAsset = JOE;
                }
            }
        }
    }
}
