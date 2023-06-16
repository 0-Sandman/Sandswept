using System;

namespace Sandswept.Survivors
{
    public class Ranger : SurvivorBase<Ranger>
    {
        public override string Name => "Ranger";

        public override string Description => "some description";

        public override string Subtitle => "some subtitel";

        public override string Outro => "some outro";

        public override string Failure => "some failure";

        public override void LoadAssets()
        {
            base.LoadAssets();

            Body = PrefabAPI.InstantiateClone(Utils.Assets.GameObject.CommandoBody, "RangerBody");
            Master = PrefabAPI.InstantiateClone(Utils.Assets.GameObject.CommandoMonsterMaster, "RangerMaster");

            GameObject DisplayPrefab = PrefabAPI.InstantiateClone(Utils.Assets.GameObject.CommandoDisplay, "RangerDisplay", false);

            ModelSkinController controller = DisplayPrefab.GetComponentInChildren<ModelSkinController>();
            if (controller)
            {
                GameObject.Destroy(controller);
            }

            controller = Body.GetComponentInChildren<ModelSkinController>();
            if (controller)
            {
                GameObject.Destroy(controller);
            }

            CharacterBody body = Body.GetComponent<CharacterBody>();
            body.baseNameToken = "SANDSWEPT_SURVIVOR_RANGER_NAME";
            body.subtitleNameToken = "SANDSWEPT_SURVIVOR_RANGER_SUBTITLE";
            body.portraitIcon = null;
            body.bodyColor = Color.cyan;

            CharacterMaster master = Master.GetComponent<CharacterMaster>();
            master.bodyPrefab = Body;

            SurvivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
            (SurvivorDef as ScriptableObject).name = "sdRanger";
            SurvivorDef.bodyPrefab = Body;
            SurvivorDef.descriptionToken = "SANDSWEPT_SURVIVOR_RANGER_DESCRIPTION";
            SurvivorDef.outroFlavorToken = "SANDSWEPT_SURVIVOR_RANGER_OUTRO";
            SurvivorDef.mainEndingEscapeFailureFlavorToken = "SANDSWEPT_SURVIVOR_RANGER_FAIL";
            SurvivorDef.displayPrefab = DisplayPrefab;
            SurvivorDef.displayNameToken = body.baseNameToken;
            SurvivorDef.desiredSortPosition = 20;

            SwapMaterials(Body, Utils.Assets.Material.matVoidBubble, true);
            SwapMaterials(DisplayPrefab, Utils.Assets.Material.matVoidBubble, true);

            SerializableEntityStateType idle = new(typeof(Idle));

            AddESM(Body, "Overdrive", idle);
            AddESM(Body, "Sidestep", idle);

            SkillLocator locator = Body.GetComponent<SkillLocator>();
            Debug.Log(Skills.Ranger.OverdriveEnter.instance == null);
            ReplaceSkills(locator.primary, new SkillDef[] { Skills.Ranger.PewPew.instance.skillDef });
            ReplaceSkills(locator.secondary, new SkillDef[] { Skills.Ranger.Blast.instance.skillDef });
            ReplaceSkills(locator.utility, new SkillDef[] { Skills.Ranger.Sidestep.instance.skillDef });
            ReplaceSkills(locator.special, new SkillDef[] { Skills.Ranger.OverdriveEnter.instance.skillDef });
        }
    }
}