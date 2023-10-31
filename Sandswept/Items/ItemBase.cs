using static Sandswept.Utils.TotallyNotStolenUtils;

namespace Sandswept.Items
{
    // The directly below is entirely from TILER2 API (by ThinkInvis) specifically the Item module. Utilized to implement instancing for classes.
    // TILER2 API can be found at the following places:
    // https://github.com/ThinkInvis/RoR2-TILER2
    // https://thunderstore.io/package/ThinkInvis/TILER2/

    public abstract class ItemBase<T> : ItemBase where T : ItemBase<T>
    {
        //This, which you will see on all the -base classes, will allow both you and other modders to enter through any class with this to access internal fields/properties/etc as if they were a member inheriting this -Base too from this class.
        public static T instance { get; private set; }

        public ItemBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class ItemBase : IConfigurable
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }

        public virtual string UnlockName { get; }
        public virtual string UnlockDesc { get; }

        public abstract ItemTier Tier { get; }
        public virtual ItemTag[] ItemTags { get; set; } = new ItemTag[] { };

        public abstract GameObject ItemModel { get; }
        public abstract Sprite ItemIcon { get; }

        public ItemDef ItemDef;

        public virtual bool CanRemove { get; } = true;
        public virtual bool AIBlacklisted { get; set; } = false;

        public virtual string AchievementName { get; }
        public virtual string AchievementDesc { get; }
        public virtual Func<string> GetHowToUnlock => () => AchievementName + "\n<style=cStack>" + AchievementDesc + "</style>";
        public virtual Func<string> GetUnlocked => () => AchievementName + "\n<style=cStack>" + AchievementDesc + "</style>";

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
            LanguageAPI.Add("ACHIEVEMENT_SANDSWEPT_" + ItemLangTokenName + "_NAME", AchievementName);
            LanguageAPI.Add("ACHIEVEMENT_SANDSWEPT_" + ItemLangTokenName + "_DESCRIPTION", AchievementDesc);
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        protected void CreateItem()
        {
            if (AIBlacklisted)
            {
                ItemTags = new List<ItemTag>(ItemTags) { ItemTag.AIBlacklist }.ToArray();
            }

            ItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ItemDef.name = "ITEM_SANDSWEPT_" + ItemLangTokenName;
            ItemDef.nameToken = "ITEM_SANDSWEPT_" + ItemLangTokenName + "_NAME";
            ItemDef.pickupToken = "ITEM_SANDSWEPT_" + ItemLangTokenName + "_PICKUP";
            ItemDef.descriptionToken = "ITEM_SANDSWEPT_" + ItemLangTokenName + "_DESCRIPTION";
            ItemDef.loreToken = "ITEM_SANDSWEPT_" + ItemLangTokenName + "_LORE";
            ItemDef.pickupModelPrefab = ItemModel;
            ItemDef.pickupIconSprite = ItemIcon;
            ItemDef.hidden = false;
            ItemDef.canRemove = CanRemove;
#pragma warning disable
            ItemDef.deprecatedTier = Tier;
#pragma warning enable

            if (AchievementName != null)
            {
                ItemDef.unlockableDef = CreateUnlock();
            }

            if (ItemTags.Length > 0) { ItemDef.tags = ItemTags; }

            ItemAPI.Add(new CustomItem(ItemDef, CreateItemDisplayRules()));
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