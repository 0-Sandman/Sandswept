
using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;

namespace Sandswept.Utils {
    public static class EnumeratorExtensions {
        /// <summary>Gets a random element from the collection</summary>
        /// <returns>the chosen element</returns>
        public static T GetRandom<T>(this IEnumerable<T> self) {
            return self.ElementAt(Random.Range(0, self.Count()));
        }

        /// <summary>Gets a random element from the collection</summary>
        /// <param name="rng">the Xoroshiro128Plus instance</param>
        /// <returns>the chosen element</returns>
        public static T GetRandom<T>(this IEnumerable<T> self, Xoroshiro128Plus rng) {
            return self.ElementAt(rng.RangeInt(0, self.Count()));
        }

        /// <summary>Gets a random element from the collection that matches the predicate</summary>
        /// <param name="predicate">the predicate to match</param>
        /// <returns>the chosen element</returns>
        public static T GetRandom<T>(this IEnumerable<T> self, System.Func<T, bool> predicate) {
            try {
                return self.Where(predicate).ElementAt(Random.Range(0, self.Count()));
            }
            catch {
                return default(T);
            }
        }

        /// <summary>Gets a random element from the collection that matches the predicate</summary>
        /// <param name="rng">the Xoroshiro128Plus instance</param>
        /// <param name="predicate">the predicate to match</param>
        /// <returns>the chosen element</returns>
        public static T GetRandom<T>(this IEnumerable<T> self, Xoroshiro128Plus rng, System.Func<T, bool> predicate) {
            try {
                return self.Where(predicate).ElementAt(rng.RangeInt(0, self.Count()));
            }
            catch {
                return default(T);
            }
        }
    }
}