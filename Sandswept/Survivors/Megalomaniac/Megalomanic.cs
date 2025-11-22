
using System;
using System.Collections;
using System.Linq;
using EntityStates.Chef;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Sandswept.Utils.Components;
using UnityEngine.SceneManagement;
using RoR2.EntityLogic;

namespace Sandswept.Survivors.Megalomaniac
{
    public class Megalomaniac : SurvivorBase<Megalomaniac>
    {
        public override string Name => "Megalomaniac";

        public override string Description => "sigma";

        public override string Subtitle => "sigma";

        public override string Outro => "sigma";

        public override string Failure => "sigma";
        public static LazyIndex MegalomaniacIndex = new("MegalomaniacBody");
        public static GameObject MegaloStickyOrb;
        public static GameObject LunarCoreProjectile;
        public static GameObject QuickSunderWave;
        public static GameObject MegaloHeadProjectile;

        public override void CreateLang()
        {
            base.CreateLang();
        }

        public override void LoadAssets()
        {
            base.LoadAssets();

            Body = Main.assets.LoadAsset<GameObject>("MegalomaniacBody.prefab");

            Body.AddComponent<MegaloController>();

            Body.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpStandardMelee;
            var networkIdentity = Body.GetComponent<NetworkIdentity>();
            networkIdentity.localPlayerAuthority = true;
            networkIdentity.enabled = true;
            networkIdentity.serverOnly = false;

            Body.GetComponent<ModelLocator>()._modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Paths.GameObject.GenericFootstepDust;

            var cb = Body.GetComponent<CharacterBody>();
            // cb._defaultCrosshairPrefab = Paths.GameObject.StandardCrosshair;
            // cb.preferredPodPrefab = Paths.GameObject.RoboCratePod;
            SurvivorDef = Main.assets.LoadAsset<SurvivorDef>("sdMegalomaniac.asset");
            SurvivorDef.cachedName = "Megalomaniac"; // for eclipse fix;
            SurvivorDef.outroFlavorToken.Add("sigma.");
            SurvivorDef.mainEndingEscapeFailureFlavorToken.Add("sigma.");
            SurvivorDef.descriptionToken.Add(
                """
                el sigma
                """
            );
            var kcm = Body.GetComponent<KinematicCharacterController.KinematicCharacterMotor>();
            kcm.playerCharacter = true;

            PrefabAPI.RegisterNetworkPrefab(Body);
            Master = PrefabAPI.InstantiateClone(Paths.GameObject.EngiMonsterMaster, "MegalomaniacMaster");

            SkillLocator locator = Body.GetComponent<SkillLocator>();
            ReplaceSkills(locator.primary, new SkillDef[] { Skills.Shatter.instance });
            ReplaceSkills(locator.secondary, new SkillDef[] { Renegade.Skills.Charge.instance });
            ReplaceSkills(locator.utility, new SkillDef[] { Renegade.Skills.Charge.instance });
            ReplaceSkills(locator.special, new SkillDef[] { Skills.Expel.instance });
            locator.passiveSkill.icon = null;

            "SANDSWEPT_MEGALO_PASSIVE_NAME".Add("???");
            "SANDSWEPT_MEGALO_PASSIVE_DESC".Add("oh my gyatt");

            MegaloStickyOrb = Main.assets.LoadAsset<GameObject>("MegaloStickyProjectile.prefab");
            MegaloStickyOrb.GetComponent<ProjectileExplosion>().explosionEffect = Paths.GameObject.ExplosionLunarSun;

            LunarCoreProjectile = Main.assets.LoadAsset<GameObject>("LunarCoreProjectile.prefab");

            QuickSunderWave = PrefabAPI.InstantiateClone(Paths.GameObject.BrotherUltLineProjectileStatic, "QuickSunderWave");
            QuickSunderWave.FindComponent<HitBox>("Hitbox").transform.localScale = new(3, 201, 100);
            QuickSunderWave.RemoveComponent<StartEvent>();
            QuickSunderWave.AddComponent<StupidQSWComponent>();

            MegaloHeadProjectile = Main.assets.LoadAsset<GameObject>("MegaloHeadProjectile.prefab");
            MegaloHeadProjectile.GetComponent<ProjectileImpactExplosion>().explosionEffect = Paths.GameObject.ExplosionLunarSun;

            CreateVFX();

            ContentAddition.AddProjectile(MegaloStickyOrb);
            ContentAddition.AddProjectile(LunarCoreProjectile);
            ContentAddition.AddProjectile(QuickSunderWave);
        }

        public void CreateVFX()
        {
            GameObject stickyGhost = PrefabAPI.InstantiateClone(Paths.GameObject.LunarExploderShardGhost, "MegaloStickyGhost");
            MegaloStickyOrb.GetComponent<ProjectileController>().ghostPrefab = stickyGhost;

            GameObject coreGhost = PrefabAPI.InstantiateClone(Paths.GameObject.LunarMissileGhost, "LunarCoreGhost");
            coreGhost.FindComponent<ParticleSystemRenderer>("Flare").transform.localScale = new(0.5f, 0.5f, 0.5f);
            LunarCoreProjectile.GetComponent<ProjectileController>().ghostPrefab = coreGhost;
        }

        public class StupidQSWComponent : MonoBehaviour
        {
            public void Start()
            {
                GetComponent<DelayedEvent>().Call();
            }
        }
    }
}
