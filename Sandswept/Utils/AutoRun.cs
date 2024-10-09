using System.Reflection;
using System.Collections;
using System;
using System.Linq;

namespace Sandswept.Utils
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class AutoRunAttribute : Attribute
    {
        public AutoRunAttribute()
        {
        }
    }

    internal sealed class AutoRunCollector
    {
        public static void HandleAutoRun()
        {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            List<Type> tmp = types.ToList();
            tmp.RemoveAll(asdf);
            types = tmp.ToArray();
            foreach (Type type in types)
            {
                if (!type.FullName.Contains("CustomEmotesAPI"))
                    continue;
                TypeInfo tInfo = type.GetTypeInfo();
                foreach (MethodInfo info in tInfo.GetMethods((BindingFlags)(-1)))
                {
                    AutoRunAttribute attr = info.GetCustomAttribute<AutoRunAttribute>();
                    if (attr != null && info.IsStatic)
                    {
                        info.Invoke(null, null);
                    }
                }
            }
        }
        static bool asdf(Type s)
        {
            return s.FullName.Contains("CustomEmotesAPI");
        }
    }
}