using System;

namespace Sandswept.Enemies.ArdentWisp
{
    [RequireComponent(typeof(EffectComponent))]
    public class ArdentFlareCharge : MonoBehaviour {
        public EffectComponent effectComponent;
        public Transform origin;
        public Transform end;
        private Vector3 target;
        public LineRenderer line;
        public BezierCurveLine bezier;
        public MeshRenderer radius;
        public AnimationCurve lerp;
        public float idealHeightAt30 = 20f;
        private float duration;
        private float stopwatch = 0f;
        private static Color32 Blank = new Color32(0, 0, 0, 0);
        private bool inactive = false;

        public void Start() {
            EffectData data = effectComponent.effectData;

            origin.transform.parent = data.ResolveChildLocatorTransformReference();
            origin.transform.position = origin.transform.parent.position;
            radius.transform.localScale = Vector3.one * data.scale;
            target = data.origin;
            duration = data.genericFloat;

            end.transform.position = target;
        }

        public void Update() {
            if (inactive) return;
            stopwatch += Time.deltaTime;

            float perct = Mathf.Clamp01(stopwatch / duration);
            float dist = Vector3.Distance(origin.position.Nullify(y: true), target.Nullify(y: true));
            Color col = Color.Lerp(Blank, Color.white, lerp.Evaluate(perct));

            line.widthMultiplier = 2f - (perct * 2f);
            line.startColor = col;
            line.endColor = col;

            float height = idealHeightAt30 * (dist / 30f);
            bezier.v1.y = height;
        }

        public void FixedUpdate() {
            if (stopwatch > duration) {
                line.widthMultiplier = 2f;
                inactive = true;
                GameObject.Destroy(base.gameObject);
            }
        }
    }
}