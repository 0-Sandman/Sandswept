using Sandswept.Buffs;

namespace Sandswept.Survivors.Ranger.SkillDefs.Passive
{
    public class OverchargedProtection : SkillBase<OverchargedProtection>
    {
        public override string Name => "Overcharged Protection";
        public override string Description => "Hold up to " + Projectiles.DirectCurrent.maxCharge + " $rcCharge$ec. $rcCharge$ec increases $shhealth regeneration$se by up to $sh2.5 hp/s$se and $sharmor$se by up to $sh15$se. $rcCharge decays over time$ec.".AutoFormat();
        public override Type ActivationStateType => typeof(GenericCharacterMain);
        public override string ActivationMachineName => "Body";
        public override float Cooldown => 0f;
        public override Sprite Icon => Main.assets.LoadAsset<Sprite>("Overheat.png");
        public override int StockToConsume => 0;
        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;

        public override void CreateSkillDef()
        {
            skillDef = ScriptableObject.CreateInstance<RangerPassiveDef>();
            var passive = (RangerPassiveDef)skillDef;
            passive.onHook += () =>
            {
                GetStatCoefficients += Charged_GetStatCoefficients;
            };
            passive.onUnhook += () =>
            {
                GetStatCoefficients -= Charged_GetStatCoefficients;
            };
        }

        private void Charged_GetStatCoefficients(CharacterBody body, StatHookEventArgs args)
        {
            if (body)
            {
                var levelScale = 0.125f * 0.2f * (body.level - 1);
                args.baseRegenAdd += (0.125f + levelScale) * body.GetBuffCount(Charge.instance.BuffDef);
                args.armorAdd += 0.75f * body.GetBuffCount(Charge.instance.BuffDef);
            }
        }
    }
}