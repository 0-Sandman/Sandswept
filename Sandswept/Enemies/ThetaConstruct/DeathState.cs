using System;
using R2API.Utils;

namespace Sandswept.Enemies.ThetaConstruct
{
    public class DeathState : GenericCharacterDeath
    {
        public static GameObject deathEffect = SpawnAndDeath.thetaConstructSpawnAndDeathVFX;

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

            Util.PlaySound("Play_scav_attack1_explode", base.gameObject);
        }
    }
}