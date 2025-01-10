/*
using HarmonyLib;
using RoR2.UI;
using System.Text.RegularExpressions;
using TMPro;

namespace Sandswept
{
    public class Based
    {
        public static void Init()
        {
            On.RoR2.Chat.AddMessage_string += Chat_AddMessage_string;
            On.RoR2.Util.EscapeRichTextForTextMeshPro += Util_EscapeRichTextForTextMeshPro;
            On.RoR2.UI.ChatBox.Start += ChatBox_Start;
        }

        private static void ChatBox_Start(On.RoR2.UI.ChatBox.orig_Start orig, RoR2.UI.ChatBox self)
        {
            orig(self);
            Transform standardRect = self.transform.Find("StandardRect");
            Transform scrollView = null;
            Transform viewport = null;
            Transform messageArea = null;
            Transform textArea = null;
            Transform text = null;
            HGTextMeshProUGUI hgTextMeshProGUI = null;

            if (standardRect)
            {
                scrollView = standardRect.Find("Scroll View");
            }
            if (scrollView)
            {
                viewport = scrollView.Find("Viewport");
            }
            if (viewport)
            {
                messageArea = viewport.Find("MessageArea");
            }
            if (messageArea)
            {
                textArea = messageArea.Find("Text Area");
            }
            if (textArea)
            {
                text = textArea.Find("Text");
            }
            if (text)
            {
                hgTextMeshProGUI = text.GetComponent<HGTextMeshProUGUI>();
            }
            if (hgTextMeshProGUI)
            {
                hgTextMeshProGUI.spriteAsset = pseudo you mf why is this shit so bad
            }
        }

        private static string Util_EscapeRichTextForTextMeshPro(On.RoR2.Util.orig_EscapeRichTextForTextMeshPro orig, string rtString)
        {
            return rtString;
        }

        private static void Chat_AddMessage_string(On.RoR2.Chat.orig_AddMessage_string orig, string message)
        {
            message = message.Replace("");
            orig(message);
        }
    }
}
*/