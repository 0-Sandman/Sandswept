/*
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

namespace Sandswept.Artifacts
{
    [ConfigSection("Artifacts :: Abysm")]
    internal class ArtifactOfAbysm : ArtifactBase<ArtifactOfAbysm>
    {
        public override string ArtifactName => "Artifact of Abysm";

        public override string ArtifactLangTokenName => "ABYSM";

        public override string ArtifactDescription => "All pickups are obscured. Lunar items appear regularly.";

        public override Sprite ArtifactEnabledIcon => Main.hifuSandswept.LoadAsset<Sprite>("texArtifactOfBlindnessEnabled.png");

        public override Sprite ArtifactDisabledIcon => Main.hifuSandswept.LoadAsset<Sprite>("texArtifactOfBlindnessDisabled.png");

        public static List<LanguageAPI.LanguageOverlay> languageOverlays = new();

        public static Dictionary<EquipmentDef, Sprite> cachedEquipmentDefIcons = new();
        public static Dictionary<EquipmentDef, GameObject> cachedEquipmentDefModels = new();
        public static Dictionary<EquipmentDef, ColorCatalog.ColorIndex> cachedEquipmentDefColorIndices = new();

        public static Dictionary<ItemDef, Sprite> cachedItemDefIcons = new();

        public static Dictionary<ItemDef, GameObject> cachedItemDefModels = new();

        // public static Dictionary<PickupDef, GameObject> cachedPickupModels = new();

        public static Dictionary<ItemTierDef, ItemTierDef.PickupRules> cachedItemTierDefPickupRules = new();
        public static Dictionary<ItemTierDef, ColorCatalog.ColorIndex> cachedItemTierDefColorIndices = new();
        public static Dictionary<ItemTierDef, ColorCatalog.ColorIndex> cachedItemTierDefDarkColorIndices = new();

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
        }

        private void PickupDisplay_RebuildModel(ILContext il)
        {
            ILCursor c = new(il);

            ILLabel label = null;

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchStloc(out _),
                x => x.MatchLdloc(out _),
                x => x.MatchLdcI4(out _),
                x => x.MatchBeq(out _)))
            {
                c.Index++;

                var goMyBeetle = c.Clone();

                if (goMyBeetle.TryGotoNext(MoveType.Before,
                    x => x.MatchLdfld(typeof(PickupDisplay), nameof(PickupDisplay.lunarParticleEffect)),
                    x => x.MatchCallOrCallvirt(out _),
                    x => x.MatchBrfalse(out label)))
                {
                    c.Emit(OpCodes.Br, label);
                }
                else
                {
                    Main.ModLogger.LogError("Failed to apply Pickup Display Rebuild Model #2 hook");
                }
            }
            else
            {
                Main.ModLogger.LogError("Failed to apply Pickup Display Rebuild Model #1 hook");
            }
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

        private void Run_onRunStartGlobal(Run run)
        {
            if (ArtifactEnabled)
            {
                ApplyArtifactChanges();
                IL.RoR2.PickupDisplay.RebuildModel += PickupDisplay_RebuildModel;
                // this is dumb but I don't wanna deal with EmitDelegate'ing allat
            }
        }

        private void Run_onRunDestroyGlobal(Run run)
        {
            ApplyArtifactChanges(true);
            IL.RoR2.PickupDisplay.RebuildModel -= PickupDisplay_RebuildModel;
        }

        private void ApplyArtifactChanges(bool remove = false)
        {
            // FuckingStupidThing(true);

            if (Run.instance)
            {
                Main.ModLogger.LogError("run instance exists");

                Run.instance.availableTier1DropList = Run.instance.availableTier1DropList.Concat(Run.instance.availableLunarItemDropList).ToList();
                Run.instance.availableTier2DropList = Run.instance.availableTier2DropList.Concat(Run.instance.availableLunarItemDropList).ToList();
                Run.instance.availableEquipmentDropList = Run.instance.availableEquipmentDropList.Concat(Run.instance.availableLunarEquipmentDropList).ToList();
            }

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

                    // if (!cachedPickupModels.ContainsKey(entry))
                    // {
                        // cachedPickupModels.Add(entry, entry.displayPrefab);
                    // }

                    pickupDef.displayPrefab = unknownModel;
                    pickupDef.baseColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Tier1Item);
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

                // FuckingStupidThing(false);
            }

            Language.SetCurrentLanguage(Language.currentLanguageName);
        }
    }
}
*/