using System;
using System.Collections.Generic;

namespace Entities
{
    public static class SaveKeys
    {
        private static SaveKey<bool?>     _disableAds;
        private static SaveKey<bool>      _settingSoundOn;
        private static SaveKey<bool>      _settingMusicOn;
        private static SaveKey<bool>      _lastDbConnectionSuccess;
        private static SaveKey<bool>      _notFirstLaunch;
        private static SaveKey<bool>      _settingNotificationsOn;
        private static SaveKey<bool>      _debugUtilsOn;
        private static SaveKey<bool>      _goodQuality;
        private static SaveKey<bool>      _allLevelsPassed;
        private static SaveKey<bool>      _settingHapticsOn;
        private static SaveKey<bool>      _gameWasRated;
        private static SaveKey<bool>      _movementTutorialFinished;
        private static SaveKey<bool>      _rotationTutorialFinished;
        private static SaveKey<bool>      _enableRotation;
        private static SaveKey<int?>      _previousAccountId;        
        private static SaveKey<int?>      _accountId;               
        private static SaveKey<int>       _gameId;                   
        private static SaveKey<int>       _dailyBonusLastClickedDay; 
        private static SaveKey<int>       _ratePanelShowsCount;
        private static SaveKey<string>    _login;        
        private static SaveKey<string>    _passwordHash;
        private static SaveKey<DateTime>  _wheelOfFortuneLastDate; 
        private static SaveKey<List<int>> _boughtPurchaseIds;      
        private static SaveKey<DateTime>  _dailyBonusLastDate;     
        
        private static readonly Dictionary<Tuple<int, int, ushort>, SaveKey<GameDataField>> GameDataFieldValues = 
            new Dictionary<Tuple<int, int, ushort>, SaveKey<GameDataField>>();
        private static readonly Dictionary<string, SaveKey<uint>> BundleVersions =
            new Dictionary<string, SaveKey<uint>>();
        
        
        public static SaveKey<bool?> DisableAds               => _disableAds ??= new SaveKey<bool?>("disable_ads");
        public static SaveKey<bool>  SettingSoundOn           => _settingSoundOn ??= new SaveKey<bool>("sound_on");
        public static SaveKey<bool>  SettingMusicOn           => _settingMusicOn ??= new SaveKey<bool>("music_on");
        public static SaveKey<bool>  LastDbConnectionSuccess  => _lastDbConnectionSuccess ??= new SaveKey<bool>("last_connection_succeeded");
        public static SaveKey<bool>  NotFirstLaunch           => _notFirstLaunch ??= new SaveKey<bool>("not_first_launch");
        public static SaveKey<bool>  SettingNotificationsOn   => _settingNotificationsOn ??= new SaveKey<bool>("notifications_on");
        public static SaveKey<bool>  DebugUtilsOn             => _debugUtilsOn ??= new SaveKey<bool>("debug");
        public static SaveKey<bool>  GoodQuality              => _goodQuality ??= new SaveKey<bool>("good_quality");
        public static SaveKey<bool>  AllLevelsPassed          => _allLevelsPassed ??= new SaveKey<bool>("all_levels_passed");
        public static SaveKey<bool>  SettingHapticsOn         => _settingHapticsOn ??= new SaveKey<bool>("haptics_on");
        public static SaveKey<bool>  GameWasRated             => _gameWasRated ??= new SaveKey<bool>("game_was_rated");
        public static SaveKey<bool>  MovementTutorialFinished => _movementTutorialFinished ??= new SaveKey<bool>("mov_tut_finished");
        public static SaveKey<bool>  RotationTutorialFinished => _rotationTutorialFinished ??= new SaveKey<bool>("rot_tut_finished");
        public static SaveKey<bool>  EnableRotation           => _enableRotation ??= new SaveKey<bool>("enable_rotation");
        
        public static SaveKey<int?> PreviousAccountId        => _previousAccountId ??= new SaveKey<int?>("previous_account_id");
        public static SaveKey<int?> AccountId                => _accountId ??= new SaveKey<int?>("account_id");
        public static SaveKey<int>  GameId                   => _gameId ??= new SaveKey<int>("game_id");
        public static SaveKey<int>  DailyBonusLastClickedDay => _dailyBonusLastClickedDay ??= new SaveKey<int>("daily_bonus_last_day");
        public static SaveKey<int>  RatePanelShowsCount      => _ratePanelShowsCount ??= new SaveKey<int>("rate_panel_shows_count");
        
        public static SaveKey<string> Login        => _login ??= new SaveKey<string>("login");
        public static SaveKey<string> PasswordHash => _passwordHash ??= new SaveKey<string>("password_hash");

        public static SaveKey<DateTime>  WheelOfFortuneLastDate => _wheelOfFortuneLastDate ??= new SaveKey<DateTime>("wof_last_date");
        public static SaveKey<List<int>> BoughtPurchaseIds      => _boughtPurchaseIds ??= new SaveKey<List<int>>("bought_purchase_ids");
        public static SaveKey<DateTime>  DailyBonusLastDate     => _dailyBonusLastDate ??= new SaveKey<DateTime>("daily_bonus_last_date");
        
        public static SaveKey<GameDataField> GameDataFieldValue(int _AccountId, int _GameId, ushort _FieldId)
        {
            var key = new Tuple<int, int, ushort>(_AccountId, _GameId, _FieldId);
            if (GameDataFieldValues.ContainsKey(key))
                return GameDataFieldValues[key];
            var saveKey = new SaveKey<GameDataField>($"df_value_cache_{_AccountId}_{_GameId}_{_FieldId}");
            GameDataFieldValues.Add(key, saveKey);
            return saveKey;
        }

        public static SaveKey<uint> BundleVersion(string _BundleName)
        {
            if (BundleVersions.ContainsKey(_BundleName))
                return BundleVersions[_BundleName];
            var saveKey = new SaveKey<uint>($"bundle_version_{_BundleName}");
            BundleVersions.Add(_BundleName, saveKey);
            return saveKey;
        } 
    }
}