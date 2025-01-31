using System;
using Sandswept.Buffs;
using static Sandswept.Buffs.MegaloCore;

namespace Sandswept.Survivors.Megalomaniac {
    public class MegaloController : MonoBehaviour {
        public const int MAX_CORES = 15;
        public const float MAX_PASSIVE_CORES_MULT = 0.5f;
        public const int CORES_PER_EGO = 2;
        public const float HP_PER_CORE = 0.05f;
        public const float UPDATE_RATE = 0.16f;
        public const float PASSIVE_GEN_DELAY = 2.5f;
        public const float FULLANGLE = 360f;
        public const float SPEED = 3f;
        public const float DISTANCE = 2f;
        public static Vector3 OFFSET = new Vector3(0, 1.6f, 0);
        public List<ProjectileOrbitalCore> ActiveCores = new();
        private float initialTime;
        private float stopwatchUpdate;
        private float stopwatchGeneration;
        private CharacterBody body;
        private int currentOrbCount;
        private int maxCores;

        public void Start() {
            body = GetComponent<CharacterBody>();
            initialTime = Run.instance.GetRunStopwatch();
        }

        public void FixedUpdate() {
            stopwatchGeneration += Time.fixedDeltaTime;
            stopwatchUpdate += Time.fixedDeltaTime;

            if (stopwatchGeneration >= PASSIVE_GEN_DELAY) {
                stopwatchGeneration = 0f;

                if (currentOrbCount < maxCores * MAX_PASSIVE_CORES_MULT) {
                    if (NetworkServer.active) {
                        FireProjectileInfo info = MiscUtils.GetProjectile(Megalomaniac.LunarCoreProjectile, 1f, body);
                        ProjectileManager.instance.FireProjectile(info);

                        DamageInfo damage = new();
                        damage.damage = body.healthComponent.fullCombinedHealth * HP_PER_CORE;
                        damage.crit = false;
                        damage.position = base.transform.position;
                        damage.damageType = DamageType.Silent | DamageType.BypassBlock | DamageType.NonLethal | DamageType.BypassArmor;
                        damage.AddModdedDamageType(Main.eclipseSelfDamage);
                        body.healthComponent.TakeDamage(damage);
                    }    
                }
            }

            if (stopwatchUpdate >= UPDATE_RATE) {
                stopwatchUpdate = 0f;

                ActiveCores.RemoveAll(x => x == null);
                currentOrbCount = ActiveCores.Count;

                body.SetBuffCount(MegaloCore.instance.BuffDef.buffIndex, currentOrbCount);

                maxCores = MAX_CORES + (body.inventory.GetItemCount(DLC1Content.Items.LunarSun) * CORES_PER_EGO);
            }

            for (int i = 0; i < ActiveCores.Count; i++) {
                ProjectileOrbitalCore orb = ActiveCores[i];
                if (!orb) {
                    continue;
                }

                float elapsed = Run.instance.GetRunStopwatch() - initialTime;

                Vector3 p1 = Vector3.up;
                Vector3 p2 = base.transform.forward;

                Vector3 target = (body.footPosition + OFFSET) + Quaternion.AngleAxis(FULLANGLE / ActiveCores.Count * i + elapsed / (SPEED) * FULLANGLE, p1) * p2 * (DISTANCE * (1 + (0.1f * i)));
                Vector3 newPosition = Vector3.MoveTowards(orb.transform.position, target, body.moveSpeed * 2f * Time.fixedDeltaTime);

                orb.transform.position = newPosition;
            }
        }
    }

    public class ProjectileOrbitalCore : MonoBehaviour {
        public const float DAMAGE_COEFF = 0.7f;
        public const float RADIUS = 5f;
        public const float ICD_TIME = 1.5f;
        public const float SCAN_TIME = 0.08f;
        public const int MAX_EXPLOSIONS = 3;
        public static LazyAddressable<GameObject> SpawnEffect = new(() => Paths.GameObject.ExplosionLunarSun);
        public static LazyAddressable<GameObject> ExplosionEffect = new(() => Paths.GameObject.LunarWispTrackingBombExplosion);
        private ProjectileController controller;
        private MegaloController megaloController;
        private OverlapAttack hitDetection;
        private float hitDetectTimer = 0f;
        private GameObject owner;
        private CharacterBody ownerBody;
        private float icd = 0f;
        private int totalExplosions = 0;

        public void Start() {
            controller = GetComponent<ProjectileController>();
            owner = controller.owner;

            if (!owner) {
                Destroy(base.gameObject);
                return;
            }

            ownerBody = owner.GetComponent<CharacterBody>();

            megaloController = owner.GetComponent<MegaloController>();
            megaloController.ActiveCores.Add(this);

            EffectManager.SpawnEffect(SpawnEffect, new EffectData {
                origin = base.transform.position,
                scale = 3f,
            }, true);

            hitDetection = new();
            hitDetection.procCoefficient = 0f;
            hitDetection.damageType = DamageType.Silent;
            hitDetection.damage = 0;
            hitDetection.teamIndex = ownerBody.teamComponent.teamIndex;
            hitDetection.hitBoxGroup = GetComponent<HitBoxGroup>();
        }

        public void FixedUpdate() {
            if (!owner) {
                Destroy(base.gameObject);
                return;
            }

            hitDetectTimer += Time.fixedDeltaTime;
            icd -= Time.fixedDeltaTime;

            if (hitDetectTimer >= SCAN_TIME) {
                hitDetectTimer = 0f;

                if (icd <= 0) {
                    bool hitTargets = hitDetection.Fire();
                    hitDetection.ResetIgnoredHealthComponents();

                    if (hitTargets) {
                        icd = ICD_TIME;
                        Detonate();
                    }
                }
            }
        }

        public void Detonate() {
            totalExplosions++;
    
            BlastAttack attack = new();
            attack.baseDamage = ownerBody.damage * DAMAGE_COEFF;
            attack.position = base.transform.position;
            attack.crit = ownerBody.RollCrit();
            attack.attacker = owner;
            attack.radius = RADIUS;
            attack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
            attack.teamIndex = ownerBody.teamComponent.teamIndex;
            attack.procCoefficient = 0.2f; // passive effect; give it proc coeff on par with items
            
            attack.Fire();

            EffectManager.SpawnEffect(ExplosionEffect, new EffectData {
                origin = attack.position,
                scale = attack.radius * 2f
            }, true);

            if (totalExplosions >= MAX_EXPLOSIONS) {
                Destroy(base.gameObject);
            }
        }

        public void OnDestroy() {
            EffectManager.SpawnEffect(SpawnEffect, new EffectData {
                origin = base.transform.position,
                scale = 3f,
            }, true);

            if (megaloController) {
                megaloController.ActiveCores.Remove(this);
            }
        }
    }
}