using System.Reflection;
using System.Collections;
using System;
using System.Linq;
using BepInEx.Configuration;
using HG.Reflection;
using static System.Collections.Specialized.BitVector32;
using System.Text.RegularExpressions;

namespace Sandswept
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ConfigFieldAttribute : SearchableAttribute
    {
        public string name;
        public string desc;
        public object defaultValue;

        public ConfigFieldAttribute(string name, string desc, object defaultValue)
        {
            this.name = name;
            this.desc = desc;
            this.defaultValue = defaultValue;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigSectionAttribute : Attribute
    {
        public string name;

        public ConfigSectionAttribute(string name)
        {
            this.name = name;
        }
    }

    public class ConfigManager
    {
        internal static bool ConfigChanged = false;
        internal static bool VersionChanged = false;

        public static void HandleConfigAttributes(Assembly assembly, ConfigFile config)
        {
            foreach (Type type in assembly.GetTypes())
            {
                TypeInfo info = type.GetTypeInfo();
                ConfigSectionAttribute secattr = info.GetCustomAttribute<ConfigSectionAttribute>();
                if (secattr == null)
                {
                    continue;
                }

                foreach (FieldInfo field in info.GetFields())
                {
                    if (!field.IsStatic)
                    {
                        continue;
                    }

                    Type t = field.FieldType;

                    ConfigFieldAttribute configattr = field.GetCustomAttribute<ConfigFieldAttribute>();
                    if (configattr == null)
                    {
                        continue;
                    }

                    MethodInfo method = typeof(ConfigFile).GetMethods().Where(x => x.Name == nameof(ConfigFile.Bind)).First();
                    method = method.MakeGenericMethod(t);
                    ConfigEntryBase val = (ConfigEntryBase)method.Invoke(config, new object[] { new ConfigDefinition(secattr.name, configattr.name), configattr.defaultValue, new ConfigDescription(configattr.desc) });
                    ConfigEntryBase backupVal = (ConfigEntryBase)method.Invoke(Main.backupConfig, new object[] { new ConfigDefinition(Regex.Replace(config.ConfigFilePath, "\\W", "") + " : " + secattr.name, configattr.name), val.DefaultValue, new ConfigDescription(configattr.desc) });

                    if (!ConfigEqual(backupVal.DefaultValue, backupVal.BoxedValue))
                    {
                        // Main.WRBLogger.LogDebug("Config Updated: " + section + " : " + configattr.name + " from " + val.BoxedValue + " to " + val.DefaultValue);
                        if (VersionChanged)
                        {
                            // Main.WRBLogger.LogDebug("Autosyncing...");
                            val.BoxedValue = val.DefaultValue;
                            backupVal.BoxedValue = backupVal.DefaultValue;
                        }
                    }
                    if (!ConfigEqual(val.DefaultValue, val.BoxedValue)) ConfigChanged = true;
                    field.SetValue(null, val.BoxedValue);
                }
            }
        }

        private static bool ConfigEqual(object a, object b)
        {
            if (a.Equals(b)) return true;
            float fa, fb;
            if (float.TryParse(a.ToString(), out fa) && float.TryParse(b.ToString(), out fb) && Mathf.Abs(fa - fb) < 0.0001) return true;
            return false;
        }
    }
}