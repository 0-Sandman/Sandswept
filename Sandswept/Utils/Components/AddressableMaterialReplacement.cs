using System;

namespace Sandswept.Utils.Components {
    public class AddressableMaterialReplacement : MonoBehaviour {
        public Renderer Target;
        public List<string> Materials;

        public void Start() {
            List<Material> mats = new();
            foreach (string str in Materials) {
                mats.Add(str.Load<Material>());
            }

            Target.sharedMaterials = mats.ToArray();
        }
    }
}