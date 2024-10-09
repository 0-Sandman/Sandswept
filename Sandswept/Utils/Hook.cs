using System.Reflection;
using System.Collections;
using System;
using System.Linq;
using BepInEx.Configuration;
using HG.Reflection;
using System.Collections.Generic;

namespace Sandswept.Utils
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HookAttribute : SearchableAttribute
    {
        public Type type;
        public string eventName;

        public HookAttribute(Type type, string eventName)
        {
            this.type = type;
            this.eventName = eventName;
        }
    }

    public class HookAttributeHandler
    {
        [AutoRun]
        internal static void Hook()
        {
            On.RoR2.RoR2Application.OnLoad += (orig, self) =>
            {
                Initialize();
                return orig(self);
            };
        }

        internal static void Initialize()
        {
            List<HookAttribute> instances = new();
            SearchableAttribute.GetInstances<HookAttribute>(instances);

            foreach (HookAttribute attr in instances)
            {
                if (!(attr.target as MethodInfo).IsStatic)
                {
                    continue;
                }
                EventInfo targetEvent = attr.type.GetEvent(attr.eventName);
                Delegate handler = Delegate.CreateDelegate(targetEvent.EventHandlerType, null, attr.target as MethodInfo);
                targetEvent.AddEventHandler(targetEvent.DeclaringType, handler);
            }
        }
    }
}