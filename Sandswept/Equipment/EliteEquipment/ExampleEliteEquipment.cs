using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace Sandswept.Equipment.EliteEquipment
{
    internal class ExampleEliteEquipment : EliteEquipmentBase<ExampleEliteEquipment>
    {
        public override string EliteEquipmentName => "Their Instruction";

        public override string EliteAffixToken => "AFFIX_EXAMPLE";

        public override string EliteEquipmentPickupDesc => "Become an aspect of teaching.";

        public override string EliteEquipmentFullDescription => "";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Tutorialized";

        public override GameObject EliteEquipmentModel => new GameObject();

        public override Sprite EliteEquipmentIcon => null;

        public override Sprite EliteBuffIcon => null;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            CreateEliteTiers();
            CreateElite();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {

        }

        private void CreateEliteTiers()
        {
            //For this, if you want to create your own elite tier def to place your elite, you can do it here.
            //Otherwise, don't set CanAppearInEliteTiers and it will appear in the first vanilla tier.

            //In this we create our own tier which we'll put our elites in. It has:
            //- 6 times the base elite cost.
            //- It can only become available to spawn after the player has looped at least once.

            //Additional note: since this accepts an array, it supports multiple elite tier defs, but do not put a cost of 0 on the cost multiplier.

            CanAppearInEliteTiers = new CombatDirector.EliteTierDef[]
            {
                new CombatDirector.EliteTierDef()
                {
                    costMultiplier = CombatDirector.baseEliteCostMultiplier * 6,
                    eliteTypes = Array.Empty<EliteDef>(),
                    isAvailable = SetAvailability
                }
            };
        }

        private bool SetAvailability(SpawnCard.EliteRules arg)
        {
            return Run.instance.loopClearCount > 0 && arg == SpawnCard.EliteRules.Default;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {

        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }
    }
}
