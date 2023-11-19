using System;

namespace Sandswept.Utils
{
    public static class Keywords
    {
        public static string Poison = "KEYWORD_POISON";
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

        public static string OverdriveFormPrimary = "SANDSWEPT_KEYWORD_OVERDRIVE_PRIMARY";
        public static string OverdriveFormSecondary = "SANDSWEPT_KEYWORD_OVERDRIVE_SECONDARY";
        public static string OverdriveFormUtility = "SANDSWEPT_KEYWORD_OVERDRIVE_UTILITY";
        public static string OverdriveFormSpecial = "SANDSWEPT_KEYWORD_OVERDRIVE_SPECIAL";

        [AutoRun]
        public static void SetupKeywords()
        {
            OverdriveFormPrimary.Add("<style=cKeywordName>Overdrive Form</style><style=cIsUtility>Agile</style>. Fire a rapid stream of bullets for <style=cIsDamage>90% damage</style>. <style=cDeath>Heat increases spread and ignite chance</style>.");
            OverdriveFormSecondary.Add("<style=cKeywordName>Overdrive Form</style><style=cIsDamage>Ignite</style>. Fire a short-range heat spread for <style=cIsDamage>8x200% damage</style>. Gain <style=cDeath>15% heat</style>.");
            OverdriveFormUtility.Add("<style=cKeywordName>Overdrive Form</style><style=cIsUtility>Agile</style>. <style=cIsDamage>Stunning</style>. <style=cIsUtility>Sidestep</style> a very short distance and deal <style=cIsDamage>250% damage</style>. Hitting enemies generates <color=#36D7A9>3 Charge</style>.");
            OverdriveFormSpecial.Add("<style=cKeywordName>Overdrive Form</style><style=cIsUtility>Agile</style>. <style=cIsDamage>Ignite</style>. Release a <style=cIsDamage>fire nova</style> around you that deals <style=cIsDamage>300%</style> damage, increasing up to <style=cIsDamage>900%</style> in full heat. <style=cIsUtility>Consume all heat</style>, gaining an <style=cIsDamage>attack speed</style> boost, and <style=cDeath>exit overdrive</style>.");
        }
    }
}