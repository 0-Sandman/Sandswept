namespace Sandswept.Survivors.Electrician.Achievements
{
    public static class UnlockableDefs
    {
        public static UnlockableDef masteryUnlock;
        public static UnlockableDef charUnlock;

        public static void Init()
        {
            masteryUnlock = new UnlockableDef()
            {
                achievementIcon = null,
                nameToken = "ACHIEVEMENT_ELECTRICIANCLEARGAMEMONSOON_NAME",
                cachedName = "Skins.Electrician.Sandswept"
            };
            ContentAddition.AddUnlockableDef(masteryUnlock);

            LanguageAPI.Add("ACHIEVEMENT_ELECTRICIANCLEARGAMEMONSOON_NAME", "VOL-T: Mastery");
            LanguageAPI.Add("ACHIEVEMENT_ELECTRICIANCLEARGAMEMONSOON_DESCRIPTION", "As VOL-T, beat the game or obliterate on Monsoon.");

            charUnlock = new UnlockableDef()
            {
                achievementIcon = null,
                nameToken = "ACHIEVEMENT_ELECTRICIANREPAIR_NAME",
                cachedName = "Unlocks.Electrician.Sandswept"
            };

            LanguageAPI.Add("ACHIEVEMENT_ELECTRICIANREPAIR_NAME", "Technician");
            LanguageAPI.Add("ACHIEVEMENT_ELECTRICIANREPAIR_DESCRIPTION", "Restart the damaged robot on Sundered Grove.");

            LanguageAPI.Add("SANDSWEPT_BROKENELEC_NAME", "Damaged Robot");

            Sprite elecSprite = Main.assets.LoadAsset<Sprite>("texElectricianIcon2.png");
            charUnlock.achievementIcon = TotallyNotStolenUtils.AddItemIconBackgroundToSprite(elecSprite, TotallyNotStolenUtils.ItemIconBackgroundType.Survivor);

            ContentAddition.AddUnlockableDef(charUnlock);
        }
    }
}