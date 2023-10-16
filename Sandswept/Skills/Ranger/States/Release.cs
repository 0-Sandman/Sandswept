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
        private float damageCoeff;

        public override void OnEnter()
        {
            base.OnEnter();
            float extraDamageCoeff = characterBody.GetBuffCount(Buffs.Charged.instance.BuffDef) * 1f;
            damageCoeff = DamageCoefficient + extraDamageCoeff;

            FireShot();

            if (NetworkServer.active)
            {
                for (int i = 0; i < characterBody.GetBuffCount(Buffs.Charged.instance.BuffDef); i++)
                {
                    characterBody.RemoveBuff(Buffs.Charged.instance.BuffDef);
                }
            }

            outer.SetNextStateToMain();
        }

        public void FireShot()
        {
            AkSoundEngine.PostEvent(Events.Play_lunar_wisp_attack2_launch, base.gameObject);

            if (!base.isAuthority)
            {
                return;
            }

            var aimDirection = GetAimRay().direction;

            BulletAttack attack = new()
            {
                aimVector = aimDirection,
                falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                damage = base.damageStat * DamageCoefficient,
                isCrit = base.RollCrit(),
                minSpread = 0,
                maxSpread = 0,
                owner = base.gameObject,
                muzzleName = "MuzzleR",
                origin = base.transform.position,
                tracerEffectPrefab = TracerEffect,
                hitEffectPrefab = ImpactEffect,
                procCoefficient = ProcCoefficient,
                weapon = base.gameObject,
                radius = 3f,
                smartCollision = true,
                stopperMask = LayerIndex.CommonMasks.bullet,
                force = 2500f,
            };

            characterMotor?.ApplyForce(-2500f * aimDirection, false, false);

            attack.Fire();
        }
    }
}