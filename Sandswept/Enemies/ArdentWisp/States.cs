using System;
using R2API.Utils;

namespace Sandswept.Enemies.ArdentWisp
{
    public class ArdentWispDeath : GenericCharacterDeath
    {
        public static LazyAddressable<GameObject> DeathEffect = new(() => Paths.GameObject.OmniExplosionVFXGreaterWisp);

        public override void CreateDeathEffects()
        {
            base.CreateDeathEffects();

            GetModelAnimator().SetBool("dying", true);

            PlayAnimation("Body", "Death", "Generic.playbackRate", 2f);

            EffectManager.SpawnEffect(DeathEffect, new EffectData
            {
                origin = base.characterBody.corePosition,
                scale = base.characterBody.bestFitRadius * 2f,
            }, false);

            Util.PlaySound("Play_minorConstruct_attack_explode", base.gameObject);
        }
    }
}