using System;
using RoR2.CharacterAI;
using RoR2.Navigation;
using Sandswept.Enemies.CannonballJellyfish.States;

namespace Sandswept.Enemies.CannonballJellyfish
{
    [ConfigSection("Enemies :: Cannonball Jellyfish")]
    public class CannonballJellyfish : EnemyBase<CannonballJellyfish>
    {
        public static GameObject JellyCoreProjectile;

        public override void LoadPrefabs()
        {
            prefab = Main.Assets.LoadAsset<GameObject>("CannonJellyBody.prefab");
            prefabMaster = Main.Assets.LoadAsset<GameObject>("CannonJellyMaster.prefab");

            JellyCoreProjectile = Main.Assets.LoadAsset<GameObject>("JellyCoreProjectile.prefab");
            JellyCoreProjectile.GetComponent<ProjectileImpactExplosion>().explosionEffect = Paths.GameObject.SojournExplosionVFX;
            ContentAddition.AddProjectile(JellyCoreProjectile);
        }

        public override void PostCreation()
        {
            base.PostCreation();
            var stages = new List<Stage>() { Stage.AbandonedAqueduct, DirectorAPI.Stage.AbandonedAqueductSimulacrum, DirectorAPI.Stage.AphelianSanctuary, DirectorAPI.Stage.ArtifactReliquary, DirectorAPI.Stage.ArtifactReliquary_AbandonedAqueduct_Theme, DirectorAPI.Stage.ArtifactReliquary_ScorchedAcres_Theme, DirectorAPI.Stage.ScorchedAcres, DirectorAPI.Stage.DisturbedImpact, DirectorAPI.Stage.GoldenDieback, DirectorAPI.Stage.HelminthHatchery, DirectorAPI.Stage.TitanicPlains, DirectorAPI.Stage.TitanicPlainsSimulacrum, DirectorAPI.Stage.SkyMeadow, DirectorAPI.Stage.SkyMeadowSimulacrum, DirectorAPI.Stage.TreebornColony, DirectorAPI.Stage.SunderedGrove, DirectorAPI.Stage.ViscousFalls };
            RegisterEnemy(prefab, prefabMaster, stages, MonsterCategory.BasicMonsters);
        }

        public override void Modify()
        {
            base.Modify();

            master.bodyPrefab = prefab;
            body.baseNameToken.Add("Cannonball Jellyfish");
            var loreToken = body.baseNameToken.Replace("_NAME", "_LORE");
            loreToken.Add("<style=cMono>Welcome to DataScraper (v3.1.53 - beta branch)\r\n$ Scraping memory� done.\r\n$ Resolving� done.\r\n$ Combing for relevant data� done.\r\nComplete!\r\n\r\n</style>UES incident report: Incident 193442\r\nTwo personnel reported seeing a Petrichorpus Scyphozoa inside the ship of the Safe Travels rescue ship. After �terminating� it, the Petrichorpus Scyphozoa was identified as a new species, unrelated to Petrichorpus Scyphozoa entirely. \r\nBegin Transcript�\r\n\r\n�W-why is there a [redacted] jellyfish, in the [redacted] CABIN?!�\r\n\r\n�I-I don't know!! H-How am I supposed to know?!�\r\n\r\n�You�re the [redacted] Xenobiologist! I thought you had a book about this stuff?!�\r\n\r\n�L-look! It�s back in my quarters� Also� aren't those Petrichorian jellyfish usually like� cyan? This one is jet black. And I can't really see inside of it��\r\n\r\n[Sounds of electricity, and the xenobiologist yelping in pain]\r\n\r\n�Shoot the damn thing you oaf! You�re supposed to be the bodyguard, if we want to get picky about jobs!!�\r\n\r\n[Sound of an m335 laser rifle being shot, and rock hitting the ground]\r\n\r\n�What the hell even� is that? I- I can't even tell what kingdom this thing should be in, certainly not an animal, or macrobacteria� \r\n�I dunno, you�re the [redacted] Xenobiologist�");
            body.portraitIcon = Main.hifuSandswept.LoadAsset<Texture>("texCannonballJellyfish.png");
            LanguageAPI.Add(prefab.GetComponent<CharacterBody>().baseNameToken.Replace("_NAME", "_LORE"), "<style=cMono>\r\n\r\nWelcome to DataScraper (v3.1.53 - beta branch)\r\n\r\n$ Scraping memory... done.\r\n\r\n$ Resolving... done.\r\n\r\n$ Combing for relevant data... done.\r\n\r\nComplete!\r\n\r\n</style>\r\n\r\n\r\nUES incident report: Incident 193442\r\nTwo personnel reported seeing a Petrichorpus Scyphozoa inside a cabin of the Safe Travels rescue ship. After ’terminating’ it, it was identified as a new species, unrelated to Petrichorpus Scyphozoa entirely. \r\nBegin Transcript...\r\n\r\n\"W-why is there a [redacted] jellyfish, in the [redacted] CABIN?!\"\r\n\r\n\"I-I don’t know! H-How am I supposed to know?!\"\r\n\r\n\"You’re the [redacted] xenobiologist! I thought you had a book about this stuff?!\"\r\n\r\n\"L-look! It’s back in my quarters... Also... aren’t those Petrichorian jellyfish usually like... cyan? This one is jet black. And I can’t really see inside of it...\"\r\n\r\n[Sounds of electricity, and the xenobiologist yelping in pain]\r\n\r\n\"Shoot the damn thing you oaf! You’re supposed to be the bodyguard, if we want to get picky about jobs!\"\r\n\r\n[Sound of an m335 laser rifle being shot, and rock hitting the ground]\r\n\r\n\"What the hell even... is it? I- I can’t even tell what kingdom this thing should be in, certainly not an animal, or macrobacteria...\" \r\n\r\n\"I dunno, you’re the [redacted] xenobiologist.\"\r\n");

            var locator = body.GetComponent<SkillLocator>();

            ReplaceSkill(locator.primary, SkillDefs.JellyDash.instance.skillDef);

            master.GetComponent<BaseAI>().aimVectorMaxSpeed = 40000f;
            master.GetComponent<BaseAI>().aimVectorDampTime = 0.1f;

            body.GetComponent<CharacterDeathBehavior>().deathState = new(typeof(JellyDeath));
        }

        public override void AddDirectorCard()
        {
            base.AddDirectorCard();
            card.selectionWeight = 1;
            card.spawnCard = isc;
            card.spawnDistance = DirectorCore.MonsterSpawnDistance.Standard;
        }

        public override void AddSpawnCard()
        {
            base.AddSpawnCard();
            isc.directorCreditCost = 70;
            isc.forbiddenFlags = NodeFlags.NoCharacterSpawn;
            isc.hullSize = HullClassification.Human;
            isc.nodeGraphType = MapNodeGroup.GraphType.Air;
            isc.sendOverNetwork = true;
            isc.prefab = prefabMaster;
        }

        public override void SetupIDRS()
        {
            AddDisplayRule(Paths.EquipmentDef.EliteFireEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 1.5233F, 0.31216F),
                localAngles = new Vector3(76.51611F, 160.9118F, 109.6519F),
                localScale = new Vector3(1F, 1F, 1F),
                followerPrefab = Paths.GameObject.DisplayEliteHorn,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteIceEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(-0.40056F, 2.67128F, 0.06114F),
                localAngles = new Vector3(276.0555F, 270F, 180F),
                localScale = new Vector3(0.45094F, 0.45094F, 0.45094F),
                followerPrefab = Paths.GameObject.DisplayEliteIceCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteEarthEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 1.06064F, 0.06114F),
                localAngles = new Vector3(309.1414F, 0F, 270F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F),
                followerPrefab = Paths.GameObject.DisplayEliteMendingAntlers,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteAurelioniteEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0.9253F, 1.84508F, 2.70886F),
                localAngles = new Vector3(0F, 20.62918F, 0F),
                localScale = new Vector3(4.46222F, 4.46222F, 4.46222F),
                followerPrefab = Paths.GameObject.DisplayEliteAurelioniteEquipment,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.ElitePoisonEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 1.17403F, 0.06114F),
                localAngles = new Vector3(271.4092F, 219.645F, 89.99973F),
                localScale = new Vector3(0.76574F, 0.76574F, 0.76574F),
                followerPrefab = Paths.GameObject.DisplayEliteUrchinCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteHauntedEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 2.03823F, 0.53629F),
                localAngles = new Vector3(60.31668F, 180F, 180F),
                localScale = new Vector3(0.82061F, 0.82061F, 0.82061F),
                followerPrefab = Paths.GameObject.DisplayEliteStealthCrown,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteLunarEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 0.96012F, 0.06114F),
                localAngles = new Vector3(90F, 0F, 0F),
                localScale = new Vector3(4.85947F, 4.85F, 4.85947F),
                followerPrefab = Paths.GameObject.DisplayEliteLunarEye,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteLightningEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(-0.15791F, 4.15213F, 0.06114F),
                localAngles = new Vector3(286.3433F, 95.41259F, 174.2014F),
                localScale = new Vector3(3F, 3F, 3F),
                followerPrefab = Paths.GameObject.DisplayEliteRhinoHorn,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteVoidEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(-0.65868F, 3.12402F, 0.06114F),
                localAngles = new Vector3(359.9704F, 287.4572F, 359.9907F),
                localScale = new Vector3(1.85743F, 1.85743F, 1.85743F),
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteBeadEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0.18878F, 1.77869F, 0.24974F),
                localAngles = new Vector3(0F, 82.33108F, 0F),
                localScale = new Vector3(13.23539F, 13.23539F, 13.23539F),
                followerPrefab = Paths.GameObject.DisplayEliteBeadEquipment,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Paths.EquipmentDef.EliteEarthEquipment, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 1.49283F, 0.06114F),
                localAngles = new Vector3(10.12806F, 57.22994F, 20.11787F),
                localScale = new Vector3(7.48976F, 7.48976F, 7.48976F),
                followerPrefab = Paths.GameObject.DisplayEliteMendingAntlers,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Elites.Osmium.Instance.EliteEquipmentDef, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0F, 4.05332F, 0.06114F),
                localAngles = new Vector3(0F, 337.0555F, 270F),
                localScale = new Vector3(2F, 2F, 2F),
                followerPrefab = Elites.Osmium.crownModel,
                limbMask = LimbFlags.None
            });

            AddDisplayRule(Elites.Motivating.Instance.EliteEquipmentDef, new() {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Core",
                localPos = new Vector3(0.31351F, 1.67552F, 0.01473F),
                localAngles = new Vector3(274.0599F, 69.8856F, 196.5728F),
                localScale = new Vector3(9.43998F, 9.21274F, 7F),
                followerPrefab = Elites.Motivating.Crown,
                limbMask = LimbFlags.None
            });

            CollapseIDRS();
        }
    }
}