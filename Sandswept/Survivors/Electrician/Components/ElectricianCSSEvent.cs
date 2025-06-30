using System;

namespace Sandswept.Survivors.Electrician
{
    public class ElectricianCSSEvent : MonoBehaviour
    {
        public Transform pivot;
        public GameObject effect;

        public void Start()
        {
            effect = Paths.GameObject.OmniImpactVFXLoaderLightning;
        }

        public void ElecBlast()
        {
            EffectManager.SpawnEffect(effect, new EffectData
            {
                origin = pivot.transform.position,
                scale = 0.5f
            }, false);

            AkSoundEngine.PostEvent("Play_elec_spin", base.gameObject);
        }
    }
}