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

        public static string Surging = "SANDSWEPT_KEYWORD_SURGING";
        public static string OverdriveFormPrimary = "SANDSWEPT_KEYWORD_OVERDRIVE_PRIMARY";
        public static string OverdriveFormSecondary = "SANDSWEPT_KEYWORD_OVERDRIVE_SECONDARY";
        public static string OverdriveFormAltSecondary = "SANDSWEPT_KEYWORD_OVERDRIVE_SECONDARY_ALT";
        public static string OverdriveFormUtility = "SANDSWEPT_KEYWORD_OVERDRIVE_UTILITY";
        public static string OverdriveFormSpecial = "SANDSWEPT_KEYWORD_OVERDRIVE_SPECIAL";

        public static void SetupKeywords()
        {
            Surging.Add("$knSurging$se$suDamage and blast radius increase with distance.$se".AutoFormat());
            OverdriveFormPrimary.Add("$knOverdriven Form$se$suAgile$se. Fire a rapid stream of bullets for $sd90% damage$se. $suFire rate and ignite chance increase with heat$se.".AutoFormat());
            OverdriveFormSecondary.Add("$knOverdriven Form$se$sdIgnite$se. $suReduce current heat by 50%$se. Fire a spread of heat for $sd4x300% damage$se. $suBurst count increases with heat spent$se.".AutoFormat());
            OverdriveFormAltSecondary.Add("$knOverdriven Form$se$sdIgnite$se. $suReduce current heat by 50%$se. Fire off a blazing orb for $sd200%$se damage that $sdengulfs$se the ground on impact for $sd200%$se damage per second. $suDamage increases with heat spent$se.".AutoFormat());
            OverdriveFormUtility.Add("$knOverdriven Form$se$suAgile$se. $sdStunning$se. $suSidestep$se a short distance and deal $sd390% damage$se. Hitting enemies generates $rc5 Charge$ec and $sdextends$se the dash. $suDash distance increases with heat$se.".AutoFormat());
            OverdriveFormSpecial.Add("$knOverdriven Form$se$suAgile$se. $sdIgnite$se. $suConsume all heat and exit overdrive$se. Release a $sdfire nova$se around you that deals $sd300%$se damage. $suDamage increases with heat spent$se.".AutoFormat());
        }
    }
}