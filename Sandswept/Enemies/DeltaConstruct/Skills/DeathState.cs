using System;
using R2API.Utils;

namespace Sandswept.Enemies.DeltaConstruct
{
    public class DeathState : GenericCharacterDeath
    {
        public static GameObject deathEffect = SpawnAndDeath.deltaConstructSpawnAndDeathVFX;

        public override void CreateDeathEffects()
        {
            base.CreateDeathEffects();

            characterBody.modelLocator.autoUpdateModelTransform = false;
            cachedModelTransform.parent = null;

            var boxes = cachedModelTransform.GetComponentsInChildren<HurtBox>(true);
            for (int i = 0; i < boxes.Length; i++)
            {
                boxes[i].enabled = false;
                boxes[i].gameObject.SetActive(false);
            }
            cachedModelTransform.GetComponent<RagdollController>().BeginRagdoll(Vector3.one * -5f);

            EffectManager.SpawnEffect(deathEffect, new EffectData
            {
                origin = base.characterBody.corePosition,
                scale = base.characterBody.bestFitRadius * 2f,
            }, false);

            Util.PlaySound("Play_engi_M2_explo", base.gameObject);
            Util.PlaySound("Play_engi_M2_explo", base.gameObject);
            Util.PlaySound("Play_magmaWorm_death_small_explos", base.gameObject);
        }
    }
}