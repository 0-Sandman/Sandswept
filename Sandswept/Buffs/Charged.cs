namespace Sandswept.Buffs
{
    public class Charged : BuffBase<Charged>
    {
        public override string BuffName => "Charge";

        public override Color Color => new Color32(45, 187, 188, 255);

        public override Sprite BuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texBuffCharged.png");
        public override bool CanStack => true;

        public override void Init()
        {
            base.Init();
            GetStatCoefficients += Charged_GetStatCoefficients;
        }

        private void Charged_GetStatCoefficients(CharacterBody body, StatHookEventArgs args)
        {
            if (NetworkServer.active && body)
            {
                var levelScale = 0.2f * 0.2f * (body.level - 1);
                args.baseRegenAdd += (0.2f + levelScale) * body.GetBuffCount(BuffDef);
            }
        }
    }
}