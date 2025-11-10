using LookingGlass.ItemStatsNameSpace;
using RoR2.ExpansionManagement;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Sandswept.Survivors
{
    public abstract class SurvivorBase<T> : SurvivorBase where T : SurvivorBase<T>
    {
        public static T instance;

        public SurvivorBase()
        {
            instance = this as T;
        }
    }

    public abstract class SurvivorBase
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Subtitle { get; }
        public abstract string Outro { get; }
        public abstract string Failure { get; }
        public GameObject Body;
        public GameObject Master;
        public SurvivorDef SurvivorDef;
        public SkinDef mastery;

        private static ItemDisplayRuleSet idrs;
        private static List<ItemDisplayRuleSet.KeyAssetRuleGroup> rules = new();

        public static bool DefaultEnabledCallback(SurvivorBase self)
        {
            ConfigSectionAttribute attribute = self.GetType().GetCustomAttribute<ConfigSectionAttribute>();
            if (attribute != null)
            {
                bool isValid = Main.config.Bind<bool>(attribute.name, "Enabled", true, "Allow this survivor to appear?").Value;
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

        public void Initialize()
        {
            if (!DefaultEnabledCallback(this))
            {
                return;
            }

            LoadAssets();
            CreateLang();

            ContentAddition.AddBody(Body);
            ContentAddition.AddMaster(Master);
            ContentAddition.AddSurvivorDef(SurvivorDef);

            var characterModel = Body.GetComponentInChildren<CharacterModel>();
            if (characterModel)
            {
                idrs = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
                characterModel.itemDisplayRuleSet = idrs;
                SetUpIDRS();
            }
        }

        public virtual void LoadAssets()
        {
        }

        public virtual void CreateLang()
        {
            CharacterBody body = Body.GetComponent<CharacterBody>();
            body.baseNameToken.Add(Name);
            body.subtitleNameToken.Add(Subtitle);
            SurvivorDef.outroFlavorToken.Add(Outro);
            SurvivorDef.mainEndingEscapeFailureFlavorToken.Add(Failure);
            SurvivorDef.descriptionToken.Add(Description);
            // var expansionRequirementComponent = Body.AddComponent<ExpansionRequirementComponent>();
            // expansionRequirementComponent.requiredExpansion = Main.SandsweptExpansionDef;
            // sots changed this I believe? so this should more closely mimic vanilla behavior
        }

        public EntityStateMachine AddESM(GameObject prefab, string name, SerializableEntityStateType initial)
        {
            EntityStateMachine esm = prefab.AddComponent<EntityStateMachine>();
            esm.customName = name;
            esm.initialStateType = initial;
            esm.mainStateType = initial;

            return esm;
        }

        public void SwapMaterials(GameObject prefab, Material mat, bool all = false, List<int> renders = null)
        {
            CharacterModel model = prefab.GetComponentInChildren<CharacterModel>();
            if (all)
            {
                for (int i = 0; i < model.baseRendererInfos.Length; i++)
                {
                    model.baseRendererInfos[i].defaultMaterial = mat;
                }
            }

            if (renders != null)
            {
                foreach (int i in renders)
                {
                    model.baseRendererInfos[i].defaultMaterial = mat;
                }
            }
        }

        public void ReplaceSkills(GenericSkill slot, params SkillDef[] skills)
        {
            SkillFamily family = ScriptableObject.CreateInstance<SkillFamily>();
            (family as ScriptableObject).name = slot.skillName ?? "default";
            List<SkillFamily.Variant> variants = new();
            foreach (SkillDef skill in skills)
            {
                variants.Add(new SkillFamily.Variant
                {
                    skillDef = skill,
                    viewableNode = new(skill.skillNameToken, false, null)
                });
            }
            family.variants = variants.ToArray();
            slot._skillFamily = family;
            ContentAddition.AddSkillFamily(family);
        }

        public string GetConfigName()
        {
            return Name;
        }

        public virtual void SetUpIDRS()
        {
        }

        public void AddDisplayRule(UnityEngine.Object asset, ItemDisplayRule rule)
        {
            rules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup()
            {
                keyAsset = asset,
                displayRuleGroup = new()
                {
                    rules = [
                        rule
                    ]
                }
            });
        }

        public void CollapseIDRS()
        {
            idrs.keyAssetRuleGroups = rules.ToArray();
        }
    }
}