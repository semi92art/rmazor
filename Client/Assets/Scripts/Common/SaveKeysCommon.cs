using System;
using System.Collections.Generic;
using Common.Entities;
using Common.Utils;
using UnityEngine;

namespace Common
{
    public static class SaveKeysCommon
    {
        private static SaveKey<bool>   _lastDbConnectionSuccess;
        private static SaveKey<bool>   _notFirstLaunch;
        private static SaveKey<bool>   _debugUtilsOn;
        private static SaveKey<bool>   _goodQuality;
        private static SaveKey<int?>   _accountId;               
        private static SaveKey<int>    _gameId;
        private static SaveKey<string> _login;        
        private static SaveKey<string> _passwordHash;
        private static SaveKey<int?>   _previousAccountId;  
        
        private static readonly Dictionary<Tuple<int, int, ushort>, SaveKey<GameDataField>> GameDataFieldValues = 
            new Dictionary<Tuple<int, int, ushort>, SaveKey<GameDataField>>();

        [RuntimeInitializeOnLoadMethod]
        public static void ResetState()
        {
            SaveUtils.PutValue(LastDbConnectionSuccess, SaveUtils.GetValue(LastDbConnectionSuccess), true);
            SaveUtils.PutValue(NotFirstLaunch,          SaveUtils.GetValue(NotFirstLaunch),          true);
            SaveUtils.PutValue(DebugUtilsOn,            SaveUtils.GetValue(DebugUtilsOn),            true);
            SaveUtils.PutValue(GoodQuality,             SaveUtils.GetValue(GoodQuality),             true);
            SaveUtils.PutValue(AccountId,               SaveUtils.GetValue(AccountId),               true);
            SaveUtils.PutValue(GameId,                  SaveUtils.GetValue(GameId),                  true);
            SaveUtils.PutValue(Login,                   SaveUtils.GetValue(Login),                   true);
            SaveUtils.PutValue(PasswordHash,            SaveUtils.GetValue(PasswordHash),            true);
            SaveUtils.PutValue(PreviousAccountId,       SaveUtils.GetValue(PreviousAccountId),       true);
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
        
        public static SaveKey<bool>   LastDbConnectionSuccess  => _lastDbConnectionSuccess ??= new SaveKey<bool>("last_connection_succeeded");
        public static SaveKey<bool>   NotFirstLaunch           => _notFirstLaunch ??= new SaveKey<bool>("not_first_launch");
        public static SaveKey<bool>   DebugUtilsOn             => _debugUtilsOn ??= new SaveKey<bool>("debug");
        public static SaveKey<bool>   GoodQuality              => _goodQuality ??= new SaveKey<bool>("good_quality");
        public static SaveKey<int?>   AccountId                => _accountId ??= new SaveKey<int?>("account_id");
        public static SaveKey<int>    GameId                   => _gameId ??= new SaveKey<int>("game_id");
        public static SaveKey<string> Login                    => _login ??= new SaveKey<string>("login");
        public static SaveKey<string> PasswordHash             => _passwordHash ??= new SaveKey<string>("password_hash");
        public static SaveKey<int?>   PreviousAccountId        => _previousAccountId ??= new SaveKey<int?>("previous_account_id");
        public static SaveKey<string> ServerUrl                => new SaveKey<string>("debug_server_url");
        public static SaveKey<string> AppVersion               => new SaveKey<string>("app_version");
    }
}