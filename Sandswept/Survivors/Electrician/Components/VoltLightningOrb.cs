using System;
using System.Linq;
using RoR2.Orbs;

namespace Sandswept.Survivors.Electrician
{
    public class VoltLightningOrb : LightningOrb
    {

        public Transform modelTransform;
        public GameObject impactVFX;
        public CharacterBody attackerBody;

        public override void Begin()
        {
            // base.Begin();
            // fuck you

            duration = 0.1f;
            attackerBody = attacker.GetComponent<CharacterBody>();
            if (attackerBody)
            {
                var modelLocator = attackerBody.GetComponent<ModelLocator>();
                if (modelLocator)
                {
                    var modelTransform = modelLocator.modelTransform;

                    if (modelTransform)
                    {
                        var skinNameToken = modelTransform.GetComponent<ModelSkinController>().skins[attackerBody.skinIndex].nameToken;

                        impactVFX = skinNameToken switch
                        {
                            "SKIN_ELEC_MASTERY" => VFX.GalvanicBolt.muzzleFlashCovenant,
                            _ => VFX.GalvanicBolt.muzzleFlashDefault
                        };
                        // wanted to recolor but it was way too garbage and made in a garbage trash piece of shit fucking kurwa jebana jego mac pizda pierdolona to robila KURWA ja pierdole kurwa co za smiecie jebane to kurwa robily wystarczy grayscale wszystko oprocz jednego koloru kurwa szmaty jebane ALE NIE KURWA TRZEBA DZIESIEC RAZY WIECEJ WORKLOAD DAC BO JESTESCIE PIZDAMI JEBANYMI I NIE POTRAFICIE MYSLEC KURWA JA PIERDOLE

                        EffectManager.SimpleEffect(impactVFX, target.transform.position, Quaternion.identity, transmit: true);
                    }
                }
            }
        }

        public override void OnArrival()
        {
            if (!target)
            {
                return;
            }
            HealthComponent healthComponent = target.healthComponent;
            if ((bool)healthComponent)
            {
                DamageInfo damageInfo = new()
                {
                    damage = damageValue,
                    attacker = attacker,
                    inflictor = inflictor,
                    force = Vector3.zero,
                    crit = isCrit,
                    procChainMask = procChainMask,
                    procCoefficient = procCoefficient,
                    position = target.transform.position,
                    damageColorIndex = damageColorIndex,
                    damageType = damageType
                };

                healthComponent.TakeDamage(damageInfo);

                GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
            }
        }
    }
}