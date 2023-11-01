namespace Sandswept2.Buffs
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
        public abstract Sprite BuffIcon { get; }

        public BuffDef BuffDef;

        public virtual void Init()
        {
            CreateBuff();
            Hooks();
        }

        public void CreateBuff()
        {
            BuffDef = ScriptableObject.CreateInstance<BuffDef>();
            BuffDef.name = BuffName;
            BuffDef.buffColor = Color;
            BuffDef.canStack = CanStack;
            BuffDef.isDebuff = IsDebuff;
            BuffDef.iconSprite = BuffIcon;

            ContentAddition.AddBuffDef(BuffDef);
        }

        public virtual void Hooks()
        {
        }
    }
}