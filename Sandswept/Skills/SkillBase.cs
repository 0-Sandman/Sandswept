using System;
using System.Reflection;

namespace Sandswept.Skills {
    public abstract class SkillBase<T> : SkillBase where T : SkillBase<T> {
        public static T instance;

        public SkillBase() {
            instance = this as T;
        }
    }

    public abstract class SkillBase {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual string LangToken => this.GetType().Name.ToUpper();
        public string NameToken => "SANDSWEPT_SKILL_" + LangToken + "_NAME";
        public string DescToken => "SANDSWEPT_SKILL_" + LangToken + "_DESC";
        public abstract Type ActivationStateType { get; }
        public abstract string ActivationMachineName { get; }
        public abstract float Cooldown { get; }
        public virtual int MaxStock { get; } = 1;
        public virtual int StockToConsume { get; } = 1;
        public virtual bool Agile { get; } = false;
        public virtual bool CanceledFromSprinting => !true;
        public virtual string[] Keywords { get; } = null;
        public virtual bool IsCombat => true;
        public virtual InterruptPriority InterruptPriority => InterruptPriority.Skill;
        public abstract Sprite Icon { get; }
        public SkillDef skillDef;

        private static bool DefaultEnabledCallback(SkillBase self) {
            ConfigSectionAttribute attribute = self.GetType().GetCustomAttribute<ConfigSectionAttribute>();
            if (attribute != null) {
                bool isValid = Main.config.Bind<bool>(attribute.name, "Enabled", true, "Allow this item to appear in runs?").Value;
                if (isValid) {
                    return true;
                }
                return false;
            }
            else {
                return true;
            }
        }

        public virtual void Init() {
            CreateSkillDef();
            SetupSkillDef();
            CreateLang();
        }

        public virtual void CreateSkillDef() {
            skillDef = ScriptableObject.CreateInstance<SkillDef>();
        }

        public virtual void SetupSkillDef() {
            skillDef.skillName = Name;
            skillDef.skillNameToken = NameToken;
            skillDef.skillDescriptionToken = DescToken;
            skillDef.baseRechargeInterval = Cooldown;
            skillDef.canceledFromSprinting = CanceledFromSprinting;
            skillDef.cancelSprintingOnActivation = !Agile;
            skillDef.baseMaxStock = MaxStock;
            skillDef.stockToConsume = StockToConsume;
            skillDef.activationStateMachineName = ActivationMachineName;
            skillDef.activationState = new(ActivationStateType);
            skillDef.icon = Icon;
            skillDef.isCombatSkill = IsCombat;
            skillDef.keywordTokens = Keywords;
            skillDef.interruptPriority = InterruptPriority;
            
            ContentAddition.AddSkillDef(skillDef);
        }

        public virtual void CreateLang() {
            NameToken.Add(Name);
            DescToken.Add(Description);
        }

        public string GetConfigName() {
            return Name;
        }
    }
}