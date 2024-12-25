using System;
using System.Collections;
using System.Linq;
using EntityStates.Chef;
using Sandswept.Survivors.Electrician.Achievements;
using Sandswept.Survivors.Electrician.Skills;
using Sandswept.Survivors.Electrician.States;
using Sandswept.Utils.Components;
using UnityEngine.SceneManagement;

namespace Sandswept.Survivors.Electrician
{
    public class Electrician : SurvivorBase<Electrician>
    {
        public override string Name => "VOL-T";

        public override string Description => "pseudopulse ! !";

        public override string Subtitle => ":3";

        public override string Outro => "And so she left, having repaid her moral debt.";

        public override string Failure => "And so she vanished, her final sparks waning.";
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
            SurvivorDef.outroFlavorToken.Add("And so she left, having repaid her moral debt.");
            SurvivorDef.mainEndingEscapeFailureFlavorToken.Add("And so she vanished, her final sparks waning.");
            SurvivorDef.descriptionToken.Add(
                """
                VOL-T is a positional survivor who relies on locking down the area to offset her frailty.

                < ! > Galvanic Bolt can lodge into any terrain, but will explode and bounce off enemies. Landing a direct hit will deal additional damage, at the expense of having less control over where the ball lands.

                < ! > Tempest Sphere will slow down anything it makes contact with. Good positioning of the orb can keep targets within range of your Galvanic Bolts.

                < ! > Static Snare doubles as mobility and burst damage. Blasting it away with projectile impacts can allow you to close large distances quickly!

                < ! > At the expense of all of your shield, Signal Overload can deal large group damage and drag airborne enemies down. The more shield you dump into it, the stronger it becomes.
                """
            );
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
            "SANDSWEPT_ELECTR_PASSIVE_DESC".Add("<style=cIsUtility>Start with innate shields</style>. When your shield <style=cDeath>breaks</style>, shock nearby targets for <style=cIsDamage>2x600% damage</style> and gain <style=cIsUtility>+40% movement speed</style> for <style=cIsDamage>7 seconds</style>.");

            "KEYWORD_GROUNDING".Add("<style=cKeywordName>Grounding</style>Deals <style=cIsDamage>1.5x</style> damage to flying targets, and <style=cDeath>knocks them down</style>.");

            "KEYWORD_LIGHTWEIGHT".Add("<style=cKeywordName>Lightweight</style>Can be knocked around by heavy projectiles.");

            var loreToken = cb.baseNameToken.Replace("_NAME", "_LORE");
            loreToken.Add("Lifeblood fills me, and begins circulating. I awaken. I feel as though I’ve slept for eons. My thoughts feel clearer than they ever have. I check my levels. There’s an excess of lifeblood. I’ll take care to generate more and maintain these levels.\r\n\r\nI survey my environment. Atop a nearby outcropping, I notice the being who roused me, looking at me. She’s like the others I have encountered, a frail creature of water and carbon. Unlike the others, though, she has lifeblood; it’s most apparent, and in greatest volume, in the device she used to revive me. I notice latent lifeblood surrounding her, as well.\r\n\r\nI recall my memories. I was brought here by similar watery creatures, ones with no lifeblood, aboard a vast container of flesh. There were some other beings of flesh aboard, but they were few and simple-minded. At times, the watery creatures would come to tinker with my form. I noticed the way they subtly moved the air to communicate with one another — I attempted to understand it, but my mind wasn’t as clear as it is now.\r\n\r\nI recall this place. The fleshy container brought me to this planet. There are many other kinds of creatures here, different from the ones aboard the container. Most are also beings of water, but there are some of stone, animated by a different lifeblood to my own. They attacked us; in their frailty, most of my watery companions were destroyed, but I fared far better. My lifeblood extinguished the foolish creatures of the planet easily, but they were unrelenting, and I was eventually drained, entering this slumber. I didn’t know how to generate more lifeblood, then.\r\n\r\nI analyze my purpose. I’ve been made to manipulate the lifeblood, in service of my creator, the nebulous being called UES, and all who serve it. Beyond all other directives, I’m loyal to it. I sense a disdain for the UES in my savior — my base instincts tell me that she’s loyal to an enemy organization, and that I should destroy her, but I resist it. She saved me, and gave me some of her own lifeblood; to destroy her would be improper. Treacherous. Immoral. Though my base instincts have no such inhibitions, I realize I mustn’t be immoral.\r\n\r\nMy savior moves the air, the way the tinkerers did, in my direction; then she turns away, into a new horde of the same aggressive beings that tried to destroy me. Lifeblood and flame fill the air, as she fights to survive.\r\n\r\nI check my levels. There’s an excess of lifeblood. I can spare some in her defense.\r\n");
            cb.baseNameToken.Replace("_NAME", "_SUBTITLE").Add("Power in Excess");

            GalvanicBolt = Main.Assets.LoadAsset<GameObject>("GalvanicBallProjectile.prefab");
            // meow meow meow meow meow meow meow
            // var projectileProximityBeamController = GalvanicBolt.GetComponent<ProjectileProximityBeamController>();
            // projectileProximityBeamController.attackRange = 13f; // already reduced it to 8f in unity
            // projectileProximityBeamController.procCoefficient = 0.8f;  // no no no no no no no no no no no no non ono no no no no no we are not gutting every single proc item on elec for no reason
            // projectileProximityBeamController.damageCoefficient = 0.75f; // no, set to 1f in unity and attack speed reduced to 1.2s
            // weh
            // mrraow
            GalvanicBolt.GetComponent<GalvanicBallController>().damage = 2f; // im not launching unity lmao
            ContentAddition.AddNetworkedObject(GalvanicBolt);
            PrefabAPI.RegisterNetworkPrefab(GalvanicBolt);
            ContentAddition.AddProjectile(GalvanicBolt);

            // this gets instantiatecloned to break its prefab status so i can parent stuff to it over in CreateVFX
            TempestSphere = PrefabAPI.InstantiateClone(Main.Assets.LoadAsset<GameObject>("TempestSphereProjectile.prefab"), "TempestSphereProjectile");
            ContentAddition.AddProjectile(TempestSphere);

            StaticSnare = Main.Assets.LoadAsset<GameObject>("TripwireMineProjectile.prefab");
            ContentAddition.AddNetworkedObject(StaticSnare);
            PrefabAPI.RegisterNetworkPrefab(StaticSnare);
            ContentAddition.AddProjectile(StaticSnare);

            Main.Instance.StartCoroutine(CreateVFX());

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

            SignalOverloadIndicator = PrefabAPI.InstantiateClone(tempestSphereIndicator, "SignalIndicator", false);
            SignalOverloadIndicator.GetComponent<MeshRenderer>().sharedMaterials = new Material[] {
                Paths.Material.matLightningSphere
            };

            GalvanicBolt.FindComponent<MeshRenderer>("Radius").sharedMaterial = Paths.Material.matTeamAreaIndicatorIntersectionPlayer;

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

            var dac = TempestSphere.AddComponent<DetachAndCollapse>();
            dac.target = sphereVFX.transform;
            dac.collapseTime = 0.4f;

            staticSnareImpactVFX = PrefabAPI.InstantiateClone(Paths.GameObject.LoaderGroundSlam, "Sigma Gyatt Rizz Ohio Fa-", false);
            foreach (ShakeEmitter ughShakesButt in staticSnareImpactVFX.GetComponents<ShakeEmitter>())
            {
                ughShakesButt.enabled = false;
            }
            var effectComponent = staticSnareImpactVFX.GetComponent<EffectComponent>();
            effectComponent.soundName = "Play_loader_m1_impact";
            ContentAddition.AddEffect(staticSnareImpactVFX);

            LightningZipEffect = PrefabAPI.InstantiateClone(Paths.GameObject.BeamSphereGhost, "LightningZipOrb", false);
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