using Sandswept.Survivors.Ranger.VFX;
using System.Collections;

namespace Sandswept.Survivors.Ranger.States
{
    public class Exhaust : BaseState
    {
        public static float DamageCoefficient = 2f;
        public static float ProcCoefficient = 0.5f;
        public static float baseDuration = 0.15f;
        public float duration;
        public bool shot = false;
        private GameObject TracerEffect;
        private GameObject ImpactEffect;
        private RangerHeatController heat;
        private Transform modelTransform;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;

            Util.PlaySound("Play_mage_m2_charge", gameObject);
            Util.PlaySound("Play_mage_m2_charge", gameObject);
            Util.PlaySound("Play_voidBarnacle_m1_chargeUp", gameObject);
            Util.PlaySound("Play_voidBarnacle_m1_chargeUp", gameObject);
            Util.PlaySound("Play_voidBarnacle_m1_chargeUp", gameObject);

            heat = GetComponent<RangerHeatController>();

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                TracerEffect = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => ExhaustVFX.tracerPrefabMajor,
                    "SKINDEF_RENEGADE" => ExhaustVFX.tracerPrefabRenegade,
                    "SKINDEF_MILEZERO" => ExhaustVFX.tracerPrefabMileZero,
                    _ => ExhaustVFX.tracerPrefabDefault
                };

                ImpactEffect = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => ExhaustVFX.impactPrefabMajor,
                    "SKINDEF_RENEGADE" => ExhaustVFX.impactPrefabRenegade,
                    "SKINDEF_MILEZERO" => ExhaustVFX.impactPrefabMileZero,
                    _ => ExhaustVFX.impactPrefabDefault
                };

                PlayAnimation("Gesture, Override", "Fire", "Fire.playbackRate", duration);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (characterBody)
            {
                characterBody.isSprinting = false;
            }

            if (fixedAge < duration || !isAuthority)
            {
                return;
            }

            characterBody.SetAimTimer(0.2f);

            var aimDirection = GetAimRay().direction;

            BulletAttack attack = new()
            {
                damage = DamageCoefficient * damageStat,
                procCoefficient = ProcCoefficient,
                minSpread = -4f,
                maxSpread = 4f,
                damageType = DamageType.IgniteOnHit,
                bulletCount = 8,
                tracerEffectPrefab = TracerEffect,
                muzzleName = "Muzzle",
                hitEffectPrefab = ImpactEffect,
                falloffModel = BulletAttack.FalloffModel.Buckshot,
                origin = GetAimRay().origin,
                owner = gameObject,
                isCrit = RollCrit(),
                aimVector = aimDirection,
                damageColorIndex = DamageColorIndex.Fragile
            };

            AkSoundEngine.PostEvent(Events.Play_wisp_attack_fire, gameObject);
            AkSoundEngine.PostEvent(Events.Play_lemurian_fireball_shoot, gameObject);
            AkSoundEngine.PostEvent(Events.Play_lunar_wisp_attack2_launch, gameObject);
            AkSoundEngine.PostEvent(Events.Play_bleedOnCritAndExplode_impact, gameObject);
            AkSoundEngine.PostEvent(Events.Play_greater_wisp_impact, gameObject);
            AkSoundEngine.PostEvent(Events.Play_item_use_molotov_impact_big, gameObject);

            attack.Fire();

            heat.currentHeat += 15f;

            AddRecoil(6f, 6f, 0f, 0f);
            characterMotor?.ApplyForce(-2000f * aimDirection, false, false);

            outer.SetNextStateToMain();
        }
    }
}