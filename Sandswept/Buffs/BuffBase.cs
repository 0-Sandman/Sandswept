namespace Sandswept.Buffs
{
    public abstract class BuffBase<T> : BuffBase where T : BuffBase<T>
    {
        public static T instance { get; private set; }

        public BuffBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting BuffBase was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class BuffBase
    {
        public abstract string BuffName { get; }
        public abstract Color Color { get; }
        public virtual bool CanStack { get; set; } = false;
        public virtual bool IsDebuff { get; set; } = false;
        public virtual bool Hidden { get; set; } = false;
        public abstract Sprite BuffIcon { get; }

        public BuffDef BuffDef;
        public static Dictionary<BuffDef, BuffBase> BuffMap = new();
        private static bool appliedHooks = false;

        public virtual void Init()
        {
            CreateBuff();
            Hooks();

            BuffMap.Add(BuffDef, this);

            if (!appliedHooks) {
                appliedHooks = true;

                On.RoR2.CharacterBody.OnBuffFirstStackGained += OnBuffFirstStackGained;
                On.RoR2.CharacterBody.OnBuffFinalStackLost += OnBuffFinalStackLost;
            }
        }

        private static void OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);

            if (BuffMap.ContainsKey(buffDef)) {
                BuffMap[buffDef].OnBuffExpired(self);
            }
        }

        private static void OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);

            if (BuffMap.ContainsKey(buffDef)) {
                BuffMap[buffDef].OnBuffApplied(self);
            }
        }

        public void CreateBuff()
        {
            BuffDef = ScriptableObject.CreateInstance<BuffDef>();
            BuffDef.name = BuffName;
            BuffDef.buffColor = Color;
            BuffDef.canStack = CanStack;
            BuffDef.isDebuff = IsDebuff;
            BuffDef.iconSprite = BuffIcon;
            BuffDef.isHidden = Hidden;

            ContentAddition.AddBuffDef(BuffDef);
        }

        public virtual void Hooks()
        {
        }

        public virtual void OnBuffApplied(CharacterBody body) {

        }

        public virtual void OnBuffExpired(CharacterBody body) {

        }
    }
}