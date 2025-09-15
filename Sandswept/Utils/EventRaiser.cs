using System;
using System.Reflection;

namespace Sandswept.Utils {
    public static class EventRaiser
    {
        private static readonly BindingFlags ALL = (BindingFlags)(-1);

        public static void RaiseEvent(this object instance, string eventName, params object[] e)
        {
            var type = instance.GetType();
            var eventField = type.GetField(eventName, ALL);

            if (eventField == null) {
                throw new Exception($"Event with name {eventName} could not be found.");
            }

            var multicastDelegate = eventField.GetValue(instance) as MulticastDelegate;
            if (multicastDelegate == null) return;

            var invocationList = multicastDelegate.GetInvocationList();

            List<object> parameters = new();
            // parameters.Add(instance);
            for (int i = 0; i < e.Length; i++) {
                parameters.Add(e[i]);
            }

            object[] finalParams = parameters.ToArray();

            foreach (var invocationMethod in invocationList)
            {
                invocationMethod.DynamicInvoke(finalParams);
            }
        }
    }
}