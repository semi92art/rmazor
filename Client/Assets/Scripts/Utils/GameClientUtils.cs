using Entities;
using GameHelpers;
using UnityEngine;

namespace Utils
{
    public static class GameClientUtils
    {
        public const int DefaultAccountId = 0;

        public static int PreviousAccountId
        {
            get => SaveUtils.GetValue<int?>(SaveKey.PreviousAccountId) ?? DefaultAccountId;
            set => SaveUtils.PutValue(SaveKey.PreviousAccountId, (int?)value);
        }
        
        public static int AccountId
        {
            get => SaveUtils.GetValue<int?>(SaveKey.AccountId) ?? DefaultAccountId;
            set
            {
                PreviousAccountId = AccountId;
                SaveUtils.PutValue(SaveKey.AccountId, (int?) value);
            }
        }

        public static string Login
        {
            get => SaveUtils.GetValue<string>(SaveKey.Login);
            set => SaveUtils.PutValue(SaveKey.Login, value);
        }

        public static string PasswordHash
        {
            get => SaveUtils.GetValue<string>(SaveKey.PasswordHash);
            set => SaveUtils.PutValue(SaveKey.PasswordHash, value);
        }

        public static int GameId
        {
            get => SaveUtils.GetValue<int>(SaveKey.GameId);
            set => SaveUtils.PutValue(SaveKey.GameId, value);
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

        public static int DefaultGameId
        {
            get
            {
#if GAME_1
                return 1;
#elif GAME_2
                return 2;
#elif GAME_3
                return 3;
#elif GAME_4
                return 4;
#elif GAME_5
                return 5;
#endif
                return 1;
            }
        }

        public static string ServerApiUrl
        {
            get
            {
#if UNITY_EDITOR
                if (CommonData.Testing)
                    return SaveUtils.GetValue<string>(SaveKeyDebug.ServerUrl);
                return SaveUtils.GetValue<string>(SaveKeyDebug.ServerUrl);
#else
               return "http://77.37.152.15:7000";
#endif
            }
        }
        
        public static bool InternetConnection
        {
            get => SaveUtils.GetValue<bool>(SaveKey.LastInternetConnectionSucceeded);
            set => SaveUtils.PutValue(SaveKey.LastInternetConnectionSucceeded, value);
        }
    }
}