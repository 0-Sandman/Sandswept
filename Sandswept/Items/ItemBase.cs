using System.Reflection;
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

        public virtual string UnlockName { get; }
        public virtual string UnlockDesc { get; }

        public abstract ItemTier Tier { get; }
        public abstract ItemTag[] ItemTags { get; }

        public abstract GameObject ItemModel { get; }
        public abstract Sprite ItemIcon { get; }

        public ItemDef ItemDef;

        public virtual bool CanRemove { get; } = true;

        public virtual string AchievementName { get; }
        public virtual string AchievementDesc { get; }
        public virtual Func<string> GetHowToUnlock => () => AchievementName + "\n<style=cStack>" + AchievementDesc + "</style>";
        public virtual Func<string> GetUnlocked => () => AchievementName + "\n<style=cStack>" + AchievementDesc + "</style>";

        public virtual bool nonstandardScaleModel { get; } = false;

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

        public UnlockableDef UnlockableDef;

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
        public virtual void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public virtual void CreateConfig(ConfigFile config)
        { }

        protected virtual void CreateLang()
        {
            LanguageAPI.Add("ITEM_SANDSWEPT_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_SANDSWEPT_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_SANDSWEPT_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_SANDSWEPT_" + ItemLangTokenName + "_LORE", ItemLore);
        }

        protected virtual void CreateUnlockLang()
        {
            if (AchievementName == null || AchievementDesc == null)
            {
                Main.ModLogger.LogError("tried adding unlockable strings to an item without them set");
            }
            LanguageAPI.Add("ACHIEVEMENT_ITEM_SANDSWEPT_" + ItemLangTokenName + "_NAME", AchievementName);
            LanguageAPI.Add("ACHIEVEMENT_ITEM_SANDSWEPT_" + ItemLangTokenName + "_DESCRIPTION", AchievementDesc);
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        protected void CreateItem()
        {
            var gyatt = Main.hifuSandswept.LoadAsset<Sprite>("texItemTemp.png");
            var sigma = Main.hifuSandswept.LoadAsset<GameObject>("TempHolder.prefab");

            ItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ItemDef.name = "ITEM_SANDSWEPT_" + ItemLangTokenName;
            ItemDef.nameToken = "ITEM_SANDSWEPT_" + ItemLangTokenName + "_NAME";
            ItemDef.pickupToken = "ITEM_SANDSWEPT_" + ItemLangTokenName + "_PICKUP";
            ItemDef.descriptionToken = "ITEM_SANDSWEPT_" + ItemLangTokenName + "_DESCRIPTION";
            ItemDef.loreToken = "ITEM_SANDSWEPT_" + ItemLangTokenName + "_LORE";
            ItemDef.pickupModelPrefab = ItemModel ?? sigma;
            ItemDef.pickupIconSprite = ItemIcon ?? gyatt;
            ItemDef.hidden = false;
            ItemDef.canRemove = CanRemove;
#pragma warning disable
            ItemDef.deprecatedTier = Tier;
#pragma warning enable
            ItemDef.requiredExpansion = Main.SandsweptExpansionDef;

            if (AchievementName != null)
            {
                ItemDef.unlockableDef = CreateUnlock();
            }

            if (ItemTags.Length > 0) { ItemDef.tags = ItemTags; }

            if (ItemModel != null)
            {
                CreateModelPanelParameters(ItemModel);
            }
            else
            {
                CreateModelPanelParameters(sigma);
            }

            ItemAPI.Add(new CustomItem(ItemDef, CreateItemDisplayRules()));
        }

        private void CreateModelPanelParameters(GameObject itemModel)
        {
            if (itemModel.GetComponent<ModelPanelParameters>() != null)
            {
                return;
            }

            var modelPanelParameters = itemModel.AddComponent<ModelPanelParameters>();
            var firstMesh = itemModel.transform.GetChild(0);
            modelPanelParameters.focusPointTransform = firstMesh;
            modelPanelParameters.cameraPositionTransform = firstMesh;
            modelPanelParameters.minDistance = nonstandardScaleModel ? 2f : 0.02f;
            modelPanelParameters.maxDistance = nonstandardScaleModel ? 10f : 0.1f;
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
    }
}