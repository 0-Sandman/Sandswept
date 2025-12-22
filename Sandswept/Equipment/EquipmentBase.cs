using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using LookingGlass.ItemStatsNameSpace;
using Rewired.ComponentControls.Effects;

namespace Sandswept.Equipment
{
    public abstract class EquipmentBase<T> : EquipmentBase where T : EquipmentBase<T>
    {
        public static T instance { get; private set; }

        public EquipmentBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting EquipmentBoilerplate/Equipment was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }

        public abstract GameObject EquipmentModel { get; }
        public abstract Sprite EquipmentIcon { get; }

        public virtual bool AppearsInSinglePlayer { get; } = true;

        public virtual bool AppearsInMultiPlayer { get; } = true;

        public virtual bool CanDrop { get; } = true;

        public virtual float Cooldown { get; } = 60f;

        public virtual bool EnigmaCompatible { get; } = true;

        public virtual bool IsBoss { get; } = false;

        public virtual bool IsLunar { get; } = false;

        public virtual string AchievementName { get; } = string.Empty;
        public virtual string AchievementDesc { get; } = string.Empty;
        public virtual Func<string> GetHowToUnlock => () => AchievementName + "\n<style=cStack>" + AchievementDesc + "</style>";
        public virtual Func<string> GetUnlocked => () => AchievementName + "\n<style=cStack>" + AchievementDesc + "</style>";

        public EquipmentDef EquipmentDef;

        public static bool DefaultEnabledCallback(EquipmentBase self)
        {
            ConfigSectionAttribute attribute = self.GetType().GetCustomAttribute<ConfigSectionAttribute>();
            if (attribute != null)
            {
                bool isValid = Main.config.Bind<bool>(attribute.name, "Enabled", true, "Allow this equipment to appear in runs?").Value;
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

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        public virtual void Init()
        {
            CreateEquipment();
            Hooks();

            if (Main.LookingGlassLoaded)
            {
                LGWrapper();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void LGWrapper()
        {
            EquipmentCatalog.availability.CallWhenAvailable(OnEquipmentCatalogAvailable);
        }

        private void OnEquipmentCatalogAvailable()
        {
            if (GetItemStatsDef() is ItemStatsDef itemStatsDef)
            {
                ItemDefinitions.allEquipmentDefinitions.Add((int)EquipmentDef.equipmentIndex, itemStatsDef);
            }
        }

        protected void CreateEquipment()
        {
            EquipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();
            EquipmentDef.name = "EQUIPMENT_SANDSWEPT_" + EquipmentLangTokenName;
            EquipmentDef.nameToken = "EQUIPMENT_SANDSWEPT_" + EquipmentLangTokenName + "_NAME";
            EquipmentDef.pickupToken = "EQUIPMENT_SANDSWEPT_" + EquipmentLangTokenName + "_PICKUP";
            EquipmentDef.descriptionToken = "EQUIPMENT_SANDSWEPT_" + EquipmentLangTokenName + "_DESCRIPTION";
            EquipmentDef.loreToken = "EQUIPMENT_SANDSWEPT_" + EquipmentLangTokenName + "_LORE";
            EquipmentDef.pickupModelPrefab = EquipmentModel;
            EquipmentDef.pickupIconSprite = EquipmentIcon;
            EquipmentDef.appearsInSinglePlayer = AppearsInSinglePlayer;
            EquipmentDef.appearsInMultiPlayer = AppearsInMultiPlayer;
            EquipmentDef.canDrop = CanDrop;
            EquipmentDef.cooldown = Cooldown;
            EquipmentDef.enigmaCompatible = EnigmaCompatible;
            EquipmentDef.isBoss = IsBoss;
            EquipmentDef.isLunar = IsLunar;
            EquipmentDef.requiredExpansion = Main.SandsweptExpansionDef;
            EquipmentDef.colorIndex = IsLunar ? ColorCatalog.ColorIndex.LunarItem : ColorCatalog.ColorIndex.Equipment;

            if (AchievementName != string.Empty && AchievementDesc != string.Empty)
            {
                LanguageAPI.Add("ACHIEVEMENT_EQUIPMENT_SANDSWEPT_" + EquipmentLangTokenName + "_NAME", AchievementName);
                LanguageAPI.Add("ACHIEVEMENT_EQUIPMENT_SANDSWEPT_" + EquipmentLangTokenName + "_DESCRIPTION", AchievementDesc);

                EquipmentDef.unlockableDef = CreateUnlock();
            }

            LanguageAPI.Add("EQUIPMENT_SANDSWEPT_" + EquipmentLangTokenName + "_NAME", EquipmentName);
            LanguageAPI.Add("EQUIPMENT_SANDSWEPT_" + EquipmentLangTokenName + "_PICKUP", EquipmentPickupDesc);
            LanguageAPI.Add("EQUIPMENT_SANDSWEPT_" + EquipmentLangTokenName + "_DESCRIPTION", EquipmentFullDescription);
            LanguageAPI.Add("EQUIPMENT_SANDSWEPT_" + EquipmentLangTokenName + "_LORE", EquipmentLore);

            if (EquipmentModel != null)
            {
                CreateModelPanelParameters(EquipmentModel);
            }

            ItemAPI.Add(new CustomEquipment(EquipmentDef, CreateItemDisplayRules()));
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
        }

        private void CreateModelPanelParameters(GameObject equipmentModel)
        {
            if (equipmentModel.GetComponent<ModelPanelParameters>() != null)
            {
                return;
            }

            GameObject model = PrefabAPI.InstantiateClone(equipmentModel, equipmentModel.name + "-fixed", false);
            GameObject focus = new("Focus");
            GameObject camera = new("Camera");
            MeshRenderer biggestRenderer = model.GetComponentsInChildren<MeshRenderer>().ToList().OrderByDescending(x => ToFloat(x.bounds.size)).First();
            float mult = ToFloat(biggestRenderer.bounds.size) / 3f;
            float min = mult;
            float max = 3f * mult;
            focus.transform.parent = model.transform;
            camera.transform.parent = model.transform;
            focus.transform.position = biggestRenderer.bounds.center;
            camera.transform.localPosition = focus.transform.position + (model.transform.forward * max);

            var modelPanelParameters = model.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = focus.transform;
            modelPanelParameters.cameraPositionTransform = camera.transform;
            modelPanelParameters.minDistance = min;
            modelPanelParameters.maxDistance = max;

            EquipmentDef.pickupModelPrefab = model;
        }

        public static float ToFloat(Vector3 vec)
        {
            vec.x = Mathf.Abs(vec.x);
            vec.y = Mathf.Abs(vec.y);
            vec.z = Mathf.Abs(vec.z);
            return vec.x + vec.y + vec.z;
        }

        private bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == EquipmentDef)
            {
                return ActivateEquipment(self);
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }

        protected abstract bool ActivateEquipment(EquipmentSlot slot);

        protected UnlockableDef CreateUnlock()
        {
            EquipmentDef.unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            EquipmentDef.unlockableDef.cachedName = "EQUIPMENT_SANDSWEPT_" + EquipmentLangTokenName;
            EquipmentDef.unlockableDef.nameToken = "EQUIPMENT_SANDSWEPT_" + EquipmentLangTokenName + "_NAME";
            EquipmentDef.unlockableDef.getHowToUnlockString = GetHowToUnlock;
            EquipmentDef.unlockableDef.getUnlockedString = GetUnlocked;

            EquipmentDef.unlockableDef.achievementIcon = IsLunar ? TotallyNotStolenUtils.CreateItemIconWithBackgroundFromLunarEquipment(EquipmentDef) : TotallyNotStolenUtils.CreateItemIconWithBackgroundFromEquipment(EquipmentDef);
            var unlockDef = EquipmentDef.unlockableDef;
            Main.Unlocks.Add(unlockDef);
            return unlockDef;
        }

        public virtual void Hooks()
        { }

        #region Targeting Setup

        //Targeting Support
        public virtual bool UseTargeting { get; } = false;

        public GameObject TargetingIndicatorPrefabBase = null;

        public enum TargetingType
        {
            Enemies,
            Friendlies,
        }

        public virtual TargetingType TargetingTypeEnum { get; } = TargetingType.Enemies;

        //Based on MysticItem's targeting code.
        protected void UpdateTargeting(On.RoR2.EquipmentSlot.orig_Update orig, EquipmentSlot self)
        {
            orig(self);

            if (self.equipmentIndex == EquipmentDef.equipmentIndex)
            {
                var targetingComponent = self.GetComponent<TargetingControllerComponent>();
                if (!targetingComponent)
                {
                    targetingComponent = self.gameObject.AddComponent<TargetingControllerComponent>();
                    targetingComponent.VisualizerPrefab = TargetingIndicatorPrefabBase;
                }

                if (self.stock > 0)
                {
                    switch (TargetingTypeEnum)
                    {
                        case (TargetingType.Enemies):
                            targetingComponent.ConfigureTargetFinderForEnemies(self);
                            break;

                        case (TargetingType.Friendlies):
                            targetingComponent.ConfigureTargetFinderForFriendlies(self);
                            break;
                    }
                }
                else
                {
                    targetingComponent.Invalidate();
                    targetingComponent.Indicator.active = false;
                }
            }
        }

        public GameObject SetUpIDRS()
        {
            var idrsPrefab = PrefabAPI.InstantiateClone(EquipmentModel, EquipmentName.Replace(" ", "") + "IDRS", false);
            var itemDisplay = idrsPrefab.AddComponent<ItemDisplay>();
            List<Renderer> rendererList = [.. idrsPrefab.GetComponentsInChildren<Renderer>()];
            Array.Resize(ref itemDisplay.rendererInfos, rendererList.Count);
            for (int j = 0; j < rendererList.Count; j++)
            {
                var renderer = rendererList[j];
                var defaultMaterial = renderer.material;
                itemDisplay.rendererInfos[j] = new CharacterModel.RendererInfo()
                {
                    renderer = renderer,
                    defaultMaterial = defaultMaterial,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false,
                    hideOnDeath = false,
                    ignoresMaterialOverrides = false
                };
            }
            return idrsPrefab;
        }

        public GameObject SetUpFollowerIDRS(float followerDampTime = 0.2f, float followerMaxSpeed = 30f, bool rotateX = true, float xRotationSpeed = 25f, bool rotateY = true, float yRotationSpeed = 15f, bool rotateZ = true, float zRotationSpeed = 10f)
        {
            var followerHolder = PrefabAPI.InstantiateClone(new GameObject(""), EquipmentName.Replace(" ", "") + "FollowerIDRS", false);
            var followerTransform = followerHolder.transform;

            followerTransform.localScale = Vector3.one;
            followerTransform.localEulerAngles = Vector3.zero;
            followerTransform.localPosition = Vector3.zero;

            var prefabForFollower = PrefabAPI.InstantiateClone(EquipmentModel, EquipmentName.Replace(" ", "") + "ForFollower", false);

            prefabForFollower.transform.SetParent(followerTransform);

            if (rotateX)
            {
                var rotateAroundX = prefabForFollower.AddComponent<RotateAroundAxis>();
                rotateAroundX.speed = RotateAroundAxis.Speed.Fast;
                rotateAroundX.slowRotationSpeed = xRotationSpeed;
                rotateAroundX.rotateAroundAxis = RotateAroundAxis.RotationAxis.X;
                rotateAroundX.relativeTo = Space.Self;
                rotateAroundX.reverse = false;
            }

            if (rotateY)
            {
                var rotateAroundY = prefabForFollower.AddComponent<RotateAroundAxis>();
                rotateAroundY.speed = RotateAroundAxis.Speed.Fast;
                rotateAroundY.slowRotationSpeed = yRotationSpeed;
                rotateAroundY.rotateAroundAxis = RotateAroundAxis.RotationAxis.Y;
                rotateAroundY.relativeTo = Space.Self;
                rotateAroundY.reverse = false;
            }

            if (rotateZ)
            {
                var rotateAroundZ = prefabForFollower.AddComponent<RotateAroundAxis>();
                rotateAroundZ.speed = RotateAroundAxis.Speed.Fast;
                rotateAroundZ.slowRotationSpeed = zRotationSpeed;
                rotateAroundZ.rotateAroundAxis = RotateAroundAxis.RotationAxis.Z;
                rotateAroundZ.relativeTo = Space.Self;
                rotateAroundZ.reverse = false;
            }

            var childLocator = followerHolder.AddComponent<ChildLocator>();
            foreach (Transform child in followerTransform.GetComponentsInChildren<Transform>())
            {
                childLocator.AddChild(child.name, child);
            }

            var idrsPrefab = PrefabAPI.InstantiateClone(new GameObject(""), EquipmentName.Replace(" ", "") + "IDRS", false);

            followerTransform.localScale = Vector3.one;
            followerTransform.localEulerAngles = Vector3.zero;
            followerTransform.localPosition = Vector3.zero;

            var itemFollower = idrsPrefab.AddComponent<ItemFollower>();
            itemFollower.followerPrefab = followerHolder;
            itemFollower.targetObject = idrsPrefab;
            itemFollower.followerCurve = null;
            itemFollower.followerLineRenderer = null;
            itemFollower.distanceDampTime = followerDampTime;
            itemFollower.distanceMaxSpeed = followerMaxSpeed;

            return idrsPrefab;
        }

        public class TargetingControllerComponent : MonoBehaviour
        {
            public GameObject TargetObject;
            public GameObject VisualizerPrefab;
            public Indicator Indicator;
            public BullseyeSearch TargetFinder;
            public Action<BullseyeSearch> AdditionalBullseyeFunctionality = (search) => { };

            public void Awake()
            {
                Indicator = new Indicator(gameObject, null);
            }

            public void OnDestroy()
            {
                Invalidate();
            }

            public void Invalidate()
            {
                TargetObject = null;
                Indicator.targetTransform = null;
            }

            public void ConfigureTargetFinderBase(EquipmentSlot self)
            {
                if (TargetFinder == null) TargetFinder = new BullseyeSearch();
                TargetFinder.teamMaskFilter = TeamMask.allButNeutral;
                TargetFinder.teamMaskFilter.RemoveTeam(self.characterBody.teamComponent.teamIndex);
                TargetFinder.sortMode = BullseyeSearch.SortMode.Angle;
                TargetFinder.filterByLoS = true;
                float num;
                Ray ray = CameraRigController.ModifyAimRayIfApplicable(self.GetAimRay(), self.gameObject, out num);
                TargetFinder.searchOrigin = ray.origin;
                TargetFinder.searchDirection = ray.direction;
                TargetFinder.maxAngleFilter = 10f;
                TargetFinder.viewer = self.characterBody;
            }

            public void ConfigureTargetFinderForEnemies(EquipmentSlot self)
            {
                ConfigureTargetFinderBase(self);
                TargetFinder.teamMaskFilter = TeamMask.GetUnprotectedTeams(self.characterBody.teamComponent.teamIndex);
                TargetFinder.RefreshCandidates();
                TargetFinder.FilterOutGameObject(self.gameObject);
                AdditionalBullseyeFunctionality(TargetFinder);
                PlaceTargetingIndicator(TargetFinder.GetResults());
            }

            public void ConfigureTargetFinderForFriendlies(EquipmentSlot self)
            {
                ConfigureTargetFinderBase(self);
                TargetFinder.teamMaskFilter = TeamMask.none;
                TargetFinder.teamMaskFilter.AddTeam(self.characterBody.teamComponent.teamIndex);
                TargetFinder.RefreshCandidates();
                TargetFinder.FilterOutGameObject(self.gameObject);
                AdditionalBullseyeFunctionality(TargetFinder);
                PlaceTargetingIndicator(TargetFinder.GetResults());
            }

            public void PlaceTargetingIndicator(IEnumerable<HurtBox> TargetFinderResults)
            {
                HurtBox hurtbox = TargetFinderResults.Any() ? TargetFinderResults.First() : null;

                if (hurtbox)
                {
                    TargetObject = hurtbox.healthComponent.gameObject;
                    Indicator.visualizerPrefab = VisualizerPrefab;
                    Indicator.targetTransform = hurtbox.transform;
                }
                else
                {
                    Invalidate();
                }
                Indicator.active = hurtbox;
            }
        }

        #endregion Targeting Setup

        public virtual object GetItemStatsDef()
        {
            return null;
        }
    }
}