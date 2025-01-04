using System;

namespace Sandswept.Survivors.Renegade.States {
    public class Revolver : BaseSkillState {
        public float duration = 0.8f;
        public float damageBase = 3.8f;
        public float damageLast = 7f;
        public static int shrapnelCount = 8;
        public static float shrapnelTotalDamage = 5f;
        public float force = 400f;
        public float forceLast = 700f;
        public static LazyAddressable<GameObject> ImpactEffect = new(() => Paths.GameObject.WoundSlashImpact);
        public static LazyAddressable<GameObject> TracerEffect = new(() => Paths.GameObject.TracerBandit2Rifle);
        public static LazyAddressable<GameObject> TracerStrong = new(() => Paths.GameObject.TracerBanditPistol);
        public static LazyAddressable<GameObject> MuzzleFlash = new(() => Paths.GameObject.MuzzleflashBanditPistol);
        public static LazyAddressable<GameObject> ShrapnelTracer = new(() => Paths.GameObject.TracerToolbotNails);

        public override void OnEnter()
        {
            base.OnEnter();

            duration /= base.attackSpeedStat;
            
            bool lastShot = base.skillLocator.primary.stock <= 0;

            if (base.isAuthority) {
                BulletAttack attack = new();

                attack.damage = base.damageStat * (lastShot ? damageLast : damageBase);
                attack.force = lastShot ? forceLast : force;
                attack.origin = inputBank.aimOrigin;
                attack.aimVector = inputBank.aimDirection;
                attack.owner = base.gameObject;
                attack.falloffModel = BulletAttack.FalloffModel.None;
                attack.hitEffectPrefab = ImpactEffect;
                attack.tracerEffectPrefab = lastShot ? TracerStrong : TracerEffect;
                attack.isCrit = base.RollCrit();
                attack.procCoefficient = 1f;
                attack.radius = lastShot ? 0.4f : 1.2f;
                attack.smartCollision = true;
                attack.muzzleName = "MuzzleCenter";

                if (lastShot) {
                    attack.hitCallback = ShrapnelEffect;
                }

                attack.Fire();
            }

            AkSoundEngine.PostEvent(Events.Play_bandit2_R_fire, base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public static bool ShrapnelEffect(BulletAttack self, ref BulletAttack.BulletHit hit) {
            bool result = BulletAttack.DefaultHitCallbackImplementation(self, ref hit);

            
            float damageCoeff = shrapnelTotalDamage / shrapnelCount;
            Vector3 spreadFocus = hit.surfaceNormal;
            CharacterBody body = self.owner.GetComponent<CharacterBody>();

            for (int i = 0; i < shrapnelCount; i++) {
                BulletAttack attack = new();

                attack.damage = body.damage * damageCoeff;
                attack.force = 200f;
                attack.origin = hit.point;
                attack.aimVector = Util.ApplySpread(spreadFocus, -180f, 180f, 1f, 1f);
                attack.owner = self.owner;
                attack.falloffModel = BulletAttack.FalloffModel.None;
                attack.hitEffectPrefab = ImpactEffect;
                attack.tracerEffectPrefab = ShrapnelTracer;
                attack.isCrit = self.isCrit;
                attack.procCoefficient = 0.5f;
                attack.radius = 0.5f;
                attack.smartCollision = true;
                attack.stopperMask = LayerIndex.world.mask;
                attack.maxDistance = 25f;
                attack.AddModdedDamageType(Renegade.ShrapnelBullet);

                attack.Fire();
            }

            return result;
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}