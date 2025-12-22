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
using System.Collections;
using Sandswept.Utils.Components;
using Rewired.ComponentControls.Effects;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using ProcSolver;
using Rebindables;
using RoR2.UI;
using Sandswept.Survivors.Electrician;
using R2API.ScriptableObjects;
using HG;

// using Sandswept.Mechanics;

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
    // [BepInDependency(MoreStats.MoreStatsPlugin.guid, MoreStats.MoreStatsPlugin.version)]
    [BepInDependency("com.weliveinasociety.CustomEmotesAPI", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("droppod.lookingglass", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.RiskOfBrainrot.ProcSolver", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Gorakh.AttackDirectionFix", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(Rebindables.Rebindables.PluginGUID)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string ModGuid = "com.TeamSandswept.Sandswept";
        public const string ModName = "Sandswept";
        public const string ModVersion = "1.4.0";

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
        public static ConfigEntry<bool> enableUnfinishedContent { get; private set; }

        public static Main Instance;

        public static bool LookingGlassLoaded = false;
        public static bool ProcSolverLoaded = false;
        public static bool AttackDirectionFixLoaded = false;
        private static string rangerBoneMapperName;
        public static MPInput input;

        public static event Action onInputAvailable;

        internal static List<DroneDef> droneDefs = new();

        private void Awake()
        {
            Instance = this;
            var stopwatch = Stopwatch.StartNew();

            On.RoR2.DroneCatalog.SetDroneDefs += OnSetDroneDefs;

            LookingGlassLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("droppod.lookingglass");
            ProcSolverLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.RiskOfBrainrot.ProcSolver");
            AttackDirectionFixLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("Gorakh.AttackDirectionFix");

            SOTV = Utils.Assets.ExpansionDef.DLC1;

            ModLogger = Logger;

            NetworkingUtils.RegisterMessages();

            SetUpConfig();
            SetUpAssets();
            SetUpContent();
            SetUpHooks();

            ModLogger.LogDebug("#SANDSWEEP");
            ModLogger.LogDebug("Initialized mod in " + stopwatch.ElapsedMilliseconds + "ms");

            RoR2Application.onLoad += () =>
            {
                input = GameObject.Find("MPEventSystem Player0").GetComponent<RoR2.UI.MPInput>();
                onInputAvailable?.Invoke();
            };

            // NewtReflection.Initialize();
        }

        private void OnSetDroneDefs(On.RoR2.DroneCatalog.orig_SetDroneDefs orig, DroneDef[] newDroneDefs)
        {
            foreach (DroneDef def in droneDefs)
            {
                ArrayUtils.ArrayAppend(ref newDroneDefs, def);
            }
            orig(newDroneDefs);
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
            enableUnfinishedContent = config.Bind("Config", "Enable Unfinished Content?", false, "Enables some disabled content that (supposedly) works but is missing assets.");

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
            Decay.Init();
            GenerateExpansionDef();
            DamageColourHelper.Init();
            Enemies.SpawnAndDeath.Init();
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
                    eliteEquipment.Init();
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
            ScanTypes<SurvivorBase>((x) => x.Initialize());

            if (SurvivorBase.DefaultEnabledCallback(Ranger.instance) || SurvivorBase.DefaultEnabledCallback(Electrician.instance))
            {
                Survivors.SelfDamageHook.Init();
            }

            if (SurvivorBase.DefaultEnabledCallback(Ranger.instance))
            {
                Sandswept.Utils.Keywords.SetupKeywords();
            }

            Survivors.Initialize.Init();

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

            CorruptItems();

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
            On.RoR2.SurvivorCatalog.Init += OnSurvivorCatalogFinished;
        }

        private void OnSurvivorCatalogFinished(On.RoR2.SurvivorCatalog.orig_Init orig)
        {
            orig();
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.weliveinasociety.CustomEmotesAPI") && SurvivorBase.DefaultEnabledCallback(Ranger.instance))
            {
                // ModLogger.LogError("custom emotes api is loaded");
                AddRangerEmotes();
            }
        }

        /*
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
        */

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
                        val.shader = Utils.Assets.Shader.HopooGamesFXCloudRemap;
                        break;

                    case "Stubbed Hopoo Games/Deferred/Standard":
                        val.shader = Resources.Load<Shader>("shaders/deferred/hgstandard");
                        break;

                    case "StubbedShader/deferred/hgstandard":
                        val.shader = Utils.Assets.Shader.HopooGamesDeferredStandard;
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
                        val.shader = Utils.Assets.Shader.HopooGamesFXCloudRemap;
                        break;

                    case "StubbedRoR2/Base/Shaders/HGIntersectionCloudRemap":
                        val.shader = LegacyShaderAPI.Find("Hopoo Games/FX/Cloud Intersection Remap");
                        break;

                    case "StubbedDecalicious/Decalicious/DeferredDecal":
                        val.shader = Utils.Assets.Shader.DecaliciousDeferredDecal;
                        break;
                }
            }
        }

        private void CorruptItems()
        {
            var itemRelationshipProvider = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
            itemRelationshipProvider.name = "SANDSWEPT_VOID_ITEM_RELATIONSHIP_PROVIDER";
            itemRelationshipProvider.relationshipType = Paths.ItemRelationshipType.ContagiousItem;

            List<ItemDef.Pair> itemRelationships = new();

            for (int i = EnabledItems.Count - 1; i >= 0; i--)
            {
                var itemBase = AllItems[i];
                var itemDef = itemBase.ItemDef;
                // Logger.LogError("itemDef is " + Language.GetString(itemDef.nameToken));
                var itemToCorrupt = itemBase.ItemToCorrupt;

                if (itemToCorrupt == null || !ItemBase.DefaultEnabledCallback(itemBase))
                {
                    continue;
                }

                itemDef.unlockableDef = itemToCorrupt.unlockableDef;

                // Logger.LogError("itemToCorrupt is " + Language.GetString(itemToCorrupt.nameToken));

                ItemDef.Pair transformation = new()
                {
                    itemDef2 = itemDef,
                    itemDef1 = itemToCorrupt
                };

                itemRelationships.Add(transformation);
            }

            itemRelationshipProvider.relationships = itemRelationships.ToArray();

            ContentAddition.AddItemRelationshipProvider(itemRelationshipProvider);
        }

        /*
        private void CreateRecipes()
        {
            // no ContentAddition.AddCraftableDef() yet
        }

        private bool ValidateAndGetRecipeIngredients(ItemDef itemDefIn, EquipmentDef equipmentDefIn, out ItemDef itemDef, out EquipmentDef equipmentDef)
        {
            itemDef = null;
            equipmentDef = null;
            var itemIndex = ItemCatalog.FindItemIndex();
            var equipmentIndex = EquipmentCatalog.FindEquipmentIndex();

            if (itemIndex != (ItemIndex)(-1))
            {
                itemDef = ItemCatalog.GetItemDef(itemIndex);
                return true;
            }

            if (equipmentIndex != (EquipmentIndex)(-1))
            {
                equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
                return true;
            }

            return false;
        }
        */

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
                {
                    gun.SetActive(true);
                }
            }
        }

        private void DroneDropFix(ILContext il)
        {
            ILCursor c = new(il);
            c.TryGotoNext(MoveType.After, x => x.MatchStloc(1));
            c.Prev.Operand = typeof(Main).GetMethod(nameof(GetDroneCard), BindingFlags.NonPublic | BindingFlags.Static);
        }

        // all DoTs without a proc coefficient => don't use these methods, only multiply by damageInfo.procCoefficient
        // total damage procs => use GetProcRateForTotalDamageProc()
        // base damage procs => use GetProcRateForBaseDamageProc() * damageInfo.procCoefficient
        // amalgamations like sun fragment or millenium? WHO KNOWS :SOB:

        public static float GetProcRateForTotalDamageProc(DamageInfo damageInfo)
        {
            if (!ProcSolverLoaded)
            {
                return damageInfo.procCoefficient;
            }
            return _GetProcRate(damageInfo);
        }

        public static float GetProcRateForBaseDamageProc(DamageInfo damageInfo)
        {
            if (!ProcSolverLoaded)
            {
                return 1f;
            }
            return _GetProcRate(damageInfo);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static float _GetProcRate(DamageInfo damageInfo)
        {
            return ProcSolverPlugin.GetProcRateMod(damageInfo);
        }

        public static GameObject tempestSphereDefault;
        public static GameObject tempestSphereCovenant;
        public static bool ran = true;

        public void FixedUpdate()
        {
            if (ran)
            {
                return;
            }

            StartCoroutine(CreateDefaultPrefab());
            StartCoroutine(CreateCovenantPrefab());
            ran = true;
        }

        public IEnumerator CreateDefaultPrefab()
        {
            var beamStartColor = new Color32(0, 77, 255, 255);
            var beamEndColor = new Color32(255, 191, 0, 255);
            var sphereFillColor = new Color32(255, 165, 0, 255);
            var sphereOutlineColor = new Color32(158, 93, 0, 255);
            var smallSphereFillColor = new Color32(0, 42, 255, 255);

            tempestSphereDefault = PrefabAPI.InstantiateClone(Main.assets.LoadAsset<GameObject>("TempestSphereProjectile.prefab"), "Tempest Sphere Projectile " + "Default");

            ContentAddition.AddProjectile(tempestSphereDefault);

            GameObject sphereVFX = new("Tempest Sphere VFX Holder Default");
            sphereVFX.transform.position = Vector3.zero;
            sphereVFX.transform.localPosition = Vector3.zero;

            var tempestSphereIndicator = PrefabAPI.InstantiateClone(Paths.GameObject.VagrantNovaAreaIndicator, "Tempest Sphere Indicator Default", false);
            // tempestSphereIndicator.GetComponentInChildren<ParticleSystemRenderer>().gameObject.SetActive(false);
            tempestSphereIndicator.transform.Find("Particle System").gameObject.SetActive(false);

            var pointLight = tempestSphereIndicator.transform.Find("Point Light").GetComponent<Light>();
            pointLight.color = sphereOutlineColor;
            pointLight.intensity = 100f;
            pointLight.GetComponent<LightIntensityCurve>().enabled = false;
            pointLight.GetComponent<LightScaleFromParent>().enabled = false;
            pointLight.range = 20f;

            var newSphereFillMaterial = new Material(Paths.Material.matWarbannerSphereIndicator);
            newSphereFillMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newSphereFillMaterial.SetColor("_TintColor", sphereFillColor);
            newSphereFillMaterial.SetFloat("_InvFade", 30f);
            newSphereFillMaterial.SetFloat("_SoftPower", 0.1f);
            newSphereFillMaterial.SetFloat("_Boost", 3f);
            newSphereFillMaterial.SetFloat("_RimPower", 17f);
            newSphereFillMaterial.SetFloat("_RimStrength", 5f);
            newSphereFillMaterial.SetFloat("_AlphaBoost", 20f);
            newSphereFillMaterial.SetFloat("_IntersectionStrength", 1.223287f);

            var newSphereOutlineMaterial = new Material(Paths.Material.matLightningSphere);
            newSphereOutlineMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone3);
            newSphereOutlineMaterial.SetTexture("_Cloud1Tex", Paths.Texture2D.texCloudLightning1);
            newSphereOutlineMaterial.SetTexture("_Cloud2Tex", null);
            newSphereOutlineMaterial.SetColor("_TintColor", sphereOutlineColor);
            newSphereOutlineMaterial.SetFloat("_InvFade", 3f);
            newSphereOutlineMaterial.SetFloat("_SoftPower", 0.85f);
            newSphereOutlineMaterial.SetFloat("_Boost", 1.5f);
            newSphereOutlineMaterial.SetFloat("_RimPower", 0.1f);
            newSphereOutlineMaterial.SetFloat("_RimStrength", 0.052f);
            newSphereOutlineMaterial.SetFloat("_AlphaBoost", 1f);
            newSphereOutlineMaterial.SetFloat("_IntersectionStrength", 6.4f);

            var newTempestSphereMaterials = new Material[2] { newSphereFillMaterial, newSphereOutlineMaterial };

            tempestSphereIndicator.GetComponent<MeshRenderer>().sharedMaterials = newTempestSphereMaterials;

            tempestSphereIndicator.RemoveComponent<ObjectScaleCurve>();
            // tempestSphereIndicator.GetComponent<ObjectScaleCurve>().enabled = false;

            yield return new WaitForSeconds(0.1f);
            tempestSphereIndicator.transform.localScale = new(14f, 14f, 14f);
            tempestSphereIndicator.RemoveComponent<AnimateShaderAlpha>();
            // tempestSphereIndicator.GetComponent<AnimateShaderAlpha>().enabled = false;

            var newOrbCoreOutlineMaterial = new Material(Paths.Material.matLoaderLightningTile);
            newOrbCoreOutlineMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone3);
            newOrbCoreOutlineMaterial.SetColor("_TintColor", sphereFillColor);
            newOrbCoreOutlineMaterial.SetFloat("_InvFade", 1.3f);
            newOrbCoreOutlineMaterial.SetFloat("_Boost", 4f);
            newOrbCoreOutlineMaterial.SetFloat("_AlphaBoost", 3.67f);
            newOrbCoreOutlineMaterial.SetFloat("_AlphaBias", 0f);

            var newOrbCoreFillMaterial = new Material(Paths.Material.matJellyfishLightningSphere);
            newOrbCoreFillMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone3);
            newOrbCoreFillMaterial.SetColor("_TintColor", smallSphereFillColor);
            newOrbCoreFillMaterial.SetFloat("_InvFade", 0.8f);
            newOrbCoreFillMaterial.SetFloat("_SoftPower", 1f);
            newOrbCoreFillMaterial.SetFloat("_Boost", 1.86f);
            newOrbCoreFillMaterial.SetFloat("_RimPower", 1.5f);
            newOrbCoreFillMaterial.SetFloat("_RimStrength", 0.7f);
            newOrbCoreFillMaterial.SetFloat("_AlphaBoost", 6f);
            newOrbCoreFillMaterial.SetFloat("_AlphaBias", 2f);

            var newOrbCoreMaterials = new Material[2] { newOrbCoreOutlineMaterial, newOrbCoreFillMaterial };

            GameObject tempestOrb = PrefabAPI.InstantiateClone(Paths.GameObject.VoidSurvivorChargeMegaBlaster, "Tempest Sphere Orb Default", false);

            tempestOrb.transform.Find("Base").gameObject.SetActive(false);
            tempestOrb.transform.Find("Base (1)").gameObject.SetActive(false);
            var tempestOrbPointLight = tempestOrb.transform.Find("Point light");
            tempestOrbPointLight.GetComponent<FlickerLight>().enabled = false;
            tempestOrbPointLight.GetComponent<LightIntensityCurve>().enabled = false;
            var tempestOrbPointLightLight = tempestOrbPointLight.GetComponent<Light>();
            /*
            tempestOrbPointLightLight.color = beamStartColor;
            tempestOrbPointLightLight.intensity = 400f;
            tempestOrbPointLightLight.range = 8f;
            */
            tempestOrbPointLightLight.enabled = false;

            var sparksIn = tempestOrb.transform.Find("Sparks, In").gameObject;
            var sparksMisc = tempestOrb.transform.Find("Sparks, Misc").gameObject;
            sparksIn.transform.localScale = Vector3.one * 2f;
            var sparksInEmission = sparksIn.GetComponent<ParticleSystem>().emission;
            var sparksInRateOverTime = sparksInEmission.rateOverTime;
            sparksInRateOverTime.curveMultiplier = 2f;

            var sparksInMaterial = new Material(Paths.Material.matLoaderCharging);
            sparksInMaterial.SetFloat("_Boost", 15f);
            sparksInMaterial.SetFloat("_AlphaBoost", 20f);
            sparksInMaterial.SetFloat("_AlphaBias", 1f);

            sparksIn.GetComponent<ParticleSystemRenderer>().sharedMaterial = sparksInMaterial;
            sparksMisc.GetComponent<ParticleSystemRenderer>().sharedMaterial = Paths.Material.matIceOrbCore;

            VFXUtils.RecolorMaterialsAndLights(sparksIn, beamStartColor, beamStartColor, true);
            VFXUtils.RecolorMaterialsAndLights(sparksMisc, beamStartColor, beamStartColor, true);

            var orbCore = tempestOrb.transform.Find("OrbCore");
            orbCore.localScale = Vector3.one * 0.5f;
            orbCore.GetComponent<MeshRenderer>().sharedMaterials = newOrbCoreMaterials;

            var rotateX = orbCore.AddComponent<RotateAroundAxis>();
            rotateX.speed = RotateAroundAxis.Speed.Fast;
            rotateX.slowRotationSpeed = 5;
            rotateX.fastRotationSpeed = 30;
            rotateX.rotateAroundAxis = RotateAroundAxis.RotationAxis.X;
            rotateX.relativeTo = Space.Self;
            rotateX.reverse = false;

            var rotateY = orbCore.AddComponent<RotateAroundAxis>();
            rotateY.speed = RotateAroundAxis.Speed.Fast;
            rotateY.slowRotationSpeed = 5;
            rotateY.fastRotationSpeed = 45;
            rotateY.rotateAroundAxis = RotateAroundAxis.RotationAxis.Y;
            rotateY.relativeTo = Space.Self;
            rotateY.reverse = false;

            var objectScaleCurve = orbCore.AddComponent<ObjectScaleCurve>();
            objectScaleCurve.useOverallCurveOnly = true;
            objectScaleCurve.timeMax = 10f;
            objectScaleCurve.overallCurve = new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(0.03f, 1f), new Keyframe(1f, 4.19f)); // 0.5x => 2.25x scale gives us a 4.5x increase, just perfect enough to line up with the outer sphere fill area (nevermind, the mesh isn't 1x scale in blender)

            // new Material[] { Paths.Material.matLoaderLightningTile, Paths.Material.matJellyfishLightningSphere };
            tempestOrb.transform.RemoveComponent<ObjectScaleCurve>();
            // tempestOrb.transform.GetComponent<ObjectScaleCurve>().enabled = false;
            yield return new WaitForSeconds(0.1f);
            tempestOrb.transform.localScale = Vector3.one * 3f;

            tempestSphereIndicator.transform.parent = sphereVFX.transform;
            tempestOrb.transform.parent = sphereVFX.transform;
            sphereVFX.transform.SetParent(tempestSphereDefault.transform);
            tempestSphereIndicator.transform.position = Vector3.zero;
            tempestSphereIndicator.transform.localPosition = Vector3.zero;
            tempestOrb.transform.position = Vector3.zero;
            tempestOrb.transform.localPosition = Vector3.zero;

            var lineRenderer = tempestSphereDefault.transform.Find("LR").GetComponent<LineRenderer>();

            lineRenderer.endWidth = 0.5f;
            lineRenderer.startColor = beamStartColor;
            lineRenderer.endColor = beamEndColor;
            lineRenderer.material = lineRendererBase;

            // projectile.GetComponentInChildren<LineRenderer>().sharedMaterial = Paths.Material.matLightningLongYellow;

            var detachAndCollapse = tempestSphereDefault.AddComponent<DetachAndCollapse>();
            detachAndCollapse.collapseTime = 0.4f;
            detachAndCollapse.target = sphereVFX.transform;

            // ContentAddition.AddEffect(tempestSphereIndicator);
            // ContentAddition.AddEffect(tempestOrb);
        }

        public static Material lineRendererBase => CreateLineRenderer();

        public IEnumerator CreateCovenantPrefab()
        {
            var beamStartColor = new Color32(255, 0, 230, 255);
            var beamEndColor = new Color32(0, 0, 255, 255);
            var sphereFillColor = new Color32(0, 12, 255, 255);
            var sphereOutlineColor = new Color32(129, 57, 176, 255);
            var smallSphereFillColor = new Color32(39, 6, 40, 255);

            tempestSphereCovenant = PrefabAPI.InstantiateClone(Main.assets.LoadAsset<GameObject>("TempestSphereProjectile.prefab"), "Tempest Sphere Projectile " + "Covenant");

            ContentAddition.AddProjectile(tempestSphereCovenant);

            GameObject sphereVFX = new("Tempest Sphere VFX Holder Covenant");
            sphereVFX.transform.position = Vector3.zero;
            sphereVFX.transform.localPosition = Vector3.zero;

            var tempestSphereIndicator = PrefabAPI.InstantiateClone(Paths.GameObject.VagrantNovaAreaIndicator, "Tempest Sphere Indicator Covenant", false);
            // tempestSphereIndicator.GetComponentInChildren<ParticleSystemRenderer>().gameObject.SetActive(false);
            tempestSphereIndicator.transform.Find("Particle System").gameObject.SetActive(false);

            var pointLight = tempestSphereIndicator.transform.Find("Point Light").GetComponent<Light>();
            pointLight.color = sphereOutlineColor;
            pointLight.intensity = 100f;
            pointLight.GetComponent<LightIntensityCurve>().enabled = false;
            pointLight.GetComponent<LightScaleFromParent>().enabled = false;
            pointLight.range = 20f;

            var newSphereFillMaterial = new Material(Paths.Material.matWarbannerSphereIndicator);
            newSphereFillMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newSphereFillMaterial.SetColor("_TintColor", sphereFillColor);
            newSphereFillMaterial.SetFloat("_InvFade", 30f);
            newSphereFillMaterial.SetFloat("_SoftPower", 0.1f);
            newSphereFillMaterial.SetFloat("_Boost", 3f);
            newSphereFillMaterial.SetFloat("_RimPower", 17f);
            newSphereFillMaterial.SetFloat("_RimStrength", 5f);
            newSphereFillMaterial.SetFloat("_AlphaBoost", 20f);
            newSphereFillMaterial.SetFloat("_IntersectionStrength", 1.223287f);

            var newSphereOutlineMaterial = new Material(Paths.Material.matLightningSphere);
            newSphereOutlineMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone3);
            newSphereOutlineMaterial.SetTexture("_Cloud1Tex", Paths.Texture2D.texCloudLightning1);
            newSphereOutlineMaterial.SetTexture("_Cloud2Tex", null);
            newSphereOutlineMaterial.SetColor("_TintColor", sphereOutlineColor);
            newSphereOutlineMaterial.SetFloat("_InvFade", 3f);
            newSphereOutlineMaterial.SetFloat("_SoftPower", 0.85f);
            newSphereOutlineMaterial.SetFloat("_Boost", 1.5f);
            newSphereOutlineMaterial.SetFloat("_RimPower", 0.1f);
            newSphereOutlineMaterial.SetFloat("_RimStrength", 0.052f);
            newSphereOutlineMaterial.SetFloat("_AlphaBoost", 1f);
            newSphereOutlineMaterial.SetFloat("_IntersectionStrength", 6.4f);

            var newTempestSphereMaterials = new Material[2] { newSphereFillMaterial, newSphereOutlineMaterial };

            tempestSphereIndicator.GetComponent<MeshRenderer>().sharedMaterials = newTempestSphereMaterials;

            tempestSphereIndicator.RemoveComponent<ObjectScaleCurve>();
            // tempestSphereIndicator.GetComponent<ObjectScaleCurve>().enabled = false;

            yield return new WaitForSeconds(0.1f);
            tempestSphereIndicator.transform.localScale = new(14f, 14f, 14f);
            tempestSphereIndicator.RemoveComponent<AnimateShaderAlpha>();
            // tempestSphereIndicator.GetComponent<AnimateShaderAlpha>().enabled = false;

            var newOrbCoreOutlineMaterial = new Material(Paths.Material.matLoaderLightningTile);
            newOrbCoreOutlineMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone3);
            newOrbCoreOutlineMaterial.SetColor("_TintColor", sphereFillColor);
            newOrbCoreOutlineMaterial.SetFloat("_InvFade", 1.3f);
            newOrbCoreOutlineMaterial.SetFloat("_Boost", 4f);
            newOrbCoreOutlineMaterial.SetFloat("_AlphaBoost", 3.67f);
            newOrbCoreOutlineMaterial.SetFloat("_AlphaBias", 0f);

            var newOrbCoreFillMaterial = new Material(Paths.Material.matJellyfishLightningSphere);
            newOrbCoreFillMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone3);
            newOrbCoreFillMaterial.SetColor("_TintColor", smallSphereFillColor);
            newOrbCoreFillMaterial.SetFloat("_InvFade", 0.8f);
            newOrbCoreFillMaterial.SetFloat("_SoftPower", 1f);
            newOrbCoreFillMaterial.SetFloat("_Boost", 1.86f);
            newOrbCoreFillMaterial.SetFloat("_RimPower", 1.5f);
            newOrbCoreFillMaterial.SetFloat("_RimStrength", 0.7f);
            newOrbCoreFillMaterial.SetFloat("_AlphaBoost", 6f);
            newOrbCoreFillMaterial.SetFloat("_AlphaBias", 2f);

            var newOrbCoreMaterials = new Material[2] { newOrbCoreOutlineMaterial, newOrbCoreFillMaterial };

            GameObject tempestOrb = PrefabAPI.InstantiateClone(Paths.GameObject.VoidSurvivorChargeMegaBlaster, "Tempest Sphere Orb Covenant", false);

            tempestOrb.transform.Find("Base").gameObject.SetActive(false);
            tempestOrb.transform.Find("Base (1)").gameObject.SetActive(false);
            var tempestOrbPointLight = tempestOrb.transform.Find("Point light");
            tempestOrbPointLight.GetComponent<FlickerLight>().enabled = false;
            tempestOrbPointLight.GetComponent<LightIntensityCurve>().enabled = false;
            var tempestOrbPointLightLight = tempestOrbPointLight.GetComponent<Light>();
            /*
            tempestOrbPointLightLight.color = beamStartColor;
            tempestOrbPointLightLight.intensity = 400f;
            tempestOrbPointLightLight.range = 8f;
            */
            tempestOrbPointLightLight.enabled = false;

            var sparksIn = tempestOrb.transform.Find("Sparks, In").gameObject;
            var sparksMisc = tempestOrb.transform.Find("Sparks, Misc").gameObject;
            sparksIn.transform.localScale = Vector3.one * 2f;
            var sparksInEmission = sparksIn.GetComponent<ParticleSystem>().emission;
            var sparksInRateOverTime = sparksInEmission.rateOverTime;
            sparksInRateOverTime.curveMultiplier = 2f;

            var sparksInMaterial = new Material(Paths.Material.matLoaderCharging);
            sparksInMaterial.SetFloat("_Boost", 15f);
            sparksInMaterial.SetFloat("_AlphaBoost", 20f);
            sparksInMaterial.SetFloat("_AlphaBias", 1f);

            sparksIn.GetComponent<ParticleSystemRenderer>().sharedMaterial = sparksInMaterial;
            sparksMisc.GetComponent<ParticleSystemRenderer>().sharedMaterial = Paths.Material.matIceOrbCore;

            VFXUtils.RecolorMaterialsAndLights(sparksIn, beamStartColor, beamStartColor, true);
            VFXUtils.RecolorMaterialsAndLights(sparksMisc, beamStartColor, beamStartColor, true);

            var orbCore = tempestOrb.transform.Find("OrbCore");
            orbCore.localScale = Vector3.one * 0.5f;
            orbCore.GetComponent<MeshRenderer>().sharedMaterials = newOrbCoreMaterials;

            var rotateX = orbCore.AddComponent<RotateAroundAxis>();
            rotateX.speed = RotateAroundAxis.Speed.Fast;
            rotateX.slowRotationSpeed = 5;
            rotateX.fastRotationSpeed = 30;
            rotateX.rotateAroundAxis = RotateAroundAxis.RotationAxis.X;
            rotateX.relativeTo = Space.Self;
            rotateX.reverse = false;

            var rotateY = orbCore.AddComponent<RotateAroundAxis>();
            rotateY.speed = RotateAroundAxis.Speed.Fast;
            rotateY.slowRotationSpeed = 5;
            rotateY.fastRotationSpeed = 45;
            rotateY.rotateAroundAxis = RotateAroundAxis.RotationAxis.Y;
            rotateY.relativeTo = Space.Self;
            rotateY.reverse = false;

            var objectScaleCurve = orbCore.AddComponent<ObjectScaleCurve>();
            objectScaleCurve.useOverallCurveOnly = true;
            objectScaleCurve.timeMax = 10f;
            objectScaleCurve.overallCurve = new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(0.03f, 1f), new Keyframe(1f, 4.19f)); // 0.5x => 2.25x scale gives us a 4.5x increase, just perfect enough to line up with the outer sphere fill area

            // new Material[] { Paths.Material.matLoaderLightningTile, Paths.Material.matJellyfishLightningSphere };
            tempestOrb.transform.RemoveComponent<ObjectScaleCurve>();
            // tempestOrb.transform.GetComponent<ObjectScaleCurve>().enabled = false;
            yield return new WaitForSeconds(0.1f);
            tempestOrb.transform.localScale = Vector3.one * 3f;

            tempestSphereIndicator.transform.parent = sphereVFX.transform;
            tempestOrb.transform.parent = sphereVFX.transform;
            sphereVFX.transform.SetParent(tempestSphereCovenant.transform);
            tempestSphereIndicator.transform.position = Vector3.zero;
            tempestSphereIndicator.transform.localPosition = Vector3.zero;
            tempestOrb.transform.position = Vector3.zero;
            tempestOrb.transform.localPosition = Vector3.zero;

            var lineRenderer = tempestSphereCovenant.transform.Find("LR").GetComponent<LineRenderer>();

            lineRenderer.endWidth = 0.5f;
            lineRenderer.startColor = beamStartColor;
            lineRenderer.endColor = beamEndColor;
            lineRenderer.material = lineRendererBase;

            // projectile.GetComponentInChildren<LineRenderer>().sharedMaterial = Paths.Material.matLightningLongYellow;

            var detachAndCollapse = tempestSphereCovenant.AddComponent<DetachAndCollapse>();
            detachAndCollapse.collapseTime = 0.4f;
            detachAndCollapse.target = sphereVFX.transform;

            // ContentAddition.AddEffect(tempestSphereIndicator);
            // ContentAddition.AddEffect(tempestOrb);
        }

        public static Material CreateLineRenderer()
        {
            var newLineMaterial = new Material(Paths.Material.matLightningLongYellow);
            newLineMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritoneHShrine);
            newLineMaterial.SetTexture("_Cloud1Tex", null);
            newLineMaterial.SetTexture("_Cloud2Tex", null);
            newLineMaterial.SetColor("_TintColor", Color.white);
            newLineMaterial.SetFloat("_InvFade", 1f);
            newLineMaterial.SetFloat("_Boost", 2f);
            newLineMaterial.SetFloat("_AlphaBoost", 1f);
            // newLineMaterial.SetFloat("_AlphaBias", 0f);
            newLineMaterial.SetTextureScale("_MainTex", new Vector2(0.02f, 1f));
            newLineMaterial.SetTextureOffset("_MainTex", Vector2.zero);

            newLineMaterial.SetFloat("_AlphaBias", 0.1f);
            newLineMaterial.SetTexture("_Cloud1Tex", Paths.Texture2D.texCloudLightning1);
            newLineMaterial.SetTextureScale("_Cloud1Tex", new Vector2(0.2f, 1f));
            newLineMaterial.SetColor("_CutoffScroll", new Color(50f, 25f, 180f, 0f));

            return newLineMaterial;
        }
    }
}