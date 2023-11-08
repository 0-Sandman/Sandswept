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
            On.RoR2.GlobalEventManager.OnHitEnemy += ScorchIgnite;
        }

        public static void ScorchIgnite(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo info, GameObject victim) {
            orig(self, info, victim);

            if (!victim || !victim.GetComponent<CharacterBody>()) {
                return;
            }

            BuffIndex index = instance.BuffDef.buffIndex;
            CharacterBody body = victim.GetComponent<CharacterBody>();

            if (body.HasBuff(index)) {
                float damageMult = 0.1f * body.GetBuffCount(index);

                InflictDotInfo dotinfo = new();
                dotinfo.attackerObject = info.attacker;
                dotinfo.totalDamage = info.damage * damageMult;
                dotinfo.damageMultiplier = 1f;
                dotinfo.victimObject = victim;
                dotinfo.preUpgradeDotIndex = DotController.DotIndex.Burn;
                dotinfo.dotIndex = DotController.DotIndex.Burn;

                DotController.InflictDot(ref dotinfo);
            }
        }
    }
}