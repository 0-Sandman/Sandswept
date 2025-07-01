using System;

namespace Sandswept.Survivors.Electrician
{
    public class ElectricianCSSEvent : MonoBehaviour
    {
        public Transform pivot;
        public GameObject effect;
        public ModelSkinController modelSkinController;

        public void Start()
        {
            modelSkinController = GetComponent<ModelSkinController>();

            effect = VFX.GalvanicBolt.muzzleFlashDefault;

            // kurwa pizda jebana nie dzialala kurwa i index outside POMIMO length checka zjebane chujstwo pierdolone
        }

        public void ElecBlast()
        {
            var skinNameToken = modelSkinController.skins[modelSkinController.currentSkinIndex].nameToken;

            effect = skinNameToken switch
            {
                "SKIN_ELEC_MASTERY" => VFX.GalvanicBolt.muzzleFlashCovenant,
                _ => VFX.GalvanicBolt.muzzleFlashDefault
            };

            EffectManager.SpawnEffect(effect, new EffectData
            {
                origin = pivot.position,
                scale = 0.5f
            }, false);

            AkSoundEngine.PostEvent("Play_elec_spin", base.gameObject);
        }
    }
}