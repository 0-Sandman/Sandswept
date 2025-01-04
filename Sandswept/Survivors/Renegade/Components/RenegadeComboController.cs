using System;

namespace Sandswept.Survivors.Renegade {
    public class RenegadeComboController : MonoBehaviour {
        public const float MAX_COMBO = 100f;
        public const float SPECIAL_COMBO = 100f;
        public const float AERIAL_COMBO = 60f;
        public const float UTIL_COMBO = 60f;
        public const float M2_COMBO = 20f;
        public const float COMBO_GRACE = 5f;
        public const float REFILL_DELAY = 0.6f;
        public const float COMBO_PENALTY = 44f;
        public const float COMBO_DRAIN = COMBO_PENALTY / REFILL_DELAY;
        public float comboMeter = 0;
        public DamageSource mostRecentAttack = DamageSource.NoneSpecified;
        public List<CharacterBody> mostRecentTargets = new();
        private float keepTargetStopwatch;
        private float consumeComboStopwatch;
        private bool aggressive = false;
        private SkillLocator locator;

        public void Start() {
            locator = GetComponent<SkillLocator>();
        }

        public bool HitTargets(params HurtBox[] boxes) {
            // bool hitAnySameTarget = false;
            bool hitAnySameTarget = true;
            
            // for (int i = 0; i < boxes.Length; i++) {
            //     if (mostRecentTargets.Contains(boxes[i].healthComponent.body)) {
            //         hitAnySameTarget = true;
            //     }
            // }
            
            keepTargetStopwatch = COMBO_GRACE;
            mostRecentTargets.Clear();

            aggressive = true;

            for (int i = 0; i < boxes.Length; i++) {
                mostRecentTargets.Add(boxes[i].healthComponent.body);
            }

            return hitAnySameTarget;
        }

        public void FixedUpdate() {
            if (keepTargetStopwatch > 0f) {
                keepTargetStopwatch -= Time.fixedDeltaTime;

                if (keepTargetStopwatch <= 0f) {
                    keepTargetStopwatch = 0f;
                    aggressive = false;
                    mostRecentTargets.Clear();
                }
            }

            if (consumeComboStopwatch > 0f) {
                consumeComboStopwatch -= Time.fixedDeltaTime;
                comboMeter -= COMBO_DRAIN;
                if (comboMeter <= 0f) comboMeter = 0f;

                if (consumeComboStopwatch <= 0f) {
                    consumeComboStopwatch = 0f;
                    locator.primary.Reset();
                }
            }

            if (!aggressive) {
                comboMeter -= Time.fixedDeltaTime * COMBO_PENALTY;

                if (comboMeter <= 0f) comboMeter = 0f;
            }
        }

        public void UpdateCombo(DamageSource attack) {
            if (mostRecentAttack == DamageSource.NoneSpecified || (mostRecentAttack == attack && attack != DamageSource.Secondary)) {
                mostRecentAttack = attack;
                return;
            }

            mostRecentAttack = attack;

            switch (attack) {
                case DamageSource.Special:
                    comboMeter += SPECIAL_COMBO;
                    break;
                case DamageSource.Secondary:
                    comboMeter += M2_COMBO;
                    break;
                case DamageSource.Utility:
                    comboMeter += UTIL_COMBO;
                    break;
                case DamageSource.Hazard:
                    comboMeter += AERIAL_COMBO;
                    break;
            }

            if (comboMeter >= MAX_COMBO) {
                consumeComboStopwatch = REFILL_DELAY;
            }
        }
    }
}