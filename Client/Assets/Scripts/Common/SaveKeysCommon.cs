using System;
using System.Collections.Generic;
using Common.Entities;
using Common.Utils;
using UnityEngine;

namespace Common
{
    public static class SaveKeysCommon
    {
        private static SaveKey<bool>         _gameWasRated;
        private static SaveKey<bool>         _settingNotificationsOn;
        private static SaveKey<bool>         _settingHapticsOn;
        private static SaveKey<bool>         _settingSoundOn;
        private static SaveKey<bool>         _settingMusicOn;
        private static SaveKey<bool?>        _disableAds;
        private static SaveKey<bool>         _lastDbConnectionSuccess;
        private static SaveKey<bool>         _notFirstLaunch;
        private static SaveKey<bool>         _debugUtilsOn;
        private static SaveKey<int?>         _accountId;               
        private static SaveKey<string>       _login;        
        private static SaveKey<string>       _passwordHash;
        private static SaveKey<int?>         _previousAccountId; 
        private static SaveKey<List<int>>    _boughtPurchaseIds;
        private static SaveKey<List<string>> _debugConsoleCommandsHistory;
        private static SaveKey<bool>         _lowPerformanceDevice;
        private static SaveKey<List<int>>    _scheduleNotificationIds;
        private static SaveKey<string>       _appVersion;
        
        private static readonly Dictionary<string, SaveKey<uint>> BundleVersions =
            new Dictionary<string, SaveKey<uint>>();
        private static readonly Dictionary<Tuple<int, int, ushort>, SaveKey<GameDataField>> GameDataFieldValues = 
            new Dictionary<Tuple<int, int, ushort>, SaveKey<GameDataField>>();

        [RuntimeInitializeOnLoadMethod]
        public static void ResetState()
        {
            SetAllToNull();
            CacheFromDisc();
        }

        public static SaveKey<GameDataField> GameDataFieldValue(int _AccountId, int _GameId, ushort _FieldId)
        {
            var key = new Tuple<int, int, ushort>(_AccountId, _GameId, _FieldId);
            if (GameDataFieldValues.ContainsKey(key))
                return GameDataFieldValues[key];
            var saveKey = new SaveKey<GameDataField>($"df_value_cache_{_AccountId}_{_GameId}_{_FieldId}");
            GameDataFieldValues.Add(key, saveKey);
            return saveKey;
        }
        
        public static SaveKey<bool> GameWasRated               => 
            _gameWasRated ??= new SaveKey<bool>(nameof(GameWasRated));
        public static SaveKey<bool> SettingNotificationsOn     =>
            _settingNotificationsOn ??= new SaveKey<bool>(nameof(SettingNotificationsOn));
        public static SaveKey<bool> SettingHapticsOn           => 
            _settingHapticsOn ??= new SaveKey<bool>(nameof(SettingHapticsOn));
        public static SaveKey<bool> SettingSoundOn             => 
            _settingSoundOn ??= new SaveKey<bool>(nameof(SettingSoundOn));
        public static SaveKey<bool>  SettingMusicOn            => 
            _settingMusicOn ??= new SaveKey<bool>(nameof(SettingMusicOn));
        public static SaveKey<bool?>  DisableAds               =>
            _disableAds ??= new SaveKey<bool?>(nameof(DisableAds));
        public static SaveKey<bool>   LastDbConnectionSuccess  => 
            _lastDbConnectionSuccess ??= new SaveKey<bool>(nameof(LastDbConnectionSuccess));
        public static SaveKey<bool>   NotFirstLaunch           =>
            _notFirstLaunch ??= new SaveKey<bool>(nameof(NotFirstLaunch));
        public static SaveKey<bool>   DebugUtilsOn             => 
            _debugUtilsOn ??= new SaveKey<bool>(nameof(DebugUtilsOn));
        public static SaveKey<int?>   AccountId                =>
            _accountId ??= new SaveKey<int?>(nameof(AccountId));
        public static SaveKey<string> Login                    => 
            _login ??= new SaveKey<string>(nameof(Login));
        public static SaveKey<string> PasswordHash             => 
            _passwordHash ??= new SaveKey<string>(nameof(PasswordHash));
        public static SaveKey<int?>   PreviousAccountId        =>
            _previousAccountId ??= new SaveKey<int?>(nameof(PreviousAccountId));
        public static SaveKey<string> ServerUrl                =>
            new SaveKey<string>(nameof(ServerUrl));
        public static SaveKey<string> AppVersion               =>
            _appVersion ??= new SaveKey<string>(nameof(AppVersion));
        public static SaveKey<List<int>> BoughtPurchaseIds     => 
            _boughtPurchaseIds ??= new SaveKey<List<int>>(nameof(BoughtPurchaseIds));
        public static SaveKey<List<string>> DebugConsoleCommandsHistory =>
            _debugConsoleCommandsHistory ??= new SaveKey<List<string>>(nameof(DebugConsoleCommandsHistory));
        public static SaveKey<bool> LowPerformanceDevice       =>
            _lowPerformanceDevice ??= new SaveKey<bool>(nameof(LowPerformanceDevice));

        public static SaveKey<List<int>> ScheduleNotificationIds =>
            _scheduleNotificationIds ??= new SaveKey<List<int>>(nameof(ScheduleNotificationIds));

        
        public static SaveKey<uint> BundleVersion(string _BundleName)
        {
            if (BundleVersions.ContainsKey(_BundleName))
                return BundleVersions[_BundleName];
            var saveKey = new SaveKey<uint>($"bundle_version_{_BundleName}");
            BundleVersions.Add(_BundleName, saveKey);
            return saveKey;
        }

        private static void SetAllToNull()
        {
            _gameWasRated                      = null;
            _settingNotificationsOn            = null;
            _settingHapticsOn                  = null;
            _settingSoundOn                    = null;
            _settingMusicOn                    = null;
            _disableAds                        = null;
            _lastDbConnectionSuccess           = null;
            _notFirstLaunch                    = null;
            _debugUtilsOn                      = null;
            _accountId                         = null;
            _login                             = null;
            _passwordHash                      = null;
            _previousAccountId                 = null;
            _boughtPurchaseIds                 = null;
            _debugConsoleCommandsHistory       = null;
            _lowPerformanceDevice              = null;
            _scheduleNotificationIds           = null;
            _appVersion                        = null;
        }

        private static void CacheFromDisc()
        {
            const bool onlyCache = true;
            SaveUtils.PutValue(GameWasRated,                SaveUtils.GetValue(GameWasRated),               onlyCache);
            SaveUtils.PutValue(SettingNotificationsOn,      SaveUtils.GetValue(SettingNotificationsOn),     onlyCache);
            SaveUtils.PutValue(SettingHapticsOn,            SaveUtils.GetValue(SettingHapticsOn),           onlyCache);
            SaveUtils.PutValue(SettingSoundOn,              SaveUtils.GetValue(SettingSoundOn),             onlyCache);
            SaveUtils.PutValue(SettingMusicOn,              SaveUtils.GetValue(SettingMusicOn),             onlyCache);
            SaveUtils.PutValue(DisableAds,                  SaveUtils.GetValue(DisableAds),                 onlyCache);
            SaveUtils.PutValue(LastDbConnectionSuccess,     SaveUtils.GetValue(LastDbConnectionSuccess),    onlyCache);
            SaveUtils.PutValue(NotFirstLaunch,              SaveUtils.GetValue(NotFirstLaunch),             onlyCache);
            SaveUtils.PutValue(DebugUtilsOn,                SaveUtils.GetValue(DebugUtilsOn),               onlyCache);
            SaveUtils.PutValue(AccountId,                   SaveUtils.GetValue(AccountId),                  onlyCache);
            SaveUtils.PutValue(Login,                       SaveUtils.GetValue(Login),                      onlyCache);
            SaveUtils.PutValue(PasswordHash,                SaveUtils.GetValue(PasswordHash),               onlyCache);
            SaveUtils.PutValue(PreviousAccountId,           SaveUtils.GetValue(PreviousAccountId),          onlyCache);
            SaveUtils.PutValue(BoughtPurchaseIds,           SaveUtils.GetValue(BoughtPurchaseIds),          onlyCache);
            SaveUtils.PutValue(DebugConsoleCommandsHistory, SaveUtils.GetValue(DebugConsoleCommandsHistory),onlyCache);
            SaveUtils.PutValue(LowPerformanceDevice,        SaveUtils.GetValue(LowPerformanceDevice),       onlyCache);
            SaveUtils.PutValue(AppVersion,                  SaveUtils.GetValue(AppVersion),                 onlyCache);
        }
    }
}