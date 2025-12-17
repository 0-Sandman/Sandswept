using System;
using System.Linq;

namespace Sandswept.Utils {
    [RequireComponent(typeof(InputBankTest))]
    [RequireComponent(typeof(CharacterBody))]
    public abstract class Tracker : MonoBehaviour {
        public Transform target;
        public Indicator indicator;
        public GameObject targetingIndicatorPrefab;
        private float stopwatch = 0f;
        public float searchDelay = 0.1f;
        public float maxSearchAngle = 40f;
        public float maxSearchDistance = 60f;
        ///<summary>used to determine if this tracker should search and display an indicator</summary>
        public bool isActive = true;
        public InputBankTest inputBank;
        public CharacterBody body;
        public float minDot => Mathf.Cos(Mathf.Clamp(maxSearchAngle, 0f, 180f) * ((float)Math.PI / 180f));
        public Func<bool> isActiveCallback = DefaultIsActiveCallback;

        private static bool DefaultIsActiveCallback() {
            return true;
        }

        public virtual void Start() {
            indicator = new(base.gameObject, targetingIndicatorPrefab);
            body = GetComponent<CharacterBody>();
            inputBank = GetComponent<InputBankTest>();
        }

        public virtual void FixedUpdate() {
            if (indicator != null && !DefaultIsActiveCallback()) {
                indicator.active = false;
                return;
            }

            if (indicator != null && !target) {
                indicator.active = false;
            }

            if (indicator != null && target) {
                indicator.active = true;
            }

            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= searchDelay) {
                stopwatch = 0f;
                target = SearchForTarget();
                if (target) {
                    indicator.targetTransform = OverrideTargetTransform(target);
                }
            }
        }

        public virtual Transform OverrideTargetTransform(Transform transform) {
            return transform;
        }

        public void OnDestroy() {
            indicator.DestroyVisualizer();
            indicator = null;
        }

        public abstract Transform SearchForTarget();
    }

    public class HurtboxTracker : Tracker
    {
        public enum TargetType {
            Enemy,
            Friendly,
            All
        }
        public TeamIndex userIndex;
        public TargetType targetType;
        public override void Start()
        {
            base.Start();
            userIndex = body.teamComponent.teamIndex;
        }
        public override Transform SearchForTarget()
        {
            BullseyeSearch search = new();
            search.searchDirection = inputBank.GetAimRay().direction;
            search.searchOrigin = base.transform.position;
            search.maxDistanceFilter = maxSearchDistance;
            search.maxAngleFilter = maxSearchAngle;
            TeamMask mask = TeamMask.all;

            switch (targetType) {
                case TargetType.Enemy:
                    mask.RemoveTeam(userIndex);
                    break;
                case TargetType.Friendly:
                    mask = TeamMask.none;
                    mask.AddTeam(userIndex);
                    break;
                case TargetType.All:
                    mask = TeamMask.all;
                    break;
            }

            search.teamMaskFilter = mask;
            search.sortMode = BullseyeSearch.SortMode.Angle;
            search.RefreshCandidates();
            search.FilterOutGameObject(base.gameObject);
            IEnumerable<HurtBox> boxes = search.GetResults();
            return boxes.FirstOrDefault()?.transform ?? null;
        }
    }

    public class ComponentTracker<T> : Tracker where T : Component
    {
        public Func<T, bool> validFilter = DefaultFilter;
        private static bool DefaultFilter(T t) {
            return true;
        }
        public override Transform SearchForTarget()
        {
            T[] coms = GameObject.FindObjectsOfType<T>();
            for (int i = 0; i < coms.Length; i++) {
                T com = coms[i];

                if (!validFilter(com)) {
                    continue;
                }
                
                if (Vector3.Distance(base.transform.position, com.transform.position) > maxSearchDistance) {
                    continue;
                }

                float dot = Vector3.Dot(inputBank.GetAimRay().direction, (com.transform.position - base.transform.position).normalized);
        
                if (dot < minDot) {
                    continue;
                }
                return com.transform;
            }
            return null;
        }
    }
}