using System;

namespace Sandswept.Buffs
{
    public class OverheatDamageBoost : BuffBase<OverheatDamageBoost>
    {
        public override string BuffName => "Overheat Damage Boost - 1% Per";

        public override Color Color => new Color32(246, 119, 32, 255);
        public override bool CanStack => true;

        public override Sprite BuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("texBuffScorched.png");

        public override void Hooks()
        {
            base.Hooks();
            GetStatCoefficients += Charged_GetStatCoefficients;
        }

        private void Charged_GetStatCoefficients(CharacterBody body, StatHookEventArgs args)
        {
            if (body)
            {
                // args.damageMultAdd += 0.06f * body.GetBuffCount(instance.BuffDef) + 0.06f * body.GetBuffCount(instance.BuffDef) * 0.1f * body.GetBuffCount(Charge.instance.BuffDef);
                args.damageMultAdd += 0.01f * body.GetBuffCount(instance.BuffDef);
            }
        }
    }
}