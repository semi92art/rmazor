using System;
using System.Collections.Generic;
using Games.RazorMaze.Models;

namespace Entities
{
    public class SaveKey
    {
        #region factory
        
        public static SaveKey AuthorizedAtLeastOnce    => new SaveKey("authorized_at_least_once", typeof(bool));
        public static SaveKey LastDbConnectionSuccess  => new SaveKey("last_connection_succeeded", typeof(bool));
        public static SaveKey LastIntConnectionSuccess => new SaveKey("last_internet_succeeded", typeof(bool));
        public static SaveKey AccountId                => new SaveKey("account_id", typeof(int?));
        public static SaveKey PreviousAccountId        => new SaveKey("previous_account_id", typeof(int?));
        public static SaveKey NotFirstLaunch           => new SaveKey("not_first_launch", typeof(bool));
        public static SaveKey Login                    => new SaveKey("login", typeof(string));
        public static SaveKey PasswordHash             => new SaveKey("password_hash", typeof(string));
        public static SaveKey GameId                   => new SaveKey("game_id", typeof(int));
        public static SaveKey SettingNotificationsOn   => new SaveKey("notifications_on", typeof(bool));
        public static SaveKey WheelOfFortuneLastDate   => new SaveKey("wof_last_date", typeof(DateTime));
        public static SaveKey DesignerMazeInfo         => new SaveKey("designer_maze_info", typeof(MazeInfo));
        public static SaveKey DesignerSelectedLevel    => new SaveKey("designer_selected_level", typeof(int));
        public static SaveKey DesignerHeapIndex        => new SaveKey("designer_heap_index", typeof(int));
        public static SaveKey PromptHowToRotateShown   => new SaveKey("prompt_how_to_rotate_shown", typeof(bool));
        public static SaveKey ServerUrl                => new SaveKey("debug_server_url", typeof(string));
        public static SaveKey DebugUtilsOn             => new SaveKey("debug", typeof(bool));
        public static SaveKey GoodQuality              => new SaveKey("good_quality", typeof(bool));
        public static SaveKey BoughtPurchaseIds        => new SaveKey("bought_purchase_ids", typeof(List<int>));
        public static SaveKey CurrentLevelIndex        => new SaveKey("current_level_index", typeof(int));
        public static SaveKey DailyBonusLastDate       => new SaveKey("daily_bonus_last_date", typeof(DateTime));
        public static SaveKey SettingSoundOn           => new SaveKey("sound_on", typeof(bool));
        public static SaveKey SettingMusicOn           => new SaveKey("music_on", typeof(bool));
        public static SaveKey SettingVibrationOn       => new SaveKey("vibration_on", typeof(bool));
        public static SaveKey DailyBonusLastClickedDay => new SaveKey("daily_bonus_last_day", typeof(int));
        public static SaveKey DisableAds               => new SaveKey("disable_ads", typeof(bool));
        public static SaveKey GameWasRated             => new SaveKey("game_was_rated", typeof(bool));
        
        public static SaveKey GameDataFieldValue(int _AccountId, int _GameId, ushort _FieldId) =>
            new SaveKey($"df_value_cache_{_AccountId}_{_GameId}_{_FieldId}", typeof(GameDataField));

        #endregion
    
        public string Key { get; }
        public Type Type { get; }
    
        private SaveKey(string _Key, Type _Type)
        {
            Key = _Key;
            Type = _Type;
        }
    }
}