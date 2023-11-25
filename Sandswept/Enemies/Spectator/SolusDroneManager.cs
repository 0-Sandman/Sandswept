using System;

namespace Sandswept.Enemies.Spectator {
    public class SolusDroneManager : MonoBehaviour {
        //
        public int DroneCountToSpawn;
        public GameObject DronePrefab;
        public List<Transform> DronePoints = new();
        public float Distance = 3f;
        public Vector3 Plane1;
        public Vector3 Plane2;
        //
        internal List<CharacterBody> drones = new();

        private void OnValidate() {
            return;
            if (DronePoints.Count == 0) {
                for (int i = 0; i < transform.childCount; i++) {
                    if (transform.GetChild(i).name.Contains("Point")) {
                        Object.Destroy(transform.GetChild(i));
                        i--;
                    }
                }
                for (int i = 0; i < DroneCountToSpawn; i++) {
                    Vector3 plane1 = Vector3.right;
                    Vector3 plane2 = Vector3.right;

                    Vector3 targetPosition = base.transform.position + Quaternion.AngleAxis(360 / DroneCountToSpawn * i, plane1) * plane2 * Distance;
                    GameObject point = new("Point " + i);
                    point.transform.position = targetPosition;
                    point.transform.SetParent(base.transform);
                    DronePoints.Add(point.transform);
                }
            }
        }
    }
}