using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using RoR2.Skills;

namespace Sandswept.Utils
{
    public static class CharacterExtensions
    {
        /// <summary>Checks if a CharacterBody has a specific SkillDef equipped</summary>
        /// <param name="skill">the SkillDef to check for</param>
        /// <returns>whether or not the SkillDef was equipped</returns>
        public static bool HasSkillEquipped(this CharacterBody body, SkillDef skill)
        {
            foreach (GenericSkill slot in body.GetComponents<GenericSkill>())
            {
                if (slot.skillDef == skill)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Clears the inventory of a CharacterBody</summary>
        public static void ClearInventory(this CharacterBody body)
        {
            if (!body.inventory)
            {
                return;
            }

            List<ItemDef> items = new();

            foreach (ItemIndex item in body.inventory.itemAcquisitionOrder)
            {
                ItemDef def = ItemCatalog.GetItemDef(item);
                items.Add(def);
            }

            foreach (ItemDef item in items)
            {
                body.inventory.RemoveItem(item, body.inventory.GetItemCount(item));
            }
        }

        /// <summary>Clears the inventory of a CharacterBody</summary>
        /// <param name="hidden">whether to clear hidden items</param>
        public static void ClearInventory(this CharacterBody body, bool hidden)
        {
            if (!body.inventory)
            {
                return;
            }

            List<ItemDef> items = new();

            foreach (ItemIndex item in body.inventory.itemAcquisitionOrder)
            {
                ItemDef def = ItemCatalog.GetItemDef(item);
                if (hidden)
                {
                    items.Add(def);
                }
                else
                {
                    if (!def.hidden)
                    {
                        items.Add(def);
                    }
                }
            }

            foreach (ItemDef item in items)
            {
                body.inventory.RemoveItem(item, body.inventory.GetItemCount(item));
            }
        }

        public static void StartParticles(this ChildLocator loc, string system, bool withChildren = true) {
            loc.FindChild(system).GetComponent<ParticleSystem>().Play(withChildren);
        }

        public static void StopParticles(this ChildLocator loc, string system, bool withChildren = true) {
            loc.FindChild(system).GetComponent<ParticleSystem>().Stop(withChildren);
        }
    }
}