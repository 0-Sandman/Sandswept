using System;
using Sandswept.Enemies.StonePillar.States;

namespace Sandswept.Enemies.OmicronConstruct {
    public class OmicronController : MonoBehaviour {
        public Transform[] PillarSpawnPoints;
        internal List<PillarInfo> pillars = new();
        public void RegisterPillar(CharacterBody body) {
            pillars.Add(new PillarInfo() {
                Body = body,
                actionTimer = 3f * Random.Range(0.8f, 2f)
            });
        }

        public void FixedUpdate() {
            foreach (PillarInfo pillar in pillars) {
                if (pillar.State == null) continue;

                if (pillar.Actionable) {
                    pillar.actionStopwatch += Time.fixedDeltaTime;

                    if (pillar.actionStopwatch >= pillar.actionTimer) {
                        pillar.actionStopwatch = 0f;
                    }
                }
            }
        }
    }

    public class PillarInfo {
        public PillarMainState State {
            get {
                if (_cached == null) {
                    if (!_cachedESM) {
                        _cachedESM = EntityStateMachine.FindByCustomName(Body.gameObject, "Body");
                    }

                    _cached = _cachedESM.state as PillarMainState;
                }

                return _cached;
            }
        }
        public CharacterBody Body;
        private PillarMainState _cached;
        private EntityStateMachine _cachedESM;
        public float actionTimer = 3f;
        public float actionStopwatch = 0f;
        public bool Actionable {
            get {
                if (State != null) {
                    return !State.moving;
                }

                return false;
            }
        }
    }
}