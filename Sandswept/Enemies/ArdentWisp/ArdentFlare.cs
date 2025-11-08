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

            if (!BZMap.ContainsKey((byte)data.genericUInt)) {
                BZMap.Add((byte)data.genericUInt, this);
            }
            else {
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
                inactive = true;
                GameObject.Destroy(base.gameObject);
            }
        }
    }

    public class ArdentFlameProjectile : MonoBehaviour {
        private ProjectileController controller;
        private Rigidbody rb;
        public BezierCurveLine bezier;
        public ArdentFlareCharge fx;
        public float warningTime;
        public float stopwatch = 0f;
        public byte id;

        public void Start() {
            controller = GetComponent<ProjectileController>();
            rb = GetComponent<Rigidbody>();
            id = controller.combo;
        }

        public void FixedUpdate() {
            if (!NetworkServer.active) return;

            stopwatch += Time.fixedDeltaTime;

            if ((stopwatch >= warningTime && warningTime != 0f) || (stopwatch >= 0.2f && warningTime == 0f)) {
                ArdentFlareCharge.BZMap.Remove(id);
                GameObject.Destroy(base.gameObject);
            }

            if (!bezier) {
                if (ArdentFlareCharge.BZMap.ContainsKey(id)) {
                    if (ArdentFlareCharge.BZMap[id] == null) {
                        return;
                    }

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
        }
    }

    [ConfigSection("Enemies :: Ardent Wisp")]
    public class ArdentBombProjectile : MonoBehaviour {
        public static float blastRadius = 22f;
        public static float blastTime = 1.5f;
        public Transform outerRadius;
        public Transform innerRadius;
        public ProjectileExplosion explosion;
        public ProjectileStickOnImpact stick;
        private float stopwatch = 0f;

        public void Start() {
            explosion.blastRadius = blastRadius;
            outerRadius.transform.localScale = blastRadius * Vector3.one;
            innerRadius.transform.localScale = blastRadius * Vector3.one;
        }

        public void FixedUpdate() {
            if (stick.stuck) {
                stopwatch += Time.fixedDeltaTime;

                if (stopwatch >= blastTime && NetworkServer.active) {
                    explosion.DetonateServer();
                    base.enabled = false;
                    GameObject.Destroy(base.gameObject);
                }
            }
        }

        public void Update() {
            innerRadius.transform.localScale = (blastRadius * (1f - Mathf.Clamp01(stopwatch / blastTime))) * Vector3.one;
        }
    }
}