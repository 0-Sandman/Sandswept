using HarmonyLib;
using LookingGlass.ItemStatsNameSpace;
using Rewired.ComponentControls.Effects;
using RoR2.Items;
using Sandswept.Items.Greens;
using Sandswept.Items.VoidGreens;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Sandswept.Utils.TotallyNotStolenUtils;

namespace Sandswept.Items
{
    public abstract class ItemBase<T> : ItemBase where T : ItemBase<T>
    {
        public static T instance { get; private set; }

        public ItemBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }

        public abstract ItemTier Tier { get; }
        public abstract ItemTag[] ItemTags { get; }

        public abstract GameObject ItemModel { get; }
        public abstract Sprite ItemIcon { get; }

        public virtual bool CanRemove { get; } = true;

        public virtual string AchievementName { get; } = string.Empty;
        public virtual string AchievementDesc { get; } = string.Empty;
        public virtual Func<string> GetHowToUnlock => () => AchievementName + "\n<style=cStack>" + AchievementDesc + "</style>";
        public virtual Func<string> GetUnlocked => () => AchievementName + "\n<style=cStack>" + AchievementDesc + "</style>";

        public virtual float modelPanelParametersMinDistance { get; } = 2f;
        public virtual float modelPanelParametersMaxDistance { get; } = 10f;

        public ItemDef ItemDef;

        public UnlockableDef UnlockableDef;

        public virtual ItemDef ItemToCorrupt { get; set; } = null;
        public virtual Dictionary<string, Func<string>> LangReplacements { get; set; } = null;
        private bool firstApplication = true;

        public static bool DefaultEnabledCallback(ItemBase self)
        {
            ConfigSectionAttribute attribute = self.GetType().GetCustomAttribute<ConfigSectionAttribute>();
            if (attribute != null)
            {
                bool isValid = Main.config.Bind<bool>(attribute.name, "Enabled", true, "Allow this item to appear in runs?").Value;
                if (isValid)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        public string GetConfName()
        {
            ConfigSectionAttribute attribute = this.GetType().GetCustomAttribute<ConfigSectionAttribute>();
            if (attribute != null)
            {
                return attribute.name;
            }
            else
            {
                return "Items :: " + ItemName;
            }
        }

        /// <summary>
        /// This method structures your code execution of this class. An example implementation inside of it would be:
        /// <para>CreateConfig(config);</para>
        /// <para>CreateLang();</para>
        /// <para>CreateItem();</para>
        /// <para>Hooks();</para>
        /// <para>This ensures that these execute in this order, one after another, and is useful for having things available to be used in later methods.</para>
        /// <para>P.S. CreateItemDisplayRules(); does not have to be called in this, as it already gets called in CreateItem();</para>
        /// </summary>
        /// <param name="config">The config file that will be passed into this from the main class.</param>
        public virtual void Init()
        {
            CreateItem();
            Hooks();

            if (Main.LookingGlassLoaded)
            {
                LGWrapper();
            }
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void LGWrapper()
        {
            ItemCatalog.availability.CallWhenAvailable(OnItemCatalogAvailable);
        }

        private void OnItemCatalogAvailable()
        {
            if (GetItemStatsDef() is ItemStatsDef itemStatsDef)
            {
                LookingGlass.ItemStatsNameSpace.ItemDefinitions.RegisterItemStatsDef(itemStatsDef, ItemDef.itemIndex);
            }
        }

        protected void CreateItem()
        {
            var temporaryItemIcon = Main.hifuSandswept.LoadAsset<Sprite>("texItemTemp.png");
            var temporaryItemModel = Main.hifuSandswept.LoadAsset<GameObject>("TempHolder.prefab");

            ItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ItemDef.name = "ITEM_SANDSWEPT_" + ItemLangTokenName;
            ItemDef.nameToken = "ITEM_SANDSWEPT_" + ItemLangTokenName + "_NAME";
            ItemDef.pickupToken = "ITEM_SANDSWEPT_" + ItemLangTokenName + "_PICKUP";
            ItemDef.descriptionToken = "ITEM_SANDSWEPT_" + ItemLangTokenName + "_DESCRIPTION";
            ItemDef.loreToken = "ITEM_SANDSWEPT_" + ItemLangTokenName + "_LORE";
            ItemDef.pickupModelPrefab = ItemModel ?? temporaryItemModel;
            ItemDef.pickupIconSprite = ItemIcon ?? temporaryItemIcon;
            ItemDef.hidden = false;
            ItemDef.canRemove = CanRemove;
#pragma warning disable
            ItemDef.deprecatedTier = Tier;
            ItemDef.requiredExpansion = Main.SandsweptExpansionDef;
            if (ItemTags.Length > 0)
            {
                ItemDef.tags = ItemTags;
            }

            ApplyLanguage();

            if (AchievementName != string.Empty && AchievementDesc != string.Empty)
            {
                LanguageAPI.Add("ACHIEVEMENT_ITEM_SANDSWEPT_" + ItemLangTokenName + "_NAME", AchievementName);
                LanguageAPI.Add("ACHIEVEMENT_ITEM_SANDSWEPT_" + ItemLangTokenName + "_DESCRIPTION", AchievementDesc);

                ItemDef.unlockableDef = CreateUnlock();
            }

            if (ItemModel != null)
            {
                CreateModelPanelParameters(ItemModel);
            }
            else
            {
                CreateModelPanelParameters(temporaryItemModel);
            }

            ItemAPI.Add(new CustomItem(ItemDef, CreateItemDisplayRules()));
        }

        protected void ApplyLanguage()
        {
            Apply("ITEM_SANDSWEPT_" + ItemLangTokenName + "_NAME", Modify(ItemName));
            Apply("ITEM_SANDSWEPT_" + ItemLangTokenName + "_PICKUP", Modify(ItemPickupDesc));
            Apply("ITEM_SANDSWEPT_" + ItemLangTokenName + "_DESCRIPTION", Modify(ItemFullDescription));
            Apply("ITEM_SANDSWEPT_" + ItemLangTokenName + "_LORE", Modify(ItemLore));

            firstApplication = false;

            void Apply(string s1, string s2)
            {
                if (firstApplication)
                {
                    LanguageAPI.Add(s1, s2);
                }
                else
                {
                    LanguageAPI.AddOverlay(s1, s2);
                }
            }

            string Modify(string input)
            {
                string output = input;
                if (LangReplacements != null)
                {
                    foreach (KeyValuePair<string, Func<string>> kvp in LangReplacements)
                    {
                        output = output.Replace(kvp.Key, kvp.Value());
                    }
                }
                return output;
            }
        }

        private void CreateModelPanelParameters(GameObject itemModel)
        {
            if (itemModel.GetComponent<ModelPanelParameters>() != null)
            {
                return;
            }

            GameObject model = PrefabAPI.InstantiateClone(itemModel, itemModel.name + "-fixed", false);
            GameObject focus = new("Focus");
            GameObject camera = new("Camera");
            MeshRenderer biggestRenderer = model.GetComponentsInChildren<MeshRenderer>().ToList().OrderByDescending(x => ToFloat(x.bounds.size)).First();
            float mult = ToFloat(biggestRenderer.bounds.size) / 3f;
            float min = mult;
            float max = 3f * mult;
            focus.transform.parent = model.transform;
            camera.transform.parent = model.transform;
            focus.transform.position = biggestRenderer.bounds.center;
            camera.transform.localPosition = focus.transform.position + (model.transform.forward * max);

            var modelPanelParameters = model.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = focus.transform;
            modelPanelParameters.cameraPositionTransform = camera.transform;
            modelPanelParameters.minDistance = min;
            modelPanelParameters.maxDistance = max;

            ItemDef.pickupModelPrefab = model;
        }

        public static float ToFloat(Vector3 vec)
        {
            vec.x = Mathf.Abs(vec.x);
            vec.y = Mathf.Abs(vec.y);
            vec.z = Mathf.Abs(vec.z);
            return vec.x + vec.y + vec.z;
        }

        protected UnlockableDef CreateUnlock()
        {
            ItemDef.unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            ItemDef.unlockableDef.cachedName = "ITEM_SANDSWEPT_" + ItemLangTokenName;
            ItemDef.unlockableDef.nameToken = "ITEM_SANDSWEPT_" + ItemLangTokenName + "_NAME";
            ItemDef.unlockableDef.getHowToUnlockString = GetHowToUnlock;
            ItemDef.unlockableDef.getUnlockedString = GetUnlocked;

            ItemDef.unlockableDef.achievementIcon = CreateItemIconWithBackgroundFromItem(ItemDef);
            var unlockDef = ItemDef.unlockableDef;
            Main.Unlocks.Add(unlockDef);
            return unlockDef;
        }

        public virtual void Hooks()
        { }

        //Based on ThinkInvis' methods
        public int GetCount(CharacterBody body)
        {
            if (body == null || body.inventory == null)
            {
                return 0;
            }

            return body.inventory.GetItemCount(ItemDef);
        }

        public int GetCount(CharacterMaster master)
        {
            if (master == null || master.inventory == null)
            {
                return 0;
            }

            return master.inventory.GetItemCount(ItemDef);
        }

        public int GetCount(PlayerCharacterMasterController pcmc)
        {
            return GetCount(pcmc.master);
        }

        public GameObject SetUpIDRS()
        {
            var idrsPrefab = PrefabAPI.InstantiateClone(ItemModel, ItemName.Replace(" ", "") + "IDRS", false);
            var itemDisplay = idrsPrefab.AddComponent<ItemDisplay>();
            List<Renderer> rendererList = [.. idrsPrefab.GetComponentsInChildren<Renderer>()];
            Array.Resize(ref itemDisplay.rendererInfos, rendererList.Count);
            for (int j = 0; j < rendererList.Count; j++)
            {
                var renderer = rendererList[j];
                var defaultMaterial = renderer.material;
                itemDisplay.rendererInfos[j] = new CharacterModel.RendererInfo()
                {
                    renderer = renderer,
                    defaultMaterial = defaultMaterial,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false,
                    hideOnDeath = false,
                    ignoresMaterialOverrides = false
                };
            }
            return idrsPrefab;
        }

        public GameObject SetUpFollowerIDRS(float followerDampTime = 0.2f, float followerMaxSpeed = 30f, bool rotateX = true, float xRotationSpeed = 25f, bool rotateY = true, float yRotationSpeed = 15f, bool rotateZ = true, float zRotationSpeed = 10f)
        {
            var followerHolder = PrefabAPI.InstantiateClone(new GameObject(""), ItemName.Replace(" ", "") + "FollowerIDRS", false);
            var followerTransform = followerHolder.transform;

            followerTransform.localScale = Vector3.one;
            followerTransform.localEulerAngles = Vector3.zero;
            followerTransform.localPosition = Vector3.zero;

            var prefabForFollower = PrefabAPI.InstantiateClone(ItemModel, ItemName.Replace(" ", "") + "ForFollower", false);

            prefabForFollower.transform.SetParent(followerTransform);

            if (rotateX)
            {
                var rotateAroundX = prefabForFollower.AddComponent<RotateAroundAxis>();
                rotateAroundX.speed = RotateAroundAxis.Speed.Fast;
                rotateAroundX.slowRotationSpeed = xRotationSpeed;
                rotateAroundX.rotateAroundAxis = RotateAroundAxis.RotationAxis.X;
                rotateAroundX.relativeTo = Space.Self;
                rotateAroundX.reverse = false;
            }

            if (rotateY)
            {
                var rotateAroundY = prefabForFollower.AddComponent<RotateAroundAxis>();
                rotateAroundY.speed = RotateAroundAxis.Speed.Fast;
                rotateAroundY.slowRotationSpeed = yRotationSpeed;
                rotateAroundY.rotateAroundAxis = RotateAroundAxis.RotationAxis.Y;
                rotateAroundY.relativeTo = Space.Self;
                rotateAroundY.reverse = false;
            }

            if (rotateZ)
            {
                var rotateAroundZ = prefabForFollower.AddComponent<RotateAroundAxis>();
                rotateAroundZ.speed = RotateAroundAxis.Speed.Fast;
                rotateAroundZ.slowRotationSpeed = zRotationSpeed;
                rotateAroundZ.rotateAroundAxis = RotateAroundAxis.RotationAxis.Z;
                rotateAroundZ.relativeTo = Space.Self;
                rotateAroundZ.reverse = false;
            }

            var childLocator = followerHolder.AddComponent<ChildLocator>();
            foreach (Transform child in followerTransform.GetComponentsInChildren<Transform>())
            {
                childLocator.AddChild(child.name, child);
            }

            var idrsPrefab = PrefabAPI.InstantiateClone(new GameObject(""), ItemName.Replace(" ", "") + "IDRS", false);

            followerTransform.localScale = Vector3.one;
            followerTransform.localEulerAngles = Vector3.zero;
            followerTransform.localPosition = Vector3.zero;

            var itemFollower = idrsPrefab.AddComponent<ItemFollower>();
            itemFollower.followerPrefab = followerHolder;
            itemFollower.targetObject = idrsPrefab;
            itemFollower.followerCurve = null;
            itemFollower.followerLineRenderer = null;
            itemFollower.distanceDampTime = followerDampTime;
            itemFollower.distanceMaxSpeed = followerMaxSpeed;

            return idrsPrefab;
        }

        public int GetPlayerItemCountGlobal(ItemIndex itemIndex, bool requiresAlive, bool requiresConnected = true)
        {
            int totalItemCount = 0;
            var playerCharacterMasterControllerReadOnlyInstances = PlayerCharacterMasterController._instancesReadOnly;
            for (int i = 0; i < playerCharacterMasterControllerReadOnlyInstances.Count; i++)
            {
                var playerCharacterMasterController = playerCharacterMasterControllerReadOnlyInstances[i];
                var inventory = playerCharacterMasterController.GetComponent<Inventory>();
                if (inventory && (!requiresAlive || playerCharacterMasterController.body) && (!requiresConnected || playerCharacterMasterController.isConnected))
                {
                    totalItemCount += inventory.GetItemCount(itemIndex);
                }
            }
            return totalItemCount;
        }

        public string GetConfigName()
        {
            Main.ModLogger.LogError(ItemName);
            return ItemName;
        }

        public virtual object GetItemStatsDef()
        {
            return null;
        }
    }
}