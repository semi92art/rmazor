using Common.Utils;
using UnityEngine;

namespace Common
{
    public static class GameClientUtils
    {
        public const int DefaultAccountId = 0;

        public static int PreviousAccountId
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

        public static string DeviceId
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return $"{SystemInfo.deviceUniqueIdentifier}";
#else
                return SystemInfo.deviceUniqueIdentifier.ToString();
#endif
            }
        }

        public static int GetDefaultGameId()
        {
            return 1;
        }

        public static string ServerApiUrl
        {
            get
            {
#if UNITY_EDITOR
                if (CommonData.Testing)
                    return SaveUtilsInEditor.GetValue(SaveKeysCommon.ServerUrl);
                return SaveUtilsInEditor.GetValue(SaveKeysCommon.ServerUrl);
#else
               return "http://77.37.152.15:7000";
#endif
            }
        }
    }
}