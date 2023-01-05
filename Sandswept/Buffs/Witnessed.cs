using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using static R2API.DotAPI;
using static Sandswept.Items.BloodMask;

namespace Sandswept.Buffs
{
    internal class Witnessed : BuffBase<Witnessed>
    {
        public static DotController.DotIndex WitnessedIndex;

        public static DotController.DotDef WitnessedDef;

        public override string BuffName => "Witnessed";

        public override Color Color => new Color(215f, 0f, 0f, 255f);

        public override Sprite BuffIcon => null;

        public bool isDebuff => false;

        public bool canStack => false;

        public override void Hooks()
        {
        }

        public override void Init(ConfigFile config)
        {
            WitnessedDef = new DotController.DotDef
            {
                associatedBuff = base.BuffDef,
                damageCoefficient = 1f,
                damageColorIndex = DamageColorIndex.Bleed,
                interval = 0.25f
            };
            DotController.DotDef dotDef = WitnessedDef;
            CustomDotBehaviour behaviour = delegate (DotController self, DotController.DotStack dotStack)
            {
                DotController.DotStack dotInfo = dotStack;

                if (!self.victimBody.HasBuff(RoR2Content.Buffs.Bleeding) || !self.victimBody.HasBuff(RoR2Content.Buffs.SuperBleed))
                {
                    self.RemoveDotStackAtServer((int)dotInfo.dotIndex);
                }

                CharacterBody attacker = dotStack.attackerObject.GetComponent<CharacterBody>();
                CharacterBody victim = self.victimObject.GetComponent<CharacterBody>();

                var token = attacker.gameObject.GetComponent<WitnessToken>();

                dotInfo.damage = (self.victimHealthComponent ? (self.victimHealthComponent.fullCombinedHealth * (0.01f * token.stacks)) : 0);
            };
            WitnessedIndex = RegisterDotDef(dotDef, behaviour, null);
            
        }
    } 
}
