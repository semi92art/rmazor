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
            get => SaveUtils.GetValue(SaveKeys.PreviousAccountId) ?? DefaultAccountId;
            set => SaveUtils.PutValue(SaveKeys.PreviousAccountId, value);
        }
        
        public static int AccountId
        {
            get => SaveUtils.GetValue(SaveKeys.AccountId) ?? DefaultAccountId;
            set
            {
                PreviousAccountId = AccountId;
                SaveUtils.PutValue(SaveKeys.AccountId, value);
            }
        }

        public static string Login
        {
            get => SaveUtils.GetValue(SaveKeys.Login);
            set => SaveUtils.PutValue(SaveKeys.Login, value);
        }

        public static string PasswordHash
        {
            get => SaveUtils.GetValue(SaveKeys.PasswordHash);
            set => SaveUtils.PutValue(SaveKeys.PasswordHash, value);
        }

        public static int GameId
        {
            get => SaveUtils.GetValue(SaveKeys.GameId);
            set => SaveUtils.PutValue(SaveKeys.GameId, value);
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
                    return SaveUtils.GetValue(SaveKeys.ServerUrl);
                return SaveUtils.GetValue(SaveKeys.ServerUrl);
#else
               return "http://77.37.152.15:7000";
#endif
            }
        }
    }
}