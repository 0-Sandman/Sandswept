using System;
using Sandswept.Components;

namespace Sandswept.Survivors
{
    public class Ranger : SurvivorBase<Ranger>
    {
        public override string Name => "Ranger";

        public override string Description => "The Ranger is an all-rounder, and a resource managing survivor with two distinct forms.<style=cSub>\r\n\r\n< ! > Power Surge works during Overdrive, making it easier to manage Heat and counteract self-damage.\r\n\r\n< ! > Direct Current is a great damage tool that works very well at any range and is able to gain multiple stacks of Charge when hitting groups of enemies. Hit your shots and increase your survivability with the help of Power Surge.\r\n\r\n< ! > Release can easily obliterate multiple enemies, boasting high burst damage with no damage falloff and a small area of effect. Manage your Charge to deal extra damage or propel yourself and disengage at the cost of Charge.\r\n\r\n< ! > Sidestep provides temporary immunity, making it great for dodging highly telegraphed attacks.\r\n\r\n< ! > Enflame fires very fast, and deals great sustained damage, making it ideal for activating many item effects quickly and eliminating high priority targets.\r\n\r\n< ! > Heat Sink is a powerful, but situational burst skill that's best used when swarmed at high Heat. Watch out for backblast.\r\n\r\n< ! > Heat Signature is a great utility for escaping sticky situations, building up Heat and extreme offense.\r\n\r\n< ! > Cancel can be used to safely exit Overdrive without sacrificing health.</style>\r\n";

        public override string Subtitle => "Dual Dynamo";

        public override string Outro => "...and so she left ready to listen to Periphery for the 41,527th time...";

        public override string Failure => "...and so she didn't leave lmao skill issue";

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