using System;
using Sandswept.Components;
using Sandswept.Skills.Ranger.Projectiles;
using Sandswept.States.Ranger;
using UnityEngine.Experimental.U2D;

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
            footstepHandler.baseFootstepString = "Play_bandit2_step";
            footstepHandler.sprintFootstepOverrideString = "Play_bandit2_step_sprint";
            footstepHandler.footstepDustPrefab = Assets.GameObject.GenericFootstepDust;

            SkillLocator locator = Body.GetComponent<SkillLocator>();
            ReplaceSkills(locator.primary, new SkillDef[] { Skills.Ranger.Skilldefs.DirectCurrent.instance.skillDef });
            ReplaceSkills(locator.secondary, new SkillDef[] { Skills.Ranger.Skilldefs.Release.instance.skillDef });
            ReplaceSkills(locator.utility, new SkillDef[] { Skills.Ranger.Skilldefs.Sidestep.instance.skillDef });
            ReplaceSkills(locator.special, new SkillDef[] { Skills.Ranger.Skilldefs.OverdriveEnter.instance.skillDef });

            "SS_RANGER_BODY_LORE".Add("jaw drops\r\neyes pop out of head\r\ntongue rolls out\r\nHUMINA HUMINA HUMINA!\r\nAWOOGA AWOOGA!\r\nEE-AW EE-AW!\r\nBOIOIOING!\r\npicks up jaw\r\nfixes eyes\r\nrolls up tongue\r\nburies face in ass\r\nBLBLBLBLBL LBLBLBLBLBLBLLB\r\nWHOA MAMA");

            "SS_RANGER_PASSIVE_NAME".Add("Power Surge");
            "SS_RANGER_PASSIVE_DESC".Add("Hold up to " + DirectCurrent.maxCharge + " $rcCharge$ec. Each $rcCharge$ec increases $shbase health regeneration$se by $sh0.12 hp/s$se.".AutoFormat());

            "SKIN_DEFAULT".Add("Default");

            mdl = _modelTransform.GetComponent<CharacterModel>();

            CreateRecolor("Major", 4.2f);
            CreateRecolor("Renegade");
            CreateRecolor("Mile Zero", 4.2f);
            // CreateRecolor("Uv");

            SkinDef sd = Main.Assets.LoadAsset<SkinDef>("Skindefault.asset");

            var scarfAndPantsColor = new Color32(88, 161, 142, 255);
            var helmetColor = new Color32(0, 255, 169, 255);
            var armorColor = new Color32(223, 127, 35, 255);
            var suitColor = new Color32(49, 62, 67, 255);

            sd.icon = Skins.CreateSkinIcon(scarfAndPantsColor, helmetColor, armorColor, suitColor);

            ContentAddition.AddBody(Body);
            ContentAddition.AddMaster(Master);
            ContentAddition.AddEntityState(typeof(DirectCurrentNew), out _);
            ContentAddition.AddEntityState(typeof(Enflame), out _);
            ContentAddition.AddEntityState(typeof(Exhaust), out _);
            ContentAddition.AddEntityState(typeof(HeatSignatureNew), out _);
            ContentAddition.AddEntityState(typeof(HeatSink), out _);
            ContentAddition.AddEntityState(typeof(OverdriveEnter), out _);
            ContentAddition.AddEntityState(typeof(OverdriveExit), out _);
            ContentAddition.AddEntityState(typeof(OverdriveExitHeatSink), out _);
            ContentAddition.AddEntityState(typeof(Release), out _);
            ContentAddition.AddEntityState(typeof(Sidestep), out _);

            // not sure if hgstandard has hdr emission color, but it would make the green texture pop, while still having that glow instead of being a white lightbulb with green glow
        }

        public static CharacterModel mdl;

        public void CreateRecolor(string skinName, float emissionValue = 2.5f)
        {
            var trimmedName = skinName.Replace(" ", "");
            var mainTex = Main.hifuSandswept.LoadAsset<Texture2D>("Assets/Sandswept/texRangerDiffuse" + trimmedName + ".png");
            var emTex = Main.hifuSandswept.LoadAsset<Texture2D>("Assets/Sandswept/texRangerEmission" + trimmedName + ".png");

            var scarfAndPantsColor = mainTex.GetPixel(272, 265);
            var helmetColor = mainTex.GetPixel(444, 168);
            var armorColor = mainTex.GetPixel(41, 390);
            var suitColor = mainTex.GetPixel(453, 465);

            var newMat = new Material(Main.Assets.LoadAsset<Material>("matRanger.mat"));

            newMat.SetTexture("_MainTex", mainTex);
            newMat.SetTexture("_EmTex", emTex);
            newMat.SetFloat("_EmPower", emissionValue);
            newMat.name = "matRanger" + skinName;

            var trans = mdl.transform;
            var gun = trans.GetChild(1).GetComponent<SkinnedMeshRenderer>();
            var legs = trans.GetChild(2).GetComponent<SkinnedMeshRenderer>();
            var scarf = trans.GetChild(4).GetComponent<SkinnedMeshRenderer>();

            var gunRendererInfo = new CharacterModel.RendererInfo()
            {
                defaultMaterial = newMat,
                renderer = gun,
                defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                ignoreOverlays = false,
                hideOnDeath = false
            };

            var legsRendererInfo = new CharacterModel.RendererInfo()
            {
                defaultMaterial = newMat,
                renderer = legs,
                defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                ignoreOverlays = false,
                hideOnDeath = false
            };

            var scarfRendererInfo = new CharacterModel.RendererInfo()
            {
                defaultMaterial = newMat,
                renderer = scarf,
                defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                ignoreOverlays = false,
                hideOnDeath = false
            };

            var newRendererInfos = new CharacterModel.RendererInfo[] { gunRendererInfo, legsRendererInfo, scarfRendererInfo };

            var newSkinDefInfo = new SkinDefInfo()
            {
                Icon = Skins.CreateSkinIcon(scarfAndPantsColor, helmetColor, armorColor, suitColor),
                Name = trimmedName,
                NameToken = "SKIN_" + trimmedName.ToUpper(),
                RendererInfos = newRendererInfos,
                RootObject = mdl.gameObject
            };

            ("SKIN_" + trimmedName.ToUpper()).Add(skinName);

            Skins.AddSkinToCharacter(Body, newSkinDefInfo);
        }
    }
}