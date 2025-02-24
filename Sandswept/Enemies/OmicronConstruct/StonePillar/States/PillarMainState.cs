using System;
using Sandswept.Enemies.OmicronConstruct;

namespace Sandswept.Enemies.StonePillar.States {
    public class PillarMainState : GenericCharacterMain {
        public static float DesiredHeight = 40f;
        public static float Speed = 50f;
        //
        public bool aboveValidGround = false;
        private Vector3 currentDestination = Vector3.zero;
        public bool moving => currentDestination != Vector3.zero;
        public bool hasResetCol = false;
        public float delayStopwatch = 0f;

        public override void OnEnter()
        {
            base.OnEnter();

            base.gameObject.layer = LayerIndex.noCollision.intVal;

            GameObject body = base.characterBody.master.minionOwnership.ownerMaster.bodyInstanceObject;
            body.GetComponent<OmicronController>().RegisterPillar(base.characterBody);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (delayStopwatch >= 0f) {
                delayStopwatch -= Time.fixedDeltaTime;

                return;
            }

            aboveValidGround = Physics.Raycast(base.transform.position, Vector3.down, out RaycastHit hit, 900f, LayerIndex.world.mask);

            if (currentDestination != Vector3.zero) {
                Vector3 targetDir = (currentDestination - base.transform.position).normalized;
                targetDir.y = 0f;

                Vector3 vel = targetDir * Speed;
                vel.y = rigidbody.velocity.y;

                rigidbody.velocity = vel;

                Vector3 ideal = base.transform.position;
                ideal.y = currentDestination.y;

                if (Vector3.Distance(ideal, currentDestination) < 1f) {
                    currentDestination = Vector3.zero;
                    CallSlam();
                }
            }
            else {
                Vector3 vel = Vector3.zero;
                vel.y = rigidbody.velocity.y;
                rigidbody.velocity = vel;
            }

            if (aboveValidGround) {
                if (!hasResetCol) {
                    hasResetCol = true;
                    base.gameObject.layer = LayerIndex.defaultLayer.intVal;
                }

                float dist = Vector3.Distance(hit.point, base.transform.position);

                if (dist < DesiredHeight) {
                    rigidbody.velocity += Vector3.up * 30f * Time.fixedDeltaTime;
                }

                if (dist > DesiredHeight) {
                    Vector3 vel = rigidbody.velocity;
                    vel.y *= 0.1f * Time.fixedDeltaTime;
                    rigidbody.velocity = vel;
                }
            }
            else if (!hasResetCol) {
                rigidbody.velocity += Vector3.up * 30f * Time.fixedDeltaTime;
            }
        }

        public override void ProcessJump()
        {
            return;
        }

        public void MarkDestination(Vector3 target) {
            currentDestination = target;
        }

        public void CallSlam() {
            base.skillLocator.primary.ExecuteIfReady();
            delayStopwatch = 2f;
        }
    }
}