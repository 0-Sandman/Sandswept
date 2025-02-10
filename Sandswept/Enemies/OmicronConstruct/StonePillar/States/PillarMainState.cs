using System;

namespace Sandswept.Enemies.StonePillar.States {
    public class PillarMainState : GenericCharacterMain {
        public static float DesiredHeight = 40f;
        //
        public bool aboveValidGround = false;
        private Vector3 currentDestination = Vector3.zero;
        public bool moving => currentDestination != Vector3.zero;

        public override void HandleMovements()
        {
            aboveValidGround = Physics.Raycast(base.transform.position, Vector3.down, out RaycastHit hit, 900f, LayerIndex.world.mask);

            if (currentDestination != Vector3.zero) {
                Vector3 targetDir = (currentDestination - base.transform.position).normalized;
                targetDir.y = 0f;

                characterMotor.moveDirection = targetDir;

                Vector3 ideal = targetDir;
                ideal.y = currentDestination.y;

                if (Vector3.Distance(ideal, currentDestination) < 1f) {
                    currentDestination = Vector3.zero;
                }
            }
            else {
                characterMotor.moveDirection = Vector3.zero;
            }

            if (aboveValidGround) {
                float dist = Vector3.Distance(hit.point, base.transform.position);

                if (dist < DesiredHeight) {
                    base.characterMotor.velocity += Vector3.up * 10f * Time.fixedDeltaTime;
                }

                if (dist > DesiredHeight) {
                    base.characterMotor.velocity.y *= 0.1f * Time.fixedDeltaTime;
                }
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
        }
    }
}