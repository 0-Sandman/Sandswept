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
        public CharacterBody victimBody;

        public override void Begin()
        {
            base.lightningType = LightningType.Loader;
            base.Begin();

            duration = 0.1f;

            // var effectData = new EffectData();
            // effectData.origin = victimBody.corePosition;
            // effectData.scale = Mathf.Sqrt(victimBody.radius * 9f);
            /*if (!attackerBody)
            {
                return;
            }
            var modelLocator = attackerBody.GetComponent<ModelLocator>();
            if (modelLocator)
            {
                var modelTransform = modelLocator.modelTransform;

                if (modelTransform)
                {
                    var skinNameToken = modelTransform.GetComponent<ModelSkinController>().skins[attackerBody.skinIndex].nameToken;

                    impactVFX = skinNameToken switch
                    {
                        "VOLT_SKIN_COVENANT_NAME" => VFX.GalvanicBolt.muzzleFlashCovenant,
                        _ => VFX.GalvanicBolt.muzzleFlashDefault
                    };
                    // wanted to recolor but it was way too garbage and made in a garbage trash piece of shit fucking kurwa jebana jego mac pizda pierdolona to robila KURWA ja pierdole kurwa co za smiecie jebane to kurwa robily wystarczy grayscale wszystko oprocz jednego koloru kurwa szmaty jebane ALE NIE KURWA TRZEBA DZIESIEC RAZY WIECEJ WORKLOAD DAC BO JESTESCIE PIZDAMI JEBANYMI I NIE POTRAFICIE MYSLEC KURWA JA PIERDOLE

                    // EffectManager.SpawnEffect(impactVFX, effectData, transmit: true);
                    // Util.PlayAttackSpeedSound("Play_mage_m1_cast_lightning", victimBody.gameObject, 2f);
                }
            }*/
        }

        public override void OnArrival()
        {
            if (!target || !victimBody || !attacker)
            {
                return;
            }

            // var effectData = new EffectData();
            // effectData.origin = victimBody.corePosition;
            // effectData.scale = 0.3f + Mathf.Sqrt(victimBody.bestFitActualRadius * 12f);

            // EffectManager.SpawnEffect(impactVFX, effectData, transmit: true);
            Util.PlayAttackSpeedSound("Play_mage_m1_cast_lightning", victimBody.gameObject, 2f);

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
                    damageType = damageType,

                };

                healthComponent.TakeDamage(damageInfo);

                GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
            }
        }
    }
}