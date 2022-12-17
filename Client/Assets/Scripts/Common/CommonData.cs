using System.Linq;
using UnityEngine;

namespace Common
{
    public class CommonData
    {
        public const string SavedGameFileName = "main_save";

        public static int  GameId;
        public static bool DevelopmentBuild;
        public static bool Release = false;
        public static bool Testing = false;

#if FIREBASE
        public static Firebase.FirebaseApp FirebaseApp;
#endif

        public static readonly Color CompanyLogoBackgroundColor = new Color(0.06f, 0f, 0.03f);
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ResetState()
        {
            SetDefaultGameIdIfNotSet();
#if FIREBASE
            FirebaseApp = null;
#endif
            DevelopmentBuild = false;
#if !UNITY_EDITOR && DEVELOPMENT_BUILD
            DevelopmentBuild = true;
#endif
        }

        private static void SetDefaultGameIdIfNotSet()
        {
            var gameIds = new[] {GameIds.RMAZOR, GameIds.ZMAZOR};
            if (!gameIds.Contains(GameId))
                GameId = GameIds.RMAZOR;
        }
    }
}