using RoR2.UI;
using Sandswept.Survivors.Ranger.ItemDisplays;
using Sandswept.Survivors.Ranger.Pod;
using Sandswept.Survivors.Ranger.States;
using System.Linq;
using UnityEngine.UI;

namespace Sandswept.Survivors.Ranger
{
    public class Ranger : SurvivorBase<Ranger>
    {
        public override string Name => "Ranger";

        public override string Description => "The Ranger is a versatile character with two distinct forms. Her base form excels at burst damage from any range using electricity, while the other is much more resource managing focused with high risk, but extremely high damage fire attacks.<style=cSub>\r\n\r\n< ! > Power Surge works during Overdrive, making it easier to manage heat and counteract self-damage.\r\n\r\n< ! > Direct Current is a great damage tool that works very well at any range and is able to gain multiple stacks of Charge when hitting groups of enemies. Hit your shots!.\r\n\r\n< ! > Release can easily obliterate multiple enemies, boasting high burst damage with no damage falloff and a small area of effect. Manage your Charge to deal extra damage or propel yourself and disengage.\r\n\r\n< ! > Sidestep is a great evasive tool, letting you dance between enemies while lining them up for Direct Current and Release, and provides temporary immunity, making it great for dodging highly telegraphed attacks.\r\n\r\n< ! > The longer you are in Overdrive, the less healing you receive! At full heat, you take increasingly high self-damage, but gain increasingly high base damage! Make sure to spend your health wisely.\r\n\r\n< ! > Enflame fires very fast, and deals great sustained damage, making it ideal for activating many item effects quickly and eliminating high priority targets.\r\n\r\n< ! > Exhaust deals extreme burst damage, use it to finish off enemies at close range and build up heat.\r\n\r\n< ! > Heat Signature is a great utility for escaping sticky situations and extreme offense.\r\n\r\n< ! > Heat Sink is a powerful burst skill that's best used when swarmed at high heat.</style>\r\n";

        public override string Subtitle => "Infernal Marshal";

        public override string Outro => "...and so she left ready to listen to Periphery for the 43,945th time (not Periphery 3 tho it sucks it's barely replayable like she's only played it 4,874 times and got extremely fed up with it it's their most overrated album I swear)...";

        public override string Failure => "...and so she didn't leave lmao skill issue btw you should listen to these albums --- Unprocessed - In Concretion, Unprocessed - Perception, Unprocessed - Covenant, Unprocessed - And Everything In Between, Periphery - Periphery 1, Periphery - Periphery 2";

        public override void LoadAssets()
        {
            base.LoadAssets();

            Body = Main.Assets.LoadAsset<GameObject>("RangerBody.prefab");
            var characterBody = Body.GetComponent<CharacterBody>();
            characterBody.portraitIcon = Main.hifuSandswept.LoadAsset<Texture2D>("Assets/Sandswept/texRangerIcon.png");
            characterBody.bodyColor = new Color32(54, 215, 169, 255);

            var networkIdentity = Body.GetComponent<NetworkIdentity>();
            networkIdentity.localPlayerAuthority = true;
            networkIdentity.enabled = true;
            networkIdentity.serverOnly = false;

            PrefabAPI.RegisterNetworkPrefab(Body);

            Master = PrefabAPI.InstantiateClone(Assets.GameObject.CommandoMonsterMaster, "RangerMaster");

            Body.AddComponent<RoR2.UI.CrosshairUtils.CrosshairOverrideBehavior>();

            Body.RemoveComponent<RangerHeatController>();
            Body.AddComponent<RangerHeatController>();

            Body.GetComponent<CameraTargetParams>().cameraParams = Assets.CharacterCameraParams.ccpStandard;

            var crosshair = Main.Assets.LoadAsset<GameObject>("Assets/Sandswept/Base/Characters/Ranger/CrosshairRanger.prefab");

            var innerSight = crosshair.transform.GetChild(1);
            var rectTransform = innerSight.GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.one * 0.43f;
            var rawImage = innerSight.GetComponent<RawImage>();
            rawImage.texture = Main.hifuSandswept.LoadAsset<Texture2D>("Assets/Sandswept/texProjectileCrosshair6.png");

            var outerCircle = crosshair.transform.GetChild(0);
            outerCircle.gameObject.SetActive(true);
            var rawImage2 = outerCircle.GetComponent<RawImage>();
            rawImage2.texture = Assets.Texture2D.texCrosshairDot;
            rawImage2.color = new Color32(200, 200, 200, 255);
            outerCircle.GetComponent<RectTransform>().localScale = Vector3.one * 1.5f;

            var crosshairController = crosshair.GetComponent<CrosshairController>().spriteSpreadPositions[0];
            crosshairController.zeroPosition = new Vector3(0f, -70f, 0f);
            crosshairController.onePosition = new Vector3(0f, -250f, 0f);

            var cb = Body.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = crosshair;
            cb.preferredPodPrefab = RangerPod.prefabDefault;

            SurvivorDef = Main.Assets.LoadAsset<SurvivorDef>("sdRanger.asset");
            SurvivorDef.cachedName = "Ranger"; // for eclipse fix

            _modelTransform = Body.GetComponent<ModelLocator>()._modelTransform;

            var modelBase = Body.transform.GetChild(0);

            modelBase.AddComponent<FUCKINGKILLYOURSELF>();

            _modelTransform.Find("HurtBox").localPosition = new(0, 0.01f, 0);

            var footstepHandler = _modelTransform.AddComponent<FootstepHandler>();
            footstepHandler.enableFootstepDust = true;
            footstepHandler.baseFootstepString = "Play_bandit2_step";
            footstepHandler.sprintFootstepOverrideString = "Play_bandit2_step_sprint";
            footstepHandler.footstepDustPrefab = Assets.GameObject.GenericFootstepDust;

            SkillLocator locator = Body.GetComponent<SkillLocator>();
            ReplaceSkills(locator.primary, new SkillDef[] { Skilldefs.DirectCurrent.instance.skillDef });
            ReplaceSkills(locator.secondary, new SkillDef[] { Skilldefs.Release.instance.skillDef });
            ReplaceSkills(locator.utility, new SkillDef[] { Skilldefs.Sidestep.instance.skillDef });
            ReplaceSkills(locator.special, new SkillDef[] { Skilldefs.OverdriveEnter.instance.skillDef });

            "SS_RANGER_BODY_LORE".Add("jaw drops\r\neyes pop out of head\r\ntongue rolls out\r\nHUMINA HUMINA HUMINA!\r\nAWOOGA AWOOGA!\r\nEE-AW EE-AW!\r\nBOIOIOING!\r\npicks up jaw\r\nfixes eyes\r\nrolls up tongue\r\nburies face in ass\r\nBLBLBLBLBL LBLBLBLBLBLBLLB\r\nWHOA MAMA");

            "SS_RANGER_PASSIVE_NAME".Add("Power Surge");
            "SS_RANGER_PASSIVE_DESC".Add("Hold up to " + Projectiles.DirectCurrent.maxCharge + " $rcCharge$ec. Each $rcCharge$ec increases $shbase health regeneration$se by $sh0.25 hp/s$se. $rcCharge decays over time$ec.".AutoFormat());

            mdl = _modelTransform.GetComponent<CharacterModel>();

            var chest = _modelTransform.GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetChild(0);
            var neck = chest.GetChild(4);
            var head = neck.GetChild(0);

            var childLocator = _modelTransform.GetComponent<ChildLocator>();
            Array.Resize(ref childLocator.transformPairs, childLocator.transformPairs.Length + 3);
            childLocator.transformPairs[1].name = "Chest";
            childLocator.transformPairs[1].transform = chest;
            childLocator.transformPairs[2].name = "Neck";
            childLocator.transformPairs[2].transform = neck;
            childLocator.transformPairs[3].name = "Head";
            childLocator.transformPairs[3].transform = head;

            AddSkins();

            CharacterBody.onBodyStartGlobal += SetupHitBox;
            RegisterStuff();

            // not sure if hgstandard has hdr emission color, but it would make the green texture pop, while still having that glow instead of being a white lightbulb with green glow
        }

        [SystemInitializer(typeof(ItemCatalog))]
        public static void AddIDRS()
        {
            Funny.Populate();

            var rangerIDRS = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
            rangerIDRS.name = "idrsRangerBody";
            mdl.itemDisplayRuleSet.keyAssetRuleGroups = null;
            mdl.itemDisplayRuleSet = null;
            // remove previous fake not working idrs set in unity editor
            mdl.itemDisplayRuleSet = rangerIDRS;

            mdl.itemDisplayRuleSet.keyAssetRuleGroups = Funny.SetItemDisplayRules().ToArray();
        }

        private void SetupHitBox(CharacterBody body)
        {
            if (body.baseNameToken != "SS_RANGER_BODY_NAME")
            {
                return;
            }

            var modelLocator = body.modelLocator;
            if (!modelLocator)
            {
                return;
            }

            var trans = modelLocator.modelTransform;
            if (!trans)
            {
                return;
            }

            if (trans.Find("gay sex hitbox") != null)
            {
                return;
            }

            GameObject hitBox = new("gay sex hitbox");
            hitBox.transform.parent = trans;
            hitBox.AddComponent<HitBox>();
            hitBox.transform.localPosition = new Vector3(0f, 0.0075f, 0.02f);
            hitBox.transform.localScale = new Vector3(0.045f, 0.05f, 0.05f);
            hitBox.transform.localEulerAngles = Vector3.zero;
            var hitBoxGroup = trans.AddComponent<HitBoxGroup>();
            hitBoxGroup.hitBoxes = new HitBox[] { hitBox.GetComponent<HitBox>() };
            hitBoxGroup.groupName = "GaySex";
        }

        public static CharacterModel mdl;
        public static ModelSkinController modelSkinController;
        public static Transform _modelTransform;
        public static SkinDef defaultDef;
        public static SkinDef majorDef;
        public static SkinDef renegadeDef;
        public static SkinDef mileZeroDef;

        public void AddSkins()
        {
            defaultDef = Main.Assets.LoadAsset<SkinDef>("Skindefault.asset");

            var scarfAndPantsColor = new Color32(88, 161, 142, 255);
            var helmetColor = new Color32(0, 255, 169, 255);
            var armorColor = new Color32(223, 127, 35, 255);
            var suitColor = new Color32(49, 62, 67, 255);

            defaultDef.icon = Skins.CreateSkinIcon(scarfAndPantsColor, helmetColor, armorColor, suitColor);

            "SKIN_DEFAULT".Add("Default");

            modelSkinController = mdl.GetComponent<ModelSkinController>();

            majorDef = CreateRecolor("Major", 4.2f, false, "perform a multikill of 10 enemies");
            renegadeDef = CreateRecolor("Renegade", 2.5f, false, "kill 3 enemies with one use of Heat Signature");
            mileZeroDef = CreateRecolor("Mile Zero", 4.2f, false, "finish off 10 enemies with one use of Exhaust");
            // CreateRecolor("Uv");
            // ideas
            // Major - as Ranger, kill 10 enemies at once
            // Renegade - as Ranger, kill 3 enemies with one use of Heat Signature in one run
            // Mile Zero - as Ranger, finish off 10 enemies with Exhaust in one run
        }

        public void RegisterStuff()
        {
            ContentAddition.AddBody(Body);
            ContentAddition.AddMaster(Master);
            ContentAddition.AddEntityState(typeof(DirectCurrent), out _);
            ContentAddition.AddEntityState(typeof(Enflame), out _);
            ContentAddition.AddEntityState(typeof(Exhaust), out _);
            ContentAddition.AddEntityState(typeof(HeatSignature), out _);
            ContentAddition.AddEntityState(typeof(HeatSink), out _);
            ContentAddition.AddEntityState(typeof(OverdriveEnter), out _);
            // ContentAddition.AddEntityState(typeof(OverdriveExit), out _);
            ContentAddition.AddEntityState(typeof(OverdriveExitHeatSink), out _);
            ContentAddition.AddEntityState(typeof(Release), out _);
            ContentAddition.AddEntityState(typeof(Sidestep), out _);
        }

        public SkinDef CreateRecolor(string skinName, float emissionValue = 2.5f, bool unlockable = false, string unlockDesc = "ugh fill me")
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

            var icon = Skins.CreateSkinIcon(scarfAndPantsColor, helmetColor, armorColor, suitColor);

            UnlockableDef unlockableDef = null;
            if (unlockable)
            {
                unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
                unlockableDef.achievementIcon = icon;
                unlockableDef.hidden = false;
                unlockableDef.nameToken = "SKIN_" + trimmedName.ToUpper();
                unlockableDef._cachedName = "Skins.Ranger." + trimmedName;

                ("ACHIEVEMENT_" + unlockableDef.nameToken + "_NAME").Add("Ranger: " + skinName);
                ("ACHIEVEMENT_" + unlockableDef.nameToken + "_DESCRIPTION").Add("As Ranger, " + unlockDesc + ".");

                ContentAddition.AddUnlockableDef(unlockableDef);
            }

            var newSkinDefInfo = new SkinDefInfo()
            {
                Icon = icon,
                Name = trimmedName,
                NameToken = "SKINDEF_" + trimmedName.ToUpper(),
                RendererInfos = newRendererInfos,
                RootObject = mdl.gameObject,
                UnlockableDef = unlockableDef,
                BaseSkins = new SkinDef[] { modelSkinController.skins[0] }
            };

            var skinDef = Skins.CreateNewSkinDef(newSkinDefInfo);

            ("SKINDEF_" + trimmedName.ToUpper()).Add(skinName);

            Skins.AddSkinToCharacter(Body, skinDef);

            return skinDef;
        }
    }
}