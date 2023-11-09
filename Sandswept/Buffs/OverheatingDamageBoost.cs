namespace Sandswept.Buffs
{
    public class OverheatingDamageBoost : BuffBase<OverheatingDamageBoost>
    {
        public override string BuffName => "Overheating Damage Boost";

        public override Color Color => new Color32(45, 187, 188, 255);

        public override Sprite BuffIcon => null;
        public override bool CanStack => true;
        public override bool Hidden => true;

        public override void Init()
        {
            base.Init();
            GetStatCoefficients += Charged_GetStatCoefficients;
        }

        private void Charged_GetStatCoefficients(CharacterBody body, StatHookEventArgs args)
        {
            if (body)
            {
                args.damageMultAdd += 0.04f * body.GetBuffCount(instance.BuffDef);
            }
        }
    }
}