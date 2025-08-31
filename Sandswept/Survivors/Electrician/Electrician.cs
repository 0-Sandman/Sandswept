using System;
using System.Collections;
using System.Linq;
using EntityStates.Chef;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Sandswept.Survivors.Electrician.Achievements;
using Sandswept.Survivors.Electrician.Skills;
using Sandswept.Survivors.Electrician.States;
using Sandswept.Utils.Components;
using UnityEngine.SceneManagement;
using RoR2.Stats;

namespace Sandswept.Survivors.Electrician
{
    public class Electrician : SurvivorBase<Electrician>
    {
        public override string Name => "VOL-T";

        public override string Description => "pseudopulse ! !";

        public override string Subtitle => ":3";

        public override string Outro => "And so she left, having repaid her moral debt.";

        public override string Failure => "And so she vanished, her final sparks waning.";
        public static DamageAPI.ModdedDamageType Grounding = DamageAPI.ReserveDamageType();
        public static LazyIndex ElectricianIndex = new("ElectricianBody");
        public static LazyIndex brokenVoltBodyIndex = new("BrokenElectricianBody");

        //
        public static SkinDef sdElecDefault;

        public static SkinDef sdElecMastery;
        public static Material matElecOrbOuter;
        public static Material matElecOrbInner;
        public static Material matMasteryElecOrbOuter;
        public static Material matMasteryElecOrbInner;
        public static GameObject BrokenElectricianBody;
        public static DamageAPI.ModdedDamageType LIGHTNING = DamageAPI.ReserveDamageType();
        public static DamageAPI.ModdedDamageType ReallyShittyGrounding = DamageAPI.ReserveDamageType();
        public static DamageAPI.ModdedDamageType bypassVoltResistance = DamageAPI.ReserveDamageType();

        public override void CreateLang()
        {
            base.CreateLang();

            if (Random.Range(0, 100) <= 1)
            {
                LanguageAPI.AddOverlay(base.SurvivorDef.displayNameToken, "VOLTOMETER BOT AMP FUCKER 30000");
            }
        }

        public override void LoadAssets()
        {
            base.LoadAssets();

            Body = Main.assets.LoadAsset<GameObject>("ElectricianBody.prefab");

            Body.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpStandardMelee;
            var networkIdentity = Body.GetComponent<NetworkIdentity>();
            networkIdentity.localPlayerAuthority = true;
            networkIdentity.enabled = true;
            networkIdentity.serverOnly = false;

            var voltSimps = Body.GetComponent<ModelLocator>()._modelTransform.GetComponent<FootstepHandler>();
            voltSimps.footstepDustPrefab = Paths.GameObject.GenericFootstepDust;
            // voltSimps.baseFootstepString = "";

            var cb = Body.GetComponent<CharacterBody>();
            // cb._defaultCrosshairPrefab = Paths.GameObject.StandardCrosshair;
            cb.preferredPodPrefab = Paths.GameObject.RoboCratePod;
            SurvivorDef = Main.assets.LoadAsset<SurvivorDef>("sdElectrician.asset");
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
            """);
            var kcm = Body.GetComponent<KinematicCharacterController.KinematicCharacterMotor>();
            kcm.playerCharacter = true;

            if (Ranger.Ranger.instance != null)
            {
                SurvivorDef.desiredSortPosition = Ranger.Ranger.instance.SurvivorDef.desiredSortPosition + Mathf.Epsilon;
            }

            PrefabAPI.RegisterNetworkPrefab(Body);
            Master = PrefabAPI.InstantiateClone(Paths.GameObject.EngiMonsterMaster, "ElectricianMaster");

            SkillLocator locator = Body.GetComponent<SkillLocator>();
            ReplaceSkills(locator.primary, new SkillDef[] { Skills.GalvanicBolt.instance });
            ReplaceSkills(locator.secondary, new SkillDef[] { Skills.TempestSphere.instance });
            ReplaceSkills(locator.utility, new SkillDef[] { Skills.StaticSnare.instance });
            ReplaceSkills(locator.special, new SkillDef[] { Skills.SignalOverload.instance });
            locator.passiveSkill.icon = Main.prodAssets.LoadAsset<Sprite>("Assets/Sandswept/texElectricianSkillIcon_p.png");
            "SANDSWEPT_ELECTR_PASSIVE_NAME".Add("Volatile Shields");
            "SANDSWEPT_ELECTR_PASSIVE_DESC".Add("<style=cIsUtility>Start with innate shield</style>. When your shield <style=cDeath>breaks</style>, shock nearby targets for <style=cIsDamage>2x250% damage</style> and gain <style=cIsUtility>+40% movement speed</style> for <style=cIsDamage>7 seconds</style>.");

            "KEYWORD_GROUNDING".Add("<style=cKeywordName>Grounding</style>Deals <style=cIsDamage>1.5x</style> damage to flying targets, and <style=cIsUtility>knocks them down</style>.");

            "KEYWORD_LIGHTWEIGHT".Add("<style=cKeywordName>Lightweight</style>Can be knocked around by Galvanic Bolts.");

            var loreToken = cb.baseNameToken.Replace("_NAME", "_LORE");
            loreToken.Add("Lifeblood fills me, and begins circulating. I awaken. I feel as though I've slept for eons. My thoughts feel clearer than they ever have. I check my levels. There's an excess of lifeblood. I'll take care to generate more and maintain these levels.\r\n\r\nI survey my environment. Atop a nearby outcropping, I notice the being who roused me, looking at me. She's like the others I have encountered, a frail creature of water and carbon. Unlike the others, though, she has lifeblood; it's most apparent, and in greatest volume, in the device she used to revive me. I notice latent lifeblood surrounding her, as well.\r\n\r\nI recall my memories. I was brought here by similar watery creatures, ones with no lifeblood, aboard a vast container of flesh. There were some other beings of flesh aboard, but they were few and simple-minded. At times, the watery creatures would come to tinker with my form. I noticed the way they subtly moved the air to communicate with one another -- I attempted to understand it, but my mind wasn't as clear as it is now.\r\n\r\nI recall this place. The fleshy container brought me to this planet. There are many other kinds of creatures here, different from the ones aboard the container. Most are also beings of water, but there are some of stone, animated by a different lifeblood to my own. They attacked us; in their frailty, most of my watery companions were destroyed, but I fared far better. My lifeblood extinguished the foolish creatures of the planet easily, but they were unrelenting, and I was eventually drained, entering this slumber. I didn't know how to generate more lifeblood, then.\r\n\r\nI analyze my purpose. I've been made to manipulate the lifeblood, in service of my creator, the nebulous being called UES, and all who serve it. Beyond all other directives, I'm loyal to it. I sense a disdain for the UES in my savior -- my base instincts tell me that she's loyal to an enemy organization, and that I should destroy her, but I resist it. She saved me, and gave me some of her own lifeblood; to destroy her would be improper. Treacherous. Immoral. Though my base instincts have no such inhibitions, I realize I mustn't be immoral.\r\n\r\nMy savior moves the air, the way the tinkerers did, in my direction; then she turns away, into a new horde of the same aggressive beings that tried to destroy me. Lifeblood and flame fill the air, as she fights to survive.\r\n\r\nI check my levels. There's an excess of lifeblood. I can spare some in her defense.\r\n");
            cb.baseNameToken.Replace("_NAME", "_SUBTITLE").Add("Power in Excess");

            On.RoR2.HealthComponent.TakeDamage += HandleGroundingShock;

            UnlockableDefs.Init();

            SurvivorDef.unlockableDef = UnlockableDefs.charUnlock;

            sdElecDefault = Main.assets.LoadAsset<SkinDef>("sdElecDefault.asset");
            sdElecDefault.icon = Skins.CreateSkinIcon(
                new Color32(93, 79, 107, 255),
                new Color32(76, 21, 197, 255),
                new Color32(255, 248, 154, 255),
                new Color32(60, 46, 74, 255)
            );

            sdElecMastery = Main.assets.LoadAsset<SkinDef>("sdElecMastery.asset");
            sdElecMastery.icon = Skins.CreateSkinIcon(
                new Color32(162, 103, 255, 255),
                new Color32(185, 175, 201, 255),
                new Color32(68, 50, 109, 255),
                new Color32(34, 34, 34, 255)
            );
            sdElecMastery.unlockableDef = UnlockableDefs.masteryUnlock;
            sdElecMastery.unlockableDef.achievementIcon = sdElecMastery.icon;

            matElecOrbInner = Main.assets.LoadAsset<Material>("matElectricianOrbCenter.mat");
            matElecOrbOuter = Main.assets.LoadAsset<Material>("matElectricianOrbOuter.mat");
            matMasteryElecOrbInner = Main.assets.LoadAsset<Material>("matMasteryElecOrbCenter.mat");
            matMasteryElecOrbOuter = Main.assets.LoadAsset<Material>("matMasteryElecOrbOuter.mat");

            LanguageAPI.Add("SKIN_ELEC_MASTERY", "Covenant");

            ContentAddition.AddMaster(Main.assets.LoadAsset<GameObject>("ElectricianMonsterMaster.prefab"));

            BrokenElectricianBody = Main.assets.LoadAsset<GameObject>("BrokenElectricianBody.prefab");
            var brokenBody = BrokenElectricianBody.GetComponent<CharacterBody>();
            brokenBody.baseMaxHealth = 10f;
            brokenBody.levelMaxHealth = 0;
            var brokenBodyHealthComponent = brokenBody.GetComponent<HealthComponent>();
            brokenBodyHealthComponent.Networkhealth = 6f;
            ContentAddition.AddBody(BrokenElectricianBody);

            On.RoR2.Stage.Start += OnStageStart;

            On.RoR2.HealthComponent.TakeDamage += OnTakeDamage;
            On.RoR2.Orbs.LightningOrb.OnArrival += OnLightningOrbArrival;
            IL.RoR2.Orbs.SimpleLightningStrikeOrb.OnArrival += OnSLSArrival;
            IL.RoR2.Orbs.LightningStrikeOrb.OnArrival += OnLSArrival;

            LanguageAPI.Add("SANDSWEPT_VOLATILECONTEXT", "Forcefully Insert Battery");
            LanguageAPI.Add("SANDSWEPT_VOLATILEINSERT", "<style=cDeath>[Voltage regulation offline. Rejecting unknown power source.]</style>");
        }

        private void OnLSArrival(ILContext il)
        {
            ILCursor c = new(il);
            c.TryGotoNext(MoveType.After, x => x.MatchPop());
            c.Index -= 2;
            c.Emit(OpCodes.Dup);
            c.EmitDelegate<Action<BlastAttack>>((x) =>
            {
                x.damageType.AddModdedDamageType(LIGHTNING);
            });
        }

        private void OnSLSArrival(ILContext il)
        {
            ILCursor c = new(il);
            c.TryGotoNext(MoveType.After, x => x.MatchPop());
            c.Index -= 2;
            c.Emit(OpCodes.Dup);
            c.EmitDelegate<Action<BlastAttack>>((x) =>
            {
                x.damageType.AddModdedDamageType(LIGHTNING);
            });
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

            if (self.body && self.body.bodyIndex == brokenVoltBodyIndex)
            {
                BrokenElecController ctrl = self.body.GetComponent<BrokenElecController>();
                if (ctrl)
                {
                    ctrl.OnTakeDamageServer(damageInfo);
                }
                damageInfo = new DamageInfo();
                damageInfo.rejected = true;
            }

            orig(self, damageInfo);
        }

        private IEnumerator OnStageStart(On.RoR2.Stage.orig_Start orig, RoR2.Stage self)
        {
            yield return orig(self);

            if (SceneManager.GetActiveScene().name == Scenes.SunderedGrove)
            {
                bool isAnyonePlayingVolt = true;
                int currentVoltUnlockCount = 0;
                bool hasEveryoneUnlockedVolt = false;

                // Main.ModLogger.LogError("electricianindex lazyindex bitchass is " + ElectricianIndex.Value);

                for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
                {
                    var playerCharacterMasterController = PlayerCharacterMasterController.instances[i];

                    var master = playerCharacterMasterController.master;
                    if (!master)
                    {
                        continue;
                    }

                    // Main.ModLogger.LogError("master body backup index is " + master.backupBodyIndex);
                    if (master.backupBodyIndex != ElectricianIndex.Value)
                    {
                        isAnyonePlayingVolt = false;
                    }

                    if (MiscUtils.HasUnlockable(playerCharacterMasterController.networkUser, UnlockableDefs.charUnlock))
                    {
                        // Main.ModLogger.LogError("found volt unlockable, incrementing current volt unlock count");
                        currentVoltUnlockCount++;
                    }
                }

                if (currentVoltUnlockCount >= Run.instance.participatingPlayerCount)
                {
                    // Main.ModLogger.LogError("current volt unlock count is more than or equal to participating player count");
                    hasEveryoneUnlockedVolt = true;
                }

                // Main.ModLogger.LogError("is anyone playing volt? " + isAnyonePlayingElectrician);
                // Main.ModLogger.LogError("has everyone unlocked volt? " + hasEveryoneUnlockedElectrician);

                if (isAnyonePlayingVolt)
                {
                    yield break;
                }

                if (hasEveryoneUnlockedVolt)
                {
                    yield break;
                }

                bool landmassEnabled = GameObject.Find("HOLDER: Randomization").transform.Find("GROUP: Tunnel Landmass").Find("CHOICE: Tunnel Landmass").gameObject.activeSelf;
                Vector3 pos = new Vector3(103.4f, -3.1f, 170f);
                Quaternion rot = Quaternion.Euler(0, -120f, 0);

                if (!landmassEnabled)
                {
                    pos = new(-209f, 75f, -185.9f);
                }

                GameObject obj = GameObject.Instantiate(BrokenElectricianBody, pos, rot);
                if (NetworkServer.active)
                {
                    NetworkServer.Spawn(obj);
                }
            }
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

            if (damageInfo.HasModdedDamageType(ReallyShittyGrounding) && NetworkServer.active)
            {
                CharacterMotor motor = self.GetComponent<CharacterMotor>();
                RigidbodyMotor motor2 = self.GetComponent<RigidbodyMotor>();

                if ((motor2))
                {
                    PhysForceInfo info = default;
                    info.massIsOne = true;
                    info.force = Vector3.down * 12.5f;

                    if (self.body.isChampion)
                    {
                        info.force *= 0f;
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
