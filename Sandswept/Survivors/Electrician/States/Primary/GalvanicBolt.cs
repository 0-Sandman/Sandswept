using System;

namespace Sandswept.Survivors.Electrician.States
{
    public class GalvanicBolt : BaseSkillState
    {
        public float baseDuration = 0.6f;
        public float duration;

        public Transform modelTransform;

        public GameObject muzzleFlash;
        public GameObject galvanicBoltProjectile;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponent<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                muzzleFlash = skinNameToken switch
                {
                    "VOLT_SKIN_COVENANT_NAME" => VFX.GalvanicBolt.muzzleFlashCovenant,
                    _ => VFX.GalvanicBolt.muzzleFlashDefault
                };

                galvanicBoltProjectile = skinNameToken switch
                {
                    "VOLT_SKIN_COVENANT_NAME" => VFX.GalvanicBolt.projectileCovenant,
                    _ => VFX.GalvanicBolt.projectileDefault
                };

                EffectManager.SimpleMuzzleFlash(muzzleFlash, gameObject, "MuzzleCannon", false);

                if (isAuthority)
                {
                    FireProjectileInfo info = MiscUtils.GetProjectile(galvanicBoltProjectile, 2f, characterBody, DamageTypeCombo.GenericPrimary);

                    GameObject pylon = SearchPylon();
                    if (pylon /*&& !Main.AttackDirectionFixLoaded*/)
                    {
                        info.rotation = Util.QuaternionSafeLookRotation((pylon.transform.position - info.position).normalized);
                    }

                    ProjectileManager.instance.FireProjectile(info);
                }
            }

            characterBody.SetSpreadBloom(12f, true);

            PlayAnimation("Gesture, Override", "ShootLeft", "Generic.playbackRate", duration / 3f);

            // Util.PlaySound("Play_loader_R_shock", base.gameObject);
            // Util.PlayAttackSpeedSound("Play_voidman_m1_shoot", base.gameObject, 0.7f);

            Util.PlaySound("Play_elec_m1_shoot", gameObject);
        }

        public GameObject SearchPylon()
        {
            RaycastHit[] hits = Physics.SphereCastAll(base.inputBank.aimOrigin, 1.5f, base.inputBank.aimDirection, 2000f, LayerIndex.debris.mask, QueryTriggerInteraction.Ignore);
            // 2000f cause epic pillar skip
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];

                if (hit.collider && hit.collider.GetComponent<TripwireController>())
                {
                    return hit.collider.gameObject;
                }
            }

            return null;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}