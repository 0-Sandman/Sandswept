using System;

namespace Sandswept.Utils.Components {
    public class StateDependentMaterial : MonoBehaviour {
        public Renderer targetRenderer;
        public Material outsideRunReplacement;
        public Material lobbyReplacement;

        public void LateUpdate() {
            if (Run.instance && Run.instance.isRunning) {
                this.enabled = false;
            }

            if (!Run.instance) {
                targetRenderer.material = outsideRunReplacement;
            }

            if (Run.instance && !Run.instance.isRunning) {
                if (lobbyReplacement) {
                    targetRenderer.material = lobbyReplacement;
                }
            }
        }
    }
}