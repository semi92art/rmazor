
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
        
        public const string TestAnalytic               = "test_analytic";
        
        public const string LevelReadyToStart          = "level_ready_to_start";
        public const string LevelStarted               = "level_started";
        public const string CharacterDied              = "death";

        public const string LevelFinished       = "level_finished";
        public const string Purchase            = "purchase";
        public const string AchievementUnlocked = "achievement_unlocked";

        public const string AdShown   = "ad_shown";
        public const string AdClicked = "ad_clicked";
        public const string AdReward  = "ad_reward";

        public static string GetLevelFinishedAnalyticId(long _LevelIndex)
        {
            return $"level_{_LevelIndex}_finished";
        }

        #endregion

        #region parameters

        public const string Parameter1ForTestAnalytic  = "test_analytic_parameter_1";
        public const string ParameterLevelIndex        = "level_index";
        public const string ParameterMoneyCount        = "money_count";
        public const string ParameterDiesCount         = "dies_count";
        public const string ParameterLevelTime         = "level_time";
        public const string ParameterAchievementId     = "achievement_id";
        public const string ParameterPurchaseProductId = "purchase_product_id";
        public const string ParameterPrice             = "price";
        public const string ParameterCurrency          = "currency";
        public const string ParameterAdSource          = "ad_source";
        public const string ParameterAdType            = "ad_type";


        #endregion
    }
}