using System;

namespace Sandswept.Enemies.CannonballJellyfish
{
    public class SpawnState : BaseState
    {
        public static GameObject spawnEffect = SpawnAndDeath.cannonballJellyfishSpawnVFX;
        public float baseDuration = 0.1f;

        public override void OnEnter()
        {
            base.OnEnter();

            base.GetModelTransform().GetComponent<CharacterModel>().invisibilityCount++;
            base.characterBody.mainHurtBox.hurtBoxGroup.hurtBoxesDeactivatorCounter++;

            EffectManager.SpawnEffect(spawnEffect, new EffectData
            {
                origin = base.transform.position,
                scale = characterBody.bestFitActualRadius * 3f
            }, false);

            Util.PlaySound("Play_item_use_gateway_vanish", base.gameObject);
            Util.PlaySound("Play_arenaCrab_swim_stroke", base.gameObject);
            Util.PlaySound("Play_arenaCrab_swim_stroke", base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.rigidbody.velocity = Vector3.zero;

            if (base.fixedAge >= baseDuration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            base.GetModelTransform().GetComponent<CharacterModel>().invisibilityCount--;
            base.characterBody.mainHurtBox.hurtBoxGroup.hurtBoxesDeactivatorCounter--;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}