using BepInEx.Configuration;
using Sandswept.Utils;

namespace Sandswept.Utils
{
    public class ConfigOption<T>
    {
        private ConfigEntry<T> Bind;

        public ConfigOption(ConfigFile config, string categoryName, string configOptionName, T defaultValue, string fullDescription)
        {
            Bind = config.Bind<T>(categoryName, configOptionName, defaultValue, fullDescription);
        }

        public static implicit operator T(ConfigOption<T> x)
        {
            return x.Bind.Value;
        }

        public override string ToString()
        {
            return Bind.Value.ToString();
        }
    }
}