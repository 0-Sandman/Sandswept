using UnityEngine;
using System;
using Unity;
using System.Linq;
using R2API.Networking.Interfaces;
using System.Threading.Tasks;

namespace Sandswept.Utils
{
    public static class UnityExtensions
    {
        public static void RemoveComponent<T>(this GameObject self) where T : Component
        {
            Object.Destroy(self.GetComponent<T>());
        }

        public static void RemoveComponents<T>(this GameObject self) where T : Component
        {
            T[] coms = self.GetComponents<T>();
            for (int i = 0; i < coms.Length; i++)
            {
                Object.Destroy(coms[i]);
            }
        }

        public static void CallNetworkedMethod(this GameObject self, string method, R2API.Networking.NetworkDestination dest = R2API.Networking.NetworkDestination.Clients) {
            new CallNetworkedMethod(self, method).Send(dest);
        }

        public static T FindComponent<T>(this GameObject self, string name) where T : Component {
            return self.GetComponentsInChildren<T>().FirstOrDefault(x => x.gameObject.name == name);
        }

        public static void RemoveComponent<T>(this Component self) where T : Component
        {
            Object.Destroy(self.GetComponent<T>());
        }

        public static void RemoveComponents<T>(this Component self) where T : Component
        {
            T[] coms = self.GetComponents<T>();
            for (int i = 0; i < coms.Length; i++)
            {
                Object.Destroy(coms[i]);
            }
        }

        public static T AddComponent<T>(this Component self) where T : Component
        {
            return self.gameObject.AddComponent<T>();
        }

        public static T AddComponent<T>(this Component self, Action<T> modification) where T : Component {
            T x = self.gameObject.AddComponent<T>();
            modification(x);
            return x;
        }

        public static T GetComponent<T>(this Component self, Action<T> modification) where T : Component {
            T x = self.gameObject.GetComponent<T>();
            modification(x);
            return x;
        }

        public static T AddComponent<T>(this GameObject self, Action<T> modification) where T : Component {
            T x = self.AddComponent<T>();
            modification(x);
            return x;
        }

        public static T GetComponent<T>(this GameObject self, Action<T> modification) where T : Component {
            T x = self.GetComponent<T>();
            modification(x);
            return x;
        }

        public static void EditComponent<T>(this Component self, Action<T> modification) where T : Component {
            T x = self.GetComponent<T>();
            modification(x);
        }
        public static void EditComponent<T>(this GameObject self, Action<T> modification) where T : Component {
            T x = self.GetComponent<T>();
            modification(x);
        }

        public static Sprite MakeSprite(this Texture2D self)
        {
            return Sprite.Create(new(0, 0, 512, 512), new(512 / 2, 512 / 2), 1, self);
        }

        public static Vector3 Nullify(this Vector3 v, bool x = false, bool y = false, bool z = false) {
            return new Vector3(x ? 0f : v.x, y ? 0f : v.y, z ? 0f : v.z);
        }
    }
}