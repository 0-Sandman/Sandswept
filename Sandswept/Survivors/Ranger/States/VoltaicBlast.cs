/*
using Sandswept.Survivors.Ranger.VFX;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sandswept.Survivors.Ranger.States
{
    public class VoltaicBlast : BaseState
    {
        public static float ProcCoeff = 1f;
        public static float DamageCoeff = 1f;
        public static float BaseDuration = 0.5f;
        public static float TotalShotsBaseDuration = 0.25f;
        public static GameObject TracerEffect => EnflameVFX.tracerPrefab; // beef this up later
        public static GameObject TracerEffectHeated => OverdriveShotHeatedVFX.tracerPrefab; // beef this up later
        private float duration;
        private float totalShotsDuration;
        private float stopwatch = 0f;
        private bool hasFired = false;
        private int shotsHit = 0;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = BaseDuration / attackSpeedStat;
            totalShotsDuration = TotalShotsBaseDuration / attackSpeedStat;

            if (characterBody)
                characterBody.SetAimTimer(1f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.SetAimTimer(0.4f);

            if (!hasFired && fixedAge >= duration)
            {
                characterBody.StartCoroutine(FireShot());
                hasFired = true;
            }

            if (shotsHit >= 3 && NetworkServer.active)
            {
                characterBody.SetBuffCount(Buffs.Charge.instance.BuffDef.buffIndex, characterBody.GetBuffCount(Buffs.Charge.instance.BuffDef) + 1);
            }

            outer.SetNextStateToMain();
        }

        public IEnumerator FireShot()
        {
            for (int i = 0; i < 3; i++)
            {
                if (characterBody)
                {
                    characterBody.isSprinting = false;
                }

                PlayAnimation("Gesture, Override", "OverdriveFire");

                var aimDirection = GetAimRay().direction;

                Util.PlayAttackSpeedSound("Play_commando_M2", gameObject, 1f);

                BulletAttack attack = new()
                {
                    aimVector = aimDirection,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    damage = damageStat * DamageCoeff,
                    isCrit = RollCrit(),
                    owner = gameObject,
                    muzzleName = "Muzzle",
                    origin = GetAimRay().origin,
                    tracerEffectPrefab = TracerEffect,
                    procCoefficient = ProcCoeff,
                    damageType = DamageType.Generic,
                    minSpread = 0.2f,
                    maxSpread = 0.3f,
                    damageColorIndex = DamageColorIndex.Default,
                    radius = 0.5f,
                    smartCollision = true
                };

                AddRecoil(0.3f, 0.4f, -0.1f, 0.1f);

                attack.hitCallback = (BulletAttack attack, ref BulletAttack.BulletHit hit) =>
                {
                    if (hit.hitHurtBox)
                    {
                        Util.PlaySound("Play_lunar_wisp_attack1_shoot_impact", hit.hitHurtBox.gameObject);
                        shotsHit++;
                    }
                    return BulletAttack.defaultHitCallback(attack, ref hit);
                };

                attack.Fire();
                yield return new WaitForSeconds(totalShotsDuration / 3);
                yield return null;
            }
        }
    }
}
*/