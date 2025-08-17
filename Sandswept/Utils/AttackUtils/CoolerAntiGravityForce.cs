using System;

namespace Sandswept.Utils {
    public class CoolerAntiGravityForce : MonoBehaviour {
        public float antiGravityCoefficient = -0.2f;
        public Func<float, float> lerpFunction = EaseInQuart;
        public float rampTime = 0.5f;
        private float stopwatch;
        private Rigidbody rb;
        public void Start() {
            rb = GetComponent<Rigidbody>();
        }

        public void FixedUpdate() {
            stopwatch += Time.fixedDeltaTime;
            float x = lerpFunction(Mathf.Clamp(stopwatch, 0, rampTime) / rampTime);
            rb.AddForce(-Physics.gravity * Mathf.Lerp(0, antiGravityCoefficient, x), ForceMode.Acceleration);
        }

        public static float EaseInQuad(float x) {
            return x * x;
        }

        public static float EaseInQuart(float x) {
            return x * x * x * x;
        }
    }
}