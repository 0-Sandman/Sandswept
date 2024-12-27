using System;

namespace Sandswept.Utils.Components {
    public class CombatParameter : MonoBehaviour {
        public Animator animator;
        public string paramName;
        public CharacterBody targetBody;
        private bool currentState = true;
        private float stateChangeStopwatch = 0f;
        private float targetState = 0f;
        private float current = 0f;
        private float x1;
        private float x2;

        public void Start() {
            animator.SetFloat(paramName, -1f);
            current = -1f;
        }

        public void Update() {
            if (targetBody) {
                if (currentState != targetBody.outOfCombat) {
                    currentState = targetBody.outOfCombat;
                    stateChangeStopwatch = 1f;
                    targetState = currentState ? -1f : 1f;
                    
                    x1 = current;
                    x2 = targetState;
                }
            }

            if (stateChangeStopwatch > 0f) {
                stateChangeStopwatch -= Time.deltaTime;
                if (stateChangeStopwatch <= 0f) stateChangeStopwatch = 0f;

                float perct = (1f - stateChangeStopwatch) / 1f;
                current = Mathf.Lerp(x1, x2, perct);
            }

            animator.SetFloat(paramName, current);
        }
    }
}