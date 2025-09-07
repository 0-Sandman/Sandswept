using System;

namespace Sandswept.Enemies.DeltaConstruct
{
    public class SpawnState : BaseState
    {
        public static GameObject spawnEffect = SpawnAndDeath.deltaConstructSpawnAndDeathVFX;
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

            Util.PlaySound("Play_UI_HUD_unfold", base.gameObject);
            Util.PlaySound("Play_UI_HUD_unfold", base.gameObject);
            Util.PlaySound("Play_voidJailer_m1_chargeUp", gameObject);
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