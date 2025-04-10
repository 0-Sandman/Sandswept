﻿using R2API.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept
{
    public static class CursedConfig
    {
        // ideas:

        // pop up that one image when picking up smouldering document and play a vine boom sound

        public static void Init()
        {
            if (Main.cursedConfig.Value)
            {
                // some are in UniversalVIPPass.cs

                LanguageAPI.Add("ITEM_SANDSWEPT_SMOULDERING_DOCUMENT_NAME", "The Nefarious Smouldering Document");
                LanguageAPI.Add("ITEM_SANDSWEPT_AMBER_KNIFE_NAME", "Barrier-focused item mod");
                LanguageAPI.Add("ITEM_SANDSWEPT_RED_SPRING_WATER_NAME", "[CHEESE PUFFS] spring water");
                LanguageAPI.Add("ITEM_SANDSWEPT_CROWNS_DIAMOND_NAME", "Barrier-focused item mod");
                LanguageAPI.Add("ITEM_SANDSWEPT_DRIFTING_PERCEPTION_NAME", "hifu when two music references in one item name");
                LanguageAPI.Add("ITEM_SANDSWEPT_DRIFTING_PERCEPTION_LORE", "Yeah but I just don’t get the point of attaching this to cloak. Cloak is a repositioning utility buff (enemies still ‘see’ you every couple seconds anyway) that helps you in a quick burst in the short term, whereas this effect just screams “I hate cloak, it’s a damage buff now”. Like whats the point of cloaking on entering combat other than brute forcing this ‘synergy’ where you incentivize ignoring the original portion of the mechanic.\r\nTake this with a grain of salt though, I am a horribly pessimistic person who is also extremely critical.\r\n\r\nno offense but I really don't understand what you're trying to mean from any of that\r\n\r\ngives you cloak when you need it to efficiently kill enemies\r\nyet in this context this is no different than it giving its own independent crit chance and damage buff; one could argue the movement speed, and yeah sure, but outside of that, the only reason cloak is doing anything for this scenario is because this item is forcing it to instead be a different buff, with a completely different goal.\r\nlets you use it for mobility outside of combat too\r\nyet it only gives you that mobility when entering combat? And on a cooldown at that? it's like if red whip was only active for 5s then went on a 15s cd \r\nplus you get a bonus synergy from it\r\nyet the only general 'synergy' gotten from implementing this, assuming only sandswept / vanilla (sandswept atm doesn't even have any other cloak items to help realize / justify this concept) is that you can deal bonus crit damage while you're more worried about staying alive when hitting 25% health with stealthkit?\r\nand it sets up other skills and items for that extra crit\r\nI never complained about the interactions or implications of the damage buff part itself, just that it felt really weird shoehorning this effect onto cloak\r\n\r\nIt just feels like this item had the crit chance and damage as an independent buff and then it was just moved into cloak for no good reason, or rather just a \"but what if I did.\"");
                LanguageAPI.Add("ITEM_SANDSWEPT_UNIVERSAL_VIP_PASS_NAME", "Universal VIP Paws");
                LanguageAPI.Add("ITEM_SANDSWEPT_SACRIFICIAL_BAND_NAME", "B(r)and of Sacrifice");
                LanguageAPI.Add("ITEM_SANDSWEPT_TEMPORAL_TRANSISTOR_NAME", "hrt install");
                LanguageAPI.Add("ITEM_SANDSWEPT_CEREMONIAL_JAR_NAME", "The Nefarious Jar");
                LanguageAPI.Add("ITEM_SANDSWEPT_TORN_FEATHER_NAME", "Respect");
                if (Items.Reds.TornFeather.instance != null)
                {
                    Items.Reds.TornFeather.instance.ItemIconOverride = Main.hifuSandswept.LoadAsset<Sprite>("respectPNG.png");
                }
                LanguageAPI.Add("ITEM_SANDSWEPT_THEIR_PROMINENCE_NAME", "must take bruh :pray:");
                LanguageAPI.Add("ITEM_SANDSWEPT_HALLOWED_ICHOR_NAME", "yet another terraria reference (borbo would be proud)");

                LanguageAPI.Add("SS_RANGER_BODY_NAME", "gay little [CHEESE PUFF]");
                LanguageAPI.Add("SS_RANGER_DESCRIPTION", "this is a [CHEESE PUFF]");
                LanguageAPI.Add("SKINDEF_MAJOR", "minor");
                LanguageAPI.Add("SKINDEF_RENEGADE", "holy shit its been more than 2 years and we still havent added renegade thagt this skin is a reference to");
                LanguageAPI.Add("SKINDEF_MILEZERO", "literally just Periphery 2");
                LanguageAPI.Add("ACHIEVEMENT_RANGERCLEARGAMEMONSOON_NAME", "[CHEESE PUFF]: Mastery");
                LanguageAPI.Add("ACHIEVEMENT_RANGERCLEARGAMEMONSOON_DESCRIPTION", "As [CHEESE PUFF], beat the game or obliterate on Monsoon.");

                LanguageAPI.Add("SANDSWEPT_ELECTR_NAME", "(WOKE DEI WARNING!) gay robot [CHEESE PUFF] girl");
                LanguageAPI.Add("SKIN_ELEC_MASTERY", "Covenant (hifu when the skin name is a music reference OMG WOJAK RAISING HANDS JPEG) https://www.youtube.com/watch?v=tjcUCwe4yaY");
                LanguageAPI.Add("ACHIEVEMENT_ELECTRICIANCLEARGAMEMONSOON_NAME", "(WOKE DEI WARNING!) gay robot [CHEESE PUFF] girl: Mastery");

                LanguageAPI.Add("SANDSWEPT_CANNONJELLY_NAME", "[CHEESE PUFF]");

                LanguageAPI.Add("SANDSWEPT_VOLATILECONTEXT", "Forcefully Insert Battery~");
                LanguageAPI.Add("SANDSWEPT_VOLATILEINSERT", "your life is as valuable as a <style=cDeath>summer ant</style>");

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