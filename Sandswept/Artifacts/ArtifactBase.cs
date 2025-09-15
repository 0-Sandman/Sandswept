using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Reflection;
using UnityEngine;

namespace Sandswept.Artifacts
{
    public abstract class ArtifactBase<T> : ArtifactBase where T : ArtifactBase<T>
    {
        public static T instance { get; private set; }

        public ArtifactBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ArtifactBase was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class ArtifactBase
    {
        public abstract string ArtifactName { get; }

        public abstract string ArtifactLangTokenName { get; }

        public abstract string ArtifactDescription { get; }

        public abstract Sprite ArtifactEnabledIcon { get; }

        public abstract Sprite ArtifactDisabledIcon { get; }
        public virtual GameObject PickupModelPrefab { get; } = null;

        public ArtifactDef ArtifactDef;

        //For use only after the run has started.
        public bool ArtifactEnabled => RunArtifactManager.instance.IsArtifactEnabled(ArtifactDef);

        public virtual void Init(ConfigFile config) {
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        public static bool DefaultEnabledCallback(ArtifactBase self)
        {
            ConfigSectionAttribute attribute = self.GetType().GetCustomAttribute<ConfigSectionAttribute>();
            if (attribute != null)
            {
                bool isValid = Main.config.Bind<bool>(attribute.name, "Enabled", true, "Allow this artifact to be selected?").Value;
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

        protected void CreateLang()
        {
            LanguageAPI.Add("0000_SANDSWEPT_ARTIFACT_OF_" + ArtifactLangTokenName + "_NAME", ArtifactName);
            LanguageAPI.Add("0000_SANDSWEPT_ARTIFACT_OF_" + ArtifactLangTokenName + "_DESCRIPTION", ArtifactDescription);
        }

        protected void CreateArtifact()
        {
            ArtifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
            ArtifactDef.cachedName = "0000_SANDSWEPT_ARTIFACT_OF_" + ArtifactLangTokenName;
            ArtifactDef.nameToken = "0000_SANDSWEPT_ARTIFACT_OF_" + ArtifactLangTokenName + "_NAME";
            ArtifactDef.descriptionToken = "0000_SANDSWEPT_ARTIFACT_OF_" + ArtifactLangTokenName + "_DESCRIPTION";
            ArtifactDef.smallIconSelectedSprite = ArtifactEnabledIcon;
            ArtifactDef.smallIconDeselectedSprite = ArtifactDisabledIcon;
            ArtifactDef.requiredExpansion = Main.SandsweptExpansionDef;
            ArtifactDef.pickupModelPrefab = PickupModelPrefab;

            ContentAddition.AddArtifactDef(ArtifactDef);
        }

        public abstract void Hooks();
    }
}