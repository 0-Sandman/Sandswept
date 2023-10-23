using System;
using BepInEx.Configuration;

namespace Sandswept.Buffs
{
    public class Charged : BuffBase<Charged>
    {
        public override string BuffName => "Charged";

        public override Color Color => new Color32(45, 187, 188, 255);

        public override Sprite BuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texBuffCharged.png");
        public override bool CanStack => true;

        public override void Init()
        {
            base.Init();
            GetStatCoefficients += (body, args) =>
            {
                if (NetworkServer.active && body)
                {
                    args.attackSpeedMultAdd += 0.025f * body.GetBuffCount(BuffDef);
                }
            };
        }
    }
}