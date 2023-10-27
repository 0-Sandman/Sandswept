using BepInEx;
using R2API.Utils;
using Sandswept.Items;
using Sandswept.Artifact;
using Sandswept.Equipment;
using System.Linq;
using System.Reflection;
using Sandswept.Buffs;
using Sandswept.Skills;
using Sandswept.Survivors;
using System.Diagnostics;
using Sandswept.Elites;
using Sandswept.Skills.Ranger.VFX;
using MonoMod.Cil;

namespace Sandswept
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(DotAPI.PluginGUID, DotAPI.PluginVersion)]
    [BepInDependency(ItemAPI.PluginGUID, ItemAPI.PluginVersion)]
    [BepInDependency(EliteAPI.PluginGUID, EliteAPI.PluginVersion)]
    [BepInDependency(DamageAPI.PluginGUID, DamageAPI.PluginVersion)]
    [BepInDependency(PrefabAPI.PluginGUID, PrefabAPI.PluginVersion)]
    [BepInDependency(LanguageAPI.PluginGUID, LanguageAPI.PluginVersion)]
    [BepInDependency(PluginGUID, PluginVersion)]
    [BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID, R2API.Networking.NetworkingAPI.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string ModGuid = "com.Sandman.Sandswept";
        public const string ModName = "Sandswept";
        public const string ModVer = "0.0.1";

        public static AssetBundle MainAssets;
        public static AssetBundle Assets;
        public static AssetBundle Asset2s;
        public static AssetBundle hifuSandswept;

        public static Dictionary<string, string> ShaderLookup = new()
    {
        { "stubbed hopoo games/deferred/hgstandard", "shaders/deferred/hgstandard" },
        { "stubbed hopoo games/fx/hgcloud intersection remap", "shaders/fx/hgintersectioncloudremap" },
        { "stubbed hopoo games/fx/hgcloud remap", "shaders/fx/hgcloudremap" },
        { "stubbed hopoo games/fx/hgdistortion", "shaders/fx/hgdistortion" },
        { "stubbed hopoo games/deferred/hgsnow topped", "shaders/deferred/hgsnowtopped" },
        { "stubbed hopoo games/fx/hgsolid parallax", "shaders/fx/hgsolidparallax" }
    };

        public List<ArtifactBase> Artifacts = new();
        public List<ItemBase> Items = new();
        public List<EquipmentBase> Equipments = new();
        public List<BuffBase> Buffs = new();
        public List<EliteEquipmentBase> EliteEquipments = new();
        public static List<UnlockableDef> Unlocks = new();
        public static List<GameObject> EffectPrefabs = new();

        public static Dictionary<BuffBase, bool> BuffStatusDictionary = new();

        //public static List<Material> SwappedMaterials = new List<Material>();

        //Provides a direct access to this plugin's logger for use in any of your other classes.
        public static BepInEx.Logging.ManualLogSource ModLogger;

        private void Awake()
        {
            var stopwatch = Stopwatch.StartNew();

            ModLogger = Logger;

            ReleaseVFX.Init();
            DirectCurrentVFX.Init();
            SidestepVFX.Init();

            AutoRunCollector.HandleAutoRun();
            ConfigManager.HandleConfigAttributes(Assembly.GetExecutingAssembly(), Config);

            // Don't know how to create/use an asset bundle, or don't have a unity project set up?
            // Look here for info on how to set these up: https://github.com/KomradeSpectre/AetheriumMod/blob/rewrite-master/Tutorials/Item%20Mod%20Creation.md#unity-project
            // (This is a bit old now, but the information on setting the unity asset bundle should be the same.)

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Sandswept.sandsweptassets"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }

            Assets = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("Sandswept.dll", "sandsweptassets2"));
            Asset2s = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("Sandswept.dll", "sandsweep3")); // temporary assetbundle bc i didnt have the other two unity projects, please merge into the other assets
            hifuSandswept = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("Sandswept.dll", "hifusandswept"));

            SwapAllShaders(MainAssets);
            SwapAllShaders(Assets);
            SwapAllShaders(Asset2s);
            SwapAllShaders(hifuSandswept);
            DamageColourHelper.Init();
            //On.RoR2.GlobalEventManager.OnHitEnemy += GenericBodyTokenAddition;

            //This section automatically scans the project for all artifacts
            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));

            foreach (var artifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                if (ValidateArtifact(artifact, Artifacts))
                {
                    artifact.Init(Config);
                }
            }

            //This section automatically scans the project for all items
            var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));

            foreach (var itemType in ItemTypes)
            {
                ItemBase item = (ItemBase)Activator.CreateInstance(itemType);
                if (ValidateItem(item, Items))
                {
                    item.Init(Config);
                }
            }

            //this section automatically scans the project for all equipment
            var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)));

            foreach (var equipmentType in EquipmentTypes)
            {
                EquipmentBase equipment = (EquipmentBase)Activator.CreateInstance(equipmentType);
                if (ValidateEquipment(equipment, Equipments))
                {
                    equipment.Init(Config);
                }
            }

            //this section automatically scans the project for all elite equipment
            var EliteEquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EliteEquipmentBase)));

            foreach (var eliteEquipmentType in EliteEquipmentTypes)
            {
                EliteEquipmentBase eliteEquipment = (EliteEquipmentBase)Activator.CreateInstance(eliteEquipmentType);
                if (ValidateEliteEquipment(eliteEquipment, EliteEquipments))
                {
                    eliteEquipment.Init(Config);
                }
            }

            var BuffTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(BuffBase)));

            foreach (var buffType in BuffTypes)
            {
                BuffBase buff = (BuffBase)Activator.CreateInstance(buffType);
                if (ValidateBuff(buff, Buffs))
                {
                    buff.Init();
                }
            }

            ScanTypes<SkillBase>((x) => x.Init());
            ScanTypes<SurvivorBase>((x) => x.Init());

            new ContentPacks().Initialize();

            ModLogger.LogDebug("#SANDSWEEP");
            ModLogger.LogDebug("Initialized mod in " + stopwatch.ElapsedMilliseconds + "ms");
        }

        internal static void ScanTypes<T>(Action<T> action)
        {
            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(T)));

            foreach (Type type in types)
            {
                T instance = (T)Activator.CreateInstance(type);
                action(instance);
            }
        }

        /// <summary>
        /// A helper to easily set up and initialize an artifact from your artifact classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="artifact">A new instance of an ArtifactBase class."</param>
        /// <param name="artifactList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateArtifact(ArtifactBase artifact, List<ArtifactBase> artifactList)
        {
            var enabled = Config.Bind<bool>("Artifact: " + artifact.ArtifactName, "Enable Artifact?", true, "Should this artifact appear for selection?").Value;

            if (enabled)
            {
                artifactList.Add(artifact);
            }
            return enabled;
        }

        /// <summary>
        /// A helper to easily set up and initialize an item from your item classes if the user has it enabled in their configuration files.
        /// <para>Additionally, it generates a configuration for each item to allow blacklisting it from AI.</para>
        /// </summary>
        /// <param name="item">A new instance of an ItemBase class."</param>
        /// <param name="itemList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateItem(ItemBase item, List<ItemBase> itemList)
        {
            var enabled = Config.Bind<bool>("Item: " + item.ItemName, "Enable Item?", true, "Should this item appear in runs?").Value;
            var aiBlacklist = Config.Bind<bool>("Item: " + item.ItemName, "Blacklist Item from AI Use?", false, "Should the AI not be able to obtain this item?").Value;
            if (enabled)
            {
                itemList.Add(item);
                if (aiBlacklist)
                {
                    item.AIBlacklisted = true;
                }
            }
            return enabled;
        }

        /// <summary>
        /// A helper to easily set up and initialize an equipment from your equipment classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="equipment">A new instance of an EquipmentBase class."</param>
        /// <param name="equipmentList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateEquipment(EquipmentBase equipment, List<EquipmentBase> equipmentList)
        {
            if (Config.Bind<bool>("Equipment: " + equipment.EquipmentName, "Enable Equipment?", true, "Should this equipment appear in runs?").Value)
            {
                equipmentList.Add(equipment);
                return true;
            }
            return false;
        }

        /// <summary>
        /// A helper to easily set up and initialize an elite equipment from your elite equipment classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="eliteEquipment">A new instance of an EliteEquipmentBase class.</param>
        /// <param name="eliteEquipmentList">The list you would like to add this to if it passes the config check.</param>
        /// <returns></returns>
        public bool ValidateEliteEquipment(EliteEquipmentBase eliteEquipment, List<EliteEquipmentBase> eliteEquipmentList)
        {
            var enabled = Config.Bind<bool>("Equipment: " + eliteEquipment.EliteEquipmentName, "Enable Elite Equipment?", true, "Should this elite equipment appear in runs? If disabled, the associated elite will not appear in runs either.").Value;

            if (enabled)
            {
                eliteEquipmentList.Add(eliteEquipment);
                return true;
            }
            return false;
        }

        public bool ValidateBuff(BuffBase buff, List<BuffBase> buffList)
        {
            var enabled = Config.Bind<bool>("Buff: " + buff.BuffName, "Enable Buff?", true, "Should this buff be registered for use in the game?").Value;

            BuffStatusDictionary.Add(buff, enabled);

            if (enabled)
            {
                buffList.Add(buff);
            }
            return enabled;
        }

        public void SwapAllShaders(AssetBundle bundle)
        {
            Material[] array = bundle.LoadAllAssets<Material>();
            foreach (Material val in array)
            {
                switch (val.shader.name)
                {
                    case "Stubbed Hopoo Games/Deferred/Standard":
                        val.shader = Resources.Load<Shader>("shaders/deferred/hgstandard");
                        break;

                    case "StubbedShader/deferred/hgstandard":
                        val.shader = Resources.Load<Shader>("shaders/deferred/hgstandard");
                        break;

                    case "Stubbed Hopoo Games/Deferred/Snow Topped":
                        val.shader = Resources.Load<Shader>("shaders/deferred/hgsnowtopped");
                        break;

                    case "Stubbed Hopoo Games/FX/Cloud Remap":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgcloudremap");
                        break;

                    case "Stubbed Hopoo Games/FX/Cloud Intersection Remap":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgintersectioncloudremap");
                        break;

                    case "Stubbed Hopoo Games/FX/Opaque Cloud Remap":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgopaquecloudremap");
                        break;

                    case "Stubbed Hopoo Games/FX/Distortion":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgdistortion");
                        break;

                    case "Stubbed Hopoo Games/FX/Solid Parallax":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgsolidparallax");
                        break;

                    case "Stubbed Hopoo Games/Environment/Distant Water":
                        val.shader = Resources.Load<Shader>("shaders/environment/hgdistantwater");
                        break;
                }
            }
        }
    }
}