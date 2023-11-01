using System;
using Sandswept2.Skills.Ranger.VFX;

namespace Sandswept2.States.Ranger
{
    public class Blast : BaseState
    {
        public static float DamageCoefficient = 5f;
        public static float ProcCoefficient = 1f;
        public static GameObject TracerEffect => ReleaseVFX.tracerPrefab;
        private float damageCoeff;

        public override void OnEnter()
        {
            base.OnEnter();
            float extraDamageCoeff = characterBody.GetBuffCount(Buffs.Charged.instance.BuffDef) * 1f;
            damageCoeff = DamageCoefficient + extraDamageCoeff;

            FireShot();

            PlayAnimation("Gesture, Override", "Blast", "Blast.playbackRate", 1f);

            if (NetworkServer.active)
            {
                characterBody.SetBuffCount(Buffs.Charged.instance.BuffDef.buffIndex, 0);
            }

            outer.SetNextStateToMain();
        }

        public void FireShot()
        {
            AkSoundEngine.PostEvent(Events.Play_commando_M2, gameObject);

            base.characterDirection.forward = base.GetAimRay().direction;

            if (!base.isAuthority)
            {
                return;
            }

            BulletAttack attack = new()
            {
                aimVector = GetAimRay().direction,
                falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                damage = damageStat * damageCoeff,
                isCrit = RollCrit(),
                minSpread = 0,
                maxSpread = 0,
                owner = gameObject,
                muzzleName = "MuzzleR",
                origin = transform.position,
                tracerEffectPrefab = TracerEffect,
                hitEffectPrefab = ReleaseVFX.impactPrefab,
                procCoefficient = ProcCoefficient,
                weapon = gameObject,
                radius = 0.1f,
                smartCollision = true
            };

            attack.Fire();
        }
    }
}