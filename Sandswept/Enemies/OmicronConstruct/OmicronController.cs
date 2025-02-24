using System;
using System.Linq;
using Sandswept.Enemies.StonePillar;
using Sandswept.Enemies.StonePillar.States;

namespace Sandswept.Enemies.OmicronConstruct {
    public class OmicronController : MonoBehaviour {
        private int maxPillars = 4;
        internal List<PillarInfo> pillars = new();
        public void RegisterPillar(CharacterBody body) {
            if (pillars.Where(x => x.Body == body).Count() != 0) return;
            
            pillars.Add(new PillarInfo() {
                Body = body,
                actionTimer = 3f * Random.Range(0.8f, 2f)
            });
        }

        public void Start() {
            if (!NetworkServer.active) return;

            List<Vector3> positions = MiscUtils.GetSafePositionsWithinDistance(base.transform.position, 50f).ToList();
            for (int i = 0; i < maxPillars; i++) {
                Vector3 pos = positions.GetRandom();
                positions.RemoveAll(x => Vector3.Distance(pos, x) < 10f);

                MasterSummon summon = new();
                summon.masterPrefab = StonePillar.StonePillar.Instance.prefabMaster;
                summon.ignoreTeamMemberLimit = true;
                summon.position = pos + Vector3.down * 50f;
                summon.summonerBodyObject = base.gameObject;
                summon.teamIndexOverride = base.GetComponent<TeamComponent>().teamIndex;
                summon.useAmbientLevel = true;

                summon.Perform();
            }
        }

        public void FixedUpdate() {
            foreach (PillarInfo pillar in pillars) {
                if (pillar.State == null) continue;

                if (pillar.Actionable) {
                    pillar.actionStopwatch += Time.fixedDeltaTime;

                    if (pillar.actionStopwatch >= pillar.actionTimer) {
                        pillar.actionStopwatch = 0f;

                        Vector3 closest = MiscUtils.FindClosestNodeToPosition(pillar.State.transform.position + (Random.onUnitSphere * 50f), HullClassification.Human, false);

                        if (closest != Vector3.zero) {
                            pillar.State.MarkDestination(closest);
                        }
                    }
                }
            }
        }

        public void CommandSlam(GameObject target) {
            PillarInfo pillar = pillars.GetRandom();
            pillar.State.MarkDestination(target.transform.position);
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
        public GameObject target;
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