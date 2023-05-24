using UnityEngine;
using System;
using Unity;

namespace Sandswept.Utils {
    public static class UnityExtensions {
        public static void RemoveComponent<T>(this GameObject self) where T : Component {
            GameObject.Destroy(self.GetComponent<T>());
        }

        public static void RemoveComponents<T>(this GameObject self) where T : Component {
            T[] coms = self.GetComponents<T>();
            for (int i = 0; i < coms.Length; i++) {
                GameObject.Destroy(coms[i]);
            }
        }

        public static void RemoveComponent<T>(this Component self) where T : Component {
            GameObject.Destroy(self.GetComponent<T>());
        }

        public static void RemoveComponents<T>(this Component self) where T : Component {
            T[] coms = self.GetComponents<T>();
            for (int i = 0; i < coms.Length; i++) {
                GameObject.Destroy(coms[i]);
            }
        }

        public static T AddComponent<T>(this Component self) where T : Component {
            return self.gameObject.AddComponent<T>();
        }

        public static Sprite MakeSprite(this Texture2D self) {
            return Sprite.Create(new(0, 0, 512, 512), new(512 / 2, 512 / 2), 1, self);
        }
    }
}