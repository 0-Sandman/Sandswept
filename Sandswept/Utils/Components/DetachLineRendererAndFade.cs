using System;

namespace Sandswept.Utils.Components
{
    public class DetachLineRendererAndFade : MonoBehaviour
    {
        public LineRenderer line;
        public float decayTime = 1f;
        private bool areWeDecaying = false;
        private Vector3[] lrPoints;
        private float widthPerSec;

        public void Start()
        {
            if (areWeDecaying)
            {
                widthPerSec = line.widthMultiplier / decayTime;
            }
        }

        public void Update()
        {
            if (areWeDecaying)
            {
                line.widthMultiplier = Mathf.Max(0f, line.widthMultiplier - (widthPerSec * Time.deltaTime));
                decayTime -= Time.deltaTime;

                if (decayTime <= 0f)
                {
                    GameObject.Destroy(base.gameObject);
                }

                line.SetPositions(lrPoints);
            }
        }

        public void OnDestroy()
        {
            if (!areWeDecaying)
            {
                lrPoints = new Vector3[line.positionCount];
                line.GetPositions(lrPoints);
                line.transform.parent = null;
                if (line.GetComponent<LineBetweenTransforms>())
                {
                    line.RemoveComponent<LineBetweenTransforms>();
                }
                // line below throws
                DetachLineRendererAndFade dlrf = line.AddComponent<DetachLineRendererAndFade>();
                dlrf.line = line;
                dlrf.decayTime = decayTime;
                dlrf.areWeDecaying = true;
                dlrf.lrPoints = lrPoints;
            }
        }
    }
}