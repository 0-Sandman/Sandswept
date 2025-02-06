using System;
using R2API.Utils;

namespace Sandswept.Enemies.CannonballJellyfish
{
    [ConfigSection("Enemies :: Cannonball Jellyfish")]
    public class JellyDeath : GenericCharacterDeath
    {
        public static LazyAddressable<GameObject> DeathEffect = new(() => Paths.GameObject.ExplosivePotExplosion);

        [ConfigField("Death Projectile Damage Coefficient", "Decimal.", 3f)]
        public static float DamageCoefficient;

        public override void CreateDeathEffects()
        {
            base.CreateDeathEffects();

            for (int i = 0; i < 2; i++)
            {
                EffectManager.SpawnEffect(Paths.GameObject.ExplosivePotExplosion, new EffectData
                {
                    origin = base.characterBody.corePosition,
                    scale = base.characterBody.bestFitRadius * 2f,
                }, false);
            }

            base.DestroyModel();

            if (base.isAuthority)
            {
                var proj = MiscUtils.GetProjectile(CannonballJellyfish.JellyCoreProjectile, DamageCoefficient, base.characterBody);
                ProjectileManager.instance.FireProjectile(proj);
            }
        }
    }
}