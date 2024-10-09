namespace Sandswept.Survivors.Ranger.Achievements
{
    public static class UnlockableDefs
    {
        public static UnlockableDef masteryUnlock;

        public static void Init()
        {
            masteryUnlock = new UnlockableDef()
            {
                achievementIcon = null,
                nameToken = "ACHIEVEMENT_RANGERCLEARGAMEMONSOON_NAME",
                cachedName = "Skins.Ranger.Sandswept"
            };
            ContentAddition.AddUnlockableDef(masteryUnlock);

            LanguageAPI.Add("ACHIEVEMENT_RANGERCLEARGAMEMONSOON_NAME", "Ranger: Mastery");
            LanguageAPI.Add("ACHIEVEMENT_RANGERCLEARGAMEMONSOON_DESCRIPTION", "As Ranger, beat the game or obliterate on Monsoon.");
        }
    }
}