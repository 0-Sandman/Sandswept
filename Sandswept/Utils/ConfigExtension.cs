using BepInEx.Configuration;
using Sandswept.Utils;

namespace Sandswept.Utils
{
    public static class ConfigExtension
    {
        public static ConfigOption<T> ActiveBind<T>(this ConfigFile configWrapper, string categoryName, string configOptionName, T defaultValue, string fullDescription)
        {
            return new ConfigOption<T>(configWrapper, categoryName, configOptionName, defaultValue, fullDescription);
        }
    }
}