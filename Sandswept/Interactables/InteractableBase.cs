﻿using System.Reflection;

namespace Sandswept.Interactables
{
    public abstract class InteractableBase<T> : InteractableBase where T : InteractableBase<T>
    {
        public static T Instance { get; private set; }

        public InteractableBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class InteractableBase
    {
        public DirectorCard directorCard;
        public InteractableSpawnCard interactableSpawnCard;
        public DirectorCardHolder directorCardHolder;
        public abstract string Name { get; }
        public abstract InteractableCategory Category { get; }
        public abstract int MaxSpawnsPerStage { get; }
        public abstract int CreditCost { get; }
        public abstract HullClassification Size { get; }
        public abstract int MinimumStageToAppearOn { get; }
        public abstract int SpawnWeight { get; }
        public virtual List<Stage> Stages { get; } = new() { Stage.DistantRoost, DirectorAPI.Stage.TitanicPlains, DirectorAPI.Stage.SiphonedForest, DirectorAPI.Stage.AbandonedAqueduct, DirectorAPI.Stage.WetlandAspect, DirectorAPI.Stage.AphelianSanctuary, DirectorAPI.Stage.RallypointDelta, DirectorAPI.Stage.ScorchedAcres, DirectorAPI.Stage.SulfurPools, DirectorAPI.Stage.AbyssalDepths, DirectorAPI.Stage.SirensCall, DirectorAPI.Stage.SunderedGrove, DirectorAPI.Stage.SkyMeadow };
        public virtual bool SpawnInSimulacrum { get; } = false;
        public virtual bool SpawnInVoid { get; } = false;
        public virtual bool SkipOnSacrifice { get; } = false;
        public virtual float SacrificeWeightMultiplier { get; } = 1f;
        public virtual bool OrientToFloor { get; } = true;
        public virtual bool SlightlyRandomizeOrientation { get; } = true;

        // public virtual ExpansionDef RequiredExpansionHolder { get; } = Main.SOTV; later

        public virtual void Init()
        {
            interactableSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
        }

        public void PostInit()
        {
            if (SpawnInSimulacrum)
            {
                Stages.Add(Stage.TitanicPlainsSimulacrum);
                Stages.Add(Stage.AbandonedAqueductSimulacrum);
                Stages.Add(Stage.AphelianSanctuarySimulacrum);
                Stages.Add(Stage.RallypointDeltaSimulacrum);
                Stages.Add(Stage.AbyssalDepthsSimulacrum);
                Stages.Add(Stage.SkyMeadowSimulacrum);
                Stages.Add(Stage.CommencementSimulacrum);
            }

            if (SpawnInVoid)
            {
                Stages.Add(Stage.VoidCell);
                Stages.Add(Stage.VoidLocus);
            }

            interactableSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = SkipOnSacrifice;
            interactableSpawnCard.maxSpawnsPerStage = MaxSpawnsPerStage;
            interactableSpawnCard.directorCreditCost = CreditCost;
            interactableSpawnCard.hullSize = Size;
            interactableSpawnCard.forbiddenFlags = RoR2.Navigation.NodeFlags.NoChestSpawn;
            interactableSpawnCard.slightlyRandomizeOrientation = SlightlyRandomizeOrientation;
            interactableSpawnCard.weightScalarWhenSacrificeArtifactEnabled = SacrificeWeightMultiplier;
            interactableSpawnCard.occupyPosition = true;
            interactableSpawnCard.orientToFloor = OrientToFloor;
            interactableSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            interactableSpawnCard.sendOverNetwork = true;
            interactableSpawnCard.name = "isc" + Name.Replace(" ", "");

            directorCard = new() { spawnCard = interactableSpawnCard, minimumStageCompletions = MinimumStageToAppearOn - 1, selectionWeight = SpawnWeight };

            directorCardHolder = new() { Card = directorCard, InteractableCategory = Category };

            for (int i = 0; i < Stages.Count; i++)
            {
                var stage = Stages[i];
                Helpers.AddNewInteractableToStage(directorCardHolder, stage);
            }
        }

        public static bool DefaultEnabledCallback(InteractableBase self)
        {
            ConfigSectionAttribute attribute = self.GetType().GetCustomAttribute<ConfigSectionAttribute>();
            if (attribute != null)
            {
                bool isValid = Main.config.Bind(attribute.name, "Enabled", true, "Allow this interactable to appear in runs?").Value;
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
    }
}