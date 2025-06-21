using BepInEx;
using R2API.Utils;
using Sandswept.Items;
using Sandswept.Artifacts;
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
using HarmonyLib;
using Sandswept.Enemies;
using RoR2.ExpansionManagement;
using Sandswept.Interactables;
using Sandswept.DoTs;
using Sandswept.Drones;
using MonoMod.Cil;
using Sandswept.Enemies.SwepSwepTheSandy;
using System.Runtime.CompilerServices;
using Sandswept.Survivors.Ranger;
using IL.RoR2.Items;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace Sandswept
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
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
    [BepInDependency(SkillsAPI.PluginGUID, SkillsAPI.PluginVersion)]
    [BepInDependency(ProcTypeAPI.PluginGUID, ProcTypeAPI.PluginVersion)]
    [BepInDependency(MoreStats.MoreStatsPlugin.guid, MoreStats.MoreStatsPlugin.version)]
    [BepInDependency("com.weliveinasociety.CustomEmotesAPI", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("droppod.lookingglass", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string ModGuid = "com.TeamSandswept.Sandswept";
        public const string ModName = "Sandswept";
        public const string ModVersion = "1.2.4";

        public static AssetBundle mainAssets;
        public static AssetBundle assets;
        public static AssetBundle prodAssets;
        public static AssetBundle hifuSandswept;
        public static AssetBundle sandsweptHIFU;
        public static AssetBundle dgoslingAssets;

        public static ModdedDamageType eclipseSelfDamage = ReserveDamageType();

        public static ExpansionDef SOTV;
        public static ExpansionDef SandsweptExpansionDef;

        public static List<ArtifactBase> Artifacts = new();
        public static List<ItemBase> AllItems = new();
        public static List<ItemBase> EnabledItems = new();
        public static List<EquipmentBase> AllEquipment = new();
        public static List<EquipmentBase> EnabledEquipment = new();
        public static List<BuffBase> Buffs = new();
        public static List<EliteEquipmentBase> EliteEquipments = new();
        public static List<UnlockableDef> Unlocks = new();
        public static List<GameObject> EffectPrefabs = new();

        public static Dictionary<BuffBase, bool> BuffStatusDictionary = new();

        public static BepInEx.Logging.ManualLogSource ModLogger;

        public static ConfigFile config;
        public static ConfigFile backupConfig;

        public static ConfigEntry<bool> enableLogging { get; set; }
        public ConfigEntry<bool> enableAutoConfig { get; private set; }
        public ConfigEntry<string> latestVersion { get; private set; }

        public static ConfigEntry<bool> cursedConfig { get; private set; }

        public static Main Instance;

        public static bool LookingGlassLoaded = false;

        private static string rangerBoneMapperName;

        private void Awake()
        {
            Instance = this;

            var stopwatch = Stopwatch.StartNew();

            LookingGlassLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("droppod.lookingglass");

            SOTV = Utils.Assets.ExpansionDef.DLC1;

            ModLogger = Logger;

            SetUpConfig();
            SetUpAssets();
            SetUpContent();
            SetUpHooks();

            ModLogger.LogDebug("#SANDSWEEP");
            ModLogger.LogDebug("Initialized mod in " + stopwatch.ElapsedMilliseconds + "ms");
        }

        public void SetUpConfig()
        {
            config = Config;
            backupConfig = new ConfigFile(BepInEx.Paths.ConfigPath + "\\com.TeamSandswept.Sandswept.Backup.cfg", true);
            backupConfig.Bind(": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :");
            enableAutoConfig = config.Bind("Config", "Enable Auto Config Sync", true, "Disabling this would stop Sandswept from syncing config whenever a new version is found.");
            bool _preVersioning = !((Dictionary<ConfigDefinition, string>)AccessTools.DeclaredPropertyGetter(typeof(ConfigFile), "OrphanedEntries").Invoke(config, null)).Keys.Any(x => x.Key == "Latest Version");
            latestVersion = config.Bind("Config", "Latest Version", ModVersion, "DO NOT CHANGE THIS");

            cursedConfig = config.Bind("Config", "Enable Cursed Config?", false, "just dumb shit");

            if (enableAutoConfig.Value && (_preVersioning || (latestVersion.Value != ModVersion)))
            {
                latestVersion.Value = ModVersion;
                ConfigManager.VersionChanged = true;
                ModLogger.LogInfo("Config Autosync Enabled.");
            }

            AutoRunCollector.HandleAutoRun();
            ConfigManager.HandleConfigAttributes(Assembly.GetExecutingAssembly(), Config);
        }

        public void SetUpAssets()
        {
            assets = LoadAssetBundle("sandsweptassets2"); // psudło assets
            prodAssets = LoadAssetBundle("sandsweep3"); // MFS I SAID MERGE INTO OTHER ASSETS // nuh uh :3c
            hifuSandswept = LoadAssetBundle("hifusandswept");
            sandsweptHIFU = LoadAssetBundle("sandswepthifu");
            dgoslingAssets = LoadAssetBundle("dgoslingstuff");

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Sandswept.sandsweptassets"))
            {
                mainAssets = AssetBundle.LoadFromStream(stream);
            }
            // sandman assets?

            SwapShaders(mainAssets);
            SwapShaders(assets);
            SwapShaders(prodAssets);
            SwapShaders(hifuSandswept);
            SwapShaders(sandsweptHIFU);
            SwapShaders(dgoslingAssets);
        }

        public void SetUpContent()
        {
            Survivors.SelfDamageHook.Init();
            Sandswept.Utils.Keywords.SetupKeywords();
            Decay.Init();
            GenerateExpansionDef();
            Survivors.Initialize.Init();
            DamageColourHelper.Init();
            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));

            foreach (var artifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                if (LoadArtifact(artifact, Artifacts))
                {
                    artifact.Init(Config);
                }
            }

            var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));

            foreach (var itemType in ItemTypes)
            {
                ItemBase item = (ItemBase)Activator.CreateInstance(itemType);
                AllItems.Add(item);
            }

            foreach (ItemBase item in AllItems)
            {
                if (LoadItem(item, new()))
                {
                    EnabledItems.Add(item);
                    item.Init();
                }
            }

            var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)));

            foreach (var equipmentType in EquipmentTypes)
            {
                EquipmentBase equipment = (EquipmentBase)Activator.CreateInstance(equipmentType);
                if (LoadEquipment(equipment, AllEquipment))
                {
                    EnabledEquipment.Add(equipment);
                    equipment.Init();
                }
            }

            var EliteEquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EliteEquipmentBase)));

            foreach (var eliteEquipmentType in EliteEquipmentTypes)
            {
                EliteEquipmentBase eliteEquipment = (EliteEquipmentBase)Activator.CreateInstance(eliteEquipmentType);
                if (LoadEliteEquipment(eliteEquipment, EliteEquipments))
                {
                    eliteEquipment.Init(Config);
                }
            }

            var BuffTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(BuffBase)));

            foreach (var buffType in BuffTypes)
            {
                BuffBase buff = (BuffBase)Activator.CreateInstance(buffType);
                if (LoadBuff(buff, Buffs))
                {
                    buff.Init();
                }
            }

            ScanTypes<SkillBase>((x) => x.Init());
            ScanTypes<SurvivorBase>((x) => x.Init());
            ScanTypes<EnemyBase>((x) => x.Create());
            ScanTypes<InteractableBase>((x) =>
            {
                if (LoadInteractable(x, new()))
                {
                    x.Init();
                }
            });
            ScanTypes<DroneBase>((x) => x.Initialize());
            ScanTypesNoInstance<EntityState>((x) =>
            {
                ContentAddition.AddEntityState(x, out _);
            });
            new ContentPacks().Initialize();

            CursedConfig.Init();
            SwepSwepTheSandy.Init();
            NetworkingAPI.RegisterMessageType<CallNetworkedMethod>();
        }

        public void SetUpHooks()
        {
            SandsweptTemporaryEffects.ApplyHooks();
            On.RoR2.RoR2Content.Init += OnWwiseInit;
            IL.EntityStates.Drone.DeathState.OnImpactServer += DroneDropFix;
            On.RoR2.Items.ContagiousItemManager.Init += CorruptItems;
        }

        [SystemInitializer(typeof(SurvivorCatalog))]
        public static void SetUpEmotes()
        {
            // ModLogger.LogError("set up emotes ran");
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.weliveinasociety.CustomEmotesAPI"))
            {
                // ModLogger.LogError("custom emotes api is loaded");
                AddRangerEmotes();
            }
        }

        public AssetBundle LoadAssetBundle(string assetBundleName)
        {
            var assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Instance.Info.Location), assetBundleName));
            return assetBundle;
        }

        private static SpawnCard GetDroneCard(string str)
        {
            switch (str)
            {
                case "SpawnCards/InteractableSpawnCard/iscBrokenInfernoDrone":
                    return Drones.Inferno.InfernoDrone.Instance.iscBroken;

                case "SpawnCards/InteractableSpawnCard/iscBrokenVoltaicDrone":
                    return Drones.Voltaic.VoltaicDrone.Instance.iscBroken;

                default:
                    return LegacyResourcesAPI.Load<SpawnCard>(str);
            }
        }

        private void OnWwiseInit(On.RoR2.RoR2Content.orig_Init orig)
        {
            orig();

            string path = typeof(Main).Assembly.Location.Replace("Sandswept.dll", "");
            AkSoundEngine.AddBasePath(path);

            AkSoundEngine.LoadBank("initsoundswept", out _);
            AkSoundEngine.LoadBank("soundswept", out _);
        }

        public void GenerateExpansionDef()
        {
            SandsweptExpansionDef = dgoslingAssets.LoadAsset<ExpansionDef>("SandSweptExpDef");
            SandsweptExpansionDef.nameToken.Add("Sandswept");
            SandsweptExpansionDef.descriptionToken.Add("Adds content from the 'Sandswept' expansion to the game <3. Have fun <3.");
            SandsweptExpansionDef.disabledIconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texUnlockIcon.png");
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

        internal static void ScanTypesNoInstance<T>(Action<Type> action)
        {
            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(T)));

            foreach (Type type in types)
            {
                action(type);
            }
        }

        public bool LoadArtifact(ArtifactBase artifact, List<ArtifactBase> artifactList)
        {
            var enabled = ArtifactBase.DefaultEnabledCallback(artifact);

            if (enabled)
            {
                artifactList.Add(artifact);
            }
            return enabled;
        }

        public bool LoadItem(ItemBase item, List<ItemBase> itemList)
        {
            var enabled = ItemBase.DefaultEnabledCallback(item);
            if (enabled)
            {
                itemList.Add(item);
            }
            return enabled;
        }

        public bool LoadEquipment(EquipmentBase equipment, List<EquipmentBase> equipmentList)
        {
            if (EquipmentBase.DefaultEnabledCallback(equipment))
            {
                equipmentList.Add(equipment);
                return true;
            }
            return false;
        }

        public bool LoadEliteEquipment(EliteEquipmentBase eliteEquipment, List<EliteEquipmentBase> eliteEquipmentList)
        {
            var enabled = EliteEquipmentBase.DefaultEnabledCallback(eliteEquipment);

            if (enabled)
            {
                eliteEquipmentList.Add(eliteEquipment);
                return true;
            }
            return false;
        }

        public bool LoadBuff(BuffBase buff, List<BuffBase> buffList)
        {
            BuffStatusDictionary.Add(buff, true);

            buffList.Add(buff);

            return true;
        }

        public bool LoadInteractable(InteractableBase interactable, List<InteractableBase> interactableList)
        {
            var enabled = InteractableBase.DefaultEnabledCallback(interactable);

            if (enabled)
            {
                interactableList.Add(interactable);
                return true;
            }
            return false;
        }

        public void SwapShaders(AssetBundle bundle)
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

        private void CorruptItems(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            // items get iterated over in reverse order using a STANDARD for loop (e.g. Whites, then Reds, then NoTier, etc)
            for (int i = AllItems.Count - 1; i >= 0; i--)
            {
                var itemBase = AllItems[i];
                var itemDef = itemBase.ItemDef;
                // Logger.LogError("itemDef is " + Language.GetString(itemDef.nameToken));
                var itemToCorrupt = itemBase.ItemToCorrupt;

                if (itemToCorrupt == null)
                {
                    continue;
                }

                // Logger.LogError("itemToCorrupt is " + Language.GetString(itemToCorrupt.nameToken));

                ItemDef.Pair transformation = new()
                {
                    itemDef2 = itemDef,
                    itemDef1 = itemToCorrupt
                };

                ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);
            }

            orig();
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddRangerEmotes()
        {
            var skele = Main.dgoslingAssets.LoadAsset<GameObject>("mdlRangerEmote");
            EmotesAPI.CustomEmotesAPI.ImportArmature(Ranger.instance.Body, skele);

            var boneMapper = skele.GetComponentInChildren<BoneMapper>();
            boneMapper.scale = 0.9f;

            rangerBoneMapperName = boneMapper.name;

            EmotesAPI.CustomEmotesAPI.animChanged += RangerEmotesFix;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void RangerEmotesFix(string newAnimation, BoneMapper mapper)
        {
            if (mapper.name == rangerBoneMapperName)
            {
                GameObject gun = mapper.transform.parent.Find("Gun").gameObject;
                if (newAnimation != "none")
                {
                    gun.SetActive(false);
                }
                else
                    gun.SetActive(true);
            }

        }

        private void DroneDropFix(ILContext il)
        {
            ILCursor c = new(il);
            c.TryGotoNext(MoveType.After, x => x.MatchStloc(1));
            c.Prev.Operand = typeof(Main).GetMethod(nameof(GetDroneCard), BindingFlags.NonPublic | BindingFlags.Static);
        }
    }
}