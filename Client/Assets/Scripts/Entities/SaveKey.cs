using System;
using System.Collections.Generic;
using Games.RazorMaze.Models;

namespace Entities
{
    public static class SaveKeys
    {
        public static SaveKey<bool?> DisableAds               => new SaveKey<bool?>("disable_ads");
        public static SaveKey<bool>  AuthorizedAtLeastOnce    => new SaveKey<bool>("authorized_at_least_once");
        public static SaveKey<bool>  LastDbConnectionSuccess  => new SaveKey<bool>("last_connection_succeeded");
        public static SaveKey<bool>  LastIntConnectionSuccess => new SaveKey<bool>("last_internet_succeeded");
        public static SaveKey<bool>  NotFirstLaunch           => new SaveKey<bool>("not_first_launch");
        public static SaveKey<bool>  SettingNotificationsOn   => new SaveKey<bool>("notifications_on");
        public static SaveKey<bool>  PromptHowToRotateShown   => new SaveKey<bool>("prompt_how_to_rotate_shown");
        public static SaveKey<bool>  DebugUtilsOn             => new SaveKey<bool>("debug");
        public static SaveKey<bool>  GoodQuality              => new SaveKey<bool>("good_quality");
        public static SaveKey<bool>  AllLevelsPassed          => new SaveKey<bool>("all_levels_passed");
        public static SaveKey<bool>  SettingSoundOn           => new SaveKey<bool>("sound_on");
        public static SaveKey<bool>  SettingMusicOn           => new SaveKey<bool>("music_on");
        public static SaveKey<bool>  SettingHapticsOn         => new SaveKey<bool>("haptics_on");
        public static SaveKey<bool>  GameWasRated             => new SaveKey<bool>("game_was_rated");
        
        public static SaveKey<int?> PreviousAccountId        => new SaveKey<int?>("previous_account_id");
        public static SaveKey<int?> AccountId                => new SaveKey<int?>("account_id");
        public static SaveKey<int>  GameId                   => new SaveKey<int>("game_id");
        public static SaveKey<int>  DesignerSelectedLevel    => new SaveKey<int>("designer_selected_level");
        public static SaveKey<int>  DesignerHeapIndex        => new SaveKey<int>("designer_heap_index");
        public static SaveKey<int>  DailyBonusLastClickedDay => new SaveKey<int>("daily_bonus_last_day");
        public static SaveKey<int>  RatePanelShowsCount      => new SaveKey<int>("rate_panel_shows_count");
        
        public static SaveKey<string> Login        => new SaveKey<string>("login");
        public static SaveKey<string> PasswordHash => new SaveKey<string>("password_hash");
        public static SaveKey<string> ServerUrl    => new SaveKey<string>("debug_server_url");
        
        public static SaveKey<DateTime>  WheelOfFortuneLastDate => new SaveKey<DateTime>("wof_last_date");
        public static SaveKey<MazeInfo>  DesignerMazeInfo       => new SaveKey<MazeInfo>("designer_maze_info");
        public static SaveKey<List<int>> BoughtPurchaseIds      => new SaveKey<List<int>>("bought_purchase_ids");
        public static SaveKey<DateTime>  DailyBonusLastDate     => new SaveKey<DateTime>("daily_bonus_last_date");
        
        public static SaveKey<GameDataField> GameDataFieldValue(int _AccountId, int _GameId, ushort _FieldId) =>
            new SaveKey<GameDataField>($"df_value_cache_{_AccountId}_{_GameId}_{_FieldId}");
    }
    
    public class SaveKey<T>
    {
        public string Key { get; }
    
        public SaveKey(string _Key)
        {
            Key = _Key;
        }
    }
}