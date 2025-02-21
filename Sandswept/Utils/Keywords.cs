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

        public static string OverdriveFormHeat = "SANDSWEPT_KEYWORD_OVERDRIVE_HEAT";
        public static string OverdriveFormPrimary = "SANDSWEPT_KEYWORD_OVERDRIVE_PRIMARY";
        public static string OverdriveFormSecondary = "SANDSWEPT_KEYWORD_OVERDRIVE_SECONDARY";
        public static string OverdriveFormAltSecondary = "SANDSWEPT_KEYWORD_OVERDRIVE_SECONDARY_ALT";
        public static string OverdriveFormUtility = "SANDSWEPT_KEYWORD_OVERDRIVE_UTILITY";
        public static string OverdriveFormSpecial = "SANDSWEPT_KEYWORD_OVERDRIVE_SPECIAL";

        public static void SetupKeywords()
        {
            OverdriveFormHeat.Add("<style=cKeywordName>Heat Mechanic</style>Passively build up $srheat$se and $srhealing reduction$se, up to $sr100%$se. While in $srfull heat$se, your $sdbase damage$se constantly increases by $sd20%$se, further amplified by $sd10%$se for each $rcCharge$ec, but you take $srincreasingly high self-damage$se.");
            OverdriveFormPrimary.Add("<style=cKeywordName>Overdriven Form</style>Fire a rapid stream of bullets for $sd75% damage$se. $srHeat$se increases $sdfire rate$se and $sdignite chance$se but $srreduces range$se.");
            OverdriveFormSecondary.Add("<style=cKeywordName>Overdriven Form</style>$sdIgnite$se. Fire $sdtwo$se short bursts of $srheat$se for $sd4x200% damage$se each. $sdBurst count$se increases up to $sdfour$se while in $srfull heat$se. $suReduce$se $srheat$se by $su25%$se.");
            OverdriveFormAltSecondary.Add("<style=cKeywordName>Overdriven Form</style>$sdIgnite$se. Fire off a $sdblazing ball$se for $sd600%$se damage that $sdengulfs$se the ground on impact for $sd250%$se damage per second. $suReduce$se $srheat$se by $su50%$se.");
            OverdriveFormUtility.Add("<style=cKeywordName>Overdriven Form</style>$suAgile$se. $sdStunning$se. $suSidestep$se a very short distance and deal $sd250% damage$se. Hitting enemies generates $rc3 Charge$ec and $sdextends$se the dash.");
            OverdriveFormSpecial.Add("<style=cKeywordName>Overdriven Form</style>$suAgile$se. $sdIgnite$se. Release a $sdfire nova$se around you that deals $sd300%$se damage, increasing up to $sd900%$se in $srfull heat$se. $suConsume all$se $srheat$se, gaining an $sdattack speed$se boost, and $suexit overdrive$se.");
        }
    }
}