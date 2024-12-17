using System;
using System.Reflection;
using Sandswept.Utils;

namespace Sandswept.Drones
{
    public abstract class DroneBase
    {
        public abstract GameObject DroneBody { get; }
        public abstract GameObject DroneMaster { get; }
        public abstract Dictionary<string, string> Tokens { get; }
        public abstract string ConfigName { get; }

        public abstract GameObject DroneBroken { get; }
        public abstract int Weight { get; }
        public abstract int Credits { get; }
        public abstract DirectorAPI.Stage[] Stages { get; }
        public abstract string iscName { get; }

        public void Initialize()
        {
            if (!DefaultEnabledCallback(this))
            {
                return;
            }

            Setup();

            ContentAddition.AddBody(DroneBody);
            ContentAddition.AddMaster(DroneMaster);

            foreach (KeyValuePair<string, string> kvp in Tokens)
            {
                LanguageAPI.Add(kvp.Key, kvp.Value);
            }

            SetupInteractables();

            DroneBroken.FindComponent<ParticleSystemRenderer>("Smoke, Point").sharedMaterial = Paths.Material.matOpaqueDustLargeDirectional;
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