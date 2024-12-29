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

namespace Sandswept.Artifacts.ArtifactOfAbysm
{
    [ConfigSection("Artifacts :: Abysm")]
    internal class ArtifactOfAbysm : ArtifactBase<ArtifactOfAbysm>
    {
        public override string ArtifactName => "Artifact of Abysm";

        public override string ArtifactLangTokenName => "ABYSM";

        public override string ArtifactDescription => "All items become unknown. Lunar items appear regularly.";

        public override Sprite ArtifactEnabledIcon => Main.hifuSandswept.LoadAsset<Sprite>("texArtifactOfBlindnessEnabled.png");

        public override Sprite ArtifactDisabledIcon => Main.hifuSandswept.LoadAsset<Sprite>("texArtifactOfBlindnessDisabled.png");

        public static List<LanguageAPI.LanguageOverlay> overlays = new();

        public static Dictionary<ItemDef, Sprite> cachedIcons = new();

        public static Dictionary<ItemDef, GameObject> cachedModels = new();

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
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
            if (ArtifactEnabled && body.isPlayerControlled && body.inventory)
            {
                body.inventory.GiveItem(DLC1Content.Items.RandomlyLunar);

                FuckingStupidThing2(body);
            }
        }

        private void Run_onRunStartGlobal(Run run)
        {
            if (ArtifactEnabled)
            {
                ApplyArtifactChanges();
            }
        }

        private void Run_onRunDestroyGlobal(Run run)
        {
            ApplyArtifactChanges(true);
        }

        private void ApplyArtifactChanges(bool remove = false)
        {
            DLC1Content.Items.RandomlyLunar.hidden = true;

            for (int i = 0; i < ContentManager._itemDefs.Length; i++)
            {
                if (overlays.Count <= ContentManager._itemDefs.Length)
                {
                    var itemDef = ContentManager._itemDefs[i];
                    var nameTokenOverlay = LanguageAPI.AddOverlay(itemDef.nameToken, "?");
                    var descriptionTokenOverlay = LanguageAPI.AddOverlay(itemDef.descriptionToken, "?");
                    var pickupTokenOverlay = LanguageAPI.AddOverlay(itemDef.pickupToken, "?");
                    var loreTokenOverlay = LanguageAPI.AddOverlay(itemDef.loreToken, "?");

                    overlays.Add(nameTokenOverlay);
                    overlays.Add(descriptionTokenOverlay);
                    overlays.Add(pickupTokenOverlay);
                    overlays.Add(loreTokenOverlay);

                    Language.SetCurrentLanguage(Language.currentLanguageName);

                    if (!cachedIcons.ContainsKey(itemDef))
                    {
                        cachedIcons.Add(itemDef, itemDef.pickupIconSprite);
                    }

                    if (!cachedModels.ContainsKey(itemDef))
                    {
                        cachedModels.Add(itemDef, itemDef.pickupModelPrefab);
                    }

                    itemDef.pickupIconSprite = unknownIcon;
                    itemDef.pickupModelPrefab = unknownModel;
                }
            }

            // TODO: do pickups, set all itemtierdef.pickupRules to Default, disable logbook button OR don't if changing pickups works (doubt), try to disable tier colors in strings?

            for (int i = 0; i < PickupCatalog.entries.Length; i++)
            {
                var entry = PickupCatalog.entries[i];
                entry.
            }

            if (remove)
            {
                DLC1Content.Items.RandomlyLunar.hidden = false;

                for (int i = 0; i < overlays.Count; i++)
                {
                    var overlay = overlays[i];
                    overlay?.Remove();
                }

                for (int i = overlays.Count - 1; i >= 0; i--)
                {
                    overlays.RemoveAt(i);
                }

                Language.SetCurrentLanguage(Language.currentLanguageName);

                foreach (var sprite in cachedIcons)
                {
                    // sprite.Key is itemDef
                    sprite.Key.pickupIconSprite = sprite.Value;
                }

                cachedIcons.Clear();

                foreach (var model in cachedModels)
                {
                    // model.Key is itemDef
                    model.Key.pickupModelPrefab = model.Value;
                }

                FuckingStupidThing();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void FuckingStupidThing()
        {
            if (Main.LookingGlassLoaded)
            {
                for (int i = 0; i < CharacterBody.instancesList.Count; i++)
                {
                    var characterBody = CharacterBody.instancesList[i];
                    if (characterBody.isPlayerControlled)
                    {
                        var master = characterBody.master;
                        if (master && master.GetComponent<LookingGlassDisabler>() != null)
                        {
                            var lookingGlassDisabler = master.GetComponent<LookingGlassDisabler>();
                            lookingGlassDisabler.shouldRun = false;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void FuckingStupidThing2(CharacterBody body)
        {
            if (Main.LookingGlassLoaded)
            {
                var master = body.master;
                if (master && master.GetComponent<LookingGlassDisabler>() == null)
                {
                    var lookingGlassDisabler = master.AddComponent<LookingGlassDisabler>();
                    lookingGlassDisabler.shouldRun = true;
                }
            }
        }
    }

    public class LookingGlassDisabler : MonoBehaviour
    {
        public bool shouldRun = true;
        public bool cachedItemStatsCalculationsValue;
        public float timer;
        public float interval = 0.15f;

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