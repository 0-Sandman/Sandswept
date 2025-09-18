using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using System;
using System.Collections.Generic;
using EntityStates;
using RoR2.ExpansionManagement;
using Unity;
using HarmonyLib;
using RoR2.CharacterAI;
using System.Reflection;

namespace Sandswept.Enemies
{
    public abstract class EnemyBase<T> : EnemyBase where T : EnemyBase<T>
    {
        public static T Instance { get; private set; }

        public EnemyBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class EnemyBase
    {
        public DirectorCard card;
        public CharacterSpawnCard csc;
        public GameObject prefab;
        public GameObject prefabMaster;
        public CharacterBody body;
        public CharacterMaster master;
        private static ItemDisplayRuleSet idrs;
        private static List<ItemDisplayRuleSet.KeyAssetRuleGroup> rules = new();
        public virtual DirectorCardCategorySelection family { get; } = null;
        public virtual DirectorAPI.MonsterCategory cat { get; } = DirectorAPI.MonsterCategory.BasicMonsters;

        public static bool DefaultEnabledCallback(EnemyBase self)
        {
            ConfigSectionAttribute attribute = self.GetType().GetCustomAttribute<ConfigSectionAttribute>();
            if (attribute != null)
            {
                bool isValid = Main.config.Bind<bool>(attribute.name, "Enabled", true, "Allow this enemy to appear in runs?").Value;
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

        public virtual void Create()
        {
            if (!DefaultEnabledCallback(this))
            {
                return;
            }

            LoadPrefabs();

            var expansionRequirementComponent = prefab.AddComponent<ExpansionRequirementComponent>();
            expansionRequirementComponent.requiredExpansion = Main.SandsweptExpansionDef;

            body = prefab.GetComponent<CharacterBody>();
            master = prefabMaster.GetComponent<CharacterMaster>();
            Modify();

            var characterModel = body.GetComponentInChildren<CharacterModel>();
            if (characterModel)
            {
                idrs = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
                characterModel.itemDisplayRuleSet = idrs;
                SetUpIDRS();
            }

            AddSpawnCard();
            AddDirectorCard();
            PostCreation();

            if (family)
            {
                DirectorAPI.AddCard(family, new DirectorCardHolder()
                {
                    Card = card,
                    MonsterCategory = cat,
                    MonsterCategorySelectionWeight = 1
                });
            }
        }

        public abstract void LoadPrefabs();

        public virtual void Modify()
        {
        }

        public virtual void PostCreation()
        {
        }

        public virtual void AddSpawnCard()
        {
            csc = ScriptableObject.CreateInstance<CharacterSpawnCard>();
        }

        public virtual void AddDirectorCard()
        {
            card = new DirectorCard();
            card.spawnCard = csc;
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
                    rules = new ItemDisplayRule[] {
                        rule
                    }
                }
            });
        }

        public void CollapseIDRS()
        {
            idrs.keyAssetRuleGroups = rules.ToArray();
        }

        public void RegisterEnemy(GameObject bodyPrefab, GameObject masterPrefab, List<DirectorAPI.Stage> stages = null, DirectorAPI.MonsterCategory category = DirectorAPI.MonsterCategory.BasicMonsters, bool all = false)
        {
            PrefabAPI.RegisterNetworkPrefab(bodyPrefab);
            PrefabAPI.RegisterNetworkPrefab(masterPrefab);
            ContentAddition.AddBody(bodyPrefab);
            ContentAddition.AddMaster(masterPrefab);
            if (stages != null)
            {
                foreach (DirectorAPI.Stage stage in stages)
                {
                    DirectorAPI.Helpers.AddNewMonsterToStage(card, category, stage);
                }
            }

            if (all)
            {
                DirectorAPI.Helpers.AddNewMonster(card, category);
            }
        }

        public void SwapStats(GameObject prefab, float damage, float crit, float speed, float health, float regen, float accel, float armor)
        {
            CharacterBody body = prefab.GetComponent<CharacterBody>();
            body.baseArmor = armor;
            body.baseCrit = crit;
            body.baseRegen = regen;
            body.baseDamage = damage;
            body.baseMoveSpeed = speed;
            body.baseAcceleration = accel;
            body.baseMaxHealth = health;
            body.autoCalculateLevelStats = true;
            body.PerformAutoCalculateLevelStats();
        }

        public void ClearESM(GameObject prefab, GameObject master = null)
        {
            foreach (EntityStateMachine esm in prefab.GetComponents<EntityStateMachine>())
            {
                GameObject.DestroyImmediate(esm);
            }

            if (master)
            {
                foreach (EntityStateMachine esm in master.GetComponents<EntityStateMachine>())
                {
                    GameObject.DestroyImmediate(esm);
                }
            }
        }

        public EntityStateMachine AddESM(GameObject prefab, string name, SerializableEntityStateType initial)
        {
            EntityStateMachine esm = prefab.AddComponent<EntityStateMachine>();
            esm.customName = name;
            esm.initialStateType = initial;
            esm.mainStateType = initial;

            prefab.GetComponent<NetworkStateMachine>().stateMachines.AddItem(esm);

            return esm;
        }

        public void DestroyModelLeftovers(GameObject prefab)
        {
            GameObject.Destroy(prefab.GetComponentInChildren<ModelLocator>().modelBaseTransform.gameObject);
        }

        /// <summary>
        /// A method to destroy the previous skill family of a slot and replace it with a new one
        /// </summary>
        public void ReplaceSkill(GenericSkill slot, SkillDef replaceWith, string familyName = "temp")
        {
            SkillFamily family = ScriptableObject.CreateInstance<SkillFamily>();
            ((ScriptableObject)family).name = familyName;
            // family.variants = new SkillFamily.Variant[1];
            slot._skillFamily = family;
            slot._skillFamily.variants = new SkillFamily.Variant[1];

            slot._skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = replaceWith
            };
        }

        public void DisableSkins(GameObject prefab)
        {
            CharacterModel model = prefab.GetComponentInChildren<CharacterModel>();
            if (prefab.GetComponentInChildren<ModelSkinController>())
            {
                ModelSkinController controller = prefab.GetComponentInChildren<ModelSkinController>();
                controller.enabled = false;
            }
        }

        public void RelocateMeshTransform(GameObject prefab, Transform transform, bool parent = false)
        {
            CharacterModel model = prefab.GetComponentInChildren<CharacterModel>();
            model.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
            if (parent)
            {
                model.transform.SetParent(transform);
            }
        }

        /// <summary>
        /// A method to replace the meshes (either all or via a list of renderers) of a prefab
        /// </summary>
        public void SwapMeshes(GameObject prefab, Mesh mesh, bool all = false, List<int> renders = null)
        {
            CharacterModel model = prefab.GetComponentInChildren<CharacterModel>();
            if (all)
            {
                foreach (CharacterModel.RendererInfo info in model.baseRendererInfos)
                {
                    if (info.renderer.GetComponentInChildren<SkinnedMeshRenderer>())
                    {
                        info.renderer.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh = mesh;
                    }
                }
            }

            if (renders != null)
            {
                foreach (int i in renders)
                {
                    if (model.baseRendererInfos[i].renderer.GetComponentInChildren<SkinnedMeshRenderer>())
                    {
                        model.baseRendererInfos[i].renderer.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh = mesh;
                    }
                }
            }

            if (all)
            {
                foreach (CharacterModel.RendererInfo info in model.baseRendererInfos)
                {
                    if (info.renderer.GetComponentInChildren<MeshRenderer>())
                    {
                        info.renderer.GetComponentInChildren<MeshFilter>().sharedMesh = mesh;
                    }
                }
            }

            if (renders != null)
            {
                foreach (int i in renders)
                {
                    if (model.baseRendererInfos[i].renderer.GetComponentInChildren<MeshRenderer>())
                    {
                        model.baseRendererInfos[i].renderer.GetComponentInChildren<MeshFilter>().sharedMesh = mesh;
                    }
                }
            }
        }

        /// <summary>
        /// A method to disable the meshes from a list of renderers
        /// </summary>
        public void DisableMeshes(GameObject prefab, List<int> renders)
        {
            CharacterModel model = prefab.GetComponentInChildren<CharacterModel>();
            foreach (int i in renders)
            {
                model.baseRendererInfos[i].renderer.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// A method to replace the materials on the meshes of a prefab
        /// </summary>
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

        /// <summary>
        /// A method to replace the model of a prefab.
        /// <para> Destroys the previous model and sets a new one, also adds a HurtBoxGroup </para>
        /// </summary>
        /// <param name="prefab">the prefab to replace</param>
        /// <param name="model">the model to replace with</param>
        /// <param name="turnSpeed">the turnspeed to assign to the CharacterDirection</param>
        public void SetupModel(GameObject prefab, GameObject model, float turnSpeed = 1200f)
        {
            DestroyModelLeftovers(prefab);
            foreach (HurtBoxGroup hurtboxes in prefab.GetComponentsInChildren<HurtBoxGroup>())
            {
                GameObject.Destroy(hurtboxes);
            }
            foreach (HurtBox hurtbox in prefab.GetComponentsInChildren<HurtBox>())
            {
                GameObject.Destroy(hurtbox);
            }

            GameObject modelbase = new("Model Base");
            modelbase.transform.SetParent(prefab.transform);
            modelbase.transform.SetPositionAndRotation(prefab.transform.position, prefab.transform.rotation);

            model.transform.SetParent(modelbase.transform);
            model.transform.SetPositionAndRotation(modelbase.transform.position, modelbase.transform.rotation);

            ModelLocator modelLocator = prefab.GetComponentInChildren<ModelLocator>();
            modelLocator.modelTransform = model.transform;
            modelLocator.modelBaseTransform = modelbase.transform;
            modelLocator.dontReleaseModelOnDeath = false;
            modelLocator.dontDetatchFromParent = false;
            modelLocator.noCorpse = true;
            modelLocator.preserveModel = false;
            modelLocator.autoUpdateModelTransform = true;

            CharacterDirection characterDirection = prefab.GetComponent<CharacterDirection>();
            if (characterDirection)
            {
                characterDirection.targetTransform = modelbase.transform;
                characterDirection.turnSpeed = turnSpeed;
            }

            CharacterDeathBehavior characterDeathBehavior = prefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = prefab.GetComponent<EntityStateMachine>();
            characterDeathBehavior.deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));

            CharacterModel mdl = model.AddComponent<CharacterModel>();
            mdl.body = prefab.GetComponent<CharacterBody>();

            model.AddComponent<HurtBoxGroup>();
        }

        /// <summary>
        /// A method to create a hurtbox on a model.
        /// </summary>
        /// <param name="prefab">the prefab</param>
        /// <param name="model">the model of the prefab</param>
        /// <param name="collider">the object to apply the hurtbox to</param>
        /// <param name="index">the index of the new hurtbox, starting from 0</param>
        /// <param name="weakPoint">whether this hurtbox is a railgunner weakpoint</param>
        /// <param name="damageModifier">the damage modifier of this hurtbox</param>
        /// <param name="targetable">whether this hurtbox should be targetable by the AI or homing attacks</param>
        public void SetupHurtbox(GameObject prefab, GameObject model, Collider collider, short index, bool weakPoint = false, HurtBox.DamageModifier damageModifier = HurtBox.DamageModifier.Normal, bool targetable = false)
        {
            HurtBoxGroup hurtBoxGroup = model.GetComponent<HurtBoxGroup>();

            HurtBox componentInChildren = collider.gameObject.AddComponent<HurtBox>();
            componentInChildren.gameObject.layer = LayerIndex.entityPrecise.intVal;
            componentInChildren.healthComponent = prefab.GetComponent<HealthComponent>();
            componentInChildren.damageModifier = damageModifier;
            componentInChildren.hurtBoxGroup = hurtBoxGroup;
            componentInChildren.indexInGroup = index;
            componentInChildren.isSniperTarget = weakPoint;
            componentInChildren.isBullseye = targetable;
        }

        public void WipeAllDrivers(GameObject prefab)
        {
            AISkillDriver[] drivers = prefab.GetComponents<AISkillDriver>();
            for (int i = 0; i < drivers.Length; i++)
            {
                GameObject.Destroy(drivers[i]);
            }
        }

        public void AddNewDriver(GameObject prefab, string name, AISkillDriver.AimType aim, AISkillDriver.MovementType move, AISkillDriver.TargetType target, float minDist, float maxDist, SkillSlot skill)
        {
            AISkillDriver driver = prefab.AddComponent<AISkillDriver>();
            driver.customName = name;
            driver.minDistance = minDist;
            driver.maxDistance = maxDist;
            driver.aimType = aim;
            driver.movementType = move;
            driver.moveTargetType = target;
            driver.skillSlot = skill;
            driver.requireSkillReady = skill != SkillSlot.None;
        }
    }
}