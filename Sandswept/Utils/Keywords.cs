using System;

namespace Sandswept.Utils {
    public static class Keywords {
        public static string Poison =  "KEYWORD_POISON";
        public static string Regenerative = "KEYWORD_RAPID_REGEN";
        public static string Agile = "KEYWORD_AGILE";
        public static string HealthCost = "KEYWORD_PERCENT_HP";
        public static string Disperse = "KEYWORD_SONIC_BOOM";
        public static string Weak = "KEYWORD_WEAK";
        public static string Heavy = "KEYWORD_HEAVY";
        public static string Freeze = "KEYWORD_FREEZING";
        public static string Stun = "KEYWORD_STUNNING";
        public static string Expose = "KEYWORD_EXPOSE";
        public static string Shock = "KEYWORD_SHOCKING";
        public static string Slayer = "KEYWORD_SLAYER";
        public static string Hemorrhage = "KEYWORD_SUPERBLEED";
        public static string Ignite = "KEYWORD_IGNITE";
        public static string Weakpoint = "KEYWORD_WEAKPOINT";
        public static string ActiveReload = "KEYWORD_ACTIVERELOAD";
        public static string VoidCorruption = "KEYWORD_VOIDCORRUPTION";
        // custom
        public static string Charged = "SANDSWEPT_KEYWORD_CHARGED";

        [AutoRun]
        public static void SetupKeywords() {
            Charged.Add("<style=cKeywordName>Charged</style>Each stack of <style=cIsUtility>Charge</style> gained reduces the cooldown of this skill by <style=cIsDamage>1 second</style>");
        }
    }
}