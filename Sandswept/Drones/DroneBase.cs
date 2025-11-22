using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using RoR2.ExpansionManagement;
using Sandswept.Utils;

namespace Sandswept.Drones
{
    public abstract class DroneBase<T> : DroneBase where T : DroneBase<T>
    {
        public static T Instance { get; private set; }

        public DroneBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class DroneBase
    {
        public abstract GameObject DroneBody { get; }
        public abstract GameObject DroneMaster { get; }
        public abstract Dictionary<string, string> Tokens { get; }

        public abstract GameObject DroneBroken { get; }
        public abstract int Weight { get; }
        public abstract int Credits { get; }
        public abstract DirectorAPI.Stage[] Stages { get; }
        public abstract string iscName { get; }
        public abstract string inspectInfoDescription { get; }
        public InteractableSpawnCard iscBroken;

        public void Initialize()
        {
            if (!DefaultEnabledCallback(this))
            {
                return;
            }

            Setup();

            ContentAddition.AddBody(DroneBody);
            ContentAddition.AddMaster(DroneMaster);

            PrefabAPI.RegisterNetworkPrefab(DroneBody);
            PrefabAPI.RegisterNetworkPrefab(DroneMaster);
            PrefabAPI.RegisterNetworkPrefab(DroneBroken);

            ContentAddition.AddNetworkedObject(DroneBroken);
            ContentAddition.AddNetworkedObject(DroneBody);
            ContentAddition.AddNetworkedObject(DroneMaster);

            foreach (KeyValuePair<string, string> kvp in Tokens)
            {
                LanguageAPI.Add(kvp.Key, kvp.Value);
            }

            SetupInteractables();

            DroneBroken.FindComponent<ParticleSystemRenderer>("Smoke, Point").sharedMaterial = Paths.Material.matOpaqueDustLargeDirectional;

            DeathState.droneCards.Add(DroneBody.name, iscBroken);

            DroneBody.GetComponent<CharacterDeathBehavior>().deathState = new(typeof(DeathState));

            var expansionRequirementComponent = DroneBroken.AddComponent<ExpansionRequirementComponent>();
            expansionRequirementComponent.requiredExpansion = Main.SandsweptExpansionDef;

            var genericInspectInfoProvider = DroneBroken.AddComponent<GenericInspectInfoProvider>();
            genericInspectInfoProvider.enabled = true;

            var genericDisplayNameProvider = DroneBroken.GetComponent<GenericDisplayNameProvider>();

            var descToken = Tokens.Where(x => x.Key.Contains("BROKEN")).First().Key; // gets something like SANDSWEPT_VOLTAIC_DRONE_BROKEN_NAME
            genericDisplayNameProvider.displayToken = descToken; // pseudopulse ! ! voltaic had inferno's name token
            descToken = descToken.Replace("_NAME", "_DESCRIPTION"); // changes _NAME suffix to _DESCRIPTION

            descToken.Add(inspectInfoDescription);

            var droneIcon = Addressables.LoadAssetAsync<Sprite>("bf6ab7be6a9954e43a786c5d88ea5585").WaitForCompletion();
            // guid is tex drone icon outlined

            var inspectDef = ScriptableObject.CreateInstance<InspectDef>();
            inspectDef.name = DroneBroken.name + "InspectDef";
            var inspectInfo = inspectDef.Info = new()
            {
                TitleToken = genericDisplayNameProvider.displayToken,
                DescriptionToken = descToken,
                FlavorToken = "sanswep",
                isConsumedItem = false,
                Visual = droneIcon,
                TitleColor = Color.white
            };

            genericInspectInfoProvider.InspectInfo = inspectDef;
            genericInspectInfoProvider.InspectInfo.Info = inspectInfo;
        }

        public static NetworkHash128 GetNetworkedObjectAssetId(GameObject gameObject)
        {
            var prefabName = gameObject.name;
            Hash128 hasher = Hash128.Compute(prefabName);
            hasher.Append(Main.ModGuid);

            return new NetworkHash128
            {
                i0_7 = hasher.u64_0,
                i8_15 = hasher.u64_1
            };
        }

        public static bool DefaultEnabledCallback(DroneBase self)
        {
            ConfigSectionAttribute attribute = self.GetType().GetCustomAttribute<ConfigSectionAttribute>();
            if (attribute != null)
            {
                bool isValid = Main.config.Bind(attribute.name, "Enabled", true, "Allow this drone to appear in runs?").Value;
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

        public virtual void Setup()
        {
        }

        public void SetupInteractables()
        {
            DirectorCard card = new();

            InteractableSpawnCard isc = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            isc.directorCreditCost = Credits;
            isc.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            isc.sendOverNetwork = true;
            isc.orientToFloor = false;
            isc.prefab = DroneBroken;
            isc.name = iscName;

            card.selectionWeight = Weight;
            card.spawnCard = isc;

#pragma warning disable
            for (int i = 0; i < Stages.Length; i++)
            {
                DirectorAPI.Helpers.AddNewInteractableToStage(card, DirectorAPI.InteractableCategory.Drones, Stages[i]);
            }
#pragma warning restore

            iscBroken = isc;
        }

        public void AssignIfExists(GenericSkill slot, SkillInfo info)
        {
            if (!slot || info == null) return;

            SkillFamily family = ScriptableObject.CreateInstance<SkillFamily>();
            family.variants = new SkillFamily.Variant[1];

            SkillDef sd = info.ToSkillDef();
            family.variants[0].skillDef = sd;

            ContentAddition.AddSkillFamily(family);
            slot._skillFamily = family;
        }

        public class SkillInfo
        {
            public float cooldown;
            public int stockToConsume = 1;
            public SerializableEntityStateType type;
            public string activationMachine = "Weapon";
            public InterruptPriority priority = InterruptPriority.Skill;

            public SkillDef ToSkillDef()
            {
                SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
                skillDef.activationState = type;
                skillDef.baseMaxStock = 1;
                skillDef.stockToConsume = stockToConsume;
                skillDef.baseRechargeInterval = cooldown;
                skillDef.activationStateMachineName = activationMachine;
                skillDef.interruptPriority = priority;
                skillDef.beginSkillCooldownOnSkillEnd = true;

                ContentAddition.AddSkillDef(skillDef);

                return skillDef;
            }
        }
    }
}