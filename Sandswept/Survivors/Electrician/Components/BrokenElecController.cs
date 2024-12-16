using System;
using Sandswept.Survivors.Electrician.Achievements;

namespace Sandswept.Survivors.Electrician
{
    public class BrokenElecController : MonoBehaviour, IOnTakeDamageServerReceiver
    {
        public static event Action<CharacterBody> OnUserUnlock;

        private bool activatable = true;
        public GameObject poweredOffOrb;
        public GameObject poweredOnOrb;
        public GameObject[] particlesToDisable;
        public Light targetLight;

        public void OnTakeDamageServer(DamageReport damageReport)
        {
            if (!activatable) return;

            bool didAnyoneUnlock = false;

            if (damageReport.damageInfo.HasModdedDamageType(Electrician.LIGHTNING))
            {
                foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController.instances)
                {
                    if (pcmc.body && ((Vector3.Distance(pcmc.body.corePosition, base.transform.position) < 40f) || damageReport.attackerBody == pcmc.body))
                    {
                        OnUserUnlock?.Invoke(pcmc.body);
                        didAnyoneUnlock = true;
                    }
                }

                if (didAnyoneUnlock)
                {
                    activatable = false;
                    base.GetComponent<CharacterBody>().AddBuff(RoR2Content.Buffs.Intangible);

                    base.gameObject.CallNetworkedMethod("HandleActivationEffects");
                    HandleActivationEffects();
                }
            }
        }

        public void HandleActivationEffects()
        {
            for (int i = 0; i < particlesToDisable.Length; i++)
            {
                GameObject obj = particlesToDisable[i];
                ParticleSystem system = obj.GetComponent<ParticleSystem>();
                system.Stop();
            }

            poweredOffOrb.SetActive(false);
            poweredOnOrb.SetActive(true);

            targetLight.intensity = targetLight.GetComponent<FlickerLight>().initialLightIntensity * 1.5f;
            targetLight.GetComponent<FlickerLight>().enabled = false;

            EffectManager.SpawnEffect(Paths.GameObject.LoaderGroundSlam, new EffectData
            {
                origin = poweredOnOrb.transform.position,
                scale = 0.25f
            }, false);
        }
    }
}