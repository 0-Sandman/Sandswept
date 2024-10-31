using BepInEx;
using R2API.Utils;
using Sandswept.Items;
using Sandswept.Artifact;
using Sandswept.Equipment;
using System.Linq;
using System.Reflection;
using Sandswept.Buffs;
using Sandswept.Survivors;
using System.Diagnostics;
using Sandswept.Elites;
using static R2API.DamageAPI;
using R2API.ContentManagement;
using R2API.Networking;
using Sandswept.Survivors.Ranger.VFX;
using Sandswept.Survivors.Ranger.Projectiles;
using Sandswept.Survivors.Ranger.Hooks;
using Sandswept.Survivors.Ranger.Crosshairs;

// using Sandswept.WIP_Content;
using Sandswept.Survivors.Ranger.Pod;
using HarmonyLib;
using Sandswept.Enemies;
using Sandswept.Elites.VFX;
using RoR2.ExpansionManagement;
using Sandswept.Interactables;
using Sandswept.DoTs;

// using Sandswept.Survivors.Ranger.ItemDisplays;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace Sandswept
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(Skins.PluginGUID, Skins.PluginVersion)]
    [BepInDependency(DotAPI.PluginGUID, DotAPI.PluginVersion)]
    [BepInDependency(ItemAPI.PluginGUID, ItemAPI.PluginVersion)]
    [BepInDependency(EliteAPI.PluginGUID, EliteAPI.PluginVersion)]
    [BepInDependency(DamageAPI.PluginGUID, DamageAPI.PluginVersion)]
    [BepInDependency(PrefabAPI.PluginGUID, PrefabAPI.PluginVersion)]
    [BepInDependency(LanguageAPI.PluginGUID, LanguageAPI.PluginVersion)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID, RecalculateStatsAPI.PluginVersion)]
    [BepInDependency(R2APIContentManager.PluginGUID, R2APIContentManager.PluginVersion)]
    [BepInDependency(NetworkingAPI.PluginGUID, NetworkingAPI.PluginVersion)]
    [BepInDependency(DirectorAPI.PluginGUID, DirectorAPI.PluginVersion)]
    [BepInDependency("com.weliveinasociety.CustomEmotesAPI", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string ModGuid = "com.TeamSandswept.Sandswept";
        public const string ModName = "Sandswept";
        public const string ModVer = "1.0.0";

        public static AssetBundle MainAssets;
        public static AssetBundle Assets;
        public static AssetBundle prodAssets;
        public static AssetBundle hifuSandswept;
        public static AssetBundle dgoslingAssets;

        public static ModdedDamageType HeatSelfDamage = ReserveDamageType();

        public static ExpansionDef SOTV;
        public static ExpansionDef SandsweptExpansionDef;

        public static Dictionary<string, string> ShaderLookup = new()
    {
        { "StubbedShader/deferred/hgstandard", "shaders/deferred/hgstandard" },
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

        public static ConfigFile config;
        public static ConfigFile backupConfig;

        public static ConfigEntry<bool> enableLogging { get; set; }
        public ConfigEntry<bool> enableAutoConfig { get; private set; }
        public ConfigEntry<string> latestVersion { get; private set; }

        public static Main Instance;

        private void Awake()
        {
            Instance = this;

            var stopwatch = Stopwatch.StartNew();

            SOTV = Utils.Assets.ExpansionDef.DLC1;

            ModLogger = Logger;

            config = Config;
            backupConfig = new ConfigFile(BepInEx.Paths.ConfigPath + "\\com.TeamSandswept.Sandswept.Backup.cfg", true);
            backupConfig.Bind(": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :");
            enableAutoConfig = config.Bind("Config", "Enable Auto Config Sync", true, "Disabling this would stop Sandswept from syncing config whenever a new version is found.");
            bool _preVersioning = !((Dictionary<ConfigDefinition, string>)AccessTools.DeclaredPropertyGetter(typeof(ConfigFile), "OrphanedEntries").Invoke(config, null)).Keys.Any(x => x.Key == "Latest Version");
            latestVersion = config.Bind("Config", "Latest Version", ModVer, "DO NOT CHANGE THIS");

            if (enableAutoConfig.Value && (_preVersioning || (latestVersion.Value != ModVer)))
            {
                latestVersion.Value = ModVer;
                ConfigManager.VersionChanged = true;
                ModLogger.LogInfo("Config Autosync Enabled.");
            }

            Assets = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("Sandswept.dll", "sandsweptassets2"));
            prodAssets = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("Sandswept.dll", "sandsweep3")); // MFS I SAID MERGE INTO OTHER ASSETS // nuh uh :3c
            hifuSandswept = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("Sandswept.dll", "hifusandswept"));
            dgoslingAssets = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("Sandswept.dll", "dgoslingstuff"));

            Decay.Init();

            GenerateExpensionDef();

            AutoRunCollector.HandleAutoRun();
            ConfigManager.HandleConfigAttributes(Assembly.GetExecutingAssembly(), Config);

            // config doesnt work pseudopulse ! ! nre @ L63 utils/config.cs
            // now it does, explode

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Sandswept.sandsweptassets"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }

            Survivors.Initialize.Init();

            if (Utils.CustomEmoteAPICheck.enabled)
            {
                On.RoR2.SurvivorCatalog.Init += CustomEmoteAPICheck.SurvivorCatalog_Init;
            }

            SwapAllShaders(MainAssets);
            // SwapAllShaders(Assets);

            SwapAllShaders(prodAssets);
            SwapAllShaders(hifuSandswept);
            SwapAllShaders(dgoslingAssets);
            //Material matMushLun = dgoslingAssets.LoadAsset<Material>("matLunarMInd.mat");
            //matMushLun.shader = LegacyShaderAPI.Find("Hopoo Games/FX/Cloud Intersection Remap");
            DamageColourHelper.Init();

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
            ScanTypes<EnemyBase>((x) => x.Create());
            ScanTypes<InteractableBase>((x) => x.Init());

            new ContentPacks().Initialize();

            ModLogger.LogDebug("#SANDSWEEP");
            ModLogger.LogDebug("Initialized mod in " + stopwatch.ElapsedMilliseconds + "ms");

            // On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { }; // for having multiple instances of the game at once - mp testing, make sure to comment out before release
        }

        public void GenerateExpensionDef()
        {
            SandsweptExpansionDef = dgoslingAssets.LoadAsset<ExpansionDef>("SandSweptExpDef");
            SandsweptExpansionDef.nameToken.Add("Sandswept");
            SandsweptExpansionDef.descriptionToken.Add("Adds content from the 'Sandswept' expansion to the game <3. Have fun <3.");
            SandsweptExpansionDef.disabledIconSprite = Utils.Assets.Sprite.texUnlockIconSprite;
            ContentAddition.AddExpansionDef(SandsweptExpansionDef);
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
            var enabled = ArtifactBase.DefaultEnabledCallback(artifact);

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
            var enabled = ItemBase.DefaultEnabledCallback(item);
            if (enabled)
            {
                itemList.Add(item);
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
            if (EquipmentBase.DefaultEnabledCallback(equipment))
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
            var enabled = EliteEquipmentBase.DefaultEnabledCallback(eliteEquipment);

            if (enabled)
            {
                eliteEquipmentList.Add(eliteEquipment);
                return true;
            }
            return false;
        }

        public bool ValidateBuff(BuffBase buff, List<BuffBase> buffList)
        {
            BuffStatusDictionary.Add(buff, true);

            buffList.Add(buff);

            return true;
        }

        public bool ValidateInteractable(InteractableBase interactable, List<InteractableBase> interactableList)
        {
            var enabled = InteractableBase.DefaultEnabledCallback(interactable);

            if (enabled)
            {
                interactableList.Add(interactable);
                return true;
            }
            return false;
        }

        public void SwapAllShaders(AssetBundle bundle)
        {
            Material[] array = bundle.LoadAllAssets<Material>();
            foreach (Material val in array)
            {
                switch (val.shader.name)
                {
                    case "Hopoo Games/FX/Cloud Remap":
                        val.shader = Utils.Assets.Shader.HGCloudRemap;
                        break;

                    case "Stubbed Hopoo Games/Deferred/Standard":
                        val.shader = Resources.Load<Shader>("shaders/deferred/hgstandard");
                        break;

                    case "StubbedShader/deferred/hgstandard":
                        val.shader = Utils.Assets.Shader.HGStandard;
                        break;

                    case "StubbedShader/fx/hgintersectioncloudremap":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgintersectioncloudremap");
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

                    case "StubbedRoR2/Base/Shaders/HGStandard":
                        val.shader = LegacyShaderAPI.Find("Hopoo Games/Deferred/Standard");
                        break;

                    case "StubbedRoR2/Base/Shaders/HGCloudRemap":
                        val.shader = Utils.Assets.Shader.HGCloudRemap;
                        break;

                    case "StubbedRoR2/Base/Shaders/HGIntersectionCloudRemap":
                        val.shader = LegacyShaderAPI.Find("Hopoo Games/FX/Cloud Intersection Remap");
                        break;
                }
            }
        }
    }
}