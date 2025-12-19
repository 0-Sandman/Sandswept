using System.Linq;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using static RoR2.CombatDirector;

namespace Sandswept.Elites
{
    public abstract class EliteEquipmentBase<T> : EliteEquipmentBase where T : EliteEquipmentBase<T>
    {
        public static T Instance { get; private set; }

        public EliteEquipmentBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting EliteEquipmentBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class EliteEquipmentBase
    {
        public abstract string EliteEquipmentName { get; }

        public abstract string EliteAffixToken { get; }

        public abstract string EliteEquipmentPickupDesc { get; }
        public abstract string EliteEquipmentFullDescription { get; }
        public abstract string EliteEquipmentLore { get; }

        /// <summary>
        /// This is what appears before the name of the creature that has this elite status.
        /// <para>E.g.: "Hypercharged Beetle" where Hypercharged is the modifier.</para>
        /// </summary>
        public abstract string EliteModifier { get; }

        public virtual bool AppearsInSinglePlayer { get; } = true;

        public virtual bool AppearsInMultiPlayer { get; } = true;
        public virtual bool CanDrop { get; } = false;

        public virtual float Cooldown { get; } = 60f;

        public abstract GameObject EliteEquipmentModel { get; }
        public abstract Sprite EliteEquipmentIcon { get; }

        public virtual GameObject EliteCrownModel { get; }

        public EquipmentDef EliteEquipmentDef;

        public BuffDef EliteBuffDef;

        /// <summary>
        /// Implement before calling CreateEliteEquipment.
        /// </summary>
        public abstract Sprite EliteBuffIcon { get; }

        public abstract Color EliteBuffColor { get; }

        /// <summary>
        /// If not overriden, the elite can spawn in all tiers defined.
        /// </summary>
        public virtual EliteTier Tier { get; } = EliteTier.Tier1;

        public enum EliteTier
        {
            Tier1,
            Tier2
        }

        /// <summary>
        /// If you want the elite to have an overlay with your custom material.
        /// </summary>
        public abstract Texture2D EliteRampTexture { get; }

        public EliteDef EliteDef;

        /// <summary>
        /// Elite stat multiplier, defaults to 4 (Tier 1) here.
        /// </summary>
        public virtual float HealthMultiplier { get; set; } = 4;

        /// <summary>
        /// Elite stat multiplier, defaults to 2 (Tier 1) here.
        /// </summary>
        public virtual float DamageMultiplier { get; set; } = 2;
        private static bool hasAppliedHook = false;
        public static Dictionary<EliteDef, EliteEquipmentBase> EliteMap = new();

        public virtual void Init() {
            CreateLang();
            CreateEquipment();
            CreateElite();
            Hooks();

            if (!hasAppliedHook) {
                hasAppliedHook = true;
                IL.RoR2.CombatDirector.PrepareNewMonsterWave += CheckEliteReqs;
            }
        }

        private void CheckEliteReqs(ILContext il)
        {
            ILCursor c = new(il);
            c.TryGotoNext(MoveType.Before, x => x.MatchCallOrCallvirt(typeof(EliteTierDef), nameof(EliteTierDef.GetRandomAvailableEliteDef)));
            c.Next.OpCode = OpCodes.Nop;
            c.Next.Operand = null;
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Func<EliteTierDef, Xoroshiro128Plus, DirectorCard, EliteDef>>((tier, rng, card) => {
                return GetRandomAvailableEliteDef(tier, rng, card);
            });
        }

        private static EliteDef GetRandomAvailableEliteDef(EliteTierDef tier, Xoroshiro128Plus rng, DirectorCard card)
        {
            List<EliteDef> available = new();
            EliteDef[] array = tier.eliteTypes;

            foreach (EliteDef eliteDef in array)
            {
                if (eliteDef && eliteDef.IsAvailable())
                {
                    if (EliteMap.ContainsKey(eliteDef)) {
                        if (!EliteMap[eliteDef].CheckEliteRequirement(card.spawnCard)) {
                            continue;
                        }
                    }

                    available.Add(eliteDef);
                }
            }

            if (available.Count > 0)
            {
                return rng.NextElementUniform(available);
            }
            return null;
        }

        private static GameObject hauntedPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/PickupEliteHaunted.prefab").WaitForCompletion();

        private static GameObject firePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/PickupEliteFire.prefab").WaitForCompletion();

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        public static Dictionary<string, Dictionary<object, List<ItemDisplayRule>>> perBodyDisplayRules = new Dictionary<string, Dictionary<object, List<ItemDisplayRule>>>();

        public virtual void AddDisplayRule(string bodyName, string childName, Vector3 localPos, Vector3 localAngles, Vector3 localScale)
        {
            AddDisplayRule(bodyName, EliteCrownModel, childName, localPos, localAngles, localScale);
        }

        public virtual void AddDisplayRule(string bodyName, GameObject itemDisplayPrefab, string childName, Vector3 localPos, Vector3 localAngles, Vector3 localScale)
        {
            AddDisplayRule(bodyName, this, itemDisplayPrefab, childName, localPos, localAngles, localScale);
        }

        public static void AddDisplayRule(string bodyName, object keyAsset, GameObject itemDisplayPrefab, string childName, Vector3 localPos, Vector3 localAngles, Vector3 localScale)
        {
            Dictionary<object, List<ItemDisplayRule>> perItemDisplayRules;
            if (!perBodyDisplayRules.TryGetValue(bodyName, out perItemDisplayRules))
            {
                perItemDisplayRules = new Dictionary<object, List<ItemDisplayRule>>();
                perBodyDisplayRules[bodyName] = perItemDisplayRules;
            }
            List<ItemDisplayRule> displayRulesForThisItem;
            if (!perItemDisplayRules.TryGetValue(keyAsset, out displayRulesForThisItem))
            {
                displayRulesForThisItem = new List<ItemDisplayRule>();
                perItemDisplayRules[keyAsset] = displayRulesForThisItem;
            }
            displayRulesForThisItem.Add(new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = itemDisplayPrefab,
                childName = childName,
                localPos = localPos,
                localAngles = localAngles,
                localScale = localScale
            });
        }

        // maybe im calling this too late, idk
        internal static void PostGameLoad()
        {
            var changedIDRS = new List<ItemDisplayRuleSet>();
            foreach (var kvp in perBodyDisplayRules)
            {
                var bodyName = kvp.Key;
                var bodyIndex = BodyCatalog.FindBodyIndex(bodyName);
                if (bodyIndex != BodyIndex.None)
                {
                    var bodyPrefab = BodyCatalog.GetBodyPrefab(bodyIndex);
                    var characterModel = bodyPrefab.GetComponentInChildren<CharacterModel>();
                    var idrs = characterModel.itemDisplayRuleSet;
                    if (idrs && !changedIDRS.Contains(idrs))
                    {
                        foreach (var displayRules in kvp.Value)
                        {
                            idrs.SetDisplayRuleGroup((Object)displayRules.Key, new DisplayRuleGroup { rules = displayRules.Value.ToArray() });
                        }
                        changedIDRS.Add(idrs);
                    }
                }
            }
            foreach (var idrs in changedIDRS)
                idrs.GenerateRuntimeValues();
        }

        public static bool DefaultEnabledCallback(EliteEquipmentBase self)
        {
            ConfigSectionAttribute attribute = self.GetType().GetCustomAttribute<ConfigSectionAttribute>();
            if (attribute != null)
            {
                bool isValid = Main.config.Bind<bool>(attribute.name, "Enabled", true, "Allow this elite to appear in runs?").Value;
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

        public virtual bool CheckEliteRequirement(SpawnCard card) {
            return true;
        }

        protected void CreateLang()
        {
            LanguageAPI.Add("ELITE_EQUIPMENT_" + EliteAffixToken + "_NAME", EliteEquipmentName);
            LanguageAPI.Add("ELITE_EQUIPMENT_" + EliteAffixToken + "_PICKUP", EliteEquipmentPickupDesc);
            LanguageAPI.Add("ELITE_EQUIPMENT_" + EliteAffixToken + "_DESCRIPTION", EliteEquipmentFullDescription);
            LanguageAPI.Add("ELITE_EQUIPMENT_" + EliteAffixToken + "_LORE", EliteEquipmentLore);
            LanguageAPI.Add("ELITE_" + EliteAffixToken + "_MODIFIER", EliteModifier + " {0}");
        }

        protected void CreateEquipment()
        {
            EliteBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            EliteBuffDef.name = EliteAffixToken;
            EliteBuffDef.buffColor = EliteBuffColor;
            EliteBuffDef.canStack = false;
            EliteBuffDef.iconSprite = EliteBuffIcon;

            EliteEquipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();
            EliteEquipmentDef.name = "ELITE_EQUIPMENT_" + EliteAffixToken;
            EliteEquipmentDef.nameToken = "ELITE_EQUIPMENT_" + EliteAffixToken + "_NAME";
            EliteEquipmentDef.pickupToken = "ELITE_EQUIPMENT_" + EliteAffixToken + "_PICKUP";
            EliteEquipmentDef.descriptionToken = "ELITE_EQUIPMENT_" + EliteAffixToken + "_DESCRIPTION";
            EliteEquipmentDef.loreToken = "ELITE_EQUIPMENT_" + EliteAffixToken + "_LORE";
            EliteEquipmentDef.pickupModelPrefab = EliteEquipmentModel;
            EliteEquipmentDef.pickupIconSprite = EliteEquipmentIcon;
            EliteEquipmentDef.appearsInSinglePlayer = AppearsInSinglePlayer;
            EliteEquipmentDef.appearsInMultiPlayer = AppearsInMultiPlayer;
            EliteEquipmentDef.canDrop = CanDrop;
            EliteEquipmentDef.cooldown = Cooldown;
            EliteEquipmentDef.passiveBuffDef = EliteBuffDef;
            EliteEquipmentDef.requiredExpansion = Main.SandsweptExpansionDef;

            ItemAPI.Add(new CustomEquipment(EliteEquipmentDef, CreateItemDisplayRules()));

            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;

            if (UseTargeting && TargetingIndicatorPrefabBase)
            {
                On.RoR2.EquipmentSlot.Update += UpdateTargeting;
            }
        }

        protected void CreateElite()
        {
            EliteDef = ScriptableObject.CreateInstance<EliteDef>();
            EliteDef.name = "ELITE_" + EliteAffixToken;
            EliteDef.modifierToken = "ELITE_" + EliteAffixToken + "_MODIFIER";
            EliteDef.eliteEquipmentDef = EliteEquipmentDef;
            EliteDef.healthBoostCoefficient = HealthMultiplier;
            EliteDef.damageBoostCoefficient = DamageMultiplier;
            EliteDef.shaderEliteRampIndex = 0;

            EliteMap.Add(EliteDef, this);

            List<EliteTierDef> tiers = new();

            EliteDef knownT1 = Paths.EliteDef.edFire;
            EliteDef knownT1H = Paths.EliteDef.edFireHonor;
            EliteDef knownT2 = Paths.EliteDef.edHaunted;

            switch (Tier)
            {
                case EliteTier.Tier1:
                    AddAllTiersThatContain(knownT1);
                    AddAllTiersThatContain(knownT1H);
                    break;

                case EliteTier.Tier2:
                    AddAllTiersThatContain(knownT2);
                    break;
            }

            EliteAPI.Add(new CustomElite(EliteDef, tiers.ToArray(), EliteRampTexture));

            EliteBuffDef.eliteDef = EliteDef;
            ContentAddition.AddBuffDef(EliteBuffDef);
            PostGameLoad();

            void AddAllTiersThatContain(EliteDef def)
            {
                var ctiers = EliteAPI.GetCombatDirectorEliteTiers();

                foreach (var tier in ctiers)
                {
                    if (tier.eliteTypes.Contains(def))
                    {
                        tiers.Add(tier);
                    }
                }
            }
        }

        protected bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == EliteEquipmentDef)
            {
                return ActivateEquipment(self);
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }

        /// <summary>
        /// Must be implemented, but you can just return false if you don't want an On Use effect for your elite equipment.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        protected abstract bool ActivateEquipment(EquipmentSlot slot);

        public abstract void Hooks();

        public virtual GameObject CreateAffixModel(Color32 color, bool tier2 = false)
        {
            GameObject gameObject = (tier2 ? hauntedPrefab : firePrefab).InstantiateClone("PickupAffix" + EliteEquipmentName, false);
            Material material = Object.Instantiate(gameObject.GetComponentInChildren<MeshRenderer>().material);
            material.color = color;
            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
                renderer.material = material;
            return gameObject;
        }

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

            if (self.equipmentIndex == EliteEquipmentDef.equipmentIndex)
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
                        case TargetingType.Enemies:
                            targetingComponent.ConfigureTargetFinderForEnemies(self);
                            break;

                        case TargetingType.Friendlies:
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
    }
}