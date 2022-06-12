using System;
using System.Collections.Generic;
using Common.Entities;
using Common.Utils;
using UnityEngine;

namespace Common
{
    public static class SaveKeysCommon
    {
        private static SaveKey<bool>      _gameWasRated;
        private static SaveKey<bool>      _settingNotificationsOn;
        private static SaveKey<bool>      _settingHapticsOn;
        private static SaveKey<bool>      _settingSoundOn;
        private static SaveKey<bool>      _settingMusicOn;
        private static SaveKey<bool?>     _disableAds;
        private static SaveKey<bool>      _lastDbConnectionSuccess;
        private static SaveKey<bool>      _notFirstLaunch;
        private static SaveKey<bool>      _debugUtilsOn;
        private static SaveKey<int?>      _accountId;               
        private static SaveKey<string>    _login;        
        private static SaveKey<string>    _passwordHash;
        private static SaveKey<int?>      _previousAccountId; 
        private static SaveKey<DateTime>  _timeSinceLastIapReviewDialogShown;
        private static SaveKey<List<int>> _boughtPurchaseIds;  
        
        private static readonly Dictionary<string, SaveKey<uint>> BundleVersions =
            new Dictionary<string, SaveKey<uint>>();
        private static readonly Dictionary<Tuple<int, int, ushort>, SaveKey<GameDataField>> GameDataFieldValues = 
            new Dictionary<Tuple<int, int, ushort>, SaveKey<GameDataField>>();

        [RuntimeInitializeOnLoadMethod]
        public static void ResetState()
        {
            if (Application.isEditor)
            {
                _gameWasRated = null;
                _settingNotificationsOn = null;
                _settingHapticsOn = null;
                _settingSoundOn = null;
                _settingMusicOn = null;
                _disableAds = null;
                _lastDbConnectionSuccess = null;
                _notFirstLaunch = null;
                _debugUtilsOn = null;
                _accountId = null;
                _login = null;
                _passwordHash = null;
                _previousAccountId = null;
                _timeSinceLastIapReviewDialogShown = null;
                _boughtPurchaseIds = null;
            }
            SaveUtils.PutValue(GameWasRated,             SaveUtils.GetValue(GameWasRated),             true);
            SaveUtils.PutValue(SettingNotificationsOn,   SaveUtils.GetValue(SettingNotificationsOn),   true);
            SaveUtils.PutValue(SettingHapticsOn,         SaveUtils.GetValue(SettingHapticsOn),       true);
            SaveUtils.PutValue(SettingSoundOn,          SaveUtils.GetValue(SettingSoundOn),          true);
            SaveUtils.PutValue(SettingMusicOn,          SaveUtils.GetValue(SettingMusicOn),          true);
            SaveUtils.PutValue(DisableAds,              SaveUtils.GetValue(DisableAds),              true);
            SaveUtils.PutValue(LastDbConnectionSuccess, SaveUtils.GetValue(LastDbConnectionSuccess), true);
            SaveUtils.PutValue(NotFirstLaunch,          SaveUtils.GetValue(NotFirstLaunch),          true);
            SaveUtils.PutValue(DebugUtilsOn,            SaveUtils.GetValue(DebugUtilsOn),            true);
            SaveUtils.PutValue(AccountId,               SaveUtils.GetValue(AccountId),               true);
            SaveUtils.PutValue(Login,                   SaveUtils.GetValue(Login),                   true);
            SaveUtils.PutValue(PasswordHash,            SaveUtils.GetValue(PasswordHash),            true);
            SaveUtils.PutValue(PreviousAccountId,       SaveUtils.GetValue(PreviousAccountId),       true);
            SaveUtils.PutValue(TimeSinceLastIapReviewDialogShown, SaveUtils.GetValue(TimeSinceLastIapReviewDialogShown),  true);
            SaveUtils.PutValue(BoughtPurchaseIds,        SaveUtils.GetValue(BoughtPurchaseIds),        true);
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
        
        public static SaveKey<bool> GameWasRated => 
            _gameWasRated ??= new SaveKey<bool>(nameof(GameWasRated));
        public static SaveKey<bool> SettingNotificationsOn =>
            _settingNotificationsOn ??= new SaveKey<bool>(nameof(SettingNotificationsOn));
        public static SaveKey<bool> SettingHapticsOn => 
            _settingHapticsOn ??= new SaveKey<bool>(nameof(SettingHapticsOn));
        public static SaveKey<bool>  SettingSoundOn           
            => _settingSoundOn ??= new SaveKey<bool>(nameof(SettingSoundOn));
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
            new SaveKey<string>(nameof(AppVersion));
        public static SaveKey<DateTime> TimeSinceLastIapReviewDialogShown => 
            _timeSinceLastIapReviewDialogShown ??= new SaveKey<DateTime>(nameof(TimeSinceLastIapReviewDialogShown));
        public static SaveKey<List<int>> BoughtPurchaseIds => 
            _boughtPurchaseIds ??= new SaveKey<List<int>>(nameof(BoughtPurchaseIds));
        
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