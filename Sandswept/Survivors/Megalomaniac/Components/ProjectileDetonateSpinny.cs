using System;

namespace Sandswept.Survivors.Megalomaniac {
    public class ProjectileDetonateSpinny : MonoBehaviour {
        public int spinnyCount = 8;
        public float spinnyDamageCoeff = 0.5f;

        public void OnImpact(ProjectileImpactInfo info) {
            GameObject owner = GetComponent<ProjectileController>().owner;
            float damage = GetComponent<ProjectileDamage>().damage;
            bool crit = GetComponent<ProjectileDamage>().crit;
            float num = 360f / spinnyCount;
            Vector3 normalized = Vector3.ProjectOnPlane(Random.onUnitSphere, Vector3.up).normalized;
            for (int i = 0; i < spinnyCount; i++) {
                Vector3 forward = Quaternion.AngleAxis(num * i, Vector3.up) * normalized;

                FireProjectileInfo pinfo = new();
                pinfo.damage = damage * spinnyDamageCoeff;
                pinfo.position = info.estimatedPointOfImpact;
                pinfo.rotation = Util.QuaternionSafeLookRotation(forward);
                pinfo.projectilePrefab = Megalomaniac.QuickSunderWave;
                pinfo.owner = owner;
                pinfo.crit = crit;

                ProjectileManager.instance.FireProjectile(pinfo);
            }
        }
    }
}