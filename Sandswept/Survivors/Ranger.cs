using System;
using Sandswept.Components;

namespace Sandswept.Survivors
{
    public class Ranger : SurvivorBase<Ranger>
    {
        public override string Name => "Ranger";

        public override string Description => "The Ranger is a versatile character with two distinct forms. Her base form excels at burst damage from any range using electricity, while the other is much more resource managing focused with high risk, but extremely high damage fire attacks.<style=cSub>\r\n\r\n< ! > Power Surge works during Overdrive, making it easier to manage heat and counteract self-damage.\r\n\r\n< ! > Direct Current is a great damage tool that works very well at any range and is able to gain multiple stacks of Charge when hitting groups of enemies. Hit your shots!.\r\n\r\n< ! > Release can easily obliterate multiple enemies, boasting high burst damage with no damage falloff and a small area of effect. Manage your Charge to deal extra damage or propel yourself and disengage.\r\n\r\n< ! > Sidestep is a great evasive tool, letting you dance between enemies while lining them up for Direct Current and Release, and provides temporary immunity, making it great for dodging highly telegraphed attacks.\r\n\r\n< ! > Overdrive provides skills with tremendous damage output, but high heat will cause you to begin burning as well! Watch the heat meter carefully.\r\n\r\n< ! > Enflame fires very fast, and deals great sustained damage, making it ideal for activating many item effects quickly and eliminating high priority targets.\r\n\r\n< ! > Exhaust deals extreme burst damage, use it to finish off enemies at close range and build up heat.\r\n\r\n< ! > Heat Signature is a great utility for escaping sticky situations and extreme offense.\r\n\r\n< ! > Heat Sink is a powerful burst skill that's best used when swarmed at high heat.\r\n\r\n< ! > The longer you are in Overdrive, the less healing you receive! At full heat, you take increasingly high self-damage, but gain increasingly high base damage! Make sure to spend your health wisely.</style>\r\n";

        public override string Subtitle => "Infernal Marshal";

        public override string Outro => "...and so she left ready to listen to Periphery for the 41,527th time (not Periphery 3 tho it sucks it's barely replayable like she's only played it 5,000 times and got extremely fed up with it it's their most overrated album I swear)...";

        public override string Failure => "...and so she didn't leave lmao skill issue btw you should listen to these albums --- Unprocessed - In Concretion, Unprocessed - Perception, Unprocessed - Covenant, Periphery - Periphery 1, Periphery - Periphery 2";

        public override void LoadAssets()
        {
            base.LoadAssets();

            Body = Main.Assets.LoadAsset<GameObject>("RangerBody.prefab");
            var characterBody = Body.GetComponent<CharacterBody>();
            characterBody.portraitIcon = Main.hifuSandswept.LoadAsset<Texture2D>("Assets/Sandswept/texRangerIcon.png");
            characterBody.bodyColor = new Color32(54, 215, 169, 255);

            Master = PrefabAPI.InstantiateClone(Assets.GameObject.CommandoMonsterMaster, "RangerMaster");

            Body.AddComponent<RoR2.UI.CrosshairUtils.CrosshairOverrideBehavior>();

            Body.GetComponent<CameraTargetParams>().cameraParams = Assets.CharacterCameraParams.ccpStandard;
            var crosshair = Body.GetComponent<CharacterBody>()._defaultCrosshairPrefab;
            var innerSight = crosshair.transform.GetChild(1).GetComponent<RectTransform>();
            innerSight.localScale = Vector3.one * 0.5f;
            innerSight.localPosition = new Vector3(0f, -8f, 0f);

            SurvivorDef = Main.Assets.LoadAsset<SurvivorDef>("sdRanger.asset");
            SurvivorDef.cachedName = "Ranger"; // for eclipse fix

            var _modelTransform = Body.GetComponent<ModelLocator>()._modelTransform;

            _modelTransform.Find("HurtBox").localPosition = new(0, 0.01f, 0);

            var footstepHandler = _modelTransform.AddComponent<FootstepHandler>();
            footstepHandler.enableFootstepDust = true;

            SkillLocator locator = Body.GetComponent<SkillLocator>();
            ReplaceSkills(locator.primary, new SkillDef[] { Skills.Ranger.Skilldefs.DirectCurrent.instance.skillDef });
            ReplaceSkills(locator.secondary, new SkillDef[] { Skills.Ranger.Skilldefs.Release.instance.skillDef });
            ReplaceSkills(locator.utility, new SkillDef[] { Skills.Ranger.Skilldefs.Sidestep.instance.skillDef });
            ReplaceSkills(locator.special, new SkillDef[] { Skills.Ranger.Skilldefs.OverdriveEnter.instance.skillDef });

            Main.ModLogger.LogError(Body.GetComponent<CharacterBody>().baseNameToken);

            "SS_RANGER_BODY_LORE".Add("jaw drops\r\neyes pop out of head\r\ntongue rolls out\r\nHUMINA HUMINA HUMINA!\r\nAWOOGA AWOOGA!\r\nEE-AW EE-AW!\r\nBOIOIOING!\r\npicks up jaw\r\nfixes eyes\r\nrolls up tongue\r\nburies face in ass\r\nBLBLBLBLBL LBLBLBLBLBLBLLB\r\nWHOA MAMA");

            "SS_RANGER_PASSIVE_NAME".Add("Power Surge");
            "SS_RANGER_PASSIVE_DESC".Add("Hold up to 10 $rcCharge$ec. Each $rcCharge$ec increases $shbase health regeneration$se by $sh0.2 hp/s$se.".AutoFormat());

            "SKIN_DEFAULT".Add("Default");

            SkinDef sd = Main.Assets.LoadAsset<SkinDef>("Skindefault.asset");

            var scarfAndPantsColor = new Color32(88, 161, 142, 255);
            var helmetColor = new Color32(0, 255, 169, 255);
            var armorColor = new Color32(223, 127, 35, 255);
            var suitColor = new Color32(49, 62, 67, 255);

            sd.icon = Skins.CreateSkinIcon(scarfAndPantsColor, helmetColor, armorColor, suitColor);

            // not sure if hgstandard has hdr emission color, but it would make the green texture pop, while still having that glow instead of being a white lightbulb with green glow
        }
    }
}