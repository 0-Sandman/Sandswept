using System;
using Sandswept.Survivors.Electrician.Achievements;

namespace Sandswept.Survivors.Electrician
{
    public class BrokenElecController : MonoBehaviour
    {
        public static event Action<CharacterBody> OnUserUnlock;

        public bool activatable = true;
        public GameObject poweredOffOrb;
        public GameObject poweredOnOrb;
        public GameObject[] particlesToDisable;
        public Light targetLight;
        public bool hasAttemptedBattery = false;
        public GameObject warningVFX;
        public CharacterBody body;

        public void Start()
        {
            body = GetComponent<CharacterBody>();
            body.bodyFlags |= CharacterBody.BodyFlags.ImmuneToVoidDeath;
            body.bodyFlags |= CharacterBody.BodyFlags.ImmuneToExecutes;
            body.bodyFlags |= CharacterBody.BodyFlags.OverheatImmune;

            // one hellelleallofallallot of a one-liner this used to be
        }
        public void OnTakeDamageServer(DamageInfo damageInfo)
        {
            if (!activatable) return;

            bool didAnyoneUnlock = false;

            if (damageInfo.HasModdedDamageType(Electrician.LIGHTNING) && damageInfo.procCoefficient > 0)
            {
                foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController.instances)
                {
                    if (pcmc.body && ((Vector3.Distance(pcmc.body.corePosition, base.transform.position) < 40f) || damageInfo.attacker.GetComponent<CharacterBody>() == pcmc.body))
                    {
                        OnUserUnlock?.Invoke(pcmc.body);
                        didAnyoneUnlock = true;
                    }
                }

                if (didAnyoneUnlock)
                {
                    activatable = false;
                    body.AddBuff(RoR2Content.Buffs.Intangible);

                    base.gameObject.CallNetworkedMethod("HandleActivationEffects");
                    HandleActivationEffects();
                }
            }
        }

        // need to insert my battery into my vol-t
        public void HandleBatteryInsertion(Interactor sigma)
        {
            if (!hasAttemptedBattery)
            {
                hasAttemptedBattery = true;
                base.GetComponent<PurchaseInteraction>().SetAvailable(true);
                AkSoundEngine.PostEvent(Events.Play_drone_deathpt1, base.gameObject);
                warningVFX.SetActive(true);

                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(RoR2Content.Equipment.QuestVolatileBattery.equipmentIndex), base.transform.position, -base.transform.right * 5f + Vector3.up);

                Chat.SendBroadcastChat(new Chat.SimpleChatMessage() { baseToken = "SANDSWEPT_VOLATILEINSERT" });
            }
            else
            {
                BlastAttack attack = new();
                attack.teamIndex = TeamIndex.Void;
                attack.baseDamage = 999999f;
                attack.damageType = DamageType.BypassArmor | DamageType.BypassBlock | DamageType.BypassOneShotProtection;
                attack.baseForce = 6000f;
                attack.radius = 45f;
                attack.position = base.transform.position;
                attack.crit = true;
                attack.canRejectForce = false;
                attack.AddModdedDamageType(Electrician.bypassVoltResistance);
                attack.Fire();

                AkSoundEngine.PostEvent(Events.Play_drone_deathpt2, base.gameObject);
                AkSoundEngine.PostEvent(Events.Play_vagrant_R_explode, base.gameObject);

                EffectManager.SpawnEffect(Paths.GameObject.ExplosionMinorConstruct, new EffectData
                {
                    scale = attack.radius * 2f,
                    origin = attack.position
                }, true);

                activatable = false;
                body.AddBuff(RoR2Content.Buffs.Intangible);
                base.gameObject.CallNetworkedMethod("HandleActivationEffects");
                HandleActivationEffects();

                foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController.instances)
                {
                    if (pcmc.body && ((Vector3.Distance(pcmc.body.corePosition, base.transform.position) < 40f)))
                    {
                        OnUserUnlock?.Invoke(pcmc.body);
                    }
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

            EffectManager.SpawnEffect(VFX.GalvanicBolt.impactDefault, new EffectData
            {
                origin = poweredOnOrb.transform.position,
                scale = 0.25f
            }, false);
        }
    }
}