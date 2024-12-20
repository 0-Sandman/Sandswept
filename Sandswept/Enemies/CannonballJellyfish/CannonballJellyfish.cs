using System;
using RoR2.CharacterAI;
using RoR2.Navigation;
using Sandswept.Enemies.CannonballJellyfish.States;

namespace Sandswept.Enemies.CannonballJellyfish
{
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
    }
}