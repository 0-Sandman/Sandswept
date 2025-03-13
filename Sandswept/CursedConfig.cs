using R2API.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept
{
    public static class CursedConfig
    {
        // ideas:

        // pop up that one image when picking up smouldering document and play a vine boom sound

        // rename volt to gay robot faggot girl (woke DEI)

        public static void Init()
        {
            if (Main.cursedConfig.Value)
            {
                // some are in UniversalVIPPass.cs

                LanguageAPI.Add("ITEM_SANDSWEPT_SMOULDERING_DOCUMENT_NAME", "The Nefarious Smouldering Document");
                LanguageAPI.Add("ITEM_SANDSWEPT_AMBER_KNIFE_NAME", "Barrier-focused item mod");
                LanguageAPI.Add("ITEM_SANDSWEPT_RED_SPRING_WATER_NAME", "faggot spring water");
                LanguageAPI.Add("ITEM_SANDSWEPT_CROWNS_DIAMOND_NAME", "Barrier-focused item mod");
                LanguageAPI.Add("ITEM_SANDSWEPT_DRIFTING_PERCEPTION_NAME", "hifu when two music references in one item name");
                LanguageAPI.Add("ITEM_SANDSWEPT_UNIVERSAL_VIP_PASS_NAME", "Universal VIP Paws");
                LanguageAPI.Add("ITEM_SANDSWEPT_SACRIFICIAL_BAND_NAME", "B(r)and of Sacrifice");
                LanguageAPI.Add("ITEM_SANDSWEPT_TEMPORAL_TRANSISTOR_NAME", "hrt ,,");
                LanguageAPI.Add("ITEM_SANDSWEPT_CEREMONIAL_JAR_NAME", "The Nefarious Jar");
                LanguageAPI.Add("ITEM_SANDSWEPT_TORN_FEATHER_NAME", "Respect");
                if (Items.Reds.TornFeather.instance != null)
                {
                    Items.Reds.TornFeather.instance.SetFieldValue(nameof(Items.Reds.TornFeather.instance.ItemIcon), Main.hifuSandswept.LoadAsset<Sprite>("Respect.png"));
                }
                LanguageAPI.Add("ITEM_SANDSWEPT_THEIR_PROMINENCE_NAME", "kokaina");
                LanguageAPI.Add("ITEM_SANDSWEPT_HALLOWED_ICHOR_NAME", "yet another terraria reference (borbo would be proud)");

                LanguageAPI.Add("SS_RANGER_BODY_NAME", "gay little faggot");
                LanguageAPI.Add("SS_RANGER_DESCRIPTION", "this is a faggot");
                LanguageAPI.Add("SKINDEF_MAJOR", "minor");
                LanguageAPI.Add("SKINDEF_RENEGADE", "holy shit its been more than 2 years and we still havent added renegade that this skin is a reference to");
                LanguageAPI.Add("SKINDEF_MILEZERO", "literally just Periphery 2");

                LanguageAPI.Add("SANDSWEPT_ELECTR_NAME", "(WOKE DEI WARNING!) gay robot faggott girl");
                LanguageAPI.Add("", "");
                LanguageAPI.Add("", "");

                LanguageAPI.Add("SANDSWEPT_CANNONJELLY_NAME", "faggot");

                LanguageAPI.Add("SANDSWEPT_INFERNO_DRONE_BODY", "Can you find the moredrones ?");
                LanguageAPI.Add("SANDSWEPT_INFERNO_DRONE_BROKEN_NAME", "Can you find the moredrones ?");
                LanguageAPI.Add("SANDSWEPT_INFERNO_DRONE_CONTEXT", "Can you find the moredrones ?");

                Main.SandsweptExpansionDef.nameToken.Add("Sandstorm 2");
                Main.SandsweptExpansionDef.descriptionToken.Add("sandswept is pretty mid\r\nsorry\r\nthere are a few cool items but other than those its all just bad or mediocre\r\nthe characters lack polish");

                On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
            }
        }

        private static void Inventory_GiveItem_ItemIndex_int(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            orig(self, itemIndex, count);
            var instance = Items.Greens.SmoulderingDocument.instance;
            if (instance != null && itemIndex == instance.ItemDef.itemIndex)
            {
                // play vine boom
                // show https://i.postimg.cc/0ySDXKxD/Gjh-z-Cw-WIAArjok-1.png on the hud
            }
        }
    }
}