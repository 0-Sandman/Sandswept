using HarmonyLib;
using LookingGlass.ItemStatsNameSpace;
using RoR2.Items;
using Sandswept.Items.Greens;
using Sandswept.Items.VoidGreens;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        public virtual Sprite ItemIconOverride { get; set; } = null;

        public ItemDef ItemDef;

        public UnlockableDef UnlockableDef;

        public virtual ItemDef ItemToCorrupt { get; set; } = null;

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
        private void LGWrapper() {
            ItemStatsDef def = GetItemStatsDef() as ItemStatsDef;

            if (def != null)
            {
                ItemCatalog.availability.CallWhenAvailable(() =>
                {
                    LookingGlass.ItemStatsNameSpace.ItemDefinitions.RegisterItemStatsDef(def, ItemDef.itemIndex);
                });
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

            LanguageAPI.Add("ITEM_SANDSWEPT_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_SANDSWEPT_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_SANDSWEPT_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_SANDSWEPT_" + ItemLangTokenName + "_LORE", ItemLore);

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

            if (ItemIconOverride != null)
            {
                ItemDef.pickupIconSprite = ItemIconOverride;
            }

            ItemAPI.Add(new CustomItem(ItemDef, CreateItemDisplayRules()));
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
            float min = 2f * mult;
            float max = 10f * mult;
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
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(ItemDef);
        }

        public int GetCount(CharacterMaster master)
        {
            if (!master || !master.inventory) { return 0; }

            return master.inventory.GetItemCount(ItemDef);
        }

        public int GetCountSpecific(CharacterBody body, ItemDef itemDef)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(itemDef);
        }

        public string d(float f)
        {
            return (f * 100f).ToString() + "%";
        }

        public string GetConfigName()
        {
            Main.ModLogger.LogError(ItemName);
            return ItemName;
        }
        public virtual object GetItemStatsDef() {
            return null;
        }
    }
}