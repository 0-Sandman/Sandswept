using System;
using System.Collections;
using System.Linq;
using EntityStates.Chef;
using Sandswept.Survivors.Electrician.Achievements;
using Sandswept.Survivors.Electrician.SkillsDefs.Special;
using Sandswept.Survivors.Electrician.States.Primary;
using Sandswept.Survivors.Electrician.States.Secondary;
using Sandswept.Survivors.Electrician.States.Special;
using Sandswept.Survivors.Electrician.States.Utility;
using UnityEngine.SceneManagement;

namespace Sandswept.Survivors.Electrician
{
    public class Electrician : SurvivorBase<Electrician>
    {
        public override string Name => "VOL-T";

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

        //
        public static SkinDef sdElecDefault;

        public static SkinDef sdElecMastery;
        public static Material matElecOrbOuter;
        public static Material matElecOrbInner;
        public static Material matMasteryElecOrbOuter;
        public static Material matMasteryElecOrbInner;
        public static GameObject ElecMuzzleFlash;
        public static GameObject BrokenElectricianBody;
        public static DamageAPI.ModdedDamageType LIGHTNING = DamageAPI.ReserveDamageType();

        public override void CreateLang()
        {
            base.CreateLang();

            if (Random.Range(0, 100) <= 1)
            {
                LanguageAPI.Add(base.SurvivorDef.displayNameToken, "VOLTOMETER BOT AMP FUCKER 30000");
            }
        }

        public override void LoadAssets()
        {
            base.LoadAssets();

            Body = Main.Assets.LoadAsset<GameObject>("ElectricianBody.prefab");

            Body.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpStandardMelee;
            var networkIdentity = Body.GetComponent<NetworkIdentity>();
            networkIdentity.localPlayerAuthority = true;
            networkIdentity.enabled = true;
            networkIdentity.serverOnly = false;

            Body.GetComponent<ModelLocator>()._modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Paths.GameObject.GenericFootstepDust;

            var cb = Body.GetComponent<CharacterBody>();
            // cb._defaultCrosshairPrefab = Paths.GameObject.StandardCrosshair;
            cb.preferredPodPrefab = Paths.GameObject.RoboCratePod;
            SurvivorDef = Main.Assets.LoadAsset<SurvivorDef>("sdElectrician.asset");
            SurvivorDef.cachedName = "Electrician"; // for eclipse fix;
            var kcm = Body.GetComponent<KinematicCharacterController.KinematicCharacterMotor>();
            kcm.playerCharacter = true;

            PrefabAPI.RegisterNetworkPrefab(Body);
            Master = PrefabAPI.InstantiateClone(Paths.GameObject.EngiMonsterMaster, "ElectricianMaster");

            SkillLocator locator = Body.GetComponent<SkillLocator>();
            ReplaceSkills(locator.primary, new SkillDef[] { SkillsDefs.Primary.GalvanicBolt.instance });
            ReplaceSkills(locator.secondary, new SkillDef[] { SkillsDefs.Secondary.TempestSphere.instance });
            ReplaceSkills(locator.utility, new SkillDef[] { SkillsDefs.Utility.StaticSnare.instance });
            ReplaceSkills(locator.special, new SkillDef[] { SignalOverload.instance });
            locator.passiveSkill.icon = Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texElectricianSkillIcon_p.png");
            "SANDSWEPT_ELECTR_PASSIVE_NAME".Add("Volatile Shields");
            "SANDSWEPT_ELECTR_PASSIVE_DESC".Add("<style=cIsUtility>Start with innate shields</style>. When your shield <style=cDeath>breaks</style>, release a blast for <style=cIsDamage>400% damage</style> and gain <style=cIsUtility>+40% movement speed</style> for <style=cIsDamage>5 seconds</style>.");

            "KEYWORD_GROUNDING".Add("<style=cKeywordName>Grounding</style>Deals <style=cIsDamage>1.5x</style> damage to flying targets, and <style=cDeath>knocks them down</style>.");

            "KEYWORD_LIGHTWEIGHT".Add("<style=cKeywordName>Lightweight</style>Can be knocked around by heavy projectiles.");

            GalvanicBolt = Main.Assets.LoadAsset<GameObject>("GalvanicBallProjectile.prefab");
            // meow meow meow meow meow meow meow
            var projectileProximityBeamController = GalvanicBolt.GetComponent<ProjectileProximityBeamController>();
            projectileProximityBeamController.attackRange = 13f;
            projectileProximityBeamController.procCoefficient = 0.8f;
            projectileProximityBeamController.damageCoefficient = 0.75f;
            // weh
            // mrraow
            ContentAddition.AddProjectile(GalvanicBolt);

            // this gets instantiatecloned to break its prefab status so i can parent stuff to it over in CreateVFX
            TempestSphere = PrefabAPI.InstantiateClone(Main.Assets.LoadAsset<GameObject>("TempestSphereProjectile.prefab"), "TempestSphereProjectile");
            ContentAddition.AddProjectile(TempestSphere);

            StaticSnare = Main.Assets.LoadAsset<GameObject>("TripwireMineProjectile.prefab");
            ContentAddition.AddProjectile(StaticSnare);

            Main.Instance.StartCoroutine(CreateVFX());

            ContentAddition.AddEntityState(typeof(GalvanicBolt), out _);

            ContentAddition.AddEntityState(typeof(TempestSphereCharge), out _);
            ContentAddition.AddEntityState(typeof(TempestSphereFire), out _);

            ContentAddition.AddEntityState(typeof(StaticSnare), out _);

            ContentAddition.AddEntityState(typeof(SignalOverloadCharge), out _);
            ContentAddition.AddEntityState(typeof(SignalOverloadFire), out _);
            ContentAddition.AddEntityState(typeof(SignalOverloadDischarge), out _);

            On.RoR2.HealthComponent.TakeDamage += HandleGroundingShock;

            UnlockableDefs.Init();

            SurvivorDef.unlockableDef = UnlockableDefs.charUnlock;

            sdElecDefault = Main.Assets.LoadAsset<SkinDef>("sdElecDefault.asset");
            sdElecDefault.icon = Skins.CreateSkinIcon(
                new Color32(93, 79, 107, 255),
                new Color32(76, 21, 197, 255),
                new Color32(255, 248, 154, 255),
                new Color32(60, 46, 74, 255)
            );

            sdElecMastery = Main.Assets.LoadAsset<SkinDef>("sdElecMastery.asset");
            sdElecMastery.icon = Skins.CreateSkinIcon(
                new Color32(162, 103, 255, 255),
                new Color32(185, 175, 201, 255),
                new Color32(68, 50, 109, 255),
                new Color32(34, 34, 34, 255)
            );
            sdElecMastery.unlockableDef = UnlockableDefs.masteryUnlock;
            sdElecMastery.unlockableDef.achievementIcon = sdElecMastery.icon;

            matElecOrbInner = Main.Assets.LoadAsset<Material>("matElectricianOrbCenter.mat");
            matElecOrbOuter = Main.Assets.LoadAsset<Material>("matElectricianOrbOuter.mat");
            matMasteryElecOrbInner = Main.Assets.LoadAsset<Material>("matMasteryElecOrbCenter.mat");
            matMasteryElecOrbOuter = Main.Assets.LoadAsset<Material>("matMasteryElecOrbOuter.mat");

            LanguageAPI.Add("SKIN_ELEC_MASTERY", "Covenant");

            ContentAddition.AddMaster(Main.Assets.LoadAsset<GameObject>("ElectricianMonsterMaster.prefab"));

            BrokenElectricianBody = Main.Assets.LoadAsset<GameObject>("BrokenElectricianBody.prefab");
            ContentAddition.AddBody(BrokenElectricianBody);

            On.RoR2.Stage.Start += OnStageStart;

            On.RoR2.HealthComponent.TakeDamage += OnTakeDamage;
            On.RoR2.Orbs.LightningOrb.OnArrival += OnLightningOrbArrival;
            On.RoR2.Orbs.SimpleLightningStrikeOrb.OnArrival += OnSLSArrival;
            On.RoR2.Orbs.LightningStrikeOrb.OnArrival += OnLSArrival;
        }

        private void OnLSArrival(On.RoR2.Orbs.LightningStrikeOrb.orig_OnArrival orig, RoR2.Orbs.LightningStrikeOrb self)
        {
            self.AddModdedDamageType(LIGHTNING);
            orig(self);
        }

        private void OnSLSArrival(On.RoR2.Orbs.SimpleLightningStrikeOrb.orig_OnArrival orig, RoR2.Orbs.SimpleLightningStrikeOrb self)
        {
            self.AddModdedDamageType(LIGHTNING);
            orig(self);
        }

        private void OnLightningOrbArrival(On.RoR2.Orbs.LightningOrb.orig_OnArrival orig, RoR2.Orbs.LightningOrb self)
        {
            if (self.lightningType == RoR2.Orbs.LightningOrb.LightningType.Ukulele ||
            self.lightningType == RoR2.Orbs.LightningOrb.LightningType.Loader || self.lightningType == RoR2.Orbs.LightningOrb.LightningType.Tesla
            || self.lightningType == RoR2.Orbs.LightningOrb.LightningType.MageLightning)
            {
                self.AddModdedDamageType(LIGHTNING);
            }
            orig(self);
        }

        private void OnTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.damageType.damageType.HasFlag(DamageType.Shock5s))
            {
                damageInfo.AddModdedDamageType(LIGHTNING);
            }

            orig(self, damageInfo);
        }

        private IEnumerator OnStageStart(On.RoR2.Stage.orig_Start orig, RoR2.Stage self)
        {
            yield return orig(self);

            if (NetworkServer.active && SceneManager.GetActiveScene().name == Scenes.SunderedGrove)
            {
                bool isAnyonePlayingElectrician = true;

                foreach (var pcmc in PlayerCharacterMasterController.instances)
                {
                    if (pcmc.networkUser && pcmc.networkUser.bodyIndexPreference != ElectricianIndex)
                    {
                        isAnyonePlayingElectrician = false;
                    }
                }

                if (!isAnyonePlayingElectrician)
                {
                    bool landmassEnabled = GameObject.Find("HOLDER: Randomization").transform.Find("GROUP: Tunnel Landmass").Find("CHOICE: Tunnel Landmass").gameObject.activeSelf;
                    Vector3 pos = new Vector3(103.4f, -3.1f, 170f);
                    Quaternion rot = Quaternion.Euler(0, -120f, 0);

                    if (!landmassEnabled)
                    {
                        pos = new(-209f, 75f, -185.9f);
                    }

                    GameObject obj = GameObject.Instantiate(BrokenElectricianBody, pos, rot);
                    NetworkServer.Spawn(obj);
                }
            }
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

            SignalOverloadIndicator = PrefabAPI.InstantiateClone(tempestSphereIndicator, "SignalIndicator");
            SignalOverloadIndicator.GetComponent<MeshRenderer>().sharedMaterials = new Material[] {
                Paths.Material.matLightningSphere
            };

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

            ElecMuzzleFlash = Main.Assets.LoadAsset<GameObject>("ElectricinMuzzleFlash.prefab");
            ContentAddition.AddEffect(ElecMuzzleFlash);
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

                    if (self.body.isChampion)
                    {
                        info.force *= 0.2f;
                    }

                    if (motor) motor.ApplyForceImpulse(in info);
                    if (motor2) motor2.ApplyForceImpulse(in info);
                }
            }

            orig(self, damageInfo);

            if (hadShield && self.body.bodyIndex == ElectricianIndex && self.shield <= 0)
            {
                EntityStateMachine machine = EntityStateMachine.FindByCustomName(self.gameObject, "Shield");
                machine.SetNextState(new SignalOverloadFire(0.65f));
            }
        }
    }
}