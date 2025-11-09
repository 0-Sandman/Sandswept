using Sandswept.Survivors.Ranger.VFX;
using System.Collections;

namespace Sandswept.Survivors.Ranger.States.Secondary
{
    public class Exhaust : BaseState
    {
        public static float damageCoefficient = 2f;
        public static float procCoefficient = 1f;
        public static float baseDurationPerVolley = 0.3f;
        public static int baseVolleyCount = 1;
        public static float heatPerExtraVolley = 20f;
        public static float heatReduction = 0.5f;
        public int finalVolleyCount;
        public float durationPerVolley;
        public float finalDuration;
        public GameObject tracerEffect;
        public GameObject impactEffect;
        public RangerHeatController rangerHeatController;
        public Transform modelTransform;
        public bool canExitState = false;

        public override void OnEnter()
        {
            base.OnEnter();

            rangerHeatController = GetComponent<RangerHeatController>();

            float heatRemoved = rangerHeatController.currentHeat * heatReduction;
            rangerHeatController.currentHeat -= Mathf.Max(0, heatRemoved);

            durationPerVolley = baseDurationPerVolley / attackSpeedStat;

            finalVolleyCount = baseVolleyCount + Mathf.FloorToInt(heatRemoved / heatPerExtraVolley);
            Main.ModLogger.LogError("final volley count is " + finalVolleyCount);

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

                tracerEffect = skinNameToken switch
                {
                    "RANGER_SKIN_MAJOR_NAME" => ExhaustVFX.tracerPrefabMajor,
                    "RANGER_SKIN_RENEGADE_NAME" => ExhaustVFX.tracerPrefabRenegade,
                    "RANGER_SKIN_MILEZERO_NAME" => ExhaustVFX.tracerPrefabMileZero,
                    "RANGER_SKIN_SANDSWEPT_NAME" => ExhaustVFX.tracerPrefabSandswept,
                    _ => ExhaustVFX.tracerPrefabDefault
                };

                impactEffect = skinNameToken switch
                {
                    "RANGER_SKIN_MAJOR_NAME" => ExhaustVFX.impactPrefabMajor,
                    "RANGER_SKIN_RENEGADE_NAME" => ExhaustVFX.impactPrefabRenegade,
                    "RANGER_SKIN_MILEZERO_NAME" => ExhaustVFX.impactPrefabMileZero,
                    "RANGER_SKIN_SANDSWEPT_NAME" => ExhaustVFX.impactPrefabSandswept,
                    _ => ExhaustVFX.impactPrefabDefault
                };

                PlayAnimation("Gesture, Override", "Fire", "Fire.playbackRate", durationPerVolley);
            }

            outer.StartCoroutine(FireShot());
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

            if (!canExitState || !isAuthority)
            {
                return;
            }

            outer.SetNextStateToMain();
        }

        public IEnumerator FireShot()
        {
            Main.ModLogger.LogError("running FireShot()");
            for (int i = 0; i < finalVolleyCount; i++)
            {
                yield return new WaitForSeconds(durationPerVolley / (i + 1));
                Main.ModLogger.LogError("shooting volley");
                characterBody.SetAimTimer(0.3f);

                var aimDirection = GetAimRay().direction;

                BulletAttack attack = new()
                {
                    damage = damageCoefficient * damageStat,
                    procCoefficient = procCoefficient,
                    minSpread = -1f * i,
                    maxSpread = 1f * i,
                    damageType = DamageType.IgniteOnHit,
                    bulletCount = 4,
                    tracerEffectPrefab = tracerEffect,
                    muzzleName = "Muzzle",
                    hitEffectPrefab = impactEffect,
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
                Util.PlayAttackSpeedSound("Play_captain_m1_hit", gameObject, 0.75f);

                attack.Fire();

                AddRecoil(4.5f, 4.5f, 0f, 0f);
                characterMotor?.ApplyForce(-2000f * aimDirection, false, false);
            }
            canExitState = true;
        }
    }
}