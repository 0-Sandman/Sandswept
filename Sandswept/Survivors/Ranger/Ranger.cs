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
    public class Ranger : SurvivorBase<Ranger>
    {
        public override string Name => "Ranger";

        public override string Description => "The Ranger is a versatile character with two distinct forms. Her base form excels at burst damage from any range using electricity, while the other is much more resource managing focused with high risk, but extremely high damage fire attacks.<style=cSub>\r\n\r\n< ! > Power Surge works during Overdrive, making it easier to manage heat and counteract self-damage.\r\n\r\n< ! > Direct Current is a great damage tool that works very well at any range and is able to gain multiple stacks of Charge when hitting groups of enemies. Hit your shots!.\r\n\r\n< ! > Release can easily obliterate multiple enemies, boasting high burst damage with no damage falloff and a small area of effect. Manage your Charge to deal extra damage or propel yourself and disengage.\r\n\r\n< ! > Sidestep is a great evasive tool, letting you dance between enemies while lining them up for Direct Current and Release, and provides temporary immunity, making it great for dodging highly telegraphed attacks.\r\n\r\n< ! > The longer you are in Overdrive, the less healing you receive! At full heat, you take increasingly high self-damage, but gain increasingly high base damage! Make sure to spend your health wisely.\r\n\r\n< ! > Enflame fires very fast, and deals great sustained damage, making it ideal for activating many item effects quickly and eliminating high priority targets.\r\n\r\n< ! > Exhaust deals extreme burst damage, use it to finish off enemies at close range and build up heat.\r\n\r\n< ! > Heat Signature is a great utility for escaping sticky situations and extreme offense.\r\n\r\n< ! > Heat Sink is a powerful burst skill that's best used when swarmed at high heat.</style>\r\n";

        public override string Subtitle => "Infernal Marshal";

        public override string Outro => "...and so she left, with more questions than answers.";

        public override string Failure => "...and so she vanished, consumed by doubt.";

        public override void LoadAssets()
        {
            base.LoadAssets();

            Body = Main.Assets.LoadAsset<GameObject>("RangerBody.prefab");
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

            var crosshair = Main.Assets.LoadAsset<GameObject>("Assets/Sandswept/Base/Characters/Ranger/CrosshairRanger.prefab");

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

            SurvivorDef = Main.Assets.LoadAsset<SurvivorDef>("sdRanger.asset");
            SurvivorDef.cachedName = "Ranger"; // for eclipse fix

            _modelTransform = Body.GetComponent<ModelLocator>()._modelTransform;

            /*var modelBase = Body.transform.GetChild(0);

            modelBase.AddComponent<FUCKINGKILLYOURSELF>();

            _modelTransform.Find("HurtBox").localPosition = new(0, 0.01f, 0);*/

            Body.GetComponent<ModelLocator>()._modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Paths.GameObject.GenericFootstepDust;

            SkillLocator locator = Body.GetComponent<SkillLocator>();
            locator.passiveSkill = default; PassiveFix.Init();
            var passive = Body.AddComponent<GenericSkill>(); passive.skillName = "Passive";
            ReplaceSkills(passive, new SkillDef[] { SkillDefs.Passive.OverchargedProtection.instance.skillDef, SkillDefs.Passive.OverchargedSpeed.instance.skillDef });
            ReplaceSkills(locator.primary, new SkillDef[] { SkillDefs.Primary.DirectCurrent.instance.skillDef });
            ReplaceSkills(locator.secondary, new SkillDef[] { SkillDefs.Secondary.Release.instance.skillDef, SkillDefs.Secondary.Galvanize.instance.skillDef });
            ReplaceSkills(locator.utility, new SkillDef[] { SkillDefs.Utility.Sidestep.instance.skillDef });
            ReplaceSkills(locator.special, new SkillDef[] { SkillDefs.Special.OverdriveEnter.instance.skillDef });

            "SS_RANGER_BODY_LORE".Add("After the Purge, the Hall of the Revered invested quite a lot into its own defense. For most groups, that meant shield generators, missile systems, or armies � but the Hall's measures were more...singular.\r\n\r\nAs they had told their chosen defense when training began, the Hall is not a military organization. All they wanted was a looming threat, a force unstoppable but not flaunted or oft-used � and unstoppable she soon became. The miracles and relics held in the trust of the Hall for eons contained strength unimaginable by the known superpowers of the galaxy. \"But do not abuse them,\" she had been told. \"Our restraint and dedication to peace are the reason we are entrusted with such power, and the Hall can never betray that trust.\"\r\n\r\nAlthough she always had firm and eternal trust in the Hall and its members, the shadowed part of her brain raised doubts about this order, echoing those words against the walls of her skull. Rather than a defense, a response to something, this felt like an attack. Sneaking aboard a UES ship was questionable to begin with; now that she was at its destination, slaughtering the denizens of the alien planet by the dozens, the doubtful questions came in floods. Out of each fatal arc of electricity and cruel burst of flame, more hesitation seeped, lodging into her mind. Had the Hall fallen? Was she receiving instructions from some malicious outsider? If not, was one ancient sword, however powerful, really worth all this destruction?\r\n\r\nYet, she refused to falter. She could not. The torrent of worry threatened only an inky darkness should she accept its embrace, a storm of great hail and thunder, unnavigable in this cruel place so far from humanity. The Hall's principles were her only guiding light here, even if that light might burn her. She would not wander into the tempest's depths, not like the one who came before her.\r\n\r\n\"Take this as a lesson.\" The words were hammered into her memory � indeed, nearly everything from the moment was preserved in agonizing clarity, even the parts of her vision which had been blurred by tears not seen since that day. \"The greatest strength of all is strength of will, and of devotion. The Hall has paved a path for you, and though thorns may sometimes cross it, they are nothing compared to the thicket of brambles that surround it.\"\r\n\r\nThe Hall had given her this directive, and as its only defense, she was soul-bound to follow it.");

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
            defaultDef = Main.Assets.LoadAsset<SkinDef>("Skindefault.asset");

            var scarfAndPantsColor = new Color32(88, 161, 142, 255);
            var helmetColor = new Color32(0, 255, 169, 255);
            var armorColor = new Color32(223, 127, 35, 255);
            var suitColor = new Color32(49, 62, 67, 255);

            defaultDef.icon = Skins.CreateSkinIcon(scarfAndPantsColor, helmetColor, armorColor, suitColor);

            "SKIN_DEFAULT".Add("Default");

            modelSkinController = mdl.GetComponent<ModelSkinController>();

            majorDef = CreateRecolor("Major", 4.2f);
            renegadeDef = CreateRecolor("Renegade", 2.5f);
            mileZeroDef = CreateRecolor("Mile Zero", 4.2f);

            Material masteryMat = Main.dgoslingAssets.LoadAsset<Material>("matRangerMastery");
            GameObject masterObject = Main.dgoslingAssets.LoadAsset<GameObject>("RangerMastery-fixArm");

            SkinDefInfo skinInfo = new()
            {
                NameToken = "SKINDEF_SANDSWEPT",
                Icon = Main.dgoslingAssets.LoadAsset<Sprite>("BeeNormalIcon"),
                RootObject = mdl.gameObject,
                UnlockableDef = UnlockableDefs.masteryUnlock,
                Name = "RangerMastery"
            };

            "SKINDEF_SANDSWEPT".Add("Sandswept");

            skinInfo.RendererInfos = new CharacterModel.RendererInfo[]
            {
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = masteryMat,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false,
                    renderer = mdl.transform.Find("Legs").GetComponent<SkinnedMeshRenderer>(),
                    hideOnDeath = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = masteryMat,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false,
                    renderer = mdl.transform.Find("Scarf").GetComponent<SkinnedMeshRenderer>(),
                }
            };

            skinInfo.BaseSkins = new SkinDef[]
            {
                defaultDef
            };

            skinInfo.MeshReplacements = new SkinDef.MeshReplacement[]{
                new SkinDef.MeshReplacement
                {
                    renderer = mdl.transform.Find("Legs").GetComponent<SkinnedMeshRenderer>(),
                    mesh = masterObject.transform.Find("Legs").GetComponent<SkinnedMeshRenderer>().sharedMesh
                },
                new SkinDef.MeshReplacement
                {
                     renderer = mdl.transform.Find("Scarf").GetComponent<SkinnedMeshRenderer>(),
                     mesh = masterObject.transform.Find("Scarf").GetComponent<SkinnedMeshRenderer>().sharedMesh
                }
            };

            SkinDef mastery = Skins.CreateNewSkinDef(skinInfo);
            Skins.AddSkinToCharacter(Body, mastery);

            On.RoR2.UI.SurvivorIconController.Rebuild += SurvivorIconController_Rebuild;
        }

        private void SurvivorIconController_Rebuild(On.RoR2.UI.SurvivorIconController.orig_Rebuild orig, SurvivorIconController self)
        {
            orig(self);

            if (self.hgButton && self.survivorDef == Ranger.instance.SurvivorDef)
            {
                Main.ModLogger.LogError("found ranger survivor icon and button exists");

                var survivorChoiceGridPanel = self.transform.parent;
                var survivorGrid = survivorChoiceGridPanel.parent;
                var leftHandPanel = survivorGrid.parent;
                var safeArea = leftHandPanel.parent;
                var anchor = safeArea;
                if (anchor.GetComponent<Image>() == null)
                {
                    Main.ModLogger.LogError("adding gay furries");
                    var image = anchor.AddComponent<Image>();

                    Main.ModLogger.LogError("image component is " + image);
                    Main.ModLogger.LogError(Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texGayFurries.png"));
                    image.sprite = Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texGayFurries.png");
                    image.enabled = false;
                    image.color = new Color32(255, 211, 216, 147);

                    self.hgButton.onClick.AddListener(() => OnClick(image));
                }
            }
        }

        private int clickCount = 0;

        private void OnClick(Image image)
        {
            clickCount++;
            if (clickCount >= 20)
            {
                image.StartCoroutine(ToggleImage(image));
                clickCount = 0;
            }
        }

        private IEnumerator ToggleImage(Image image)
        {
            image.enabled = true;
            yield return new WaitForSecondsRealtime(0.33f);
            image.enabled = false;
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
            ContentAddition.AddEntityState(typeof(Galvanize), out _);
        }

        public SkinDef CreateRecolor(string skinName, float emissionValue = 2.5f, UnlockableDef unlockableDef = null)
        {
            var trimmedName = skinName.Replace(" ", "");
            var mainTex = Main.hifuSandswept.LoadAsset<Texture2D>("texRangerDiffuse" + trimmedName + ".png");
            var emTex = Main.hifuSandswept.LoadAsset<Texture2D>("texRangerEmission" + trimmedName + ".png");

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