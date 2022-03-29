using Common.Utils;
using UnityEngine;

namespace Common
{
    public static class GameClientUtils
    {
        private const int DefaultAccountId = 0;

        private static int PreviousAccountId
        {
            get => SaveUtils.GetValue(SaveKeysCommon.PreviousAccountId) ?? DefaultAccountId;
            set => SaveUtils.PutValue(SaveKeysCommon.PreviousAccountId, value);
        }
        
        public static int AccountId
        {
            get => SaveUtils.GetValue(SaveKeysCommon.AccountId) ?? DefaultAccountId;
            set
            {
                PreviousAccountId = AccountId;
                SaveUtils.PutValue(SaveKeysCommon.AccountId, value);
            }
        }

        public static string Login
        {
            get => SaveUtils.GetValue(SaveKeysCommon.Login);
            set => SaveUtils.PutValue(SaveKeysCommon.Login, value);
        }

        public static string PasswordHash
        {
            get => SaveUtils.GetValue(SaveKeysCommon.PasswordHash);
            set => SaveUtils.PutValue(SaveKeysCommon.PasswordHash, value);
        }

        public static int GameId
        {
            get => SaveUtils.GetValue(SaveKeysCommon.GameId);
            set => SaveUtils.PutValue(SaveKeysCommon.GameId, value);
        }

        public static int GetDefaultGameId()
        {
            return 1;
        }

        public static string ServerApiUrl => !Application.isEditor ? 
            "http://77.37.152.15:7000" : SaveUtilsInEditor.GetValue(SaveKeysCommon.ServerUrl);
    }
}