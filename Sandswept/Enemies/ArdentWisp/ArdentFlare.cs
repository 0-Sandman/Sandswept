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
        internal float duration;
        private float stopwatch = 0f;
        private static Color32 Blank = new Color32(0, 0, 0, 0);
        private bool inactive = false;

        public static Dictionary<byte, ArdentFlareCharge> BZMap = new();

        public void Start() {
            EffectData data = effectComponent.effectData;

            origin.transform.parent = data.ResolveChildLocatorTransformReference();
            origin.transform.position = origin.transform.parent.position;
            radius.transform.localScale = Vector3.one * data.scale;
            target = data.origin;
            duration = data.genericFloat;

            end.transform.position = target;

            if (BZMap.ContainsKey((byte)data.genericUInt)) {
                BZMap[(byte)data.genericUInt] = this;
            }
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
                // line.widthMultiplier = 2f;
                inactive = true;
                GameObject.Destroy(base.gameObject);
            }
        }

        public void OnDestroy() {
            BZMap.Remove((byte)effectComponent.effectData.genericUInt);
        }
    }

    public class ArdentFlameProjectile : MonoBehaviour {
        private ProjectileController controller;
        private Rigidbody rb;
        private BezierCurveLine bezier;
        private ArdentFlareCharge fx;
        private float warningTime;
        private float stopwatch = 0f;
        private byte id;

        public void Start() {
            controller = GetComponent<ProjectileController>();
            rb = GetComponent<Rigidbody>();
            id = controller.combo;

            Debug.Log(id);
        }

        public void FixedUpdate() {
            if (!NetworkServer.active) return;

            stopwatch += Time.fixedDeltaTime;

            if (!bezier) {
                if (ArdentFlareCharge.BZMap.ContainsKey(id)) {
                    fx = ArdentFlareCharge.BZMap[id];
                    warningTime = fx.duration;
                    bezier = ArdentFlareCharge.BZMap[id].bezier;
                    rb.position = bezier.EvaluateBezier(Mathf.Clamp01(stopwatch / warningTime));
                }
            }

            if (bezier) {
                float perct = Mathf.Clamp01(stopwatch / warningTime);

                Vector3 pos = bezier.EvaluateBezier(perct);

                rb.MovePosition(Vector3.Lerp(rb.position, pos, 20f));
            }

            if (stopwatch >= warningTime && warningTime != 0f) {
                GameObject.Destroy(base.gameObject);
            }
        }
    }
}