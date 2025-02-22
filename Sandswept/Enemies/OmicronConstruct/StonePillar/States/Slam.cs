using System;

namespace Sandswept.Enemies.StonePillar.States {
    public class Slam : BaseState {
        public float downwardVelocity = 200f;
        public int shockwaveCount = 8;
        public float shockwaveDamageCoefficient = 5f;
        public static LazyAddressable<GameObject> shockwave = new(() => Paths.GameObject.BrotherSunderWave);
        public static LazyAddressable<GameObject> impact = new(() => Paths.GameObject.ExplosionMinorConstruct);

        public override void OnEnter()
        {
            base.OnEnter();

            rigidbody.velocity = Vector3.down * downwardVelocity;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (rigidbody.velocity.y <= 0.5f && base.fixedAge >= 0.1f) {
                rigidbody.velocity = Vector3.zero;
                outer.SetNextStateToMain();
                OnHitGround(base.transform.position);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public void OnHitGround(Vector3 sigma) {
            FireRingAuthority(sigma, Vector3.up);

            EffectManager.SpawnEffect(impact, new EffectData {
                origin = sigma,
                scale = 24f
            }, true);
        }

        private void FireRingAuthority(Vector3 footPosition, Vector3 normal)
        {
            float num = 360f / shockwaveCount;
            Vector3 vector = Vector3.ProjectOnPlane(normal, Vector3.up);
            for (int i = 0; i < shockwaveCount; i++)
            {
                Vector3 forward = Quaternion.AngleAxis(num * i, Vector3.up) * vector;
                if (NetworkServer.active)
                {
                    ProjectileManager.instance.FireProjectile(shockwave, footPosition, Util.QuaternionSafeLookRotation(forward), base.gameObject, base.damageStat * shockwaveDamageCoefficient, 4000f, false);
                }
            }
        }
    }
}