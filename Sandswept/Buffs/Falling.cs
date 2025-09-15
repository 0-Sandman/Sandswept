using System;
using System.Linq;

namespace Sandswept.Buffs {
    public class Falling : BuffBase<Falling>
    {
        public override string BuffName => "Falling";

        public override Color Color => Color.grey;

        public override Sprite BuffIcon => null;
        public override bool Hidden => true;

        public override void Hooks()
        {
            base.Hooks();
        }

        public override void OnBuffApplied(CharacterBody body)
        {
            base.OnBuffApplied(body);
            
            if (!body.GetComponent<CameraMirror>()) {
                body.AddComponent<CameraMirror>();
            }
        }

        public override void OnBuffExpired(CharacterBody body)
        {
            base.OnBuffExpired(body);

            if (body.GetComponent<CameraMirror>()) {
                body.RemoveComponent<CameraMirror>();
            }
        }

        public class CameraMirror : MonoBehaviour {
            public CharacterBody body;
            public CameraRigController rig;
            public Camera camera;
            public float flipBackDuration;
            public float stopwatch;
            public Vector3 initialLocalRot;
            public Vector3 flippedRot = new Vector3(0, 0, 180f);
            public static float offset = 2.5f;
            public void Start() {
                body = GetComponent<CharacterBody>();
                rig = CameraRigController.instancesList.First(x => x.targetBody == body);
                body.GetTimedBuffTotalDurationForIndex(Falling.instance.BuffDef.buffIndex, out flipBackDuration);
                camera = rig.GetComponentInChildren<Camera>();
                initialLocalRot = camera.transform.localRotation.eulerAngles;
                flipBackDuration -= offset;

                camera.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            }

            public void Update() {
                stopwatch += Time.deltaTime;

                float perct = EaseInExpo(Mathf.Clamp01((Mathf.Max(0, stopwatch - offset)) / flipBackDuration));

                Vector3 rot = Vector3.Lerp(flippedRot, initialLocalRot, perct);

                camera.transform.localRotation = Quaternion.Euler(rot);
            }
            private Vector3 OverridePivotPos(On.RoR2.CameraModes.CameraModePlayerBasic.orig_CalculateTargetPivotPosition orig, RoR2.CameraModes.CameraModePlayerBasic self, ref RoR2.CameraModes.CameraModeBase.CameraModeContext context)
            {
                if (self == rig.cameraMode) {
                    float perct = EaseInExpo(Mathf.Clamp01((Mathf.Max(0, stopwatch - offset)) / flipBackDuration));

                    Vector3 origin = context.targetInfo.targetParams.transform.position;
                    Vector3 ret = orig(self, ref context);
                    Vector3 relative = ret - origin;

                    return origin + relative.Nullify(y: true) + (relative.Nullify(x: true, z: true) * Mathf.Lerp(-1f, 1f, perct));
                }
                return orig(self, ref context);
            }

            public void OnEnable() { On.RoR2.CameraModes.CameraModePlayerBasic.CalculateTargetPivotPosition += OverridePivotPos; }
            public void OnDisable() { On.RoR2.CameraModes.CameraModePlayerBasic.CalculateTargetPivotPosition -= OverridePivotPos; }

            public float EaseInExpo(float x) {
                return x == 0 ? 0 : Mathf.Pow(2f, 10f * x - 10f);
            }
        }
    }
}