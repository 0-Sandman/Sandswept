/*
using System.Xml.Serialization;

using BepInEx.Configuration;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using RoR2;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.AddressableAssets;
using R2API;
using System.Collections.Generic;
using Sandswept.Artifacts;
using Sandswept;
using RoR2.ContentManagement;
using System.Runtime.CompilerServices;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using R2API.Utils;

namespace Sandswept.Artifacts
{
    [ConfigSection("Artifacts :: Obscurity")]
    internal class ArtifactOfObscurity : ArtifactBase<ArtifactOfObscurity>
    {
        public override string ArtifactName => "Artifact of Obscurity";

        public override string ArtifactLangTokenName => "OBSCURITY";

        public override string ArtifactDescription => "All pickups are obscured. Lunar items appear regularly.";

        public override Sprite ArtifactEnabledIcon => Main.sandsweptHIFU.LoadAsset<Sprite>("texFlushed.png");

        public override Sprite ArtifactDisabledIcon => Main.sandsweptHIFU.LoadAsset<Sprite>("texEyebrow.png");

        public static List<LanguageAPI.LanguageOverlay> languageOverlays = new();

        public static Dictionary<EquipmentDef, Sprite> cachedEquipmentDefIcons = new();
        public static Dictionary<EquipmentDef, GameObject> cachedEquipmentDefModels = new();
        public static Dictionary<EquipmentDef, ColorCatalog.ColorIndex> cachedEquipmentDefColorIndices = new();

        public static Dictionary<ItemDef, Sprite> cachedItemDefIcons = new();
        public static Dictionary<ItemDef, GameObject> cachedItemDefModels = new();

        public static Dictionary<ItemTierDef, ItemTierDef.PickupRules> cachedItemTierDefPickupRules = new();
        public static Dictionary<ItemTierDef, ColorCatalog.ColorIndex> cachedItemTierDefColorIndices = new();
        public static Dictionary<ItemTierDef, ColorCatalog.ColorIndex> cachedItemTierDefDarkColorIndices = new();

        public static Dictionary<PickupDef, Sprite> cachedPickupDefIcons = new();

        public static Dictionary<PickupDef, GameObject> cachedPickupDefModels = new();

        public static Dictionary<PickupDef, Color> cachedPickupDefColors = new();
        public static Dictionary<PickupDef, Color> cachedPickupDefDarkColors = new();
        public static Dictionary<PickupDef, GameObject> cachedPickupDefDroplets = new();

        public static Sprite unknownIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();

        public static GameObject unknownModel = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        public override void Hooks()
        {
            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
            On.RoR2.ClassicStageInfo.RebuildCards += ClassicStageInfo_RebuildCards;
            IL.RoR2.Items.RandomlyLunarUtils.CheckForLunarReplacement += SetEulogyStacksValue;
            IL.RoR2.Items.RandomlyLunarUtils.CheckForLunarReplacementUniqueArray += SetEulogyStacksValueUniqueArray;
        }

        private void SetEulogyStacksValue(ILContext il)
        {
            ILCursor c = new(il);

            bool conditionFound = c.TryGotoNext(MoveType.Before,
            x => x.MatchLdloc(out _),
            x => x.MatchLdcI4(out _));

            if (!conditionFound)
            {
                Main.ModLogger.LogError("Failed to apply Artifact of Obscurity Eulogy Zero 1 hook");
                return;
            }

            c.Index++;
            c.EmitDelegate<Func<int, int>>((orig) =>
            {
                var eulogyCount = ArtifactEnabled ? 1 : orig;
                return eulogyCount;
            });

            SetEulogyStacksAndReplaceScalar(c, 2);

        }

        private void SetEulogyStacksValueUniqueArray(ILContext il)
        {
            ILCursor c = new(il);

            bool conditionFound = c.TryGotoNext(MoveType.Before,
            x => x.MatchLdloc(out _),
            x => x.MatchLdcI4(out _),
            x => x.MatchBle(out _));

            if (!conditionFound)
            {
                Main.ModLogger.LogError("Failed to apply Artifact of Obscurity Eulogy Zero 3 hook");
                return;
            }

            c.Index++;
            c.EmitDelegate<Func<int, int>>((orig) =>
            {
                var eulogyCount = ArtifactEnabled ? 1 : orig;
                return eulogyCount;
            });

            SetEulogyStacksAndReplaceScalar(c, 4);
        }

        private void SetEulogyStacksAndReplaceScalar(ILCursor c, int hookNumber)
        {
            bool scalarFound = c.TryGotoNext(MoveType.Before,
            x => x.MatchLdcR4(out _),
            x => x.MatchLdloc(out _),
            x => x.MatchConvR4());

            if (!scalarFound)
            {
                Main.ModLogger.LogError($"Failed to apply Artifact of Obscurity Eulogy Zero {hookNumber} hook");
                return;
            }

            c.Index++;
            c.EmitDelegate<Func<float, float>>((orig) =>
            {
                var eulogyScalar = ArtifactEnabled ? 0.05f : orig;
                return eulogyScalar;
            });

            c.Index++;
            c.EmitDelegate<Func<int, int>>((orig) =>
            {
                var eulogyCount = ArtifactEnabled ? 1 : orig;
                return eulogyCount;
            });

        }

        private void Run_onRunStartGlobal(Run run)
        {
            if (ArtifactEnabled)
            {
                ApplyArtifactChanges();
                On.RoR2.PickupDisplay.RebuildModel += ObscureParticleEffects;
                // this is dumb but I don't wanna deal with EmitDelegate'ing allat
            }
        }

        private void Run_onRunDestroyGlobal(Run run)
        {
            ApplyArtifactChanges(true);
            On.RoR2.PickupDisplay.RebuildModel -= ObscureParticleEffects;
        }

        private void ClassicStageInfo_RebuildCards(On.RoR2.ClassicStageInfo.orig_RebuildCards orig, ClassicStageInfo self, DirectorCardCategorySelection forcedMonsterCategory, DirectorCardCategorySelection forcedInteractableCategory)
        {
            orig(self, forcedMonsterCategory, forcedInteractableCategory);
            if (ArtifactEnabled)
            {
                self.interactableCategories.RemoveCardsThatFailFilter(FuckOff);

                static bool FuckOff(DirectorCard card)
                {
                    var prefab = card.spawnCard.prefab;
                    if (prefab.GetComponent<ScrapperController>() || prefab.GetComponent<ShrineCleanseBehavior>())
                    {
                        return false;
                    }
                    return true;
                }
            }
        }

        private void ObscureParticleEffects(On.RoR2.PickupDisplay.orig_RebuildModel orig, PickupDisplay self, GameObject modelObjectOverride)
        {
            // modelObjectOverride = unknownModel;
            // dumbass that makes models invisible somehow
            orig(self, modelObjectOverride);
            if (self)
            {
                if (self.tier1ParticleEffect)
                {
                    self.tier1ParticleEffect.SetActive(true);
                }
                if (self.tier2ParticleEffect)
                {
                    self.tier2ParticleEffect.SetActive(false);
                }
                if (self.tier3ParticleEffect)
                {
                    self.tier3ParticleEffect.SetActive(false);
                }
                if (self.voidParticleEffect)
                {
                    self.voidParticleEffect.SetActive(false);
                }
                if (self.equipmentParticleEffect)
                {
                    self.equipmentParticleEffect.SetActive(false);
                }
                if (self.bossParticleEffect)
                {
                    self.bossParticleEffect.SetActive(false);
                }
                if (self.lunarParticleEffect)
                {
                    self.lunarParticleEffect.SetActive(false);
                }
                // horridble
            }
        }

        private void ApplyArtifactChanges(bool remove = false)
        {
            FuckingStupidThing(true);

            if (!remove)
            {
                for (int i = 0; i < ContentManager._itemTierDefs.Length; i++)
                {
                    var itemTierDef = ContentManager._itemTierDefs[i];
                    if (!cachedItemTierDefPickupRules.ContainsKey(itemTierDef))
                    {
                        cachedItemTierDefPickupRules.Add(itemTierDef, itemTierDef.pickupRules);
                    }

                    if (!cachedItemTierDefColorIndices.ContainsKey(itemTierDef))
                    {
                        cachedItemTierDefColorIndices.Add(itemTierDef, itemTierDef.colorIndex);
                    }

                    if (!cachedItemTierDefDarkColorIndices.ContainsKey(itemTierDef))
                    {
                        cachedItemTierDefDarkColorIndices.Add(itemTierDef, itemTierDef.darkColorIndex);
                    }

                    itemTierDef.pickupRules = ItemTierDef.PickupRules.Default;
                    itemTierDef.colorIndex = ColorCatalog.ColorIndex.Tier1Item;
                    itemTierDef.darkColorIndex = ColorCatalog.ColorIndex.Tier1ItemDark;
                }

                for (int i = 0; i < ContentManager._equipmentDefs.Length; i++)
                {
                    var equipmentDef = ContentManager._equipmentDefs[i];

                    var nameTokenOverlay = LanguageAPI.AddOverlay(equipmentDef.nameToken, "???");
                    var descriptionTokenOverlay = LanguageAPI.AddOverlay(equipmentDef.descriptionToken, "???");
                    var pickupTokenOverlay = LanguageAPI.AddOverlay(equipmentDef.pickupToken, "???");
                    var loreTokenOverlay = LanguageAPI.AddOverlay(equipmentDef.loreToken, "???");

                    languageOverlays.Add(nameTokenOverlay);
                    languageOverlays.Add(descriptionTokenOverlay);
                    languageOverlays.Add(pickupTokenOverlay);
                    languageOverlays.Add(loreTokenOverlay);

                    if (!cachedEquipmentDefIcons.ContainsKey(equipmentDef))
                    {
                        cachedEquipmentDefIcons.Add(equipmentDef, equipmentDef.pickupIconSprite);
                    }

                    if (!cachedEquipmentDefModels.ContainsKey(equipmentDef))
                    {
                        cachedEquipmentDefModels.Add(equipmentDef, equipmentDef.pickupModelPrefab);
                    }

                    if (!cachedEquipmentDefColorIndices.ContainsKey(equipmentDef))
                    {
                        cachedEquipmentDefColorIndices.Add(equipmentDef, equipmentDef.colorIndex);
                    }

                    equipmentDef.pickupIconSprite = unknownIcon;
                    equipmentDef.pickupModelPrefab = unknownModel;
                    equipmentDef.colorIndex = ColorCatalog.ColorIndex.Equipment;
                }

                for (int i = 0; i < ContentManager._itemDefs.Length; i++)
                {
                    var itemDef = ContentManager._itemDefs[i];
                    if (itemDef.nameToken != null)
                    {
                        var nameTokenOverlay = LanguageAPI.AddOverlay(itemDef.nameToken, "???");
                        languageOverlays.Add(nameTokenOverlay);
                    }

                    if (itemDef.descriptionToken != null)
                    {
                        var descriptionTokenOverlay = LanguageAPI.AddOverlay(itemDef.descriptionToken, "???");
                        languageOverlays.Add(descriptionTokenOverlay);
                    }

                    if (itemDef.pickupToken != null)
                    {
                        var pickupTokenOverlay = LanguageAPI.AddOverlay(itemDef.pickupToken, "???");
                        languageOverlays.Add(pickupTokenOverlay);
                    }

                    if (itemDef.loreToken != null)
                    {
                        var loreTokenOverlay = LanguageAPI.AddOverlay(itemDef.loreToken, "???");
                        languageOverlays.Add(loreTokenOverlay);
                    }

                    if (!cachedItemDefIcons.ContainsKey(itemDef))
                    {
                        cachedItemDefIcons.Add(itemDef, itemDef.pickupIconSprite);
                    }

                    if (!cachedItemDefModels.ContainsKey(itemDef))
                    {
                        cachedItemDefModels.Add(itemDef, itemDef.pickupModelPrefab);
                    }

                    itemDef.pickupIconSprite = unknownIcon;
                    itemDef.pickupModelPrefab = unknownModel;
                }

                var droplet = Paths.GameObject.Tier1Orb;

                for (int i = 0; i < PickupCatalog.entries.Length; i++)
                {
                    var pickupDef = PickupCatalog.entries[i];

                    var nameTokenOverlay = LanguageAPI.AddOverlay(pickupDef.nameToken, "???");
                    languageOverlays.Add(nameTokenOverlay);

                    if (!cachedPickupDefIcons.ContainsKey(pickupDef))
                    {
                        cachedPickupDefIcons.Add(pickupDef, pickupDef.iconSprite);
                    }

                    if (!cachedPickupDefModels.ContainsKey(pickupDef))
                    {
                        cachedPickupDefModels.Add(pickupDef, pickupDef.displayPrefab);
                    }

                    if (!cachedPickupDefColors.ContainsKey(pickupDef))
                    {
                        cachedPickupDefColors.Add(pickupDef, pickupDef.baseColor);
                    }

                    if (!cachedPickupDefDarkColors.ContainsKey(pickupDef))
                    {
                        cachedPickupDefDarkColors.Add(pickupDef, pickupDef.darkColor);
                    }

                    if (!cachedPickupDefDroplets.ContainsKey(pickupDef))
                    {
                        cachedPickupDefDroplets.Add(pickupDef, pickupDef.dropletDisplayPrefab);
                    }

                    pickupDef.iconSprite = unknownIcon;
                    // for pickuppickerpanel (potentials, aurelionite blessings, etc ) fix
                    pickupDef.displayPrefab = unknownModel;
                    // for turning off artifact fix
                    // memOPP update fixes
                    pickupDef.baseColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Tier1Item);
                    pickupDef.darkColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Tier1ItemDark);
                    pickupDef.dropletDisplayPrefab = droplet;
                }
            }

            if (remove)
            {
                for (int i = 0; i < languageOverlays.Count; i++)
                {
                    var overlay = languageOverlays[i];
                    overlay?.Remove();
                }

                for (int i = languageOverlays.Count - 1; i >= 0; i--)
                {
                    languageOverlays.RemoveAt(i);
                }

                foreach (var itemIcon in cachedItemDefIcons)
                {
                    // sprite.Key is itemDef
                    itemIcon.Key.pickupIconSprite = itemIcon.Value;
                }

                cachedItemDefIcons.Clear();

                foreach (var itemModel in cachedItemDefModels)
                {
                    // model.Key is itemDef
                    itemModel.Key.pickupModelPrefab = itemModel.Value;
                }

                cachedItemDefModels.Clear();

                foreach (var itemTierDefPickupRule in cachedItemTierDefPickupRules)
                {
                    itemTierDefPickupRule.Key.pickupRules = itemTierDefPickupRule.Value;
                }

                cachedItemTierDefPickupRules.Clear();

                foreach (var itemTierDefColor in cachedItemTierDefColorIndices)
                {
                    itemTierDefColor.Key.colorIndex = itemTierDefColor.Value;
                }

                cachedItemTierDefColorIndices.Clear();

                foreach (var itemTierDefDarkColor in cachedItemTierDefDarkColorIndices)
                {
                    itemTierDefDarkColor.Key.darkColorIndex = itemTierDefDarkColor.Value;
                }

                cachedItemTierDefDarkColorIndices.Clear();

                foreach (var equipmentIcon in cachedEquipmentDefIcons)
                {
                    equipmentIcon.Key.pickupIconSprite = equipmentIcon.Value;
                }

                cachedEquipmentDefIcons.Clear();

                foreach (var equipmentModel in cachedEquipmentDefModels)
                {
                    equipmentModel.Key.pickupModelPrefab = equipmentModel.Value;
                }

                cachedEquipmentDefModels.Clear();

                foreach (var equipmentColor in cachedEquipmentDefColorIndices)
                {
                    equipmentColor.Key.colorIndex = ColorCatalog.ColorIndex.Equipment;
                }

                cachedEquipmentDefColorIndices.Clear();

                foreach (var pickupModel in cachedPickupDefModels)
                {
                    pickupModel.Key.displayPrefab = pickupModel.Value;
                }

                cachedPickupDefModels.Clear();

                foreach (var pickupIcon in cachedPickupDefIcons)
                {
                    pickupIcon.Key.iconSprite = pickupIcon.Value;

                }

                cachedPickupDefIcons.Clear();

                foreach (var pickupColor in cachedPickupDefColors)
                {
                    pickupColor.Key.baseColor = pickupColor.Value;
                }

                cachedPickupDefColors.Clear();

                foreach (var pickupDarkColor in cachedPickupDefDarkColors)
                {
                    pickupDarkColor.Key.darkColor = pickupDarkColor.Value;
                }

                cachedPickupDefDarkColors.Clear();

                foreach (var pickupDroplet in cachedPickupDefDroplets)
                {
                    pickupDroplet.Key.dropletDisplayPrefab = pickupDroplet.Value;
                }

                cachedPickupDefDroplets.Clear();

                FuckingStupidThing(false);
            }

            Language.SetCurrentLanguage(Language.currentLanguageName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void FuckingStupidThing(bool enable)
        {
            if (Main.LookingGlassLoaded)
            {
                // Main.ModLogger.LogError("looking glass loaded");
                for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
                {
                    var pcmc = PlayerCharacterMasterController.instances[i];
                    var lookingGlassDisabler = pcmc.GetComponent<LookingGlassDisabler>() ? pcmc.GetComponent<LookingGlassDisabler>() : pcmc.AddComponent<LookingGlassDisabler>();
                    lookingGlassDisabler.shouldRun = enable;
                }
            }
        }
    }

    public class LookingGlassDisabler : MonoBehaviour
    {
        public bool shouldRun = false;
        public bool cachedItemStatsCalculationsValue;
        public float timer;
        public float interval = 0.05f;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void Start()
        {
            cachedItemStatsCalculationsValue = LookingGlass.ItemStatsNameSpace.ItemStats.itemStatsCalculations.Value;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (timer >= interval)
            {
                if (shouldRun)
                {
                    LookingGlass.ItemStatsNameSpace.ItemStats.itemStatsCalculations.Value = false;
                }
                else
                {
                    LookingGlass.ItemStatsNameSpace.ItemStats.itemStatsCalculations.Value = cachedItemStatsCalculationsValue;
                }
                timer = 0f;
            }
        }
    }
}
*/