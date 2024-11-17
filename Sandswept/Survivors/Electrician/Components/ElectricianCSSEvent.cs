using System;

namespace Sandswept.Survivors.Electrician {
    public class ElectricianCSSEvent : MonoBehaviour {
        public Transform pivot;
        private GameObject effect;
        
        public void Start() {
            effect = Paths.GameObject.OmniImpactVFXLoaderLightning;
        }

        public void ElecBlast() {
            EffectManager.SpawnEffect(effect, new EffectData {
                origin = pivot.transform.position,
                scale = 0.5f
            }, false);
        }
    }
}