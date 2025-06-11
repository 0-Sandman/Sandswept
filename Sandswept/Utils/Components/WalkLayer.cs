using System;

namespace Sandswept.Utils.Components {
    public class WalkLayer : MonoBehaviour {
        public Animator animator;
        public string layer;
        public CharacterBody targetBody;
        private bool currentState = true;
        private float stateChangeStopwatch = 0f;
        private float targetState = 0f;
        private float current = 0f;
        private float x1;
        private float x2;
        private int index;

        public void Start() {
            index = animator.GetLayerIndex(layer);
            animator.SetLayerWeight(index, 1f);
            current = 1f;
        }

        public void Update() {
            if (!targetBody || !targetBody.characterMotor) return;

            
            if (targetBody) {
                if (currentState != (targetBody.characterMotor.isGrounded && targetBody.characterMotor.velocity == Vector3.zero)) {
                    currentState = (targetBody.characterMotor.isGrounded && targetBody.characterMotor.velocity == Vector3.zero);
                    stateChangeStopwatch = 1f;
                    targetState = currentState ? 0f : 1f;
                    
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

            animator.SetLayerWeight(index, current);
        }
    }
}