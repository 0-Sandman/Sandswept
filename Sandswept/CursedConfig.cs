using R2API.Utils;
using Sandswept.Equipment;
using Sandswept.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

namespace Sandswept
{
    public static class CursedConfig
    {
        public static LazyIndex VoltIndex = new("ElectricianBody");

        public static void Init()
        {
            // Main.ModLogger.LogError("cursed config init");
            if (Main.cursedConfig.Value)
            {
                // Main.ModLogger.LogError("cursed config is enabled");
                // some are in UniversalVIPPass.cs

                LanguageAPI.AddOverlay("ITEM_SANDSWEPT_SMOULDERING_DOCUMENT_NAME", "The Nefarious Smouldering Document");
                LanguageAPI.AddOverlay("ITEM_SANDSWEPT_AMBER_KNIFE_NAME", "Barrier-focused item mod");
                LanguageAPI.AddOverlay("ITEM_SANDSWEPT_RED_SPRING_WATER_NAME", "[CHEESE PUFF] spring water");
                LanguageAPI.AddOverlay("ITEM_SANDSWEPT_CROWNS_DIAMOND_NAME", "Barrier-focused item mod");
                LanguageAPI.AddOverlay("ITEM_SANDSWEPT_DRIFTING_PERCEPTION_NAME", "hifu when two music references in one item name");
                LanguageAPI.AddOverlay("ITEM_SANDSWEPT_DRIFTING_PERCEPTION_LORE", "Yeah but I just don't get the point of attaching this to cloak. Cloak is a repositioning utility buff (enemies still 'see' you every couple seconds anyway) that helps you in a quick burst in the short term, whereas this effect just screams \"I hate cloak, it's a damage buff now\". Like whats the point of cloaking on entering combat other than brute forcing this 'synergy' where you incentivize ignoring the original portion of the mechanic.\r\nTake this with a grain of salt though, I am a horribly pessimistic person who is also extremely critical.\r\n\r\nno offense but I really don't understand what you're trying to mean from any of that\r\n\r\ngives you cloak when you need it to efficiently kill enemies\r\nyet in this context this is no different than it giving its own independent crit chance and damage buff; one could argue the movement speed, and yeah sure, but outside of that, the only reason cloak is doing anything for this scenario is because this item is forcing it to instead be a different buff, with a completely different goal.\r\nlets you use it for mobility outside of combat too\r\nyet it only gives you that mobility when entering combat? And on a cooldown at that? it's like if red whip was only active for 5s then went on a 15s cd \r\nplus you get a bonus synergy from it\r\nyet the only general 'synergy' gotten from implementing this, assuming only sandswept / vanilla(sandswept atm doesn't even have any other cloak items to help realize / justify this concept) is that you can deal bonus crit damage while you're more worried about staying alive when hitting 25 % health with stealthkit ?\r\nand it sets up other skills and items for that extra crit\r\nI never complained about the interactions or implications of the damage buff part itself, just that it felt really weird shoehorning this effect onto cloak\r\n\r\nIt just feels like this item had the crit chance and damage as an independent buff and then it was just moved into cloak for no good reason, or rather just a \"but what if I did.\"");
                LanguageAPI.AddOverlay("ITEM_SANDSWEPT_UNIVERSAL_VIP_PASS_NAME", "Universal VIP Paws");
                LanguageAPI.AddOverlay("ITEM_SANDSWEPT_SACRIFICIAL_BAND_NAME", "B(r)and of Sacrifice");
                LanguageAPI.AddOverlay("ITEM_SANDSWEPT_TEMPORAL_TRANSISTOR_NAME", "hrt install");
                LanguageAPI.AddOverlay("ITEM_SANDSWEPT_CEREMONIAL_JAR_NAME", "The Nefarious Jar");
                LanguageAPI.AddOverlay("ITEM_SANDSWEPT_TORN_FEATHER_NAME", "Respect");

                SwapIcon(Items.Reds.TornFeather.instance, "respectPNG.png");
                SwapIcon(Items.NoTier.TwistedWound.instance, "texItemTemp.png");
                SwapIcon(Equipment.Lunar.FlawlessDesign.instance, "texObama.png");

                LanguageAPI.AddOverlay("ITEM_SANDSWEPT_THEIR_PROMINENCE_NAME", "must take bruh :pray:");
                LanguageAPI.AddOverlay("ITEM_SANDSWEPT_HALLOWED_ICHOR_NAME", "yet another terraria reference (borbo would be proud)");

                LanguageAPI.AddOverlay("SS_RANGER_BODY_NAME", "gay little [CHEESE PUFF]");
                LanguageAPI.AddOverlay("SS_RANGER_DESCRIPTION", "this is a [CHEESE PUFF]");
                LanguageAPI.AddOverlay("RANGER_SKIN_MAJOR_NAME", "minor");
                LanguageAPI.AddOverlay("RANGER_SKIN_RENEGADE_NAME", "holy shit its been more than 2 years and we still havent added renegade thagt this skin is a reference to");
                LanguageAPI.AddOverlay("RANGER_SKIN_MILEZERO_NAME", "literally just Periphery 2");
                LanguageAPI.AddOverlay("ACHIEVEMENT_RANGERCLEARGAMEMONSOON_NAME", "[CHEESE PUFF]: Mastery");
                LanguageAPI.AddOverlay("ACHIEVEMENT_RANGERCLEARGAMEMONSOON_DESCRIPTION", "As [CHEESE PUFF], beat the game or obliterate on Monsoon.");

                LanguageAPI.AddOverlay("SANDSWEPT_ELECTR_NAME", "(WOKE DEI WARNING!) gay robot [CHEESE PUFF] girl");
                LanguageAPI.AddOverlay("VOLT_SKIN_COVENANT_NAME", "Covenant (hifu when the <content type> name is a music reference OMG WOJAK RAISING HANDS JPEG) https://www.youtube.com/watch?v=tjcUCwe4yaY");
                LanguageAPI.AddOverlay("ACHIEVEMENT_ELECTRICIANCLEARGAMEMONSOON_NAME", "(WOKE DEI WARNING!) gay robot [CHEESE PUFF] girl: Mastery");

                LanguageAPI.AddOverlay("SANDSWEPT_CANNONJELLY_NAME", "[CHEESE PUFF]");
                LanguageAPI.AddOverlay("SANDSWEPT_SWEPSWEP_NAME", "https://www.youtube.com/watch?v=XYCQcy76100");

                LanguageAPI.AddOverlay("SANDSWEPT_VOLATILECONTEXT", "Forcefully Insert Battery~");
                LanguageAPI.AddOverlay("SANDSWEPT_VOLATILEINSERT", "your life is as valuable as a <style=cDeath>summer ant</style>");

                LanguageAPI.AddOverlay("SANDSWEPT_INFERNO_DRONE_BODY", "Can you find the moredrones ?");
                LanguageAPI.AddOverlay("SANDSWEPT_INFERNO_DRONE_BROKEN_NAME", "Can you find the moredrones ?");
                LanguageAPI.AddOverlay("SANDSWEPT_INFERNO_DRONE_CONTEXT", "Can you find the moredrones ?");

                LanguageAPI.AddOverlay("SANDSWEPT_VOLTAIC_DRONE_BODY", "Can you find the moredrones ?");
                LanguageAPI.AddOverlay("SANDSWEPT_VOLTAIC_DRONE_BROKEN_NAME", "Can you find the moredrones ?");
                LanguageAPI.AddOverlay("SANDSWEPT_VOLTAIC_DRONE_CONTEXT", "Can you find the moredrones ?");

                Main.SandsweptExpansionDef.nameToken.AddOverlay("Sandstorm 2");
                Main.SandsweptExpansionDef.descriptionToken.AddOverlay("sandswept is pretty mid\r\nsorry\r\nthere are a few cool items but other than those its all just bad or mediocre\r\nthe characters lack polish");

                On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
                GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            }
        }

        private static void SwapIcon(ItemBase instance, string path)
        {
            if (Items.ItemBase.DefaultEnabledCallback(instance))
            {
                instance.ItemDef.pickupIconSprite = Main.hifuSandswept.LoadAsset<Sprite>(path);
            }
        }

        private static void SwapIcon(EquipmentBase instance, string path)
        {
            if (Equipment.EquipmentBase.DefaultEnabledCallback(instance))
            {
                instance.EquipmentDef.pickupIconSprite = Main.hifuSandswept.LoadAsset<Sprite>(path);
            }
        }

        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            if (attackerBody.bodyIndex != VoltIndex.Value)
            {
                return;
            }

            if (!Util.CheckRoll(3f))
            {
                return;
            }

            Chat.AddMessage("<color=#7D42B2>Cheese puff girl</color>: Wlasnie zabilam kiegos chlopa czy tam babe :3");
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

    public class EggController : MonoBehaviour
    {
        private float decayStopwatch = 0.5f;
        private bool decaying = false;
        private Vector3 baseScale;
        //
        private Vector3 tl;
        private Vector3 bl;
        private Vector3 tr;
        private Vector3 br;
        internal Vector3 velocity;
        private float boundsSize;
        public void Start()
        {
            baseScale = base.transform.localScale;

            bl = Camera.main.ViewportToScreenPoint(new Vector3(0f, 0f));
            tl = Camera.main.ViewportToScreenPoint(new Vector3(0f, 1f));
            br = Camera.main.ViewportToScreenPoint(new Vector3(1f, 0f));
            tr = Camera.main.ViewportToScreenPoint(new Vector3(1f, 1f));

            BoxCollider2D col = base.GetComponent<BoxCollider2D>();
            boundsSize = col.bounds.size.magnitude / 2f;
        }
        public void OnMouseDown()
        {
            if (!decaying)
            {
                decaying = true;
            }
        }

        public void Update()
        {
            if (decaying)
            {
                decayStopwatch -= Time.deltaTime;

                if (decayStopwatch <= 0f)
                {
                    Destroy(base.gameObject);
                }

                base.transform.localScale = baseScale * (1f - (decayStopwatch / 0.5f));
            }
        }

        public void FixedUpdate()
        {
            base.transform.position += velocity * Time.fixedDeltaTime;

            Vector3 point = Camera.main.WorldToScreenPoint(base.transform.position) + (boundsSize * velocity.normalized);

            if (point.y >= tr.y)
            {
                velocity = Vector3.Reflect(velocity, Vector3.down);
            }

            if (point.y <= br.y)
            {
                velocity = Vector3.Reflect(velocity, Vector3.up);
            }

            if (point.x >= br.x)
            {
                velocity = Vector3.Reflect(velocity, Vector3.left);
            }

            if (point.x <= bl.x)
            {
                velocity = Vector3.Reflect(velocity, Vector3.right);
            }
        }
    }
}