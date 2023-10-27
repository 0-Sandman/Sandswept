using Sandswept.Skills.Ranger.VFX;
using System;

namespace Sandswept.States.Ranger
{
    public class Release : BaseState
    {
        public static float DamageCoefficient = 5f;
        public static float ProcCoefficient = 1f;
        public static float baseDuration = 0.25f;
        public float duration;
        public bool hasFired = false;
        public static GameObject TracerEffect => ReleaseVFX.tracerPrefab;
        public static GameObject ImpactEffect => ReleaseVFX.impactPrefab;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;

            Util.PlaySound("Play_mage_m2_charge", gameObject);
            Util.PlaySound("Play_mage_m2_charge", gameObject);
            Util.PlaySound("Play_voidBarnacle_m1_chargeUp", gameObject);
            Util.PlaySound("Play_voidBarnacle_m1_chargeUp", gameObject);
            Util.PlaySound("Play_voidBarnacle_m1_chargeUp", gameObject);
            Util.PlaySound("Play_railgunner_R_gun_chargeUp", gameObject);
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

            if (!hasFired)
            {
                var chargedCount = characterBody.GetBuffCount(Buffs.Charged.instance.BuffDef);
                FireShot(chargedCount);
                hasFired = true;
            }

            outer.SetNextStateToMain();
        }

        public void FireShot(int buffCount)
        {
            AkSoundEngine.PostEvent(Events.Play_lunar_wisp_attack2_launch, gameObject);

            if (isAuthority)
            {
                var aimDirection = GetAimRay().direction;

                BulletAttack attack = new()
                {
                    aimVector = aimDirection,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    damage = damageStat * (DamageCoefficient + 1f * buffCount),
                    isCrit = RollCrit(),
                    minSpread = 0,
                    maxSpread = 0,
                    owner = gameObject,
                    muzzleName = "MuzzleR",
                    origin = transform.position,
                    tracerEffectPrefab = TracerEffect,
                    hitEffectPrefab = ImpactEffect,
                    procCoefficient = ProcCoefficient,
                    weapon = gameObject,
                    radius = 2.5f,
                    smartCollision = true,
                    stopperMask = LayerIndex.world.mask,
                    force = 5000f + 500f * buffCount,
                };

                AddRecoil(3f + 0.3f * buffCount, 3f + 0.3f * buffCount, 0f, 0f);

                characterMotor?.ApplyForce((-4000f - 400f * buffCount) * aimDirection, false, false);

                attack.Fire();
            }

            if (NetworkServer.active)
            {
                for (int i = 0; i < buffCount; i++)
                {
                    characterBody.RemoveBuff(Buffs.Charged.instance.BuffDef);
                }
            }
        }
    }
}