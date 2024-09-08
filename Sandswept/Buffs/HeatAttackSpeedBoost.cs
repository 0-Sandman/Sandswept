namespace Sandswept.Buffs
{
    public class HeatAttackSpeedBoost : BuffBase<HeatAttackSpeedBoost>
    {
        public override string BuffName => "Heat Attack Speed Boost - 50%";

        public override Color Color => new Color32(246, 119, 32, 255);

        public override Sprite BuffIcon => Paths.BuffDef.bdAttackSpeedOnCrit.iconSprite;
        public override bool CanStack => false;
        public override bool Hidden => false;

        public override void Init()
        {
            base.Init();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody body, StatHookEventArgs args)
        {
            if (body && body.HasBuff(instance.BuffDef))
            {
                args.baseAttackSpeedAdd += 0.5f;
            }
        }
    }
}