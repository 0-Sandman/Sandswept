using System;

namespace Sandswept.Buffs
{
    public class Scorched : BuffBase<Scorched>
    {
        public override string BuffName => "Scorched";

        public override Color Color => new Color32(246, 119, 32, 255);
        public override bool CanStack => true;

        public override Sprite BuffIcon => null;

        public override void Hooks()
        {
            base.Hooks();
            On.RoR2.CharacterBody.RecalculateStats += ScorchAway;
        }

        public void ScorchAway(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) {
            orig(self);
            self.armor -= 10 * self.GetBuffCount(BuffDef);
        }
    }
}