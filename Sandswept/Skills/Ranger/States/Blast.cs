using System;
using Sandswept.Skills.Ranger.VFX;

namespace Sandswept.States.Ranger
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

            if (NetworkServer.active) {
                characterBody.SetBuffCount(Buffs.Charged.instance.BuffDef.buffIndex, 0);
            }

            outer.SetNextStateToMain();
        }

        public void FireShot()
        {
            AkSoundEngine.PostEvent(Events.Play_commando_M2, base.gameObject);

            base.characterDirection.forward = base.GetAimRay().direction;

            if (!base.isAuthority)
            {
                return;
            }

            BulletAttack attack = new();
            attack.aimVector = base.GetAimRay().direction;
            attack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
            attack.damage = base.damageStat * damageCoeff;
            attack.isCrit = base.RollCrit();
            attack.minSpread = 0;
            attack.maxSpread = 0;
            attack.owner = base.gameObject;
            attack.muzzleName = "MuzzleR";
            attack.origin = base.transform.position;
            attack.tracerEffectPrefab = TracerEffect;
            attack.hitEffectPrefab = ReleaseVFX.impactPrefab;
            attack.procCoefficient = ProcCoefficient;
            attack.weapon = base.gameObject;
            attack.radius = 0.1f;
            attack.smartCollision = true;

            attack.Fire();
        }
    }
}