using System;

namespace Sandswept.Utils.Components
{
    public class DetachAndCollapse : MonoBehaviour
    {
        public float collapseTime = 0.5f;
        public Transform target;
        private Vector3 originalScale;
        private bool isDecaying = false;
        public string ghostPath;
        private float fullCollapseTime;

        public void Start()
        {
            if (!target) {
                target = GetComponent<ProjectileController>().ghost.transform.Find(ghostPath);
            }
            
            originalScale = target.localScale;
            fullCollapseTime = collapseTime;
        }

        public void Update()
        {
            if (isDecaying)
            {
                collapseTime -= Time.deltaTime;
                base.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, 1f - (collapseTime / fullCollapseTime));

                if (collapseTime <= 0f)
                {
                    Destroy(base.gameObject);
                }
            }
        }

        public void OnDestroy()
        {
            if (!isDecaying)
            {
                target.parent = null;
                var dac = target.AddComponent<DetachAndCollapse>();
                dac.isDecaying = true;
                dac.target = target;
                dac.collapseTime = collapseTime;
                dac.fullCollapseTime = fullCollapseTime;
                dac.originalScale = originalScale;
                isDecaying = true;
            }
        }
    }
}