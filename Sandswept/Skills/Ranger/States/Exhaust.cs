using Sandswept.Components;
using Sandswept.Skills.Ranger.VFX;
using System.Collections;

namespace Sandswept.States.Ranger
{
    public class Exhaust : BaseState
    {
        public static float DamageCoefficient = 5f;
        public static float ProcCoefficient = 1f;
        public static float baseDuration = 0.15f;
        public float duration;
        public bool shot = false;
        public static GameObject TracerEffect => ExhaustVFX.tracerPrefab;
        public static GameObject ImpactEffect => ExhaustVFX.impactPrefab;
        private RangerHeatManager heat;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;

            Util.PlaySound("Play_mage_m2_charge", gameObject);
            Util.PlaySound("Play_mage_m2_charge", gameObject);
            Util.PlaySound("Play_voidBarnacle_m1_chargeUp", gameObject);
            Util.PlaySound("Play_voidBarnacle_m1_chargeUp", gameObject);
            Util.PlaySound("Play_voidBarnacle_m1_chargeUp", gameObject);
            // Util.PlaySound("Play_railgunner_R_gun_chargeUp", gameObject);

            heat = GetComponent<RangerHeatManager>();

            PlayAnimation("Gesture, Override", "Fire", "Fire.playbackRate", duration);

            if (characterBody)
                characterBody.SetAimTimer(1.5f);
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

            if (!shot)
            {
                int shotCount = Mathf.Max(1, Mathf.RoundToInt(heat.CurrentHeat / 40f));
                characterBody.StartCoroutine(FireShot(shotCount));
                shot = true;
            }
        }

        public IEnumerator FireShot(int shotCount)
        {
            for (int i = 0; i < shotCount; i++)
            {
                yield return new WaitForSeconds(duration);

                AkSoundEngine.PostEvent(Events.Play_lunar_wisp_attack2_launch, gameObject);

                if (isAuthority)
                {
                    var aimDirection = GetAimRay().direction;

                    BulletAttack attack = new()
                    {
                        aimVector = aimDirection,
                        falloffModel = BulletAttack.FalloffModel.None,
                        damage = damageStat * DamageCoefficient,
                        isCrit = RollCrit(),
                        minSpread = 1f * shotCount,
                        maxSpread = 1f * shotCount,
                        owner = gameObject,
                        muzzleName = "MuzzleR",
                        origin = transform.position,
                        tracerEffectPrefab = TracerEffect,
                        hitEffectPrefab = ImpactEffect,
                        procCoefficient = ProcCoefficient,
                        weapon = gameObject,
                        radius = 2f,
                        smartCollision = true,
                        stopperMask = LayerIndex.world.mask,
                        force = 0f
                    };

                    AddRecoil(3f + 1f * shotCount, 3f + 1f * shotCount, 0f, 0f);

                    characterMotor?.ApplyForce(-2000f * aimDirection, false, false);

                    Vector3 hitPoint = Vector3.zero;

                    attack.hitCallback = (BulletAttack attack, ref BulletAttack.BulletHit hit) =>
                    {
                        hitPoint = hit.entityObject ? hit.entityObject.transform.position : hit.point;
                        return BulletAttack.defaultHitCallback(attack, ref hit);
                    };

                    attack.Fire();

                    Main.ModLogger.LogError(hitPoint);

                    characterBody.StartCoroutine(SummonExplosion(hitPoint));
                }

                if (i == shotCount - 1)
                    outer.SetNextStateToMain();
            }

            yield return null;
        }

        public IEnumerator SummonExplosion(Vector3 hitPoint)
        {
            yield return new WaitForSeconds(1f);

            new BlastAttack()
            {
                baseDamage = damageStat * 2f,
                damageColorIndex = DamageColorIndex.Fragile,
                falloffModel = BlastAttack.FalloffModel.None,
                radius = 4f,
                attacker = gameObject,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                baseForce = 0f,
                bonusForce = Vector3.zero,
                crit = RollCrit(),
                inflictor = gameObject,
                position = hitPoint,
                teamIndex = teamComponent.teamIndex,
                damageType = DamageType.IgniteOnHit,
                procCoefficient = 0,
                procChainMask = default,
                impactEffect = ImpactEffect.GetComponent<EffectComponent>().effectIndex
            }.Fire();

            yield return null;
        }
    }
}