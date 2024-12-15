using System;

namespace Sandswept.Utils {
    public class LockLocalTransform : MonoBehaviour {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;

        public void LateUpdate() {
            this.transform.localPosition = Position;
            this.transform.localRotation = Quaternion.Euler(Rotation);
            this.transform.localScale = Scale;
        }
    }
}