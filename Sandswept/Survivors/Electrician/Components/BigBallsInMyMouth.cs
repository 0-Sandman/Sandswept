using System;
using System.Linq;
using RoR2.Orbs;

namespace Sandswept.Survivors.Electrician
{
    public class BigBallsInMyMouth
    {
        public static GameObject impactDefault;
        public static GameObject impactCovenant;
        public static GameObject pizdaJebanaDefault;
        public static GameObject pizdaJebanaCovenant;
        public static void Init()
        {
            pizdaJebanaDefault = CreateKurwaPizdaJebanaRecolor("Default", new Color32(255, 213, 0, 255));
            pizdaJebanaCovenant = CreateKurwaPizdaJebanaRecolor("Covenant", new Color32(98, 28, 113, 255));
        }

        public static void CreateOrbEffectRecolors(string name)
        {
            var pieceOfShit = PrefabAPI.InstantiateClone(Paths.GameObject.LoaderLightningOrbEffect, "Fucking Piece of Garbage Orb Piece of Shit Fucking Impact Trash Default", false);

            var fuckingPieceOfGarbage = pieceOfShit.GetComponent<OrbEffect>();

            fuckingPieceOfGarbage.endEffect = pizdaJebanaDefault;

            ContentAddition.AddEffect(pieceOfShit);

            //

            var szmataJebana = PrefabAPI.InstantiateClone(Paths.GameObject.LoaderLightningOrbEffect, "Fucking Piece of Garbage Orb Piece of Shit Fucking Impact Trash Covenant", false);

            var spierdalajKurwo = szmataJebana.GetComponent<OrbEffect>();

            spierdalajKurwo.endEffect = pizdaJebanaCovenant;

            ContentAddition.AddEffect(szmataJebana);
        }

        public static GameObject CreateKurwaPizdaJebanaRecolor(string name, Color32 effectColor)
        {
            var fuckingTroglodyte = PrefabAPI.InstantiateClone(Paths.GameObject.OmniImpactVFXLightning, "Fucking Piece of Garbage KURWA PIZDA JEBANA " + name, false);
            // FUCK THIS MOTHERFUCKER UP KILL HIM KILL HIUM FUCKING PIECE OF SHITTTT
            VFXUtils.RecolorMaterialsAndLights(fuckingTroglodyte, effectColor, effectColor, true);
            // CHUJU SPIERDOLONY GIŃ

            return fuckingTroglodyte;
        }
    }

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
                Main.ModLogger.LogError("kurwa 1");
                var modelLocator = attackerBody.GetComponent<ModelLocator>();
                if (modelLocator)
                {
                    Main.ModLogger.LogError("kurwa mac 1");
                    var modelTransform = modelLocator.modelTransform;
                    EffectData effectData = new()
                    {
                        origin = origin,
                        genericFloat = duration
                    };

                    if (modelTransform)
                    {
                        Main.ModLogger.LogError("kurwa jego jebana mac 1");
                        var skinNameToken = modelTransform.GetComponent<ModelSkinController>().skins[attackerBody.skinIndex].nameToken;

                        impactVFX = skinNameToken switch
                        {
                            "SKIN_ELEC_MASTERY" => BigBallsInMyMouth.impactCovenant,
                            _ => BigBallsInMyMouth.impactDefault
                        };

                        effectData.SetHurtBoxReference(target);

                        EffectManager.SpawnEffect(impactVFX, effectData, transmit: true);
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