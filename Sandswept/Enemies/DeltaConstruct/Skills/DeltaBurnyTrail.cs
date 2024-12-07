using System;

namespace Sandswept.Enemies.DeltaConstruct {
    public class DeltaBurnyTrail : MonoBehaviour {
        public CharacterBody owner;
        public float damagePerSecond;
        private int ticksPerSecond = 5;
        private float delay => 1f / ticksPerSecond;
        private float stopwatch = 0f;
        private float damage => damagePerSecond / ticksPerSecond;
        private OverlapAttack attack;
        public void Start() {
            if (!NetworkServer.active) {
                return;
            }

            attack = new();
            attack.damage = damage;
            attack.attacker = owner.gameObject;
            attack.procCoefficient = 0;
            attack.damageColorIndex = DamageColorIndex.Poison;
            attack.hitBoxGroup = GetComponent<HitBoxGroup>();
            attack.isCrit = false;
        }

        public void FixedUpdate() {
            if (NetworkServer.active) {
                stopwatch += Time.fixedDeltaTime;

                if (stopwatch >= delay) {
                    stopwatch = 0f;
                    attack.ResetIgnoredHealthComponents();
                    attack.Fire();
                }
            }
        }
    }
}