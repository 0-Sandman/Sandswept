using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine.AddressableAssets;

namespace Sandswept.Utils
{
    public static class StringExtensions
    {
        /// <summary>Adds a string-value pair to the ROR2 language strings</summary>
        /// <param name="text">the value when the string is searched</param>
        public static string Add(this string str, string text)
        {
            LanguageAPI.Add(str, text);
            return str;
        }

        /// <summary>Adds a string-value pair to the ROR2 language strings via an overlay</summary>
        /// <param name="text">the value when the string is searched</param>
        public static void AddOverlay(this string str, string text)
        {
            LanguageAPI.AddOverlay(str, text);
        }

        /// <summary>Attempts to load the string through AddressableAssets</summary>
        /// <returns>the asset if it was found, otherwise returns default(T)</returns>
        public static T Load<T>(this string str)
        {
            try
            {
                T asset = Addressables.LoadAssetAsync<T>(str).WaitForCompletion();
                return asset;
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Filters unsafe characters from a string. Default characters: \n ' (whitespace) ! ` - () {} | @ . \
        /// </summary>
        /// <returns>the filtered string</returns>
        public static string RemoveUnsafeCharacters(this string str)
        {
            string[] unsafeCharacters = { "\n", "'", " ", "!", "`", "&", "-", ")", "(", "{", "}", "|", "@", "<", ">", ".", "\\" };
            string filtered = str;

            foreach (string c in unsafeCharacters)
            {
                filtered = filtered.Replace(c, "");
            }

            return filtered;
        }

        /// <summary>
        /// Filters unsafe characters from a string.
        /// </summary>
        /// <param name="unsafeChars">an array of characters to filter out</param>
        /// <returns>the filtered string</returns>
        public static string RemoveUnsafeCharacters(this string str, string[] unsafeChars)
        {
            string filtered = str;

            foreach (string c in unsafeChars)
            {
                filtered = filtered.Replace(c, "");
            }

            return filtered;
        }

        /// <summary>
        /// Uses a list of pre-defined matches to attempt to automatically format a string
        /// </summary>
        /// <returns>the formatted string</returns>
        public static string AutoFormat(this string str)
        {
            return Formatter.FormatString(str);
        }
    }

    internal class Formatter
    {
        internal struct Format
        {
            public string match;
            public string expanded;
        }

        private static List<Format> formats = new()
        {
            new Format
            {
                match = "$su",
                expanded = "<style=cIsUtility>"
            },
            new Format
            {
                match = "$sd",
                expanded = "<style=cIsDamage>"
            },
            new Format {
                match = "$ss",
                expanded = "<style=cStack>"
            },
            new Format {
                match = "$sr",
                expanded = "<style=cDeath>"
            },
            new Format {
                match = "$sh",
                expanded = "<style=cIsHealing>"
            },
            new Format {
                match = "$se",
                expanded = "</style>"
            },
            new Format
            {
                match = "$rc",
                expanded = "<color=#36D7A9>"
            },
            new Format
            {
                match = "$ec",
                expanded = "</color>"
            },
            new Format
            {
                match = "$pc",
                expanded = "<color=#406096>"
            },
            new Format
            {
                match = "$sv",
                expanded = "<style=cIsVoid>"
            },
            new Format
            {
                match = "$lc",
                expanded = "<color=#FF7F7F>"
            },
            new Format
            {
                match = "$kn",
                expanded = "<style=cKeywordName>"
            }
        };

        internal static string FormatString(string str)
        {
            foreach (Format format in formats)
            {
                str = str.Replace(format.match, format.expanded);
            }

            return str;
        }
    }
}