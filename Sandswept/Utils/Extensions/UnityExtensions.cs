using UnityEngine;
using System;
using Unity;
using System.Linq;
using R2API.Networking.Interfaces;
using System.Threading.Tasks;
using HG;

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

        public static void CallNetworkedMethod(this GameObject self, string method, R2API.Networking.NetworkDestination dest = R2API.Networking.NetworkDestination.Clients)
        {
            new CallNetworkedMethod(self, method).Send(dest);
        }

        public static T FindComponent<T>(this GameObject self, string name) where T : Component
        {
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

        public static T AddComponent<T>(this Component self, Action<T> modification) where T : Component
        {
            T x = self.gameObject.AddComponent<T>();
            modification(x);
            return x;
        }

        public static T GetComponent<T>(this Component self, Action<T> modification) where T : Component
        {
            T x = self.gameObject.GetComponent<T>();
            modification(x);
            return x;
        }

        public static T AddComponent<T>(this GameObject self, Action<T> modification) where T : Component
        {
            T x = self.AddComponent<T>();
            modification(x);
            return x;
        }

        public static T GetComponent<T>(this GameObject self, Action<T> modification) where T : Component
        {
            T x = self.GetComponent<T>();
            modification(x);
            return x;
        }

        public static void EditComponent<T>(this Component self, Action<T> modification) where T : Component
        {
            T x = self.GetComponent<T>();
            modification(x);
        }
        public static void EditComponent<T>(this GameObject self, Action<T> modification) where T : Component
        {
            T x = self.GetComponent<T>();
            modification(x);
        }

        public static void GetOrAddComponent<T>(this GameObject self, Action<T> modification) where T : Component
        {
            T x = self.EnsureComponent<T>();
            modification(x);

        }

        public static Texture2D ToTexture2D(this Texture texture)
        {
            return Texture2D.CreateExternalTexture(
                texture.width,
                texture.height,
                TextureFormat.RGB24,
                false, false,
                texture.GetNativeTexturePtr());
        }

        public static Sprite MakeSprite(this Texture2D tex)
        {
            // unity absoutely sucks so we need to make a square sprite and pad it manually to preserve aspect ratio
            int width = tex.width;
            int height = tex.height;
            int padding = Math.Max(width, height);

            int xOff = (padding - width) / 2;
            int yOff = (padding - height) / 2;

            Texture2D padded = tex;
            if (width != height)
            {
                padded = new Texture2D(padding, padding, tex.format, false);
                Color transparent = new Color(0, 0, 0, 0);

                for (int y = 0; y < padding; y++)
                {
                    for (int x = 0; x < padding; x++) padded.SetPixel(x, y, transparent);
                }

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++) padded.SetPixel(x + xOff, y + yOff, tex.GetPixel(x, y));
                }

                padded.Apply(false, false);
            }

            return Sprite.Create(padded, new Rect(0f, 0f, padded.width, padded.height), new(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        }

        public static Vector3 Nullify(this Vector3 v, bool x = false, bool y = false, bool z = false)
        {
            return new Vector3(x ? 0f : v.x, y ? 0f : v.y, z ? 0f : v.z);
        }
    }
}