using System;
using System.Collections;
using System.Linq;
using EntityStates.Chef;
using Sandswept.Survivors.Electrician.States;

namespace Sandswept.Survivors.Electrician
{
    public class Electrician : SurvivorBase<Electrician>
    {
        public override string Name => "Electrician";

        public override string Description => "TBD";

        public override string Subtitle => "TBD";

        public override string Outro => "TBD";

        public override string Failure => "TBD";
        public static GameObject GalvanicBolt;
        public static GameObject TempestSphere;
        public static GameObject StaticSnare;
        public static DamageAPI.ModdedDamageType Grounding = DamageAPI.ReserveDamageType();

        public static GameObject staticSnareImpactVFX;
        public static GameObject LightningZipEffect;
        public static GameObject SignalOverloadIndicator;
        public static LazyIndex ElectricianIndex = new("ElectricianBody");

        public override void LoadAssets()
        {
            base.LoadAssets();

            Body = Main.Assets.LoadAsset<GameObject>("ElectricianBody.prefab");

            Body.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpStandardMelee;
            var networkIdentity = Body.GetComponent<NetworkIdentity>();
            networkIdentity.localPlayerAuthority = true;
            networkIdentity.enabled = true;
            networkIdentity.serverOnly = false;

            var cb = Body.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Paths.GameObject.StandardCrosshair;
            cb.preferredPodPrefab = Paths.GameObject.RoboCratePod;

            SurvivorDef = Main.Assets.LoadAsset<SurvivorDef>("sdElectrician.asset");
            SurvivorDef.cachedName = "Electrician"; // for eclipse fix
            var kcm = Body.GetComponent<KinematicCharacterController.KinematicCharacterMotor>();
            kcm.playerCharacter = true;

            PrefabAPI.RegisterNetworkPrefab(Body);
            Master = PrefabAPI.InstantiateClone(Paths.GameObject.EngiMonsterMaster, "ElectricianMaster");

            SkillLocator locator = Body.GetComponent<SkillLocator>();
            ReplaceSkills(locator.primary, new SkillDef[] { Skills.GalvanicBolt.instance });
            ReplaceSkills(locator.secondary, new SkillDef[] { Skills.TempestSphere.instance });
            ReplaceSkills(locator.utility, new SkillDef[] { Skills.StaticSnare.instance });
            ReplaceSkills(locator.special, new SkillDef[] { Skills.SignalOverload.instance });
            locator.passiveSkill.icon = Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texElectricianSkillIcon_p.png");
            "SANDSWEPT_ELECTR_PASSIVE_NAME".Add("Volatile Shields");
            "SANDSWEPT_ELECTR_PASSIVE_DESC".Add("<style=cIsUtility>Start with innate shields</style>. When your shield <style=cDeath>breaks</style>, release a blast for <style=cIsDamage>400% damage</style> and gain <style=cIsUtility>+40% movement speed</style> for <style=cIsDamage>5 seconds</style>.");

            "KEYWORD_GROUNDING".Add("<style=cKeywordName>Grounding</style>Deals <style=cIsDamage>1.5x</style> damage to flying targets, and <style=cDeath>knocks them down</style>.");

            "KEYWORD_LIGHTWEIGHT".Add("<style=cKeywordName>Lightweight</style>Can be knocked around by heavy projectiles.");

            GalvanicBolt = Main.Assets.LoadAsset<GameObject>("GalvanicBallProjectile.prefab");
            ContentAddition.AddProjectile(GalvanicBolt);

            // this gets instantiatecloned to break its prefab status so i can parent stuff to it over in CreateVFX
            TempestSphere = PrefabAPI.InstantiateClone(Main.Assets.LoadAsset<GameObject>("TempestSphereProjectile.prefab"), "TempestSphereProjectile");
            ContentAddition.AddProjectile(TempestSphere);

            StaticSnare = Main.Assets.LoadAsset<GameObject>("TripwireMineProjectile.prefab");
            ContentAddition.AddProjectile(StaticSnare);

            Main.Instance.StartCoroutine(CreateVFX());

            On.RoR2.HealthComponent.TakeDamage += HandleGroundingShock;
        }

        public IEnumerator CreateVFX()
        {
            GameObject sphereVFX = new("joe sigma");
            sphereVFX.transform.position = Vector3.zero;
            sphereVFX.transform.localPosition = Vector3.zero;

            GameObject tempestSphereIndicator = PrefabAPI.InstantiateClone(Paths.GameObject.VagrantNovaAreaIndicator, "TempestSphereIndicator", false);
            tempestSphereIndicator.GetComponentInChildren<ParticleSystemRenderer>().gameObject.SetActive(false);
            tempestSphereIndicator.GetComponent<MeshRenderer>().sharedMaterials = new Material[] {
                Paths.Material.matWarbannerSphereIndicator,
                Paths.Material.matLightningSphere
            };
            tempestSphereIndicator.RemoveComponent<ObjectScaleCurve>();
            yield return new WaitForSeconds(0.1f);
            tempestSphereIndicator.transform.localScale = new(14f, 14f, 14f);
            tempestSphereIndicator.RemoveComponent<AnimateShaderAlpha>();

            GameObject tempestOrb = PrefabAPI.InstantiateClone(Paths.GameObject.VoidSurvivorChargeMegaBlaster, "TempestOrb", false);
            tempestOrb.transform.Find("Base").gameObject.SetActive(false);
            tempestOrb.transform.Find("Base (1)").gameObject.SetActive(false);
            tempestOrb.transform.Find("Sparks, In").GetComponent<ParticleSystemRenderer>().sharedMaterial = Paths.Material.matLoaderCharging;
            tempestOrb.transform.Find("Sparks, Misc").GetComponent<ParticleSystemRenderer>().sharedMaterial = Paths.Material.matIceOrbCore;
            tempestOrb.transform.Find("OrbCore").GetComponent<MeshRenderer>().sharedMaterials = new Material[] { Paths.Material.matLoaderLightningTile, Paths.Material.matJellyfishLightningSphere };
            tempestOrb.transform.RemoveComponent<ObjectScaleCurve>();
            yield return new WaitForSeconds(0.1f);
            tempestOrb.transform.localScale = new(3f, 3f, 3f);

            tempestSphereIndicator.transform.parent = sphereVFX.transform;
            tempestOrb.transform.parent = sphereVFX.transform;
            sphereVFX.transform.SetParent(TempestSphere.transform);
            tempestSphereIndicator.transform.position = Vector3.zero;
            tempestSphereIndicator.transform.localPosition = Vector3.zero;
            tempestOrb.transform.position = Vector3.zero;
            tempestOrb.transform.localPosition = Vector3.zero;
            TempestSphere.GetComponentInChildren<LineRenderer>().sharedMaterial = Paths.Material.matLightningLongYellow;

            staticSnareImpactVFX = PrefabAPI.InstantiateClone(Paths.GameObject.LoaderGroundSlam, "Sigma Gyatt Rizz Ohio Fa-", false);
            foreach (ShakeEmitter ughShakesButt in staticSnareImpactVFX.GetComponents<ShakeEmitter>())
            {
                ughShakesButt.enabled = false;
            }
            var effectComponent = staticSnareImpactVFX.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_loader_m1_impact";
            ContentAddition.AddEffect(staticSnareImpactVFX);

            LightningZipEffect = PrefabAPI.InstantiateClone(Paths.GameObject.BeamSphereGhost, "LightningZipOrb");
            LightningZipEffect.RemoveComponent<ProjectileGhostController>();
            LightningZipEffect.transform.Find("Lightning").gameObject.SetActive(false);
            LightningZipEffect.transform.Find("Fire").transform.localScale *= 0.3f;
            LightningZipEffect.transform.Find("Fire").GetComponent<ParticleSystemRenderer>().sharedMaterial = Paths.Material.matLoaderLightningTile;
            LightningZipEffect.transform.Find("Fire").Find("Beams").GetComponent<ParticleSystemRenderer>().sharedMaterial = Paths.Material.matLoaderLightningTile;
        }

        private void HandleGroundingShock(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            bool hadShield = self.shield > 0;

            if (damageInfo.HasModdedDamageType(Grounding) && NetworkServer.active)
            {
                CharacterMotor motor = self.GetComponent<CharacterMotor>();
                RigidbodyMotor motor2 = self.GetComponent<RigidbodyMotor>();

                if ((motor2))
                {
                    damageInfo.damage *= 1.5f;
                    damageInfo.damageType |= DamageType.Shock5s;
                    damageInfo.damageColorIndex = DamageColorIndex.Luminous;

                    EffectManager.SpawnEffect(Paths.GameObject.SojournExplosionVFX, new EffectData
                    {
                        origin = self.body.corePosition,
                        scale = self.body.bestFitRadius
                    }, true);

                    PhysForceInfo info = default;
                    info.massIsOne = true;
                    info.force = Vector3.down * 40f;

                    if (motor) motor.ApplyForceImpulse(in info);
                    if (motor2) motor2.ApplyForceImpulse(in info);
                }
            }

            orig(self, damageInfo);

            if (hadShield && self.body.bodyIndex == ElectricianIndex && self.shield <= 0) {
                EntityStateMachine machine = EntityStateMachine.FindByCustomName(self.gameObject, "Shield");
                machine.SetNextState(new SignalOverloadFire(0.65f));
            }
        }
    }
}
