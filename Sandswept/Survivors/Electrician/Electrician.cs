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
using R2API.Networking;

namespace Sandswept.Survivors.Electrician
{
    [ConfigSection("Survivors :: VOL-T")]
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

            if (DefaultEnabledCallback(Ranger.Ranger.instance))
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

            On.RoR2.HealthComponent.TakeDamageProcess += HandleGroundingShockAndPassive;

            UnlockableDefs.Init();

            SurvivorDef.unlockableDef = UnlockableDefs.charUnlock;

            sdElecDefault = Main.assets.LoadAsset<SkinDef>("sdElecDefault.asset");
            sdElecDefault.nameToken = "VOLT_SKIN_DEFAULT_NAME";
            sdElecDefault.icon = Skins.CreateSkinIcon(
                new Color32(93, 79, 107, 255),
                new Color32(76, 21, 197, 255),
                new Color32(255, 248, 154, 255),
                new Color32(60, 46, 74, 255)
            );

            sdElecMastery = Main.assets.LoadAsset<SkinDef>("sdElecMastery.asset");
            sdElecMastery.nameToken = "VOLT_SKIN_COVENANT_NAME";
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

            LanguageAPI.Add("VOLT_SKIN_DEFAULT_NAME", "Default");
            LanguageAPI.Add("VOLT_SKIN_DEFAULT_DESC", "This survivor's default skin.");
            LanguageAPI.Add("VOLT_SKIN_COVENANT_NAME", "Covenant");
            LanguageAPI.Add("VOLT_SKIN_COVENANT_DESC", "A successful experiment. The wrath of storms, captured.");

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

        private void HandleGroundingShockAndPassive(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
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

                if (motor2)
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
                machine.SetNextState(new SignalOverloadFire());
            }
        }

        public override void SetUpIDRS()
        {
            AddDisplayRule(Paths.EquipmentDef.EliteFireEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(0.15102F, 0.12445F, 0.00312F),
                localAngles = new Vector3(299.6079F, 293.1193F, 0.83963F),
                localScale = new Vector3(0.19198F, 0.19198F, 0.19198F),
                followerPrefab = Paths.GameObject.DisplayEliteHorn,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteIceEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(-0.00157F, 0.12259F, -0.01931F),
                localAngles = new Vector3(274.1474F, 184.8704F, 355.4697F),
                localScale = new Vector3(0.07106F, 0.07106F, 0.07106F),
                followerPrefab = Paths.GameObject.DisplayEliteIceCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteAurelioniteEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(0.00464F, -0.02292F, 0.20342F),
                localAngles = new Vector3(357.5067F, 355.5899F, 0.28962F),
                localScale = new Vector3(0.61315F, 0.61315F, 0.61315F),
                followerPrefab = Paths.GameObject.DisplayEliteAurelioniteEquipment,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.ElitePoisonEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(-0.00157F, 0.12259F, -0.01931F),
                localAngles = new Vector3(274.1474F, 184.8704F, 355.4697F),
                localScale = new Vector3(0.07106F, 0.07106F, 0.07106F),
                followerPrefab = Paths.GameObject.DisplayEliteUrchinCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteHauntedEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(-0.01879F, 0.09421F, 0.0312F),
                localAngles = new Vector3(317.3474F, 271.5199F, 262.8486F),
                localScale = new Vector3(0.10436F, 0.10436F, 0.10436F),
                followerPrefab = Paths.GameObject.DisplayEliteStealthCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteLunarEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(0.00412F, 0.12228F, -0.24139F),
                localAngles = new Vector3(358.4215F, 2.26561F, 0F),
                localScale = new Vector3(0.37098F, 0.37026F, 0.37098F),
                followerPrefab = Paths.GameObject.DisplayEliteLunarEye,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteLightningEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(-0.16882F, 0.14272F, -0.02344F),
                localAngles = new Vector3(61.59446F, 65.21429F, 166.8794F),
                localScale = new Vector3(0.14094F, 0.14094F, 0.14094F),
                limbMask = LimbFlags.None,
                followerPrefab = Paths.GameObject.DisplayEliteHorn
            });

            AddDisplayRule(Paths.EquipmentDef.EliteBeadEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(0.00438F, 0.17946F, -0.02627F),
                localAngles = new Vector3(17.54483F, 147.7092F, 351.5205F),
                localScale = new Vector3(0.02559F, 0.02559F, 0.02559F),
                followerPrefab = Paths.GameObject.DisplayEliteBeadSpike,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteEarthEquipment, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(-0.00169F, 0.26817F, -0.01414F),
                localAngles = new Vector3(340.1616F, 181.5028F, 0.48644F),
                localScale = new Vector3(0.95987F, 0.95987F, 0.95987F),
                followerPrefab = Paths.GameObject.DisplayEliteMendingAntlers,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Elites.Osmium.Instance.EliteEquipmentDef, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(-0.00276F, 0.2543F, -0.15331F),
                localAngles = new Vector3(88.43302F, 166.817F, 255.5621F),
                localScale = new Vector3(0.23207F, 0.23207F, 0.23207F),
                followerPrefab = Elites.Osmium.crownModel,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Elites.Motivating.Instance.EliteEquipmentDef, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Head",
                localPos = new Vector3(0.00069F, 0.21535F, -0.02629F),
                localAngles = new Vector3(0.17021F, 173.9592F, 359.9677F),
                localScale = new Vector3(1.0493F, 1.0493F, 1.0493F),
                followerPrefab = Elites.Motivating.Crown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.Missile, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "MuzzleCannon",
                localPos = new Vector3(0.44203F, 0.00705F, -0.10548F),
                localAngles = new Vector3(359.879F, 355.2826F, 269.6059F),
                localScale = new Vector3(0.15058F, 0.15058F, 0.15058F),
                followerPrefab = Paths.GameObject.DisplayMissileLauncher,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.MissileVoid, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "MuzzleCannon",
                localPos = new Vector3(-0.02395F, 0.42053F, -0.16782F),
                localAngles = new Vector3(0.89037F, 353.2066F, 359.0491F),
                localScale = new Vector3(0.13981F, 0.13981F, 0.13981F),
                followerPrefab = Paths.GameObject.DisplayMissileLauncherVoid,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.IceRing, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "MuzzleCannon",
                localPos = new Vector3(0.023F, 0.31638F, -0.53858F),
                localAngles = new Vector3(89.31158F, 150.7557F, 226.7192F),
                localScale = new Vector3(0.46789F, 0.46789F, 0.46789F),
                followerPrefab = Paths.GameObject.DisplayIceRing,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.FireRing, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "MuzzleCannon",
                localPos = new Vector3(0.02305F, -0.30426F, -0.53852F),
                localAngles = new Vector3(89.31129F, 150.7551F, 226.7184F),
                localScale = new Vector3(0.46789F, 0.46789F, 0.46789F),
                followerPrefab = Paths.GameObject.DisplayFireRing,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.ElementalRingVoid, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "MuzzleOrb",
                localPos = new Vector3(-0.0116F, -0.0129F, 0.0133F),
                localAngles = new Vector3(0.3398F, 359.3324F, 181.0462F),
                localScale = new Vector3(0.89726F, 0.89726F, 0.8892F),
                followerPrefab = Paths.GameObject.DisplayVoidRing,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.BFG, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "MuzzleCannon",
                localPos = new Vector3(0.00315F, -0.01011F, -0.32272F),
                localAngles = new Vector3(359.8549F, 352.9451F, 179.6131F),
                localScale = new Vector3(0.29502F, 0.29502F, 0.29502F),
                followerPrefab = Paths.GameObject.DisplayBFG,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.Behemoth, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "MuzzleOrb",
                localPos = new Vector3(-0.31696F, 0.07576F, -0.21301F),
                localAngles = new Vector3(342.8137F, 272.3962F, 270.8445F),
                localScale = new Vector3(0.10796F, 0.10796F, 0.10796F),
                followerPrefab = Paths.GameObject.DisplayBehemoth,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.PermanentDebuffOnHit, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "ClavicleR",
                localPos = new Vector3(0.06684F, -0.10952F, 0.00646F),
                localAngles = new Vector3(271.2686F, 286.3356F, 159.5639F),
                localScale = new Vector3(0.70528F, 0.70528F, 0.70528F),
                followerPrefab = Paths.GameObject.DisplayScorpion,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.ArmorReductionOnHit, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "LegL",
                localPos = new Vector3(-0.00609F, 0.82839F, 0.0023F),
                localAngles = new Vector3(272.0602F, 274.6225F, 174.9532F),
                localScale = new Vector3(0.31007F, 0.31007F, 0.31007F),
                followerPrefab = Paths.GameObject.DisplayWarhammer,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.ArmorPlate, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "ThighL",
                localPos = new Vector3(0.07245F, 0.17259F, -0.07027F),
                localAngles = new Vector3(84.00079F, 260.89F, 347.9175F),
                localScale = new Vector3(0.54409F, 0.55068F, 0.54187F),
                followerPrefab = Paths.GameObject.DisplayRepulsionArmorPlate,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.SecondarySkillMagazine, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "MuzzleOrb",
                localPos = new Vector3(-0.002F, -0.18357F, -0.24757F),
                localAngles = new Vector3(341.1747F, 178.3867F, 1.47223F),
                localScale = new Vector3(0.0824F, 0.0824F, 0.0824F),
                followerPrefab = Paths.GameObject.DisplayDoubleMag,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.IncreaseDamageOnMultiKill, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "ThighR",
                localPos = new Vector3(-0.11984F, 0.05834F, -0.05162F),
                localAngles = new Vector3(274.9813F, 264.1147F, 6.0653F),
                localScale = new Vector3(0.22404F, 0.22404F, 0.22404F),
                followerPrefab = Paths.GameObject.DisplayIncreaseDamageOnMultiKill,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.SprintBonus, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.00315F, 0.06733F, -0.32271F),
                localAngles = new Vector3(277.9451F, 191.2201F, 286.983F),
                localScale = new Vector3(0.70239F, 0.70239F, 0.70239F),
                followerPrefab = Paths.GameObject.DisplaySoda,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.CritGlasses, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Chest",
                localPos = new Vector3(0.00145F, 0.27635F, -0.30563F),
                localAngles = new Vector3(0F, 180F, 0F),
                localScale = new Vector3(0.55757F, 0.54762F, 0.49804F),
                followerPrefab = Paths.GameObject.DisplayGlasses,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.CritGlassesVoid, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Chest",
                localPos = new Vector3(0F, 0.12423F, -0.32271F),
                localAngles = new Vector3(0F, 180F, 0F),
                localScale = new Vector3(0.47462F, 1.26719F, 0.58078F),
                followerPrefab = Paths.GameObject.DisplayGlassesVoid,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.Bandolier, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Coil",
                localPos = new Vector3(0.01471F, -0.20326F, -0.02905F),
                localAngles = new Vector3(277.6144F, 331.5123F, 59.69401F),
                localScale = new Vector3(0.56154F, 0.77659F, 0.49639F),
                followerPrefab = Paths.GameObject.DisplayBandolier,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.PersonalShield, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Chest",
                localPos = new Vector3(0.00547F, 0.11018F, 0.41647F),
                localAngles = new Vector3(90F, 180F, 0F),
                localScale = new Vector3(0.29502F, 0.29502F, 0.29502F),
                followerPrefab = Paths.GameObject.DisplayShieldGenerator,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.Blackhole, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "MuzzleOrb",
                localPos = new Vector3(-0.02222F, -0.06466F, 0.45125F),
                localAngles = new Vector3(90F, 180F, 0F),
                localScale = new Vector3(0.27092F, 0.27092F, 0.27092F),

                followerPrefab = Paths.GameObject.DisplayGravCube,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.BossDamageBonus, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "ArmR",
                localPos = new Vector3(0.0417F, 0.05364F, 0.15773F),
                localAngles = new Vector3(84.44582F, 77.29805F, 243.7534F),
                localScale = new Vector3(0.46712F, 0.46712F, 0.46712F),

                followerPrefab = Paths.GameObject.DisplayAPRound,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.Firework, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "ThighR",
                localPos = new Vector3(-0.10805F, 0.01304F, 0.10625F),
                localAngles = new Vector3(90F, 180F, 0F),
                localScale = new Vector3(0.29502F, 0.29502F, 0.29502F),

                followerPrefab = Paths.GameObject.DisplayFirework,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.HealWhileSafe, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "ClavicleR",
                localPos = new Vector3(-0.0014F, -0.10851F, 0.1159F),
                localAngles = new Vector3(24.48458F, 77.01466F, 136.4743F),
                localScale = new Vector3(0.05323F, 0.05323F, 0.05323F),

                followerPrefab = Paths.GameObject.DisplaySnail,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.FragileDamageBonus, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "ClavicleR",
                localPos = new Vector3(0.02753F, 0.201F, 0.00171F),
                localAngles = new Vector3(83.11189F, 279.0676F, 189.5616F),
                localScale = new Vector3(0.65641F, 1.28782F, 0.66928F),

                followerPrefab = Paths.GameObject.DisplayDelicateWatch,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.NearbyDamageBonus, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "ClavicleL",
                localPos = new Vector3(-0.00534F, 0.00316F, 0.00055F),
                localAngles = new Vector3(90F, 180F, 0F),
                localScale = new Vector3(0.28695F, 0.28695F, 0.28695F),

                followerPrefab = Paths.GameObject.DisplayDiamond,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.Medkit, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "MuzzleCannon",
                localPos = new Vector3(0.15206F, -0.15915F, -0.23044F),
                localAngles = new Vector3(70.33351F, 276.3448F, 100.0341F),
                localScale = new Vector3(0.30933F, 0.43828F, 0.30933F),

                followerPrefab = Paths.GameObject.DisplayMedkit,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.MoveSpeedOnKill, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "MuzzleCannon",
                localPos = new Vector3(0.15206F, -0.15915F, -0.23044F),
                localAngles = new Vector3(70.33351F, 276.3448F, 100.0341F),
                localScale = new Vector3(0.30933F, 0.43828F, 0.30933F),

                followerPrefab = Paths.GameObject.DisplayGrappleHook,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.IncreasePrimaryDamage, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.16923F, -0.02905F, 0.19832F),
                localAngles = new Vector3(357.1011F, 69.14262F, 358.8921F),
                localScale = new Vector3(0.70423F, 0.70423F, 0.70423F),

                followerPrefab = Paths.GameObject.DisplayIncreasePrimaryDamage,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.ChainLightning, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "LegR",
                localPos = new Vector3(-0.13869F, 0.71311F, -0.02354F),
                localAngles = new Vector3(0.08567F, 91.38188F, 179.8218F),
                localScale = new Vector3(1.04461F, 0.95293F, 1.04461F),

                followerPrefab = Paths.GameObject.DisplayUkulele,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.ChainLightningVoid, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "LegR",
                localPos = new Vector3(0.03046F, 0.42533F, 0.05822F),
                localAngles = new Vector3(357.2328F, 98.62356F, 138.2117F),
                localScale = new Vector3(1.0975F, 1.0975F, 1.0975F),

                followerPrefab = Paths.GameObject.DisplayUkuleleVoid,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.Dagger, new()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "ClavicleR",
                localPos = new Vector3(0.12395F, -0.1862F, -0.09846F),
                localAngles = new Vector3(38.79285F, 347.1523F, 133.9545F),
                localScale = new Vector3(1.1221F, 1.1221F, 1.1221F),

                followerPrefab = Paths.GameObject.DisplayDagger,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.ItemDef.LunarSun, new()
            {
                ruleType = ItemDisplayRuleType.LimbMask,
                childName = "Head",
                localPos = new Vector3(0.12395F, -0.1862F, -0.09846F),
                localAngles = new Vector3(38.79285F, 347.1523F, 133.9545F),
                localScale = new Vector3(1.1221F, 1.1221F, 1.1221F),

                followerPrefab = Paths.GameObject.DisplaySunHead,
                limbMask = LimbFlags.Head
            });

            CollapseIDRS();
        }
    }
}
//