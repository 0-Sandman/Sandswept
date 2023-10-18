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

            Body = Main.Assets.LoadAsset<GameObject>("RangerBody.prefab");
            Master = PrefabAPI.InstantiateClone(Assets.GameObject.CommandoMonsterMaster, "RangerMaster");

            Body.GetComponent<CameraTargetParams>().cameraParams = Assets.CharacterCameraParams.ccpStandard;

            SurvivorDef = Main.Assets.LoadAsset<SurvivorDef>("sdRanger.asset");

            SkillLocator locator = Body.GetComponent<SkillLocator>();
            ReplaceSkills(locator.primary, new SkillDef[] { Skills.Ranger.GunGoShoot.instance.skillDef });
            ReplaceSkills(locator.secondary, new SkillDef[] { Skills.Ranger.Release.instance.skillDef });
            ReplaceSkills(locator.utility, new SkillDef[] { Skills.Ranger.Sidestep.instance.skillDef });
            ReplaceSkills(locator.special, new SkillDef[] { Skills.Ranger.OverdriveEnter.instance.skillDef });

            "SS_RANGER_PASSIVE_NAME".Add("Power Surge");
            "SS_RANGER_PASSIVE_DESC".Add("Gain $sd2.5% attack speed$se for each $suCharge$se currently held. You can have up to 10 $suCharge$se.".AutoFormat());
        }
    }
}