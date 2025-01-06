using Sandswept.Survivors.Ranger.VFX;
using System.Collections;

namespace Sandswept.Survivors.Ranger.States.Secondary
{
    public class Exhaust : BaseState
    {
        public static float DamageCoefficient = 1.5f;
        public static float ProcCoefficient = 1f;
        public static float baseDurationPerVolley = 0.15f;
        public static int baseVolleyCount = 2;
        public int extraVolleyCount;
        public int finalVolleyCount;
        public float durationPerVolley;
        public float finalDuration;
        public bool shot = false;
        private GameObject TracerEffect;
        private GameObject ImpactEffect;
        private RangerHeatController rangerHeatController;
        private Transform modelTransform;

        public override void OnEnter()
        {
            base.OnEnter();

            rangerHeatController = GetComponent<RangerHeatController>();

            durationPerVolley = baseDurationPerVolley / attackSpeedStat;

            extraVolleyCount = (int)Util.Remap(rangerHeatController.currentHeat, 0, 100, 0, 2);
            finalVolleyCount = baseVolleyCount + extraVolleyCount;

            finalDuration = durationPerVolley * finalVolleyCount;

            Util.PlaySound("Play_mage_m2_charge", gameObject);
            Util.PlaySound("Play_mage_m2_charge", gameObject);
            Util.PlaySound("Play_voidBarnacle_m1_chargeUp", gameObject);
            Util.PlaySound("Play_voidBarnacle_m1_chargeUp", gameObject);
            Util.PlaySound("Play_voidBarnacle_m1_chargeUp", gameObject);

            rangerHeatController = GetComponent<RangerHeatController>();

            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                var skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                TracerEffect = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => ExhaustVFX.tracerPrefabMajor,
                    "SKINDEF_RENEGADE" => ExhaustVFX.tracerPrefabRenegade,
                    "SKINDEF_MILEZERO" => ExhaustVFX.tracerPrefabMileZero,
                    "SKINDEF_SANDSWEPT" => ExhaustVFX.tracerPrefabSandswept,
                    _ => ExhaustVFX.tracerPrefabDefault
                };

                ImpactEffect = skinNameToken switch
                {
                    "SKINDEF_MAJOR" => ExhaustVFX.impactPrefabMajor,
                    "SKINDEF_RENEGADE" => ExhaustVFX.impactPrefabRenegade,
                    "SKINDEF_MILEZERO" => ExhaustVFX.impactPrefabMileZero,
                    "SKINDEF_SANDSWEPT" => ExhaustVFX.impactPrefabSandswept,
                    _ => ExhaustVFX.impactPrefabDefault
                };

                PlayAnimation("Gesture, Override", "Fire", "Fire.playbackRate", durationPerVolley);
            }

            outer.StartCoroutine(FireShot());
            rangerHeatController.currentHeat -= 35f;
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

            if (fixedAge < finalDuration || !isAuthority)
            {
                return;
            }

            outer.SetNextStateToMain();
        }

        public IEnumerator FireShot()
        {
            for (int i = 0; i < finalVolleyCount; i++)
            {
                yield return new WaitForSeconds(durationPerVolley);
                characterBody.SetAimTimer(0.2f);

                var aimDirection = GetAimRay().direction;

                BulletAttack attack = new()
                {
                    damage = DamageCoefficient * damageStat,
                    procCoefficient = ProcCoefficient,
                    minSpread = -1.5f,
                    maxSpread = 1.5f,
                    damageType = DamageType.IgniteOnHit,
                    bulletCount = 6,
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

                attack.damageType.damageSource = DamageSource.Secondary;

                Util.PlaySound("Play_wisp_attack_fire", gameObject);
                Util.PlaySound("Play_lemurian_fireball_shoot", gameObject);
                Util.PlaySound("Play_lunar_wisp_attack2_launch", gameObject);
                Util.PlaySound("Play_bleedOnCritAndExplode_impact", gameObject);
                Util.PlaySound("Play_greater_wisp_impact", gameObject);
                Util.PlaySound("Play_item_use_molotov_impact_big", gameObject);
                Util.PlaySound("Play_captain_m1_hit", gameObject);

                attack.Fire();

                AddRecoil(4.5f, 4.5f, 0f, 0f);
                characterMotor?.ApplyForce(-1600f * aimDirection, false, false);
            }
        }
    }
}