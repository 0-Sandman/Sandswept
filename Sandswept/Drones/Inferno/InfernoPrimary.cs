using System;
using EntityStates.Drone.Command;

namespace Sandswept.Drones.Inferno
{
    public class InfernoPrimary : BaseSkillState
    {
        public float baseDuration = 0.6f;
        public float duration;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;

            if (isAuthority)
            {
                FireProjectileInfo info = MiscUtils.GetProjectile(InfernoDrone.MortarProjectile, 3f, characterBody);
                ProjectileManager.instance.FireProjectile(info);
            }

            base.StartAimMode(duration);

            PlayAnimation("Gesture, Override", "ShootLeft", "Generic.playbackRate", duration / 3f);

            Util.PlaySound("Play_MULT_m1_grenade_launcher_shoot", base.gameObject);
            Util.PlaySound("Play_MULT_m1_grenade_launcher_explo", base.gameObject);
            Util.PlaySound("Play_MULT_m1_grenade_launcher_beep", base.gameObject);

            EffectManager.SimpleMuzzleFlash(Paths.GameObject.MagmaOrbExplosion, base.gameObject, "Muzzle", false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }

    public class CommandInfernoTrail : BaseCommandState {
        public static float duration = 1.5f;
        public static float forwardSpeed = 90f;
        public static float minimumHeight = 3f;
        public static float damageCoefficient = 3f;
        public static int baseMolotovCount = 7;
        private float fireStopwatch;
        private float fireInterval;
        private int timesFired;
        private int molotovCount;
        private Vector3 velocityHeight;
        private Vector3 direction;
        private float height;
        public override void OnEnter()
        {
            base.shouldFollow = true;
            base.followTime = 0.01f;
            base.OnEnter();
            if (target.hurtBox)
            {
                direction = GetTargetPosition() - base.transform.position;
            }
            else
            {
                direction = GetTargetPosition() - GetAttackerBody().transform.position;
            }

            direction.y = 0f;
            direction = direction.normalized;
            molotovCount = Mathf.FloorToInt(baseMolotovCount * attackSpeedStat);
            fireInterval = duration / molotovCount;
            
            base.breakAwayDuration = duration;

            base.rigidbody.velocity = direction * forwardSpeed;
            height = base.transform.position.y;

            if (Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, minimumHeight, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                height = hitInfo.point.y + minimumHeight;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            fireStopwatch += Time.fixedDeltaTime;

            if (fireStopwatch >= fireInterval && base.fixedAge >= 0.7f)
            {
                fireStopwatch -= fireInterval;
                timesFired++;
                Fire();
            }

            Vector3 position = base.transform.position;
            position.y = height;
            base.transform.position = Vector3.SmoothDamp(base.transform.position, position, ref velocityHeight, 0.3f);
            base.transform.forward = direction;
            base.rigidbodyMotor.moveVector = direction * forwardSpeed;
            
            if (base.isAuthority && timesFired >= molotovCount)
            {
                outer.SetNextStateToMain();
            }
        }

        private void Fire()
        {
            if (base.isAuthority)
            {
                CharacterBody attackerBody = GetAttackerBody();
                ProjectileManager.instance.FireProjectile(InfernoDrone.SigmaProjectile2, transform ? transform.transform.position : base.transform.position, Util.QuaternionSafeLookRotation(Vector3.down), attackerBody.gameObject, attackerBody.damage * damageCoefficient * base.tierDamageMultiplier, 0f, attackerBody.RollCrit(), DamageColorIndex.Default, null, -1f, DamageTypeCombo.GenericSecondary);
            }
        }
    }
}