using Sandswept.Skills.Ranger.Projectiles;
using Sandswept.Skills.Ranger.VFX;
using System;
using static UnityEngine.SendMouseEvents;

namespace Sandswept.States.Ranger
{
    public class Release : BaseState
    {
        public static float DamageCoefficient = 6f;
        public static float MaxDamageCoefficient = 15f;
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
            // Util.PlaySound("Play_railgunner_R_gun_chargeUp", gameObject);

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

            if (fixedAge < duration || !isAuthority)
            {
                return;
            }

            if (!hasFired)
            {
                var chargedCount = characterBody.GetBuffCount(Buffs.Charge.instance.BuffDef);
                FireShot(chargedCount);
                hasFired = true;
            }

            outer.SetNextStateToMain();
        }

        public void FireShot(int buffCount)
        {
            AkSoundEngine.PostEvent(Events.Play_lunar_wisp_attack2_launch, gameObject);
            AkSoundEngine.PostEvent(Events.Play_bleedOnCritAndExplode_impact, gameObject);
            AkSoundEngine.PostEvent(Events.Play_item_use_meteor_impact, gameObject);
            AkSoundEngine.PostEvent(Events.Play_commando_M2_impact, gameObject);

            if (isAuthority)
            {
                var aimDirection = GetAimRay().direction;

                BulletAttack attack = new()
                {
                    aimVector = aimDirection,
                    falloffModel = BulletAttack.FalloffModel.None,
                    damage = damageStat * Util.Remap(buffCount, 0, DirectCurrent.maxCharge, DamageCoefficient, MaxDamageCoefficient),
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
                    radius = 2f,
                    smartCollision = true,
                    stopperMask = LayerIndex.world.mask,
                    force = 2500f + 100f * buffCount,
                };

                AddRecoil(3f + 0.15f * buffCount, 3f + 0.15f * buffCount, 0f, 0f);

                characterMotor?.ApplyForce((-3000f - 150f * buffCount) * aimDirection, false, false);

                attack.Fire();
            }

            if (NetworkServer.active)
            {
                for (int i = 0; i < buffCount; i++)
                {
                    characterBody.RemoveBuff(Buffs.Charge.instance.BuffDef);
                }
            }
        }
    }
}