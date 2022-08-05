
namespace Common.Constants
{
    public static class AnalyticIds
    {
        #region analytics

        public const string ShopButtonPressed                  = "shop_button_pressed";
        public const string SettingsButtonPressed              = "settings_button_pressed";
        public const string RateGameButton1Pressed             = "rate_game_button_1_pressed";
        public const string RateGameButton2Pressed             = "rate_game_button_2_pressed";
        public const string LeaderboardsButtonPressed          = "leaderboards_button_2_pressed";
        public const string LanguageButtonPressed              = "language_button_2_pressed";
        public const string EnableMusicButtonPressed           = "enable_music_button_pressed";
        public const string DisableMusicButtonPressed          = "disable_music_button_pressed";
        public const string EnableSoundButtonPressed           = "enable_sound_button_pressed";
        public const string DisableSoundButtonPressed          = "disable_sound_button_pressed";
        public const string EnableHapticsButtonPressed         = "enable_sound_button_pressed";
        public const string DisableHapticsButtonPressed        = "disable_sound_button_pressed";
        public const string RestorePurchasesButtonPressed      = "restore_purchases_button_pressed";
        public const string WatchAdInCharacterDiedPanelPressed = "watch_ad_in_character_died_panel_pressed";
        public const string WatchAdInShopPanelPressed          = "watch_ad_in_shop_panel_pressed";
        public const string WatchAdInFinishGroupPanelPressed   = "watch_ad_in_finish_group_panel_pressed";
        
        public const string TestAnalytic      = "test_analytic";
        public const string LevelReadyToStart = "level_ready_to_start";
        public const string LevelStarted      = "level_started";
        public const string CharacterDied     = "death";

        public const string LevelFinished       = "level_finished";
        public const string Purchase            = "purchase";
        public const string AchievementUnlocked = "achievement_unlocked";

        public const string AdShown   = "ad_shown";
        public const string AdClicked = "ad_clicked";
        
        private const string Level1Finished    = "level_1_finished";
        private const string Level2Finished    = "level_2_finished";
        private const string Level3Finished    = "level_3_finished";
        private const string Level4Finished    = "level_4_finished";
        private const string Level5Finished    = "level_5_finished";
        private const string Level6Finished    = "level_6_finished";
        private const string Level7Finished    = "level_7_finished";
        private const string Level8Finished    = "level_8_finished";
        private const string Level9Finished    = "level_9_finished";
        private const string Level10Finished   = "level_10_finished";
        private const string Level20Finished   = "level_20_finished";
        private const string Level30Finished   = "level_30_finished";
        private const string Level40Finished   = "level_40_finished";
        private const string Level50Finished   = "level_50_finished";
        private const string Level60Finished   = "level_60_finished";
        private const string Level70Finished   = "level_70_finished";
        private const string Level80Finished   = "level_80_finished";
        private const string Level90Finished   = "level_90_finished";
        private const string Level100Finished  = "level_100_finished";
        private const string Level200Finished  = "level_200_finished";
        private const string Level300Finished  = "level_300_finished";
        private const string Level400Finished  = "level_400_finished";
        private const string Level500Finished  = "level_500_finished";
        private const string Level600Finished  = "level_600_finished";
        private const string Level700Finished  = "level_700_finished";
        private const string Level800Finished  = "level_800_finished";
        private const string Level900Finished  = "level_900_finished";
        private const string Level1000Finished = "level_1000_finished";

        public static string GetLevelFinishedAnalyticId(long _LevelIndex)
        {
            string anId = _LevelIndex switch
            {
                1    => Level1Finished,
                2    => Level2Finished,
                3    => Level3Finished,
                4    => Level4Finished,
                5    => Level5Finished,
                6    => Level6Finished,
                7    => Level7Finished,
                8    => Level8Finished,
                9    => Level9Finished,
                10   => Level10Finished,
                20   => Level20Finished,
                30   => Level30Finished,
                40   => Level40Finished,
                50   => Level50Finished,
                60   => Level60Finished,
                70   => Level70Finished,
                80   => Level80Finished,
                90   => Level90Finished,
                100  => Level100Finished,
                200  => Level200Finished,
                300  => Level300Finished,
                400  => Level400Finished,
                500  => Level500Finished,
                600  => Level600Finished,
                700  => Level700Finished,
                800  => Level800Finished,
                900  => Level900Finished,
                1000 => Level1000Finished,
                _    => null
            };
            return anId;
        }

        #endregion

        #region parameters

        public const string LevelIndex        = "level_index";
        public const string MoneyCount        = "money_count";
        public const string DiesCount         = "dies_count";
        public const string LevelTime         = "level_time";
        public const string AchievementId     = "achievement_id";
        public const string PurchaseProductId = "purchase_product_id";
        public const string Price             = "price";
        public const string Currency          = "currency";
        public const string AdSource          = "ad_source";
        public const string AdType            = "ad_type";


        #endregion
    }
}