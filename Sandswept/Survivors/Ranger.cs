using System;

namespace Sandswept2.Survivors
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

            Body = Main.Assets.LoadAsset<GameObject>("RangerBody.prefab");
            var characterBody = Body.GetComponent<CharacterBody>();
            characterBody.portraitIcon = Main.hifuSandswept.LoadAsset<Texture2D>("Assets/Sandswept/texRangerIcon.png");
            characterBody.bodyColor = new Color32(54, 215, 169, 255);

            Master = PrefabAPI.InstantiateClone(Assets.GameObject.CommandoMonsterMaster, "RangerMaster");

            Body.GetComponent<CameraTargetParams>().cameraParams = Assets.CharacterCameraParams.ccpStandard;
            Body.GetComponent<CharacterBody>()._defaultCrosshairPrefab = Assets.GameObject.StandardCrosshair;

            SurvivorDef = Main.Assets.LoadAsset<SurvivorDef>("sdRanger.asset");
            SurvivorDef.cachedName = "Ranger"; // for eclipse fix

            SkillLocator locator = Body.GetComponent<SkillLocator>();
            ReplaceSkills(locator.primary, new SkillDef[] { Skills.Ranger.Skilldefs.DirectCurrent.instance.skillDef });
            ReplaceSkills(locator.secondary, new SkillDef[] { Skills.Ranger.Skilldefs.Release.instance.skillDef });
            ReplaceSkills(locator.utility, new SkillDef[] { Skills.Ranger.Skilldefs.Sidestep.instance.skillDef });
            ReplaceSkills(locator.special, new SkillDef[] { Skills.Ranger.Skilldefs.OverdriveEnter.instance.skillDef });

            "SS_RANGER_PASSIVE_NAME".Add("Power Surge");
            "SS_RANGER_PASSIVE_DESC".Add("Increase $shhealth regeneration$se by $sh0.2hp/s$se for each $rcCharge$ec currently held. You can have up to 10 $rcCharge$ec.".AutoFormat());
        }
    }
}