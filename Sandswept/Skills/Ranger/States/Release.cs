using Sandswept.Skills.Ranger.VFX;
using System;

namespace Sandswept.States.Ranger
{
    public class Release : BaseState
    {
        public static float DamageCoefficient = 5f;
        public static float ProcCoefficient = 1f;
        public static GameObject TracerEffect => ReleaseVFX.tracerPrefab;
        public static GameObject ImpactEffect => ReleaseVFX.impactPrefab;

        public override void OnEnter()
        {
            base.OnEnter();
            var chargedCount = characterBody.GetBuffCount(Buffs.Charged.instance.BuffDef);

            FireShot(chargedCount);

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
                    damage = damageStat * DamageCoefficient + 1f * buffCount,
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
                    radius = 3f,
                    smartCollision = true,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    force = 2500f + 200f * buffCount,
                };

                characterMotor?.ApplyForce((-2500f - 200f * buffCount) * aimDirection, false, false);

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