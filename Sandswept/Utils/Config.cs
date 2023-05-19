using System.Reflection;
using System.Collections;
using System;
using System.Linq;
using BepInEx.Configuration;
using HG.Reflection;

namespace Sandswept.Utils {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ConfigFieldAttribute : SearchableAttribute {
        public string name;
        public string desc;
        public object defaultValue;

        public ConfigFieldAttribute(string name, string desc, object defaultValue) {
            this.name = name;
            this.desc = desc;
            this.defaultValue = defaultValue;
        }
    }

    public interface IConfigurable {
        string GetConfigName();
    }
    public class ConfigManager {
        public static void HandleConfigAttributes(Assembly assembly, ConfigFile config) {
            foreach (Type type in assembly.GetTypes()) {
                TypeInfo info = type.GetTypeInfo();

                if (!typeof(IConfigurable).IsAssignableFrom(type)) {
                    continue;
                }

                IConfigurable configurable = type as IConfigurable;

                foreach (FieldInfo field in info.GetFields()) {
                    if (!field.IsStatic) {
                        continue;
                    }

                    Type t = field.FieldType;

                    ConfigFieldAttribute configattr = field.GetCustomAttribute<ConfigFieldAttribute>();
                    if (configattr == null) {
                        continue;
                    }

                    MethodInfo method = typeof(ConfigFile).GetMethods().Where(x => x.Name == nameof(ConfigFile.Bind)).First();
                    method = method.MakeGenericMethod(t);
                    ConfigEntryBase val = (ConfigEntryBase)method.Invoke(config, new object[] { new ConfigDefinition(configurable.GetConfigName(), configattr.name), configattr.defaultValue, new ConfigDescription(configattr.desc)});

                    field.SetValue(null, val.BoxedValue);
                }
            }
        }
    }
}