using KinematicCharacterController;
using RoR2.UI;
using Sandswept.Survivors.Ranger.Achievements;

// using Sandswept.Survivors.Ranger.ItemDisplays;
using Sandswept.Survivors.Ranger.Pod;
using Sandswept.Survivors.Ranger.SkillDefs.Passive;
using Sandswept.Survivors.Ranger.States.Primary;
using Sandswept.Survivors.Ranger.States.Secondary;
using Sandswept.Survivors.Ranger.States.Special;
using Sandswept.Survivors.Ranger.States.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace Sandswept.Survivors.Ranger
{
    [ConfigSection("Survivors :: Ranger")]
    public class Ranger : SurvivorBase<Ranger>
    {
        public override string Name => "Ranger";

        public override string Description =>
        """
        The Ranger is a versatile combatant, who alternates between long-range electric barrages and close quarters incendiary fire to efficiently take down targets.

        < ! > Direct Current excels at dealing long range group damage, whilst Enflame can melt away large singular threats.

        < ! > Release can be used to retreat or snipe from afar, while Exhaust deals high close range damage to continue the assault.

        < ! > Sidestep and Heat Signature are valuable tools in close quarters, allowing you to weave between targets for a better position.

        < ! > Entering Overdrive offers immense single target capabilities, at the expense of your health. Heat Sink will deal heavy area of effect damage when you need to back down.
        """;

        public override string Subtitle => "Infernal Marshal";

        public override string Outro => "...and so she left, with more questions than answers.";

        public override string Failure => "...and so she vanished, consumed by doubt.";

        public override void LoadAssets()
        {
            base.LoadAssets();

            Body = Main.assets.LoadAsset<GameObject>("RangerBody.prefab");
            var characterBody = Body.GetComponent<CharacterBody>();
            characterBody.portraitIcon = Main.hifuSandswept.LoadAsset<Texture2D>("texRangerIcon.png");
            characterBody.bodyColor = new Color32(54, 215, 169, 255);

            var networkIdentity = Body.GetComponent<NetworkIdentity>();
            networkIdentity.localPlayerAuthority = true;
            networkIdentity.enabled = true;
            networkIdentity.serverOnly = false;

            PrefabAPI.RegisterNetworkPrefab(Body);

            Master = PrefabAPI.InstantiateClone(Paths.GameObject.CommandoMonsterMaster, "RangerMaster");

            Body.AddComponent<RoR2.UI.CrosshairUtils.CrosshairOverrideBehavior>();

            Body.RemoveComponent<RangerHeatController>();
            Body.AddComponent<RangerHeatController>();

            Body.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpStandard;

            ContentAddition.AddMaster(Main.assets.LoadAsset<GameObject>("RangerMonsterMaster.prefab"));

            var crosshair = Main.assets.LoadAsset<GameObject>("Assets/Sandswept/Base/Characters/Ranger/CrosshairRanger.prefab");

            /*var innerSight = crosshair.transform.GetChild(1);
            var rectTransform = innerSight.GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.one * 0.43f;
            var rawImage = innerSight.GetComponent<RawImage>();
            rawImage.texture = Main.hifuSandswept.LoadAsset<Texture2D>("texProjectileCrosshair6.png");

            var outerCircle = crosshair.transform.GetChild(0);
            outerCircle.gameObject.SetActive(true);
            var rawImage2 = outerCircle.GetComponent<RawImage>();
            rawImage2.texture = Paths.Texture2D.texCrosshairDot;
            rawImage2.color = new Color32(200, 200, 200, 255);
            outerCircle.GetComponent<RectTransform>().localScale = Vector3.one * 1.5f;

            var crosshairController = crosshair.GetComponent<CrosshairController>().spriteSpreadPositions[0];
            crosshairController.zeroPosition = new Vector3(0f, -70f, 0f);
            crosshairController.onePosition = new Vector3(0f, -250f, 0f);*/

            var cb = Body.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = crosshair;
            cb.preferredPodPrefab = RangerPod.prefabDefault;

            cb.GetComponent<KinematicCharacterMotor>().playerCharacter = true;

            SurvivorDef = Main.assets.LoadAsset<SurvivorDef>("sdRanger.asset");
            SurvivorDef.cachedName = "Ranger"; // for eclipse fix

            _modelTransform = Body.GetComponent<ModelLocator>()._modelTransform;

            /*var modelBase = Body.transform.GetChild(0);

            modelBase.AddComponent<FUCKINGKILLYOURSELF>();

            _modelTransform.Find("HurtBox").localPosition = new(0, 0.01f, 0);*/

            Body.GetComponent<ModelLocator>()._modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Paths.GameObject.GenericFootstepDust;

            SkillLocator locator = Body.GetComponent<SkillLocator>();
            locator.passiveSkill = default; // PassiveFix.Init();
            var passive = Body.AddComponent<GenericSkill>(); passive.skillName = "Passive";
            SkillsAPI.SetLoadoutTitleTokenOverride(passive, "Passive");
            SkillsAPI.SetOrderPriority(passive, -1);
            ReplaceSkills(passive, new SkillDef[] { SkillDefs.Passive.OverchargedProtection.instance.skillDef, SkillDefs.Passive.OverchargedSpeed.instance.skillDef });
            ReplaceSkills(locator.primary, new SkillDef[] { SkillDefs.Primary.DirectCurrent.instance.skillDef });
            ReplaceSkills(locator.secondary, new SkillDef[] { SkillDefs.Secondary.Release.instance.skillDef, SkillDefs.Secondary.Galvanize.instance.skillDef });
            ReplaceSkills(locator.utility, new SkillDef[] { SkillDefs.Utility.Sidestep.instance.skillDef });
            ReplaceSkills(locator.special, new SkillDef[] { SkillDefs.Special.OverdriveEnter.instance.skillDef });

            "SS_RANGER_BODY_LORE".Add(
            """
            After the Purge, the Hall of the Revered invested quite a lot into its own defense. For most groups, that meant shield generators, missile systems, or armies -- but the Hall's measures were more...singular.

            As they had told their chosen defense when training began, the Hall is not a military organization. All they wanted was a looming threat, a force unstoppable but not flaunted or oft-used -- and unstoppable she soon became. The miracles and relics held in the trust of the Hall for eons contained strength unimaginable by the known superpowers of the galaxy. "But do not abuse them," she had been told. "Our restraint and dedication to peace are the reason we are entrusted with such power, and the Hall can never betray that trust."

            Although she always had firm and eternal trust in the Hall and its members, the shadowed part of her brain raised doubts about this order, echoing those words against the walls of her skull. Rather than a defense, a response to something, this felt like an attack. Sneaking aboard a UES ship was questionable to begin with; now that she was at its destination, slaughtering the denizens of the alien planet by the dozens, the doubtful questions came in floods. Out of each fatal arc of electricity and cruel burst of flame, more hesitation seeped, lodging into her mind. Had the Hall fallen? Was she receiving instructions from some malicious outsider? If not, was one ancient sword, however powerful, really worth all this destruction?

            Yet, she refused to falter. She could not. The torrent of worry threatened only an inky darkness should she accept its embrace, a storm of great hail and thunder, unnavigable in this cruel place so far from humanity. The Hall's principles were her only guiding light here, even if that light might burn her. She would not wander into the tempest's depths, not like the one who came before her.

            "Take this as a lesson." The words were hammered into her memory -- indeed, nearly everything from the moment was preserved in agonizing clarity, even the parts of her vision which had been blurred by tears not seen since that day. "The greatest strength of all is strength of will, and of devotion. The Hall has paved a path for you, and though thorns may sometimes cross it, they are nothing compared to the thicket of brambles that surround it."

            The Hall had given her this directive, and as its only defense, she was soul-bound to follow it.
            """);
            cb.baseNameToken.Replace("_NAME", "_SUBTITLE").Add("Wrath of the Scholars");
            mdl = _modelTransform.GetComponent<CharacterModel>();

            var rig = _modelTransform.Find("Ranger Rig");
            var chest = rig.Find("Root/Base/Stomach/Chest");
            var neck = chest.Find("Neck");
            var head = neck.Find("Head");

            // foot master!? what

            var childLocator = _modelTransform.GetComponent<ChildLocator>();
            List<ChildLocator.NameTransformPair> sigma = childLocator.transformPairs.ToList();
            sigma.Add(new()
            {
                name = "Chest",
                transform = chest
            });
            sigma.Add(new()
            {
                name = "Neck",
                transform = neck
            });
            sigma.Add(new()
            {
                name = "Head",
                transform = head
            });
            childLocator.transformPairs = sigma.ToArray();

            Achievements.UnlockableDefs.Init();

            AddSkins();

            RegisterStuff();

            // SurvivorDef.displayPrefab = _modelTransform.gameObject;

            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;

            // not sure if hgstandard has hdr emission color, but it would make the green texture pop, while still having that glow instead of being a white lightbulb with green glow
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody body)
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
            hitBox.transform.localPosition = Vector3.zero;
            hitBox.transform.localScale = Vector3.one * 7f;
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
        public static SkinDef racecarDef;
        public static SkinDef sandsweptDef;
        public static SkinDef masteryDef;
        // public static SkinDef rainbowDef;

        public void AddSkins()
        {
            defaultDef = Main.assets.LoadAsset<SkinDef>("Skindefault.asset");
            defaultDef.nameToken = "RANGER_SKIN_DEFAULT_NAME";
            masteryDef = Main.assets.LoadAsset<SkinDef>("sdRangerMastery.asset");
            masteryDef.nameToken = "RANGER_SKIN_SANDSWEPT_NAME";

            var scarfAndPantsColor = new Color32(88, 161, 142, 255);
            var helmetColor = new Color32(0, 255, 169, 255);
            var armorColor = new Color32(223, 127, 35, 255);
            var suitColor = new Color32(49, 62, 67, 255);

            defaultDef.icon = Skins.CreateSkinIcon(scarfAndPantsColor, helmetColor, armorColor, suitColor);
            masteryDef.icon = Skins.CreateSkinIcon(
                new Color32(228, 146, 55, 255),
                new Color32(201, 178, 143, 255),
                new Color32(74, 79, 77, 255),
                new Color32(108, 68, 45, 255)
            );
            masteryDef.unlockableDef = UnlockableDefs.masteryUnlock;

            "RANGER_SKIN_DEFAULT_NAME".Add("Default");
            "RANGER_SKIN_DEFAULT_DESC".Add("This survivor's default skin.");

            modelSkinController = mdl.GetComponent<ModelSkinController>();

            modelSkinController.skins = new SkinDef[] {
                defaultDef,
                masteryDef
            };

            majorDef = CreateRecolor("Major", "A uniform reserved for high-ranking devotees, indicated by its blue color. Always remembered, but never worn.", 4.2f);
            renegadeDef = CreateRecolor("Renegade", "An alternative path of apostasy. Always imagined, but never worn.", 2.5f);
            mileZeroDef = CreateRecolor("Mile Zero", "His colors.", 4.2f);

            "RANGER_SKIN_SANDSWEPT_NAME".Add("Sandswept");
            "RANGER_SKIN_SANDSWEPT_DESC".Add("The garb from a fateful mission that can never be rectified. Always kept, but never worn.");

            On.RoR2.UI.SurvivorIconController.Rebuild += SurvivorIconController_Rebuild;
        }

        private void SurvivorIconController_Rebuild(On.RoR2.UI.SurvivorIconController.orig_Rebuild orig, SurvivorIconController self)
        {
            orig(self);

            if (self.hgButton && self.survivorDef == Ranger.instance.SurvivorDef)
            {
                // Main.ModLogger.LogError("found ranger survivor icon and button exists");

                var survivorGrid = self.transform.parent;
                var survivorChoiceGridContainer = survivorGrid.parent;
                var survivorChoiceGridPanel = survivorChoiceGridContainer.parent;
                var leftHandPanel = survivorChoiceGridPanel.parent;
                var safeArea = leftHandPanel.parent;
                var anchor = safeArea;
                if (anchor.GetComponent<Image>() == null)
                {
                    self.hgButton.onClick.AddListener(() => OnClick(self.transform.root.GetComponent<Canvas>(), self));
                }
            }
        }

        private int clickCount = 0;

        private void OnClick(Canvas canvas, SurvivorIconController icon)
        {
            if (icon.survivorDef != Ranger.instance.SurvivorDef)
            {
                return;
            }

            clickCount++;
            if (clickCount >= 100 || (Main.cursedConfig.Value && clickCount >= 2))
            {
                for (int i = 0; i < 5; i++)
                {
                    GameObject.Instantiate(Main.assets.LoadAsset<GameObject>("EggPrefab.prefab"), canvas.transform).GetComponent<EggController>().velocity = Random.insideUnitCircle.normalized * 50f;
                }

                clickCount = 0;
            }
        }

        public void RegisterStuff()
        {
            ContentAddition.AddBody(Body);
            ContentAddition.AddMaster(Master);
            ContentAddition.AddEntityState(typeof(DirectCurrent), out _);
            ContentAddition.AddEntityState(typeof(Enflame), out _);
            ContentAddition.AddEntityState(typeof(Exhaust), out _);
            ContentAddition.AddEntityState(typeof(Survivors.Ranger.States.Secondary.Char), out _);
            ContentAddition.AddEntityState(typeof(HeatSignature), out _);
            ContentAddition.AddEntityState(typeof(HeatSink), out _);
            ContentAddition.AddEntityState(typeof(OverdriveEnter), out _);
            // ContentAddition.AddEntityState(typeof(OverdriveExit), out _);
            ContentAddition.AddEntityState(typeof(Release), out _);
            ContentAddition.AddEntityState(typeof(Sidestep), out _);
            ContentAddition.AddEntityState(typeof(Galvanize), out _);
        }

        public SkinDef CreateRecolor(string skinName, string skinDescription, float emissionValue = 2.5f, UnlockableDef unlockableDef = null)
        {
            var trimmedName = skinName.Replace(" ", "");
            var mainTex = Main.hifuSandswept.LoadAsset<Texture2D>("texRangerDiffuse" + trimmedName + ".png");
            var emTex = Main.hifuSandswept.LoadAsset<Texture2D>("texRangerEmission" + trimmedName + ".png");

            var scarfAndPantsColor = mainTex.GetPixel(272, 265);
            var helmetColor = mainTex.GetPixel(444, 168);
            var armorColor = mainTex.GetPixel(41, 390);
            var suitColor = mainTex.GetPixel(453, 465);

            var newMat = new Material(Main.assets.LoadAsset<Material>("matRanger.mat"));

            newMat.SetTexture("_MainTex", mainTex);
            newMat.SetTexture("_EmTex", emTex);
            newMat.SetFloat("_EmPower", emissionValue);
            newMat.SetColor("_EmColor", Color.white);
            newMat.EnableKeyword("DITHER");
            newMat.name = "matRanger" + skinName;

            var trans = mdl.transform;
            var gun = trans.Find("Gun").GetComponent<SkinnedMeshRenderer>();
            var legs = trans.Find("Legs").GetComponent<SkinnedMeshRenderer>();
            var scarf = trans.Find("Scarf").GetComponent<SkinnedMeshRenderer>();

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

            var newSkinDefInfo = new SkinDefInfo()
            {
                Icon = icon,
                Name = trimmedName,
                NameToken = "RANGER_SKIN_" + trimmedName.ToUpper() + "_NAME",
                RendererInfos = newRendererInfos,
                RootObject = mdl.gameObject,
                UnlockableDef = unlockableDef,
                BaseSkins = [modelSkinController.skins[0]]
            };

            var skinDef = Skins.CreateNewSkinDef(newSkinDefInfo);

            ("RANGER_SKIN_" + trimmedName.ToUpper() + "_NAME").Add(skinName);
            ("RANGER_SKIN_" + trimmedName.ToUpper() + "_DESC").Add(skinDescription);

            // Main.ModLogger.LogError("token is " + "RANGER_SKIN_" + trimmedName.ToUpper() + "_NAME");

            Skins.AddSkinToCharacter(Body, skinDef);

            return skinDef;
        }
    }
}