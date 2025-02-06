namespace Sandswept.Survivors.Ranger.Achievements
{
    public static class UnlockableDefs
    {
        public static UnlockableDef masteryUnlock;

        public static void Init()
        {
            masteryUnlock = new UnlockableDef()
            {
                achievementIcon = Skins.CreateSkinIcon(
                new Color32(228, 146, 55, 255),
                new Color32(201, 178, 143, 255),
                new Color32(74, 79, 77, 255),
                new Color32(108, 68, 45, 255)),
                nameToken = "ACHIEVEMENT_RANGERCLEARGAMEMONSOON_NAME",
                cachedName = "Skins.Ranger.Sandswept"
            };

            ContentAddition.AddUnlockableDef(masteryUnlock);

            LanguageAPI.Add("ACHIEVEMENT_RANGERCLEARGAMEMONSOON_NAME", "Ranger: Mastery");
            LanguageAPI.Add("ACHIEVEMENT_RANGERCLEARGAMEMONSOON_DESCRIPTION", "As Ranger, beat the game or obliterate on Monsoon.");
        }
    }
}