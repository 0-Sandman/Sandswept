using BepInEx.Configuration;
using Sandswept2.Utils;

namespace Sandswept2.Utils
{
    public static class ConfigExtension
    {
        public static ConfigOption<T> ActiveBind<T>(this ConfigFile configWrapper, string categoryName, string configOptionName, T defaultValue, string fullDescription)
        {
            return new ConfigOption<T>(configWrapper, categoryName, configOptionName, defaultValue, fullDescription);
        }
    }
}