namespace Sandswept.Survivors.Electrician.Achievements
{
    public static class UnlockableDefs
    {
        public static UnlockableDef masteryUnlock;

        public static void Init()
        {
            masteryUnlock = new UnlockableDef()
            {
                achievementIcon = null,
                nameToken = "ACHIEVEMENT_ELECTRICIANCLEARGAMEMONSOON_NAME",
                cachedName = "Skins.Electrician.Sandswept"
            };
            ContentAddition.AddUnlockableDef(masteryUnlock);

            LanguageAPI.Add("ACHIEVEMENT_ELECTRICIANCLEARGAMEMONSOON_NAME", "Electrician: Mastery");
            LanguageAPI.Add("ACHIEVEMENT_ELECTRICIANCLEARGAMEMONSOON_DESCRIPTION", "As Electrician, beat the game or obliterate on Monsoon.");
        }
    }
}